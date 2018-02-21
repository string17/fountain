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
        private FountainDb context = FountainDb.GetInstance();


        public bool CreditRemAccount(PureRemittance account)
        {
            try
            {
                context.Insert(account);
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
            var actual = context.FirstOrDefault<AccountBalance>(sql, AccountNos);
            return actual;
        }

        //Get the till Account
        public List<PureWithdrawal> GetWithdrawalByUsername(string UserName)
        {
            string sql = "select * from Pure_Withdrawal where Processor=@0 order by WithdrawalId desc;";
            var actual = context.Fetch<PureWithdrawal>(sql, UserName).ToList();
            return actual;
        }

        //Get the till Account
        public PureTillAccount GetTillAccount(string UserName, string TillDay)
        {
            string sql = "select top 1 TillId,AccountName,AccountNos,AccountBal,TellerId from Pure_TillAccount where TellerId=@0 and CreatedOn=@1 order by TillId desc;";
            var actual = context.FirstOrDefault<PureTillAccount>(sql, UserName,TillDay);
            return actual;
        }


        //Get the Teller till
        public PureTellerTill GetTillByUserName(string UserName, string TillDay)
        {
            string sql = "select top 1 DebitId,TellerId,InitialBalance from Pure_TellerTill where TellerId=@0 and DebitedDate=@1 order by DebitId desc";
            var actual = context.FirstOrDefault<PureTellerTill>(sql, UserName, TillDay);
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
                context.Insert(account);
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
                context.Insert(account);
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
                var Account = context.SingleOrDefault<PureAccountDetail>("Where AccountNos=@0", AccountNos);
                Account.Accountbal = CurrentBal;
                Account.Modifiedon = DateTime.Now;
                context.Update(Account);
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
                var Account = context.SingleOrDefault<PureAccountDetail>("Where AccountNos=@0", AccountNos);
                Account.Accountbal = CurrentBal;
                Account.Modifiedon = DateTime.Now;
                //Account.Modifiedby = User.Identity.Name;
                context.Update(Account);
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
            //string MethodName="Debit Till";
            bool PostingStatus = false;
            DateTime PostdDate = DateTime.Now;
            string ddate= PostdDate.ToString("yyyy-MM-dd");
            var TillDetails = GetTillByUserName(UserName, ddate);
            decimal TillBal = Convert.ToDecimal(TillDetails.Initialbalance);
            var CustomerAcc = GetAccount(AccountNos);
            decimal? customerBal = CustomerAcc.AccountBal;
            string Indicator = "DR";
            //string CRIndicator = "CR";
            try
            {
              
                    TillBal = TillBal - Amount;
                    //bool UpdateTill = DebitTillBal(TillBal, UserName);//Debit the Till Account

                    decimal CurrentBal = Convert.ToDecimal(customerBal + Amount);
                    bool UpdateCustomer = UpdateCustomerBal(CurrentBal, AccountNos);//Credit the customer
                    bool PostTransaction = InsertDebit(TillBal,Amount, UserName, Indicator);

                    //bool Transaction = UpdateTransactionLog(RequestId, Approver, TranStatus);//Debit the Till Account
                    //bool Deposit = UpdateDeposit(RequestId, TranStatus);//Debit the Till Account
                    //bool FlexPostDR = UpdateFlexRequest(RequestId, TranStatus,DRIndicator);//Debit the Till Account
                    //bool FlexPostCR = UpdateFlexRequest(RequestId, TranStatus, CRIndicator);//Debit the Till Account
                    PostingStatus = true;
   
            }
            catch(Exception ex)
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
            var actual = context.FirstOrDefault<PureTransactionLog>(sql, RequesId);
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
                var Account = context.SingleOrDefault<PureDeposit>("Where RequestId=@0", RequestId);
                Account.Status = TranStatus;
                context.Update(Account);
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
                var Account = context.SingleOrDefault<PureTransactionLog>("Where RequestId=@0", RequestId);
                Account.Transtatus = TranStatus;
                Account.Tranapprover = UserName;
                Account.Approveddate = DateTime.Now;
                //Account.Modifiedby = User.Identity.Name;
                context.Update(Account);
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
                var Account = context.SingleOrDefault<PurePostRequest>("Where RequestId=@0 and DRCRIndicator=@1", RequestId,DRCRIndicator);
                Account.Transtatus = TranStatus;
                context.Update(Account);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



    }
}
