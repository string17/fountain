using DAL.CustomObjects;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ApplicationLogic
{
    public class AccountManagement
    {
        private FountainDb context = FountainDb.GetInstance();

        public List<PureCountry> GetAllCountries()
        {
            string sql = "select * from Pure_Country";
            var actual = context.Fetch<PureCountry>(sql);
            return actual;
        }

        public List<PureState> GetStatesById(string CountryCode)
        {
            string sql = "select * from Pure_States where CountryCode=@0";
            var actual = context.Fetch<PureState>(sql,CountryCode);
            return actual;
        }

        public List<PureBank> GetAllBank()
        {
            string sql = "select * from Pure_Bank";
            var actual = context.Fetch<PureBank>(sql).ToList();
            return actual;
        }


        public List<PureUser> GetReferral()
        {
            string sql = "select * from Pure_User";
            var actual = context.Fetch<PureUser>(sql).ToList();
            return actual;
        }

        public bool InsertAccount(PureCustomerInfo LastName)
        {
            try
            {
                context.Insert(LastName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<CustomerInfoViewModel> GetCustomerInfo()
        {
            string sql = "select A.FirstName,A.MiddleName,A.LastName,A.UserEmail,A.PhoneNos1,A.PhoneNos2,A.AccountImg,A.HomeAddress,A.LGA,A.DOB,A.HomeCity,A.HomeLGA,A.JobTitle,A.Department,A.BusinessType,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.KRelationship,A.KLGA,A.KCity,A.Signature,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,B.TitleName,C.OccupationName,D.CountryName, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_UserTitle B on A.NameTitle = B.TitleId inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.HomeCountry inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos";
            var actual = context.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }

        public List<CustomerInfoViewModel> NewAccount()
        {
            string sql = "select A.FirstName,A.MiddleName,A.LastName,A.UserEmail,A.PhoneNos1,A.PhoneNos2,A.AccountImg,A.HomeAddress,A.LGA,A.DOB,A.HomeCity,A.HomeLGA,A.JobTitle,A.Department,A.BusinessType,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.KRelationship,A.KLGA,A.KCity,A.Signature,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,B.TitleName,C.OccupationName,D.CountryName, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_UserTitle B on A.NameTitle = B.TitleId inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.HomeCountry inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos where A.AccountStatus='false'";
            var actual = context.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }

        public List<CustomerInfoViewModel> NewAccountDetails(int AccountId)
        {
            string sql = "select A.FirstName,A.MiddleName,A.LastName,A.UserEmail,A.PhoneNos1,A.PhoneNos2,A.AccountImg,A.HomeAddress,A.LGA,A.DOB,A.HomeCity,A.HomeLGA,A.JobTitle,A.Department,A.BusinessType,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.KRelationship,A.KLGA,A.KCity,A.Signature,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,B.TitleName,C.OccupationName,D.CountryName, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_UserTitle B on A.NameTitle = B.TitleId inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.HomeCountry inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos where A.AccountStatus='false' and A.AccountId=@0";
            var actual = context.Fetch<CustomerInfoViewModel>(sql,AccountId).ToList();
            return actual;
        }
    }
}
