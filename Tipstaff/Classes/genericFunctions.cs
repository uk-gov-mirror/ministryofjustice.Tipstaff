using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tipstaff;
using Tipstaff.Models;
using System.Xml;
using System.Text;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
namespace Tipstaff
{
    public static partial class genericFunctions
    {
        /// <summary>
        /// Send in a pluralised string descriptor and the number of records
        /// </summary>
        /// <param name="toAssess">Field descriptor in plural form</param>
        /// <param name="numberofRecords">Number of records (from database?)</param>
        /// <returns></returns>
        public static string SingulariseString(string toAssess, int numberofRecords)
        {
            string returnVal = toAssess;
            if (numberofRecords == 1)
            {
                PluralizationService ps = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-GB"));
                returnVal = ps.Singularize(toAssess);
            }
            return returnVal;
        }
        public static string TypeOfTipstaffRecord(TipstaffRecord obj)
        {
            return isTipstaffRecordChildAbduction(obj) ? "ChildAbduction" : "Warrant";
        }
        public static string TypeOfTipstaffRecord(int id)
        {
            using (TipstaffDB tempDB = new TipstaffDB())
            {
                TipstaffRecord obj = tempDB.TipstaffRecord.Find(id);
                return isTipstaffRecordChildAbduction(obj) ? "ChildAbduction" : "Warrant";
            }
        }
        public static bool isTipstaffRecordChildAbduction(TipstaffRecord obj)
        {
            try
            {
                ChildAbduction CA = (ChildAbduction)obj;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetLowestError(Exception ex)
        {
            while (ex.Message.Contains("inner ex"))
            {
                return GetLowestError(ex.InnerException);
            };
            return ex.Message;
        }

    }
}