using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Data.Entity.Validation;
using System.Security.Principal;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Metadata.Edm;
using System.Configuration;
using System.Reflection;

namespace Tipstaff.Models
{
    public class TipstaffDB : DbContext
    {
        public static string GetRDSConnectionString()
        {
            var appConfig = ConfigurationManager.AppSettings;
            string dbname = Environment.GetEnvironmentVariable("DB_NAME");
            if (!string.IsNullOrEmpty(dbname))
            {
                string username = Environment.GetEnvironmentVariable("RDS_USERNAME");
                string password = Environment.GetEnvironmentVariable("RDS_PASSWORD");
                string hostname = Environment.GetEnvironmentVariable("RDS_HOSTNAME");
                string port = Environment.GetEnvironmentVariable("RDS_PORT");

                var settings = ConfigurationManager.ConnectionStrings[1];
                var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                fi.SetValue(settings, false);
                settings.ConnectionString = "Server=" + hostname + ";port=" + port + ";database=" + dbname + ";user id=" + username + ";password=" + password;
            }
            return appConfig["DataContextName"];
        }

        public TipstaffDB()
           : base(GetRDSConnectionString())
        { }

        //SSG Standard Models
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<AuditEventDataRow> AuditEventRows { get; set; }
        public DbSet<AuditEventDescription> AuditDescriptions { get; set; }
        //Application Specifc Models
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactType> ContactTypes { get; set; }
        public DbSet<Solicitor> Solicitors { get; set; }
        public DbSet<SolicitorFirm> SolicitorsFirms { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<FaxCode> FaxCodes { get; set; }
        public DbSet<AttendanceNoteCode> AttendanceNoteCodes { get; set; }
        public DbSet<TipstaffRecord> TipstaffRecord { get; set; }
        public DbSet<ChildAbduction> ChildAbductions { get; set; }
        public DbSet<Warrant> Warrants { get; set; }
        public DbSet<ProtectiveMarking> ProtectiveMarkings { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<CaseStatus> CaseStatuses { get; set; }
        //public DbSet<WarrantCaseStatus> WarrantCaseStatuses { get; set; }
        //public DbSet<ChildAbductionCaseStatus> ChildAbductionCaseStatuses { get; set; }
        public DbSet<CaseReviewStatus> CaseReviewStatuses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Passport> Passports { get; set; }
        public DbSet<DocumentStatus> DocumentStatuses { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Country> IssuingCountries { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<TipstaffRecordSolicitor> TipstaffRecordSolicitors { get; set; }
        public DbSet<CaseReview> CaseReviews { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Respondent> Respondents { get; set; }
        public DbSet<AttendanceNote> AttendanceNotes { get; set; }
        //public DbSet<party> Parties { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<ChildRelationship> ChildRelationships { get; set; }
        public DbSet<Salutation> Salutations { get; set; }
        public DbSet<CAOrderType> CAOrderTypes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        //public DbSet<Contemnor> Contemnors { get; set; }
        public DbSet<DeletedTipstaffRecord> DeletedTipstaffRecords{ get; set; }
        public DbSet<DeletedReason> DeletedReasons { get; set; }
        public DbSet<SkinColour> SkinColours { get; set; }
        public DbSet<PoliceForce> PoliceForces { get; set; }
        public DbSet<TipstaffPoliceForce> TipstaffPoliceForces { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }


        public override int SaveChanges()
        {
            //int result = -1;
            DateTime eventDateTime = DateTime.Now;
            //string eventUser = HttpContext.Current.User.Identity.Name;
            User u = GetUserByLoginName(HttpContext.Current.User.Identity.Name.Split('\\').Last(), false);
            string eventUser = u.DisplayName;
            ChangeTracker.DetectChanges(); // Important!

            ObjectContext ctx = ((IObjectContextAdapter)this).ObjectContext;

            List<ObjectStateEntry> objectStateEntryList =
                ctx.ObjectStateManager.GetObjectStateEntries(EntityState.Added
                                                           | EntityState.Modified
                                                           | EntityState.Deleted)
                .ToList();


            foreach (ObjectStateEntry entry in objectStateEntryList)
            {
                AuditEvent auditRecord = new AuditEvent();
                auditRecord.EventDate = DateTime.Now;
                auditRecord.UserID = eventUser;
                if (entry.Entity == null) continue;
                string objModel = entry.Entity.ToString();
                string[] objArray = objModel.Split('.');
                string objType = objArray.Last(); ;
                if (objType.Contains("_"))
                {
                    objType = objType.Substring(0, objType.IndexOf('_'));
                    var testAuditType = this.AuditDescriptions.Where(a => a.AuditDescription.StartsWith(objType));
                    if (testAuditType.Count() == 0)
                    {
                        objType = entry.EntitySet.ToString();
                    }
                    else
                    {
                        string newObjType = testAuditType.First().AuditDescription.ToString();
                        int spaceLoc = newObjType.IndexOf(' ');
                        if (spaceLoc > 0)
                        {
                            objType = newObjType.Substring(0, spaceLoc);
                        }
                    }
                }
                if (!entry.IsRelationship && !objType.StartsWith("Audit") && objType != "EdmMetadata")
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            {
                                //result = base.SaveChanges();
                                string objName = string.Format("{0} Added", objType);
                                int AuditType = this.AuditDescriptions.Where(a => a.AuditDescription.ToLower() == objName.ToLower()).Take(1).Single().idAuditEventDescription;
                                auditRecord.EventDescription = objName;
                                auditRecord.idAuditEventDescription = AuditType;
                                auditRecord.RecordChanged = entry.CurrentValues.GetValue(0).ToString();
                                try
                                {
                                    int ord = entry.CurrentValues.GetOrdinal("tipstaffRecordID");
                                    string value = entry.CurrentValues.GetValue(ord).ToString();
                                    auditRecord.RecordAddedTo = Int32.Parse(value);
                                }
                                catch
                                {
                                    try
                                    {
                                        int ord = entry.CurrentValues.GetOrdinal("warrantID");
                                        string value = entry.CurrentValues.GetValue(ord).ToString();
                                        auditRecord.RecordAddedTo = Int32.Parse(value);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            int ord = entry.CurrentValues.GetOrdinal("childAbductionID");
                                            string value = entry.CurrentValues.GetValue(ord).ToString();
                                            auditRecord.RecordAddedTo = Int32.Parse(value);
                                        }
                                        catch
                                        {
                                            auditRecord.RecordAddedTo = null;
                                        }
                                    }
                                }
                                break;
                            }
                        case EntityState.Deleted:
                            {
                                string objName = string.Format("{0} Deleted", objType);
                                int AuditType = this.AuditDescriptions.Where(a => a.AuditDescription.ToLower() == objName.ToLower()).Take(1).Single().idAuditEventDescription;
                                auditRecord.EventDescription = objName;
                                auditRecord.idAuditEventDescription = AuditType;
                                auditRecord.RecordChanged = entry.OriginalValues.GetValue(0).ToString();
                                try
                                {
                                    int ord = entry.OriginalValues.GetOrdinal("tipstaffRecordID");
                                    string value = entry.OriginalValues.GetValue(ord).ToString();
                                    auditRecord.RecordAddedTo = Int32.Parse(value);
                                }
                                catch
                                {
                                    try
                                    {
                                        int ord = entry.OriginalValues.GetOrdinal("warrantID");
                                        string value = entry.OriginalValues.GetValue(ord).ToString();
                                        auditRecord.RecordAddedTo = Int32.Parse(value);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            int ord = entry.OriginalValues.GetOrdinal("childAbductionID");
                                            string value = entry.OriginalValues.GetValue(ord).ToString();
                                            auditRecord.RecordAddedTo = Int32.Parse(value);
                                        }
                                        catch
                                        {
                                            auditRecord.RecordAddedTo = null;
                                        }
                                    }
                                }
                                // Iterate over the members (i.e. properties (including complex properties), references, collections) of the entity type
                                List<AuditEventDataRow> data = new List<AuditEventDataRow>();
                                foreach (EdmMember member in entry.EntitySet.ElementType.Members)
                                {
                                    string propertyName = member.Name.ToString();
                                    DbPropertyValues oldData = this.Entry(entry.Entity).GetDatabaseValues();
                                    string oldValue = "";
                                    string newValue = "deleted";
                                    try
                                    {
                                        oldValue = (oldData.GetValue<object>(propertyName) != null) ? oldData.GetValue<object>(propertyName).ToString() : "Empty";
                                        if (oldValue == "") oldValue = "Empty";
                                    }
                                    catch
                                    { oldValue = "Could not be mapped"; }

                                    if ((oldValue != newValue) && (oldValue != "Could not be mapped")) // probably not necessary
                                    {
                                        AuditEventDataRow newAuditRow = new AuditEventDataRow();
                                        newAuditRow.ColumnName = propertyName;
                                        newAuditRow.Was = oldValue.Length <= 199 ? oldValue : oldValue.Substring(0, 199);
                                        newAuditRow.Now = newValue.Length <= 199 ? newValue : newValue.Substring(0, 199);
                                        data.Add(newAuditRow);
                                    }

                                }
                                if (data.Count() > 0)
                                {
                                    auditRecord.AuditEventDataRows = data;
                                }
                                break;
                            }
                        case EntityState.Modified:
                            {                                
                                string objName = string.Format("{0} Amended", objType);
                                int AuditType = this.AuditDescriptions.Where(a => a.AuditDescription.ToLower() == objName.ToLower()).Take(1).Single().idAuditEventDescription;
                                auditRecord.EventDescription = objName;
                                auditRecord.idAuditEventDescription = AuditType;
                                auditRecord.RecordChanged = entry.CurrentValues.GetValue(0).ToString();
                                List<AuditEventDataRow> data = new List<AuditEventDataRow>();
                                foreach (string propertyName in entry.GetModifiedProperties())
                                {
                                    DbPropertyValues oldData = this.Entry(entry.Entity).GetDatabaseValues();
                                    string oldValue = (oldData.GetValue<object>(propertyName) != null) ? oldData.GetValue<object>(propertyName).ToString() : "Empty";
                                    if (oldValue == "") oldValue = "Empty";

                                    CurrentValueRecord current = entry.CurrentValues;
                                    string newValue = (current.GetValue(current.GetOrdinal(propertyName)) != null) ? current.GetValue(current.GetOrdinal(propertyName)).ToString() : "Empty";
                                    if (newValue == "") newValue = "Empty";

                                    if (objType == "Template" && propertyName == "templateDOTX")
                                    {
                                        oldValue = "DOTX";
                                        newValue = "DOTX - Too long to record new version";
                                    }

                                    if (oldValue != newValue) // probably not necessary
                                    {
                                        AuditEventDataRow newAuditRow = new AuditEventDataRow();
                                        newAuditRow.ColumnName = propertyName;
                                        newAuditRow.Was = oldValue.Length <= 199 ? oldValue : oldValue.Substring(0, 199);
                                        newAuditRow.Now = newValue.Length <= 199 ? newValue : newValue.Substring(0, 199);
                                        data.Add(newAuditRow);
                                    }
                                }
                                if (data.Count() > 0)
                                {
                                    auditRecord.AuditEventDataRows = data;
                                }
                                break;
                            }
                    }
                }

                if (auditRecord.RecordChanged == "0" && auditRecord.RecordAddedTo == 0 && auditRecord.EventDescription.Contains("Added"))
                {
                    //New TipstaffRecord derivative record added, so...
                    //save the record
                    base.SaveChanges();
                    //extract the new identity
                    auditRecord.RecordChanged = entry.CurrentValues.GetValue(0).ToString();
                    //update the audit event 
                    this.AuditEvents.Add(auditRecord);
                    //and savechanges at the end of the code block
                }
                else if (auditRecord.RecordChanged == "0" && auditRecord.RecordAddedTo != 0 && auditRecord.EventDescription.Contains("Added"))
                {
                    //New record added, so...
                    //save the record
                    base.SaveChanges();
                    //extract the new identity
                    auditRecord.RecordChanged = entry.CurrentValues.GetValue(0).ToString();
                    //update the audit event 
                    this.AuditEvents.Add(auditRecord);
                    //and savechanges at the end of the code block
                }
                else if (auditRecord.RecordChanged != "0" && auditRecord.RecordChanged != null && (auditRecord.AuditEventDataRows != null && auditRecord.AuditEventDataRows.Count > 0))
                {
                    this.AuditEvents.Add(auditRecord);
                    //base.SaveChanges();
                }
                try
                {  
                    //base.SaveChanges();
                    //only uncomment for error handling
                }
                catch (DbEntityValidationException ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }
            }
            return base.SaveChanges();
        }

        public User GetUserByLoginName(string loginName, bool save = true)
        {
            var users = Users.Where(x => x.Name.ToLower() == loginName.ToLower());
            switch (users.Count())
            {
                case 0:
                    throw new Exception(string.Format("No users match {0}", loginName));
                case 1:
                    User usr = users.SingleOrDefault();
                    usr.LastActive = DateTime.Now;
                    if (save)
                       SaveChanges();
                    return usr;
                default:
                    throw new Exception(string.Format("There are {0} users matching {1}", users.Count(), loginName));
            }
        }

        public AccessLevel UserAccessLevel(IPrincipal User)
        {
            try
            {
                AccessLevel? grpLvl = null;
                AccessLevel? usrLvl = null;

                string userName = ((string)User.Identity.Name).Split('\\').Last();
                User usr = Users.Where(x => x.Name.ToLower()==userName.ToLower()).FirstOrDefault();
                if (usr != null)
                {
                    usrLvl = (AccessLevel)usr.RoleStrength;
                    if (usrLvl != null)
                    {
                        return (AccessLevel)usrLvl;
                    }
                }
                return grpLvl ?? AccessLevel.Denied;
            }
            catch (Exception ex)
            {
                //return an error code
                return AccessLevel.Denied;
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            return Users.OrderBy(u => u.DisplayName);
        }

        public IEnumerable<Role> GetAllRoles()
        {
            CPrincipal usr = new CPrincipal(HttpContext.Current.User.Identity);
            if (usr.AccessLevel == AccessLevel.SystemAdmin)
            {
                return Roles.ToList();
            }
            else
            {
                return Roles.Where(x => x.strength != 100);
            }
        }

        public User GetUserByID(int id)
        {
            return Users.Find(id);
        }

        public void UserAdd(User model)
        {
            Entry(model).State = EntityState.Added;
            SaveChanges();
        }
        
        public void UpdateUser(User model)
        {
            Entry(model).State = EntityState.Modified;
            SaveChanges();
        }
    }
}