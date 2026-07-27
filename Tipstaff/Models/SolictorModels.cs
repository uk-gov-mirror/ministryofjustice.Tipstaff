using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList.Mvc;
using PagedList;
using System.Collections;
using System.Security;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Tipstaff.Models
{
    #region Solicitor Models

    public class ListTipstaffRecordSolicitorByTipstaffRecord : IListByTipstaffRecord
    {
        public int tipstaffRecordID { get; set; }
        public ICollection<TipstaffRecordSolicitor> LinkedSolicitors { get; set; }
        public Tipstaff.xPagedList<TipstaffRecordSolicitor> pLinkedSolicitors { get; set; }
        public bool TipstaffRecordClosed { get; set; }
    }

    public class ChooseSolicitorModel
    {
        public IPagedList<Solicitor> pSolicitors { get; set; }
        public TipstaffRecord tipstaffRecord { get; set; }
        public int id { get; set; }
        public string searchString { get; set; }
        public string searchFirm { get; set; }
        public int? page { get; set; }

        public ChooseSolicitorModel()
        {
            searchString = "";
            searchFirm = "";
        }
    }

    //public class CreateSolicitorForWarrant
    //{
    //    public Solicitor solicitor { get; set; }
    //    public Warrant warrant { get; set; }
    //}

    public class SolicitorFirm
    {
        [Key]
        public int solicitorFirmID { get; set; }

        [Required, MaxLength(50), Display(Name = "Name of Firm")]
        public string firmName { get; set; }

        [Required, MaxLength(100), Display(Name = "Address Line 1")]
        public string addressLine1 { get; set; }

        [MaxLength(100), Display(Name = "Address Line 2")]
        public string addressLine2 { get; set; }

        [MaxLength(100), Display(Name = "Address Line 3")]
        public string addressLine3 { get; set; }

        [MaxLength(100), Display(Name = "Town")]
        public string town { get; set; }

        [MaxLength(100), Display(Name = "County")]
        public string county { get; set; }

        [MaxLength(10), Display(Name = "Post code")]
        public string postcode { get; set; }

        [MaxLength(50), Display(Name = "DX address")]
        public string DX { get; set; }

        [MaxLength(20), Display(Name = "Day time phone number")]
        public string phoneDayTime { get; set; }

        [MaxLength(20), Display(Name = "Out of Hours phone number")]
        public string phoneOutofHours { get; set; }

        [MaxLength(60, ErrorMessage = "Maximum of 60 characters"), Display(Name = "Email address")]
        public string email { get; set; }

        public bool active { get; set; }
        public DateTime? deactivated { get; set; }

        [MaxLength(50)]
        public string deactivatedBy { get; set; }

        public virtual ICollection<Solicitor> Solicitors { get; set; }

        public virtual List<string> populatedLines
        {
            get
            {
                List<string> outputAddress = new List<string>();
                outputAddress.Add(firmName);
                outputAddress.Add(addressLine1);
                if (addressLine2 != null) outputAddress.Add(addressLine2);
                if (addressLine3 != null) outputAddress.Add(addressLine3);
                if (town != null) outputAddress.Add(town);
                if (county != null) outputAddress.Add(county);
                if (postcode != null) outputAddress.Add(postcode);
                return outputAddress;
            }
        }

        public virtual string printAddressMultiLine
        {
            get
            {
                return string.Join("\n", populatedLines.Where(l => l != null));
            }
        }

        public virtual string screenAddressSingleLine
        {
            get
            {
                List<string> popLines = populatedLines;
                return string.Join(",", popLines.ToArray());
            }
        }

        public virtual string screenAddressSingleLineExcludeName
        {
            get
            {
                List<string> popLines = populatedLines;
                popLines.RemoveAt(0);
                return string.Join(",", popLines.ToArray());
            }
        }
    }

    public class Solicitor
    {
        [Key]
        public int solicitorID { get; set; }

        [MaxLength(50), Display(Name = "First name")]
        public string firstName { get; set; }

        [Required, MaxLength(50), Display(Name = "Last name")]
        public string lastName { get; set; }

        [Display(Name = "Solicitor firm")]
        public int? solicitorFirmID { get; set; }

        [Required, Display(Name = "Title")]
        public int salutationID { get; set; }

        [MaxLength(20), Display(Name = "Day time phone number")]
        public string phoneDayTime { get; set; }

        [MaxLength(20), Display(Name = "Out of Hours phone number")]
        public string phoneOutofHours { get; set; }

        [MaxLength(60, ErrorMessage = "Maximum of 60 characters"), Display(Name = "Email address")]
        public string email { get; set; }

        public bool active { get; set; }
        public DateTime? deactivated { get; set; }

        [MaxLength(50)]
        public string deactivatedBy { get; set; }

        public bool Retention { get; set; }
        public virtual SolicitorFirm SolicitorFirm { get; set; }
        public virtual Salutation salutation { get; set; }
        public virtual ICollection<TipstaffRecordSolicitor> TipstaffRecords { get; set; }

        [Display(Name = "Name")]
        public virtual string solicitorName
        {
            get
            {
                return string.Format("{0} {1} {2}", salutation.Detail ?? "", firstName, lastName);
            }
        }

        public virtual string AddresseeName
        {
            get
            {
                return string.Format("{0} {1}", salutation.Detail ?? "", lastName);
            }
        }
    }

    public class TipstaffRecordSolicitor
    {
        [Key, Column(Order = 0)]
        public int tipstaffRecordID { get; set; }

        [Key, Column(Order = 1)]
        public int solicitorID { get; set; }

        public virtual TipstaffRecord tipstaffRecord { get; set; }
        public virtual Solicitor solicitor { get; set; }
    }

    public class TipstaffRecordSolicitorErrorViewModel
    {
        public string ErrorMessage { get; set; }
        public TipstaffRecordSolicitor tipstaffrecordsolicitor { get; set; }
    }

    public class SolicitorbyTipstaffRecordViewModel
    {
        public int solicitorID { get; set; }
        public int tipstaffRecordID { get; set; }

        public virtual Solicitor Solicitor { get; set; }
        public virtual TipstaffRecord TipstaffRecord { get; set; }

        public SolicitorbyTipstaffRecordViewModel()
        {
        }

        public SolicitorbyTipstaffRecordViewModel(int SolicitorID, int TipstaffRecordID)
        {
            solicitorID = SolicitorID;
            tipstaffRecordID = TipstaffRecordID;
            Solicitor = myDBContextHelper.CurrentContext.Solicitors.Find(solicitorID);
            TipstaffRecord = myDBContextHelper.CurrentContext.TipstaffRecord.Find(tipstaffRecordID);
        }
    }

    public class EditSolicitorbyTipstaffRecordViewModel : SolicitorbyTipstaffRecordViewModel
    {
        public SelectList SolicitorsFirmList { get; set; }
        public SelectList SalutationList { get; set; }

        public EditSolicitorbyTipstaffRecordViewModel()
        {
        }

        public EditSolicitorbyTipstaffRecordViewModel(int SolicitorID, int TipstaffRecordID)
        {
            solicitorID = SolicitorID;
            tipstaffRecordID = TipstaffRecordID;
            Solicitor = myDBContextHelper.CurrentContext.Solicitors.Find(solicitorID);
            TipstaffRecord = myDBContextHelper.CurrentContext.TipstaffRecord.Find(tipstaffRecordID);
            SolicitorsFirmList = new SelectList(myDBContextHelper.CurrentContext.SolicitorsFirms.OrderBy(s => s.firmName), "solicitorFirmID", "firmName", Solicitor.solicitorFirmID);
            SalutationList = new SelectList(myDBContextHelper.CurrentContext.Salutations.Where(x => x.active == true), "salutationID", "Detail", Solicitor.salutationID);
        }
    }

    public class SolicitorFirmByTipstaffRecordViewModel
    {
        public int solicitorFirmID { get; set; }
        public int tipstaffRecordID { get; set; }

        public virtual SolicitorFirm SolicitorFirm { get; set; }
        public virtual TipstaffRecord TipstaffRecord { get; set; }

        public SolicitorFirmByTipstaffRecordViewModel()
        {
        }

        public SolicitorFirmByTipstaffRecordViewModel(int SolicitorFirmID, int TipstaffRecordID)
        {
            solicitorFirmID = SolicitorFirmID;
            tipstaffRecordID = TipstaffRecordID;
            SolicitorFirm = myDBContextHelper.CurrentContext.SolicitorsFirms.Find(solicitorFirmID);
            TipstaffRecord = myDBContextHelper.CurrentContext.TipstaffRecord.Find(tipstaffRecordID);
        }
    }

    #endregion Solicitor Models
}