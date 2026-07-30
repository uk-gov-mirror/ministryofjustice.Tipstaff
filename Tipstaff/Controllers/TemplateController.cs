using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Reflection;
using Tipstaff.Models;
using Tipstaff.Helpers;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TPLibrary.Logger;

namespace Tipstaff.Controllers
{
    [AuthorizeRedirect(MinimumRequiredAccessLevel = AccessLevel.User)]
    [Authorize]
    [ValidateAntiForgeryTokenOnAllPosts]
    public class TemplateController : Controller
    {
        private TipstaffDB db = myDBContextHelper.CurrentContext;

        private readonly ICloudWatchLogger _logger;

        public TemplateController(ICloudWatchLogger logger)
        {
            _logger = logger;
        }
        //
        // GET: /Template/
        public ActionResult Create(int tipstaffRecordID, int templateID)
        {
            _logger.LogInfo($"in TemplateController.Create");
            try
            {
                //Get TipstaffRecord from warrantID
                TipstaffRecord tipstaffRecord = db.TipstaffRecord.Find(tipstaffRecordID);
                if (tipstaffRecord.caseStatus.sequence > 3)
                {
                    TempData["UID"] = tipstaffRecord.UniqueRecordID;
                    return RedirectToAction("ClosedFile", "Error");
                }
                //Get Template from templateID
                Template template = db.Templates.Find(templateID);
                ValidateTemplate(template, templateID);

                //set fileOutput details
                WordFile fileOutput = new WordFile(tipstaffRecord, Server.MapPath("~/Documents/"), template);

                var placeholderFields = BuildPlaceholderFields(template, tipstaffRecord, null, null);
                byte[] fileBytes = GenerateDocument(template.templateDOTX, placeholderFields, template, tipstaffRecord);

                //Create and add a Document to TipstaffRecord
                Tipstaff.Models.Document doc = CreateDocument(fileOutput, template, fileBytes);
                tipstaffRecord.Documents.Add(doc);

                //Save Changes
                db.SaveChanges();

                return File(doc.binaryFile, doc.mimeType, doc.fileName);
            }
            catch (DbEntityValidationException ex)
            {
                _logger.LogError(ex, $"DbEntityValidationException in TemplateController in Create method, for user {((CPrincipal)User).UserID}");

                ErrorModel model = new ErrorModel(2);
                model.ErrorMessage = ex.Message;
                TempData["ErrorModel"] = model;
                return RedirectToAction("IndexByModel", "Error", model ?? null);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in TemplateController in Create method, for user {((CPrincipal)User).UserID}");

                ErrorModel model = new ErrorModel(2);
                model.ErrorMessage = ex.Message;
                TempData["ErrorModel"] = model;
                return RedirectToAction("IndexByModel", "Error", model ?? null);
            }
        }

        public ActionResult Create4(int tipstaffRecordID, int templateID, int solicitorID)
        {
            try
            {
                //get solicitor from solicitorID
                Solicitor solicitor = db.Solicitors.Find(solicitorID);

                //Get TipstaffRecord from warrantID
                TipstaffRecord tipstaffRecord = db.TipstaffRecord.Find(tipstaffRecordID);
                if (tipstaffRecord.caseStatus.sequence > 3)
                {
                    TempData["UID"] = tipstaffRecord.UniqueRecordID;
                    return RedirectToAction("ClosedFile", "Error");
                }

                //Get Template from templateID
                Template template = db.Templates.Find(templateID);
                ValidateTemplate(template, templateID);

                //set fileOutput details
                WordFile fileOutput = new WordFile(tipstaffRecord, Server.MapPath("~/Documents/"), template);

                var placeholderFields = BuildPlaceholderFields(template, tipstaffRecord, solicitor, null);
                byte[] fileBytes = GenerateDocument(template.templateDOTX, placeholderFields, template, tipstaffRecord);

                //Create and add a Document to TipstaffRecord
                Tipstaff.Models.Document doc = CreateDocument(fileOutput, template, fileBytes);
                tipstaffRecord.Documents.Add(doc);

                //Save Changes
                db.SaveChanges();

                return File(doc.binaryFile, doc.mimeType, doc.fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in TemplateController in Create4 method, for user {((CPrincipal)User).UserID}");

                ErrorModel model = new ErrorModel(2);
                model.ErrorMessage = ex.Message;
                TempData["ErrorModel"] = model;
                return RedirectToAction("IndexByModel", "Error", model ?? null);
            }
        }

        public ActionResult Create8(int tipstaffRecordID, int templateID, int applicantID)
        {
            try
            {
                //get applicant from applicantID
                Applicant applicant = db.Applicants.Find(applicantID);

                //Get TipstaffRecord from warrantID
                TipstaffRecord tipstaffRecord = db.TipstaffRecord.Find(tipstaffRecordID);
                if (tipstaffRecord.caseStatus.sequence > 3)
                {
                    TempData["UID"] = tipstaffRecord.UniqueRecordID;
                    return RedirectToAction("ClosedFile", "Error");
                }

                //Get Template from templateID
                Template template = db.Templates.Find(templateID);
                ValidateTemplate(template, templateID);

                //set fileOutput details
                WordFile fileOutput = new WordFile(tipstaffRecord, Server.MapPath("~/Documents/"), template);

                var placeholderFields = BuildPlaceholderFields(template, tipstaffRecord, null, applicant);
                byte[] fileBytes = GenerateDocument(template.templateDOTX, placeholderFields, template, tipstaffRecord);

                //Create and add a Document to TipstaffRecord
                Tipstaff.Models.Document doc = CreateDocument(fileOutput, template, fileBytes);
                tipstaffRecord.Documents.Add(doc);

                //Save Changes
                db.SaveChanges();

                return File(doc.binaryFile, doc.mimeType, doc.fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in TemplateController in Create8 method, for user {((CPrincipal)User).UserID}");

                ErrorModel model = new ErrorModel(2);
                model.ErrorMessage = ex.Message;
                TempData["ErrorModel"] = model;
                return RedirectToAction("IndexByModel", "Error", model ?? null);
            }
        }

        private static void ValidateTemplate(Template template, int templateID)
        {
            if (template == null)
                throw new FileLoadException(string.Format("No database record found for template reference {0}", templateID));
            if (template.templateDOTX == null || template.templateDOTX.Length == 0)
                throw new FileLoadException(string.Format("Template '{0}' (reference {1}) has no .dotx file uploaded", template.templateName, templateID));
        }

        // The original mergeData chain worked by successive string.Replace on the template
        // XML, so the FIRST value written for a token was the one that reached the document —
        // every later Replace for that same token found nothing left to match and did nothing.
        // The dictionary is last-write-wins, so writes after the initial seed must not clobber.
        private static void SetIfAbsent(Dictionary<string, string> placeholderFields, string key, string value)
        {
            if (!placeholderFields.ContainsKey(key))
                placeholderFields[key] = value;
        }

        private Dictionary<string, string> BuildPlaceholderFields(Template template, TipstaffRecord tipstaffRecord, Solicitor solicitor, Applicant applicant)
        {
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var ukTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ukTimeZone);

            var placeholderFields = new Dictionary<string, string>
            {
                //merge generic fields
                { "||DATE||", ukTime.ToShortDateString() },
                { "||TIME||", ukTime.ToShortTimeString() },
                { "||NOW||", ukTime.ToString("dd/MM/yy @ HH:mm") },
                { "||UNIQUERECORDID||", tipstaffRecord.UniqueRecordID },
                { "||USERNAME||", User.Identity.Name },
                { "||NPOREFERENCE||", tipstaffRecord.NPO ?? string.Empty }
            };

            // Possible addresses
            if (tipstaffRecord.addresses != null && tipstaffRecord.addresses.Any())
                placeholderFields["||POSSIBLEADDRESSES||"] = string.Join("\n\n",
                    tipstaffRecord.addresses.Select(a => a.printAddressMultiLine));
            else
                placeholderFields["||POSSIBLEADDRESSES||"] = string.Empty;

            // ||RESPONDENTSNAME||
            if (tipstaffRecord.Respondents != null && tipstaffRecord.Respondents.Any())
            {
                string respNames = string.Join(" | ",
                    tipstaffRecord.Respondents.Select(r => r.PoliceDisplayName));
                if (respNames.EndsWith(" | "))
                    respNames = respNames.Substring(0, respNames.Length - 3);
                placeholderFields["||RESPONDENTSNAME||"] = respNames;
            }
            else
            {
                placeholderFields["||RESPONDENTSNAME||"] = "<<Please enter respondent's name";
            }

            // ||ADDRESSES||
            if (tipstaffRecord.addresses != null && tipstaffRecord.addresses.Any())
                placeholderFields["||ADDRESSES||"] = string.Join("\n",
                    tipstaffRecord.addresses.Select(a => a.PrintAddressSingleLine));
            else
                placeholderFields["||ADDRESSES||"] = string.Empty;

            // ChildAbduction discriminator block
            if (genericFunctions.TypeOfTipstaffRecord(tipstaffRecord) == "ChildAbduction" && template.Discriminator == "ChildAbduction")
            {
                ChildAbduction ca = (ChildAbduction)tipstaffRecord;
                PropertyInfo[] properties = typeof(ChildAbduction).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    var propValue="";
                    object value = property.GetValue(ca, null);
                    if (value != null)
                    {
                        Type type = value.GetType();
                        if (type == typeof(string) || type == typeof(int))
                        {
                            propValue = value.ToString();
                        }
                        else if (type == typeof(DateTime))
                        {
                            propValue = ((DateTime)value).ToShortDateString();
                        }
                        else if (type == typeof(object))
                        {
                            //loop through properties of sub object
                            System.Diagnostics.Debug.Print(propValue.ToString());
                        }
                    }
                    SetIfAbsent(placeholderFields, string.Format("||{0}||", property.Name.ToUpper()), propValue);
                }

                placeholderFields["||MULTICHILD||"] = ca.children.Count() > 1 ? "children" : "child";
                placeholderFields["||MULTIRESP||"]  = ca.Respondents.Count() > 1 ? "people" : "person";

                // ||PNCIDS|| — respondents AND children combined (base mergeData behaviour)
                var pncidLines = new List<string>();
                foreach (Respondent r in ca.Respondents)
                    if (!string.IsNullOrEmpty(r.PNCID))
                        pncidLines.Add($"(Respondent) {r.PoliceDisplayName} \u2013 {r.PNCID} \u2013 {r.DateofBirthDisplay}");
                foreach (Child c in ca.children)
                    if (!string.IsNullOrEmpty(c.PNCID))
                        pncidLines.Add($"(Child) {c.PoliceDisplayName} \u2013 {c.PNCID} \u2013 {c.DateofBirthDisplay}");
                placeholderFields["||PNCIDS||"] = string.Join("\n", pncidLines);
            }
            else if (template.Discriminator == "Warrant")
            {
                Warrant warrant = tipstaffRecord as Warrant;
                PropertyInfo[] properties = typeof(Warrant).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    var propValue = "";
                    object value = property.GetValue(warrant, null);
                    if (value != null)
                    {
                        Type type = value.GetType();
                        if (type == typeof(string) || type == typeof(int))
                        {
                            propValue = value.ToString();
                        }
                        else if (type == typeof(DateTime))
                        {
                            propValue = ((DateTime)value).ToShortDateString();
                        }
                        else if (type == typeof(object))
                        {
                            //loop through properties of sub object
                            System.Diagnostics.Debug.Print(propValue.ToString());
                        }
                    }
                    SetIfAbsent(placeholderFields, string.Format("||{0}||", property.Name.ToUpper()), propValue);
                }

                if (warrant.Respondents.Count() == 1)
                {
                    var resp = warrant.Respondents.FirstOrDefault();
                    PropertyInfo[] respProp = typeof(Respondent).GetProperties();
                    foreach (PropertyInfo property in respProp)
                    {
                        var propValue = "";
                        object value = property.GetValue(resp, null);
                        if (value != null)
                        {
                            Type type = value.GetType();
                            if (type == typeof(string) || type == typeof(int))
                            {
                                propValue = value.ToString();
                            }
                            else if (type == typeof(DateTime))
                            {
                                propValue = ((DateTime)value).ToShortDateString();
                            }
                            else if (type == typeof(object))
                            {
                                //loop through properties of sub object
                                System.Diagnostics.Debug.Print(propValue.ToString());
                            }
                        }
                        SetIfAbsent(placeholderFields, string.Format("||{0}||", property.Name.ToUpper()), propValue);
                    }

                    SetIfAbsent(placeholderFields, "||GENDER.DETAIL||",      resp.gender?.detail ?? string.Empty);
                    SetIfAbsent(placeholderFields, "||NATIONALITY.DETAIL||", resp.nationality?.Detail ?? string.Empty);
                    SetIfAbsent(placeholderFields, "||COUNTRY.DETAIL||",     resp.country?.Detail ?? string.Empty);
                    SetIfAbsent(placeholderFields, "||SKINCOLOUR.DETAIL||",  resp.SkinColour?.Detail ?? string.Empty);
                    SetIfAbsent(placeholderFields, "||PNCID||", !string.IsNullOrEmpty(resp.PNCID) ? resp.PNCID : string.Empty);
                }
            }

            // ---------------------------------------------------------------
            // Overloaded mergeData logic — these are fallbacks only. In the original
            // they ran after the base mergeData had already consumed the tokens, so
            // they only ever applied where the branches above left a token unset
            // (e.g. a template with Discriminator "All"). SetIfAbsent preserves that.
            // ---------------------------------------------------------------

            if (tipstaffRecord.addresses != null)
            {
                string addresses = string.Join("\n\n",
                    tipstaffRecord.addresses.Select(a => a.printAddressMultiLine));
                SetIfAbsent(placeholderFields, "||POSSIBLEADDRESSES||", addresses);
            }

            if (genericFunctions.TypeOfTipstaffRecord(tipstaffRecord) != "Warrant")
            {
                ChildAbduction ca2 = tipstaffRecord as ChildAbduction;
                if (ca2 != null)
                {
                    var childPncids = ca2.children
                        .Where(c => !string.IsNullOrEmpty(c.PNCID))
                        .Select(c => c.PNCID)
                        .ToList();
                    SetIfAbsent(placeholderFields, "||PNCIDS||", string.Join("\n", childPncids));
                }
            }

            // Addressee and address — solicitor precedence then applicant
            if (solicitor == null && applicant == null)
            {
                SetIfAbsent(placeholderFields, "||ADDRESSEENAME||", string.Empty);
                SetIfAbsent(placeholderFields, "||ADDRESS||", "Add Address here");
            }
            else if (solicitor != null)
            {
                SetIfAbsent(placeholderFields, "||ADDRESSEENAME||", solicitor.AddresseeName ?? string.Empty);
                SetIfAbsent(placeholderFields, "||ADDRESS||", solicitor.SolicitorFirm != null
                    ? solicitor.SolicitorFirm.printAddressMultiLine
                    : string.Empty);
            }
            else
            {
                SetIfAbsent(placeholderFields, "||ADDRESSEENAME||", applicant.fullname ?? string.Empty);
                SetIfAbsent(placeholderFields, "||ADDRESS||", applicant.printAddressMultiLine ?? string.Empty);
            }

            return placeholderFields;
        }

        private Tipstaff.Models.Document CreateDocument(WordFile fileOutput, Template template, byte[] fileBytes)
        {
            return new Tipstaff.Models.Document
            {
                binaryFile = fileBytes,
                mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName = fileOutput.fileName,
                countryID = 244, //UK!
                nationalityID = 27,
                documentTypeID = 1, //generated
                documentStatusID = 1, //generated
                documentReference = template.templateName,
                templateID = template.templateID,
                createdOn = DateTime.Now,
                createdBy = User.Identity.Name
            };
        }

        private byte[] GenerateDocument(byte[] templateBytes, Dictionary<string, string> replacementFields, Template template, TipstaffRecord tipstaffRecord)
        {
            using (var outputStream = new MemoryStream())
            {
                outputStream.Write(templateBytes, 0, templateBytes.Length);
                outputStream.Position = 0;

                using (var wordDoc = WordprocessingDocument.Open(outputStream, true))
                {
                    wordDoc.ChangeDocumentType(WordprocessingDocumentType.Document);
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Structural block insertions — before text replacements
                    InsertAddressBlocks(body, "||ADDRESSBLOCK||", tipstaffRecord.addresses);

                    if (genericFunctions.TypeOfTipstaffRecord(tipstaffRecord) == "ChildAbduction"
                        && template.Discriminator == "ChildAbduction")
                    {
                        ChildAbduction ca = (ChildAbduction)tipstaffRecord;
                        InsertChildBlocks(body, "||CHILDBLOCK||", ca.children);
                        InsertRespondentBlocks(body, "||RESPONDENTBLOCK||", tipstaffRecord.Respondents);
                    }

                    // Multi-line token replacements
                    string[] multiLinePlaceholders = new[]
                    {
                        "||ADDRESS||",
                        "||POSSIBLEADDRESSES||",
                        "||ADDRESSES||",
                        "||PNCIDS||",
                        "||PNCID||"
                    };

                    foreach (var placeholder in multiLinePlaceholders)
                    {
                        if (replacementFields.ContainsKey(placeholder))
                        {
                            ReplaceTextWithLineBreaks(body, placeholder, replacementFields[placeholder]);
                            replacementFields.Remove(placeholder);
                        }
                    }

                    // Simple single-line token replacements
                    foreach (var text in body.Descendants<Text>())
                    {
                        foreach (var replacement in replacementFields)
                        {
                            if (text.Text.Contains(replacement.Key))
                                text.Text = text.Text.Replace(replacement.Key, replacement.Value);
                        }
                    }

                    wordDoc.MainDocumentPart.Document.Save();
                }

                return outputStream.ToArray();
            }
        }

        private void ReplaceTextWithLineBreaks(Body body, string placeholder, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                foreach (var text in body.Descendants<Text>().Where(t => t.Text.Contains(placeholder)).ToList())
                    text.Text = text.Text.Replace(placeholder, string.Empty);
                return;
            }

            var lines = value.Split(new[] { "\n" }, StringSplitOptions.None);

            foreach (var text in body.Descendants<Text>().Where(t => t.Text.Contains(placeholder)).ToList())
            {
                var run = text.Parent as Run;
                if (run == null) continue;

                text.Text = text.Text.Replace(placeholder, lines[0]);
                for (int i = 1; i < lines.Length; i++)
                {
                    run.Append(new Break());
                    run.Append(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
                }
            }
        }

        private void InsertAddressBlocks(Body body, string placeholder, IEnumerable<Address> addresses)
        {
            var tokenText = body.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(placeholder));
            if (tokenText == null) return;

            var tokenParagraph = tokenText.Ancestors<Paragraph>().FirstOrDefault();
            if (tokenParagraph == null) return;

            if (addresses != null)
                foreach (var addr in addresses)
                    foreach (var element in WordTableBuilder.BuildAddressTable(addr.populatedLines))
                        tokenParagraph.InsertBeforeSelf(element);

            tokenParagraph.Remove();
        }

        private void InsertChildBlocks(Body body, string placeholder, IEnumerable<Child> children)
        {
            var tokenText = body.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(placeholder));
            if (tokenText == null) return;

            var tokenParagraph = tokenText.Ancestors<Paragraph>().FirstOrDefault();
            if (tokenParagraph == null) return;

            int kidNumber = 1;
            if (children != null)
                foreach (var child in children)
                {
                    foreach (var element in WordTableBuilder.BuildChildTable(child, kidNumber))
                        tokenParagraph.InsertBeforeSelf(element);
                    kidNumber++;
                }

            tokenParagraph.Remove();
        }

        private void InsertRespondentBlocks(Body body, string placeholder, IEnumerable<Respondent> respondents)
        {
            var tokenText = body.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(placeholder));
            if (tokenText == null) return;

            var tokenParagraph = tokenText.Ancestors<Paragraph>().FirstOrDefault();
            if (tokenParagraph == null) return;

            if (respondents != null)
                foreach (var resp in respondents)
                    foreach (var element in WordTableBuilder.BuildRespondentTables(resp))
                        tokenParagraph.InsertBeforeSelf(element);

            tokenParagraph.Remove();
        }

    }
}
