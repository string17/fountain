using DAL.CustomObjects;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ApplicationLogic
{
    public class TransactionManagement
    {
        private readonly FountainDb _db = FountainDb.GetInstance();


        public bool CreditRemAccount(PureRemittance account)
        {
            try
            {
                _db.Insert(account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool InsertTellerTill(PureTellerTill account)
        {
            try
            {
                _db.Insert(account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool InsertTillHistory(PureTillHistory Till)
        {
            try
            {
                _db.Insert(Till);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateDailyTill(decimal AmtDebited, decimal AmtCredited, string userName, bool Status, DateTime TranDate,string AccountNo)
        {

            try
            {
                var Account = _db.SingleOrDefault<PureTillHistory>("Where AccountNo=@0",AccountNo);
                Account.Amtdebited = AmtDebited;
                Account.Amtcredited = AmtCredited;
                Account.Tellerid = userName;
                Account.Trandate = DateTime.Now;
                Account.Tillstatus = Status;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool AllocateTill( bool Status, string UserName,string AccountNo)
        {

            try
            {
                var Account = _db.SingleOrDefault<PureTillHistory>("Where AccountNo=@0", AccountNo);
                Account.Tillstatus = Status;
                Account.Tellerid = UserName;
                Account.Createddate = DateTime.Now;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        //get the customer details
        public AccountBalance GetAccount(string AccountNos)
        {
            string sql = "select * from Pure_Account_Details where AccountNos=@0";
            var actual = _db.FirstOrDefault<AccountBalance>(sql, AccountNos);
            return actual;
        }

        //Get the withdrawal by Teller
        public List<PureWithdrawal> GetWithdrawalByUsername(string UserName)
        {
            string sql = "select * from Pure_Withdrawal where Processor=@0 order by WithdrawalId desc;";
            var actual = _db.Fetch<PureWithdrawal>(sql, UserName).ToList();
            return actual;
        }

        //Get the till Account
        public PureTillAccount GetTillAccount(string UserName, string TillDay)
        {
            string sql = "select top 1 TillId,AccountName,AccountNos,AccountBal,TellerId from Pure_TillAccount where TellerId=@0 and CreatedOn=@1 order by TillId desc;";
            var actual = _db.FirstOrDefault<PureTillAccount>(sql, UserName,TillDay);
            return actual;
        }


        //Get the Teller till
        public PureTellerTill GetTillByUserName(string UserName, string TillDay)
        {
            string sql = "select top 1 DebitId,TellerId,InitialBalance from Pure_TellerTill where TellerId=@0 and DebitedDate=@1 order by DebitId desc";
            var actual = _db.FirstOrDefault<PureTellerTill>(sql, UserName, TillDay);
            return actual;
        }


        //Update Till Account
        public bool DebitTillBal(decimal TillAmt, string UserName)
        {
            DateTime PostdDate = DateTime.Now;
            string ddate = PostdDate.ToString("yyyyMMdd");
            var TillDetails=GetTillAccount(UserName, ddate);
            var account = new PureTillAccount();
            account.Accountname = TillDetails.Accountname;
            account.Accountnos = TillDetails.Accountnos;
            account.Accountbal = TillDetails.Accountbal- TillAmt;
            account.Amountdebited = TillAmt;
            account.Currencycode = "NGN";
            account.Tellerid = UserName;
            account.Drcrindicator = "DR";
            account.Accountstatus = TillDetails.Accountstatus;
            account.Createdon = DateTime.Now;
            account.Creditedby = UserName;
           try
            {
                _db.Insert(account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool InsertDebit(decimal TillBal, decimal Amount, string UserName, string Indicator)
        {
            //var TillBal = new TransactionManagement().GetTillAccount(TellerId);
            var account = new PureTellerTill();
            account.Tellerid = UserName;
            account.Initialbalance = TillBal;
            account.Amount = Amount;
            account.Drcrindicator = Indicator;
            account.Debiteddate = DateTime.Now;
            try
            {
                _db.Insert(account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        public bool InsertWithdrawal(PureWithdrawal UserName)
        {
            //var TillBal = new TransactionManagement().GetTillAccount(TellerId);
            try
            {
                _db.Insert(UserName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }




        public bool DebitCustomer(decimal Amount, string AccountNos)
        {
            var CustomerDetails = GetAccount(AccountNos);
            decimal CurrentBal = CustomerDetails.AccountBal - Amount;
          try
            {
                var Account = _db.SingleOrDefault<PureAccountDetail>("Where AccountNos=@0", AccountNos);
                Account.Accountbal = CurrentBal;
                Account.Modifiedon = DateTime.Now;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateCustomerBal(decimal CurrentBal, string AccountNos)
        {

            try
            {
                var Account = _db.SingleOrDefault<PureAccountDetail>("Where AccountNos=@0", AccountNos);
                Account.Accountbal = CurrentBal;
                Account.Modifiedon = DateTime.Now;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Debit till account
        public bool ConfirmTransaction(string RequestId,string AccountNos, decimal Amount, string UserName)
        {
       
            bool PostingStatus = false;
            DateTime PostdDate = DateTime.Now;
            string ddate= PostdDate.ToString("yyyy-MM-dd");
            var TillDetails = GetTillByUserName(UserName, ddate);
            decimal TillBal = Convert.ToDecimal(TillDetails.Initialbalance);
            var CustomerAcc = GetAccount(AccountNos);
            decimal? customerBal = CustomerAcc.AccountBal;
            string Indicator = "DR";
           
            try
            {
              
                    TillBal = TillBal - Amount;
                    decimal CurrentBal = Convert.ToDecimal(customerBal + Amount);
                    bool UpdateCustomer = UpdateCustomerBal(CurrentBal, AccountNos);//Credit the customer
                    bool PostTransaction = InsertDebit(TillBal,Amount, UserName, Indicator);
                    PostingStatus = true;
   
            }
            catch(Exception ex)
            {
                PostingStatus = false;
            }
           
            return PostingStatus;
        }


        public bool PostWithdrawal(string RequestId, string AccountNos, decimal Amount, string UserName)
        {
          
            bool PostingStatus = false;
            DateTime PostdDate = DateTime.Now;
            string ddate = PostdDate.ToString("yyyy-MM-dd");
            var TillDetails = GetTillByUserName(UserName, ddate);
            decimal TillBal = Convert.ToDecimal(TillDetails.Initialbalance);
            var CustomerAcc = GetAccount(AccountNos);
            decimal? customerBal = CustomerAcc.AccountBal;
            string Indicator = "CR";
         
            try
            {

                TillBal = TillBal + Amount;
                decimal CurrentBal = Convert.ToDecimal(customerBal - Amount);
                        
                bool UpdateCustomer = UpdateCustomerBal(CurrentBal, AccountNos);//Credit the customer
                bool PostTransaction = InsertDebit(TillBal, Amount, UserName, Indicator);
                PostingStatus = true;

            }
            catch (Exception ex)
            {
                PostingStatus = false;
            }

            return PostingStatus;
        }
        public bool ApproveTransaction(string RequestId, string Approver, string TranStatus)
        {
           try
            {
                
                bool Transaction = UpdateTransactionLog(RequestId, Approver, TranStatus);//Debit the Till Account
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public decimal CreditTill(string AccountNos, decimal Amount,string UserName)
        {
            decimal CurrentBal = 0;
            var CustomerAcc = GetAccount(AccountNos);
            CurrentBal = Convert.ToDecimal(CustomerAcc.AccountBal - Amount);
            return CurrentBal;
        }

        //Get Transaction details
        public PureTransactionLog GetTransactionDetails(string RequesId)
        {
            string sql = "select * from Pure_TransactionLog where RequestId=@0";
            var actual = _db.FirstOrDefault<PureTransactionLog>(sql, RequesId);
            return actual;
        }

        public bool RejectTransaction(string RequestId, string UserName, string TranStatus)
        {
            bool PostingStatus =false;
            string DRIndicator = "DR";
            string CRIndicator = "CR";
            try
            {
                bool Transaction = UpdateTransactionLog(RequestId, UserName, TranStatus);//Debit the Till Account
                bool Deposit = UpdateDeposit(RequestId, TranStatus);//Debit the Till Account
                bool FlexPostDR = UpdateFlexRequest(RequestId, TranStatus, DRIndicator);//Debit the Till Account
                bool FlexPostCR = UpdateFlexRequest(RequestId, TranStatus, CRIndicator);//Debit the Till Account
                PostingStatus = true;
            }
            catch (Exception ex)
            {
                PostingStatus = false;
            }
            return PostingStatus;

          }

        public bool UpdateDeposit(string RequestId, string TranStatus)
        {

            try
            {
                var Account = _db.SingleOrDefault<PureDeposit>("Where RequestId=@0", RequestId);
                Account.Status = TranStatus;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateTransactionLog(string RequestId, string UserName, string TranStatus)
        {

            try
            {
                var Account = _db.SingleOrDefault<PureTransactionLog>("Where RequestId=@0", RequestId);
                Account.Transtatus = TranStatus;
                Account.Tranapprover = UserName;
                Account.Approveddate = DateTime.Now;
                //Account.Modifiedby = User.Identity.Name;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateFlexRequest(string RequestId,string TranStatus,string DRCRIndicator)
        {

            try
            {
                var Account = _db.SingleOrDefault<PurePostRequest>("Where RequestId=@0 and DRCRIndicator=@1", RequestId,DRCRIndicator);
                Account.Transtatus = TranStatus;
                _db.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<DepositViewModel> GetDepositHistory(string userName)
        {
            string sql = "select * from Pure_Deposit where Processor=@0 order by DepositId Desc";
            var actual = _db.Fetch<DepositViewModel>(sql, userName).ToList();
            return actual;
        }

    }
}
