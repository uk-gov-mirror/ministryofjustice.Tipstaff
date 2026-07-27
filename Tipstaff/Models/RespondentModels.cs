using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Tipstaff.Models
{
    public class Respondent
    {
        [Key]
        public int respondentID { get; set; }

        [Required, MaxLength(50), Display(Name = "Last name")]
        public string nameLast { get; set; }

        [Required, MaxLength(50), Display(Name = "First name")]
        public string nameFirst { get; set; }

        [MaxLength(50), Display(Name = "Middle name(s)"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string nameMiddle { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true),
            Display(Name = "Date of Birth"), PastDateorNull(ErrorMessage = "Birth date cannot be in the future")]
        public DateTime? dateOfBirth { get; set; }

        [Required, Display(Name = "Gender")]
        public int genderID { get; set; }

        [Required, Display(Name="Relationship to child")]
        public int? childRelationshipID { get; set; }

        [MaxLength(50), Display(Name = "Hair colour"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string hairColour { get; set; }

        [MaxLength(50), Display(Name = "Eye colour"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string eyeColour { get; set; }

        [Required, Display(Name = "Skin colour")]
        public int skinColourID { get; set; }

        [MaxLength(50), Display(Name = "Height"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string height { get; set; }

        [MaxLength(50), Display(Name = "Build"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string build { get; set; }

        [MaxLength(250), Display(Name = "Special Features"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string specialfeatures { get; set; }

        [Required, Display(Name = "Country of Origin")]
        public int countryID { get; set; }

        [Required, Display(Name = "Nationality")]
        public int? nationalityID { get; set; }

        [MaxLength(100), Display(Name = "Risk of Violence?"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string riskOfViolence { get; set; }

        [MaxLength(100), Display(Name="Risk of Drugs?"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string riskOfDrugs { get; set; }

        [Required]
        public int tipstaffRecordID  { get; set; }
        
        public string PNCID { get; set; }

        [MaxLength(100), Display(Name = "Address Line 1"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string addressLine1 { get; set; }

        [MaxLength(100), Display(Name = "Address Line 2"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string addressLine2 { get; set; }

        [MaxLength(100), Display(Name = "Address Line 3"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string addressLine3 { get; set; }

        [MaxLength(100), Display(Name = "Town"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string town { get; set; }

        [MaxLength(100), Display(Name = "County"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string county { get; set; }

        [MaxLength(10), Display(Name = "Postcode")]
        public string postcode { get; set; }

        [MaxLength(20), Display(Name = "Phone"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string phone { get; set; }

        [MaxLength(100), Display(Name = "Email"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string email { get; set; }

        [MaxLength(20), Display(Name = "Secondary Phone")]
        public string secondaryPhone { get; set; }

        [Display(Name ="Gender")]
        public virtual Gender gender { get; set; }
        [Display(Name = "Country of Origin")]
        public virtual Country country { get; set; }
        [Display(Name = "Nationality")]
        public virtual Nationality nationality { get; set; }
        [Display(Name = "Skin colour")]
        public virtual SkinColour SkinColour { get; set; }
        //public virtual ChildAbduction childAbduction { get; set; }
        public virtual TipstaffRecord tipstaffRecord { get; set; }
        public virtual ChildRelationship childRelationship { get; set; }

        [Display(Name = "Full name of respondent")]
        public virtual string fullname
        {
            get
            {
                return string.Format("{0} {1} {2}", nameFirst, nameMiddle, nameLast).Replace("  ", " ");
            }
        }
        [Display(Name = "Full name of respondent")]
        public virtual string PoliceDisplayName
        {
            get
            {
                return string.Format("{0}, {1} {2}", nameLast.ToUpper(), nameFirst, nameMiddle).Replace("  ", " ");
            }
        }
        [Display(Name = "Date of Birth")]
        public virtual string DateofBirthDisplay
        {
            get
            {
                if ((this.dateOfBirth.Equals(new DateTime(1901,1,1))) || (this.dateOfBirth==null)) 
                {
                    return "Unknown";
                } 
                else
                {
                    return ((DateTime)this.dateOfBirth).ToShortDateString();
                }
            }
        }

        [Display(Name = "Age")]
        public virtual string Age
        {
            get
            {
                if ((this.dateOfBirth.Equals(new DateTime(1901,1,1))) || (this.dateOfBirth==null)) {
                    return "Unknown";
                }
                int now = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
                int dob = int.Parse(((DateTime)dateOfBirth).ToString("yyyyMMdd"));
                string dif = (now - dob).ToString();
                int age = 0;
                if (dif.Length > 4)
                    age = int.Parse(dif.Substring(0, dif.Length - 4));
                return age.ToString();
            }
        }

        [Display(Name = "Address")]
        public virtual string printAddressMultiLine
        {
            get
            {
                return string.Join("\n", populatedLines.Where(l => l != null));
            }
        }
        [Display(Name = "Address")]
        public virtual string screenAddressMultiLine
        {
            get
            {
                List<string> popLines = populatedLines;
                return string.Join("<br />", popLines.ToArray());
            }
        }
        [Display(Name = "Address")]
        public virtual string screenAddressSingleLine
        {
            get
            {
                List<string> popLines = populatedLines;
                string result = string.Join(",", popLines.ToArray());
                return result;
            }
        }
        [Display(Name = "Address")]
        public virtual string PrintAddressSingleLine
        {
            get
            {
                return string.Join(", ", populatedLines.Where(l => l != null));
            }
        }

        private List<string> populatedLines
        {
            get
            {
                List<string> outputAddress = new List<string>();
                outputAddress.Add(addressLine1);
                if (addressLine2 != null) outputAddress.Add(addressLine2);
                if (addressLine3 != null) outputAddress.Add(addressLine3);
                if (town != null) outputAddress.Add(town);
                if (county != null) outputAddress.Add(county);
                if (postcode != null) outputAddress.Add(postcode);
                return outputAddress;
            }
        }
    }

    public class RespondentCreationModel
    {
        public int tipstaffRecordID { get; set; }
        public Respondent respondent { get; set; }
        public SelectList CountryList { get; set; }
        public SelectList GenderList { get; set; }
        public SelectList RelationToChildList { get; set; }
        public SelectList NationalityList { get; set; }
        public SelectList SkinColourList { get; set; }
        public TipstaffRecord tipstaffRecord { get; set; }
        public bool initial { get; set; }
        public RespondentCreationModel()
        {
            GenderList = new SelectList(myDBContextHelper.CurrentContext.Genders.Where(x => x.active == true).ToList(), "genderID", "Detail");
            CountryList = new SelectList(myDBContextHelper.CurrentContext.IssuingCountries.Where(x => x.active == true).ToList(), "countryID", "Detail");
            NationalityList = new SelectList(myDBContextHelper.CurrentContext.Nationalities.Where(x => x.active == true).ToList(), "nationalityID", "Detail");
            RelationToChildList = new SelectList(myDBContextHelper.CurrentContext.ChildRelationships.Where(x => x.active == true).ToList(), "childRelationshipID", "Detail");
            SkinColourList = new SelectList(myDBContextHelper.CurrentContext.SkinColours.Where(x => x.active == true).ToList(), "skinColourID", "Detail");
        }
        public RespondentCreationModel(int id)
        {
            tipstaffRecord = myDBContextHelper.CurrentContext.TipstaffRecord.Find(id);
            tipstaffRecordID = id;
            GenderList = new SelectList(myDBContextHelper.CurrentContext.Genders.Where(x => x.active == true).ToList(), "genderID", "Detail");
            CountryList = new SelectList(myDBContextHelper.CurrentContext.IssuingCountries.Where(x => x.active == true).ToList(), "countryID", "Detail");
            NationalityList = new SelectList(myDBContextHelper.CurrentContext.Nationalities.Where(x => x.active == true).ToList(), "nationalityID", "Detail");
            RelationToChildList = new SelectList(myDBContextHelper.CurrentContext.ChildRelationships.Where(x => x.active == true).ToList(), "childRelationshipID", "Detail");
            SkinColourList = new SelectList(myDBContextHelper.CurrentContext.SkinColours.Where(x => x.active == true).ToList(), "skinColourID", "Detail");
        }
    }
    public class ListRespondentsByTipstaffRecord:IListByTipstaffRecord
    {
        public int tipstaffRecordID { get; set; }
        public Tipstaff.xPagedList<Respondent> Respondents { get; set; }
        public bool TipstaffRecordClosed { get; set; }
    }
}