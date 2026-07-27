using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PagedList;
using System.Security;
using System.Collections.Generic;
namespace Tipstaff.Models
{
    public class ListAddressesByTipstaffRecord : IListByTipstaffRecord
    {
        public int tipstaffRecordID { get; set; }
        public Tipstaff.xPagedList<Address> Addresses { get; set; }
        //public IPagedList<Address> Addresses { get; set; }
        public bool TipstaffRecordClosed { get; set; }
    }

    public class AddressCreationModel
    {
        public int tipstaffRecordID { get; set; }
        public TipstaffRecord tipstaffRecord { get; set; }
        public Address address { get; set; }

        public AddressCreationModel() { }

        public AddressCreationModel(int id)
        {
            tipstaffRecord = myDBContextHelper.CurrentContext.TipstaffRecord.Find(id);
            tipstaffRecordID = id;
        }
    }

    public class Address
    {
        [Key]
        public int addressID { get; set; }
        [MaxLength(100), Display(Name = "Name")]
        public string addresseeName { get; set; }
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
        [MaxLength(20), Display(Name = "Phone")]
        public string phone { get; set; }
        [MaxLength(100), Display(Name = "Email"), DisplayFormat(ConvertEmptyStringToNull = true)]
        public string email { get; set; }
        [MaxLength(20), Display(Name = "Secondary Phone")]
        public string secondaryPhone { get; set; }
        [Required]
        public int tipstaffRecordID { get; set; }

        public virtual TipstaffRecord tipstaffRecord { get; set; }

        public virtual List<string> populatedLines
        {
            get
            {
                List<string> outputAddress = new List<string>();
                if (addresseeName != null) outputAddress.Add(addresseeName);
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
        public virtual string screenAddressMultiLine
        {
            get
            {
                List<string> popLines = populatedLines;
                return string.Join("<br />", popLines.ToArray());
            }
        }
        public virtual string outputAddressSingleLine
        {
            get
            {
                List<string> popLines = populatedLines;
                string result = string.Join(",", popLines.ToArray());
                return result;
            }
        }
        public virtual string PrintAddressSingleLine
        {
            get
            {
                return string.Join(", ", populatedLines.Where(l => l != null));
            }
        }
    }
}