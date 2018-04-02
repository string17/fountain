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
        private readonly FountainDb _db = FountainDb.GetInstance();

        public List<PureCountry> GetAllCountries()
        {
            string sql = "select * from Pure_Country";
            var actual = _db.Fetch<PureCountry>(sql);
            return actual;
        }

        public List<PureState> GetStatesById(string CountryCode)
        {
            string sql = "select * from Pure_States where CountryCode=@0";
            var actual = _db.Fetch<PureState>(sql,CountryCode);
            return actual;
        }


        // monitor Account by username
        public List<PureCustomerInfo> GetAllCustomerByUsername(string UserName)
        {
            string sql = "";
            var actual = _db.Fetch<PureCustomerInfo>(sql, UserName).ToList();
            return actual;
        }

        public List<PureBank> GetAllBank()
        {
            string sql = "select * from Pure_Bank";
            var actual = _db.Fetch<PureBank>(sql).ToList();
            return actual;
        }


        public List<PureUser> GetReferral()
        {
            string sql = "select * from Pure_User";
            var actual = _db.Fetch<PureUser>(sql).ToList();
            return actual;
        }

        public bool InsertAccount(PureCustomerInfo LastName)
        {
            try
            {
                _db.Insert(LastName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool InsertTransaction(PureTransactionLog RequestId)
        {
            try
            {
                _db.Insert(RequestId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool InsertWithdrawal(PureWithdrawal RequestId)
        {
            try
            {
                _db.Insert(RequestId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool InsertDeposit(PureDeposit RequestId)
        {
            try
            {
                _db.Insert(RequestId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool InsertStatement(PureStatement RequestId)
        {
            try
            {
                _db.Insert(RequestId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool InsertRequest(PurePostRequest RequestId)
        {
            try
            {
                _db.Insert(RequestId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public TillManager GetTillDetails(int tillId)
        {
            string sql = "select * from Pure_TillAccount where TillId=@0 ";
            var _actual = _db.FirstOrDefault<TillManager>(sql, tillId);
            return _actual;
        }

        public List<TillManager> GetTillHistory()
        {
            string sql = "select * from Pure_TillAccount order by TillId desc ";
            var _actual = _db.Fetch<TillManager>(sql).ToList();
            return _actual;
        }

        public int ValidateTill(string tellerTill, string createdOn)
        {
            string sql = "select Count(*) from Pure_TillAccount where TellerId=@0 and CreatedOn=@1";
            var actual = _db.ExecuteScalar<int>(sql, tellerTill, createdOn);
            return Convert.ToInt32(actual);
        }


        public CreditAccount CreditAccount(float Amount, float Balance)
        {
            string sql = "SELECT CAST(SUM(@0 + @1) AS NUMERIC(18,2)) AS BALANCE";
            var actual = _db.FirstOrDefault<CreditAccount>(sql, Amount, Balance);
            return actual;
        }

        public List<CustomerInfoViewModel> GetCustomerByReferral(string UserName)
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,I.AccountTitle,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName,K.AccountBal,K.AccountNos,L.LoanDuration,L.LoanAmount,L.LoanInterest,L.LoanStatus,L.ProcessedDate,L.ApprovedDate from Pure_Customer_Info A inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId inner join Pure_Account_Details K on K.AccountNos=A.AccountNos inner join Pure_Loan L on L.AccountNos=A.AccountNos where A.RefName=@0 order by CustomerId desc";
                var actual = _db.Fetch<CustomerInfoViewModel>(sql,UserName).ToList();
            return actual;
        }



        public List<CustomerInfoViewModel> GetOverallCustomerLoan()
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,I.AccountTitle,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName,K.AccountBal,K.AccountNos,L.LoanDuration,L.LoanAmount,L.LoanInterest,L.LoanStatus,L.ProcessedDate,L.ApprovedDate from Pure_Customer_Info A inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId inner join Pure_Account_Details K on K.AccountNos=A.AccountNos inner join Pure_Loan L on L.AccountNos=A.AccountNos order by CustomerId desc";
            var actual = _db.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }

        public List<CustomerInfoViewModel> GetCustomerInfo()
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,I.AccountTitle,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId order by CustomerId desc";
            var actual = _db.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }

        public List<CustomerInfoViewModel> GetPendingAccount()
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,I.AccountTitle,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId where A.AccountStatus='false' order by CustomerId desc";
            var actual = _db.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }


        public List<CustomerInfoViewModel> GetAllAccount()
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,I.AccountTitle,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName,B.AccountBal from Pure_Customer_Info A inner join Pure_Account_Details B on B.AccountNos=A.AccountNos inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId where A.AccountStatus='true' order by CustomerId desc";
            var actual = _db.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }



        public List<CustomerInfoViewModel> NewAccount()
        {
            string sql = "select A.FirstName,A.MiddleName,A.LastName,A.UserEmail,A.PhoneNos1,A.PhoneNos2,A.AccountImg,A.HomeAddress,A.LGA,A.DOB,A.HomeCity,A.HomeLGA,A.JobTitle,A.Department,A.BusinessType,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.KRelationship,A.KLGA,A.KCity,A.Signature,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,B.TitleName,C.OccupationName,D.CountryName, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_UserTitle B on A.NameTitle = B.TitleId inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.HomeCountry inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos where A.AccountStatus='false'";
            var actual = _db.Fetch<CustomerInfoViewModel>(sql).ToList();
            return actual;
        }

        public CustomerInfoViewModel NewAccountDetails(int AccountId)
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,A.PhoneNos1,A.UserSex,A.AccountImg,A.HomeAddress,A.DOB,A.HomeCity,A.HomeLGA,A.JobTitle,A.Department,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.AccountStatus,A.NextOfKin,A.KNumber,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,B.TitleName,C.OccupationName,D.CountryName, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_UserTitle B on A.NameTitle = B.TitleId inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.HomeCountry inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos where A.AccountStatus='false' and A.CustomerId=@0";
            var actual = _db.SingleOrDefault<CustomerInfoViewModel>(sql,AccountId);
            return actual;
        }

        public CustomerInfoViewModel GetAccountDetails(int CustomerId)
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,I.AccountTitle,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId where A.CustomerId=@0 order by CustomerId desc";
            var actual = _db.SingleOrDefault<CustomerInfoViewModel>(sql, CustomerId);
            return actual;
        }

        public CustomerInfoViewModel GetAccountBal(string AccountNos)
        {
            string sql = "select A.CustomerId,A.FirstName,A.MiddleName,A.LastName,A.UserEmail,J.ReligionName,A.PhoneNos1,A.PhoneNos2,A.MaidenName,A.UserSex,A.UserLGA,A.IDIssueAuth,A.AccountImg,A.HomeAddress,A.DOB,A.JobTitle,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.AccountSign,A.KRelationship,K.AccountNos,CAST(K.AccountBal AS NUMERIC(18,2)) As Balance,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,A.ReligionId,C.OccupationName,D.CountryName,A.Nationality,A.EmploymentDate,A.AccountType,A.OfficeAddress, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.Nationality inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos inner join Pure_Account_Category I on A.AccountType=I.AccountId inner join Pure_Religion J on J.ReligionId=A.ReligionId inner join Pure_Account_Details K on K.AccountNos=A.AccountNos where K.AccountNos=@0";
            var actual = _db.SingleOrDefault<CustomerInfoViewModel>(sql,AccountNos);
            return actual;
        }
        public List<PureIdCard> GetAllCard()
        {
            string sql = "select * from Pure_IDCard";
            var actual = _db.Fetch<PureIdCard>(sql).ToList();
            return actual;
        }

        //Get all Transaction by Teller
        public List<PureDeposit> GetDepositByUserName(string UserName)
        {
            string sql = "select * from Pure_Deposit where Processor=@0 order by DepositId desc";
            var actual = _db.Fetch<PureDeposit>(sql, UserName).ToList();
            return actual;
        }


        //Get Pending Transaction
        public List<PureTransactionLog> GetPendingTransaction(string TranType)
        {
            string sql = "select * from Pure_TransactionLog where TranStatus='P' and TranType=@0";
            var actual = _db.Fetch<PureTransactionLog>(sql,TranType).ToList();
            return actual;
        }

        // display all Till account
        public List<TillManager> GetTillAccount()
        {
            string sql = "select * from Pure_TillAccount order by TillId desc";
            var actual = _db.Fetch<TillManager>(sql).ToList();
            return actual;
        }

        // Fetch Till Account details
        public TillManager GetTillAccountDetails(int TillId)
        {
            string sql = "select * from Pure_TillAccount where TillId=@0";
            var actual = _db.SingleOrDefault<TillManager>(sql, TillId);
            return actual;
        }


        // Fetch Till Account details
        public TillManager GetTellerTill(string TellerId, string dDate)
        {
            string sql = "select top 1 TillId,AccountName,AccountNos,AccountBal,TellerId from Pure_TillAccount where TellerId=@0 and CreatedOn=@1 order by TillId desc";
            var actual = _db.SingleOrDefault<TillManager>(sql,TellerId,dDate);
            return actual;
        }


        public PureTellerTill GetDailyTill(string TellerId, string dDate)
        {
            string sql = "select top 1 * from Pure_TellerTill where TellerId=@0 and DebitedDate=@1 order by DebitId desc";
            var actual = _db.FirstOrDefault<PureTellerTill>(sql, TellerId, dDate);
            return actual;
        }

        // create new Till Account
        public bool CreateTill(PureTillAccount account)
        {
            try
            {
                _db.Insert(account);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        // Modify Till Account
        public bool UpdateTill(string AccountName,decimal? AccountBal, string TellerId,bool? AccountStatus,string Drcrindicator,string CreditedBy, DateTime? CreditedOn, int TillId)
        {

            try
            {
                var Account = _db.SingleOrDefault<PureTillAccount>("Where TillId=@0", TillId);
                Account.Accountstatus = AccountStatus;
                Account.Accountname = AccountName;
                Account.Accountbal = AccountBal;
                Account.Creditedby = CreditedBy;
                Account.Drcrindicator = "DR";
                Account.Amountdebited = 0;
                Account.Createdon = CreditedOn;
                Account.Tellerid = TellerId;
                _db.Update(Account);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }


        public List<PureAccountCategory> GetAccountCategory()
        {
            string sql = "select * from Pure_Account_Category";
            var actual = _db.Fetch<PureAccountCategory>(sql).ToList();
            return actual;
        }

        public List<PureReligion> GetAllReligion()
        {
            string sql = "select * from Pure_Religion";
            var actual = _db.Fetch<PureReligion>(sql).ToList();
            return actual;
        }
        public CustomerInfoViewModel GetAccountNos(string AccountNos)
        {
            string sql = "select A.FirstName,A.MiddleName,A.LastName,A.UserEmail,A.UserSex,A.KAddress,A.PhoneNos1,A.AccountImg,A.HomeAddress,A.DOB,A.HomeCity,A.HomeLGA,A.JobTitle,A.Department,A.IncomeRange,A.IDIssueDate,A.IDExpiryDate,A.CustomerId,A.UserBVN,A.IDDetails,A.OtherAccountNos,A.NextOfKin,A.KNumber,A.KRelationship,A.RefName,A.ReasonForAccount,A.AccountName,A.AccountNos,A.AccountStatus,A.CreatedBy,A.CreatedOn,A.ApprovedBy,A.ApprovedOn,B.TitleName,C.OccupationName,D.CountryName, E.StateName,F.BankName, H.IDName,G.FirstName,G.MiddleName,G.LastName from Pure_Customer_Info A inner join Pure_UserTitle B on A.NameTitle = B.TitleId inner join Pure_Occupation C on C.OccupationId = A.OccupationId inner join Pure_Country D on D.CountryCode = A.HomeCountry inner join Pure_States E on E.StateId = A.StateOrigin inner join Pure_Bank F on F.BankId = A.OtherBankId inner join Pure_User G on G.UserName = A.RefName inner join Pure_IDCard H on H.IDNos = A.IDNos where A.AccountNos=@0";
            var actual = _db.SingleOrDefault<CustomerInfoViewModel>(sql, AccountNos);
            return actual;
        }

        public bool ApproveAccount(int CustomerId, string AccountNos, bool? AccountStatus)
        {
            try
            {
                var Account = _db.SingleOrDefault<PureCustomerInfo>("Where CustomerId=@0", CustomerId);
                Account.Accountnos = AccountNos;
                Account.Accountstatus = AccountStatus;
                _db.Update(Account);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool NewAccountNos(PureAccountNo CustomerId)
        {
            try
            {
                _db.Insert(CustomerId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
         
        public bool FreshAccount(PureAccountDetail CustomerId)
        {
            try
            {
                _db.Insert(CustomerId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
