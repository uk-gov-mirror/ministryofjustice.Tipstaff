using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Security;

namespace Tipstaff.Models
{
    public class Child
    {
        [Key]
        public int childID { get; set; }
        [Required, MaxLength(50), Display(Name = "Last name")]
        public string nameLast { get; set; }
        [Required, MaxLength(50), Display(Name = "First name")]
        public string nameFirst { get; set; }
        [MaxLength(50), Display(Name = "Middle name(s)"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string nameMiddle { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Birth"),PastDateorNull(ErrorMessage="Birth date cannot be in the future")]
        public DateTime? dateOfBirth { get; set; }
        [Required,Display(Name="Gender")]
        public int genderID { get; set; }
        [Display(Name="Height"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string height { get; set; }
        [Display(Name = "Build"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string build { get; set; }
        [Display(Name = "Hair colour"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string hairColour { get; set; }
        [Display(Name = "Eye colour"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string eyeColour { get; set; }
        [Required, Display(Name = "Skin colour")]
        public int? skinColourID { get; set; }
        [Display(Name = "Special Features"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string specialfeatures { get; set; }
        [Required, Display(Name="Country of Origin")]
        public int countryID { get; set; }
        [Required, Display(Name="Nationality")]
        public int? nationalityID { get; set; }
        [Required]
        public int tipstaffRecordID { get; set; }
        
        public string PNCID { get; set; }
        
        public virtual Gender gender { get; set; }
        [Display(Name = "Country of Origin")]
        public virtual Country country { get; set; }
        [Display(Name = "Nationality")]
        public virtual Nationality nationality { get; set; }
        [Display(Name = "Skin colour")]
        public virtual SkinColour SkinColour { get; set; }
        public virtual ChildAbduction childAbduction { get; set; }
        //public virtual TipstaffRecord tipstaffRecord { get; set; }
        [Display(Name="Full name of Child")]
        public virtual string fullname
        {
            get
            {
                return string.Format("{0} {1} {2}", nameFirst, nameMiddle, nameLast).Replace("  ", " ");
            }
        }
        [Display(Name="Full name of Child")]
        public virtual string PoliceDisplayName
        {
            get
            {
                return string.Format("{0}, {1} {2}", nameLast.ToUpper(), nameFirst, nameMiddle).Replace("  ", " ");
            }
        }
        [Display(Name = "Age")]
        public virtual string Age
        {
            get
            {
                if ((this.dateOfBirth.Equals(new DateTime(1901, 1, 1))) || (this.dateOfBirth == null))
                {
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
        [Display(Name = "Date of Birth")]
        public virtual string DateofBirthDisplay
        {
            get
            {
                if ((this.dateOfBirth.Equals(new DateTime(1901, 1, 1))) || (this.dateOfBirth == null))
                {
                    return "Unknown";
                }
                else
                {
                    return ((DateTime)this.dateOfBirth).ToShortDateString();
                }
            }
        }
    }
    public class ChildCreationModel
    {
        public int tipstaffRecordID { get; set; }
        public Child child { get; set; }
        public SelectList CountryList { get; set; }
        public SelectList GenderList { get; set; }
        public SelectList NationalityList { get; set; }
        public SelectList SkinColourList { get; set; }
        public virtual TipstaffRecord tipstaffRecord { get; set; }
        public bool initial { get; set; }

        public ChildCreationModel()
        {
            GenderList = new SelectList(myDBContextHelper.CurrentContext.Genders.Where(x=>x.active==true).ToList(), "genderID", "Detail");
            CountryList = new SelectList(myDBContextHelper.CurrentContext.IssuingCountries.Where(x => x.active == true).ToList(), "countryID", "Detail");
            NationalityList = new SelectList(myDBContextHelper.CurrentContext.Nationalities.Where(x => x.active == true).ToList(), "nationalityID", "Detail");
            SkinColourList = new SelectList(myDBContextHelper.CurrentContext.SkinColours.Where(x => x.active == true).ToList(), "skinColourID", "Detail");
        }
        public ChildCreationModel(int id)
        {
            tipstaffRecord = myDBContextHelper.CurrentContext.TipstaffRecord.Find(id);
            tipstaffRecordID=id;
            GenderList = new SelectList(myDBContextHelper.CurrentContext.Genders.Where(x=>x.active==true).ToList(), "genderID", "Detail");
            CountryList = new SelectList(myDBContextHelper.CurrentContext.IssuingCountries.Where(x => x.active == true).ToList(), "countryID", "Detail");
            NationalityList = new SelectList(myDBContextHelper.CurrentContext.Nationalities.Where(x => x.active == true).ToList(), "nationalityID", "Detail");
            SkinColourList = new SelectList(myDBContextHelper.CurrentContext.SkinColours.Where(x => x.active == true).ToList(), "skinColourID", "Detail");
            //Note: ChildCreationModel working dropdown validation (partially, no specific message)
        }

    }
    public class ListChildrenByTipstaffRecord:IListByTipstaffRecord
    {
        public int tipstaffRecordID { get; set; }
        public Tipstaff.xPagedList<Child> Children { get; set; }
        public bool TipstaffRecordClosed { get; set; }
    }
}