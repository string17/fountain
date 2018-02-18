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
        public PureAccountDetail GetAccount(string AccountNos)
        {
            string sql = "select * from Pure_AccountNos where AccountNos=@0";
            var actual = context.FirstOrDefault<PureAccountDetail>(sql, AccountNos);
            return actual;
        }

        //Get the till Account
        public PureTillAccount GetTillAccount(string UserName)
        {
            string sql = "select * from Pure_TillAccount where TellerId=@0";
            var actual = context.FirstOrDefault<PureTillAccount>(sql, UserName);
            return actual;
        }

        //Update Till Account
        public bool UpdateTillBal(double TillBal, string UserName)
        {
         
            try
            {
                var Account = new PureRemittance();
                Account.Remamount = TillBal;
                Account.Creditedby = UserName;
                Account.Creditedon = DateTime.Now;
                context.Insert(Account);
                return true;
             
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool UpdateCustomerBal(double CurrentBal, string AccountNos)
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
        public bool ApproveTransaction(string RequestId,string AccountNos, double Amount, string UserName, string TranStatus)
        {
            //string MethodName="Debit Till";
            bool PostingStatus = false;
            double CurrentBal = 0;
            var TillDetails = GetTillAccount(UserName);
            double TillBal = Convert.ToDouble(TillDetails.Accountbal);
            var CustomerAcc = GetAccount(AccountNos);
            string DRIndicator = "DR";
            string CRIndicator = "CR";
            try
            {
                if (TillBal > Amount)
                {
                    TillBal = TillBal - Amount;
                    bool UpdateTill = UpdateTillBal(TillBal, UserName);//Debit the Till Account

                    CurrentBal = Convert.ToDouble(CustomerAcc.Accountbal + Amount);
                    bool UpdateCustomer = UpdateCustomerBal(CurrentBal, AccountNos);//Credit the customer

                    bool Transaction = UpdateTransactionLog(RequestId, UserName, TranStatus);//Debit the Till Account
                    bool Deposit = UpdateDeposit(RequestId, TranStatus);//Debit the Till Account
                    bool FlexPostDR = UpdateFlexRequest(RequestId, TranStatus,DRIndicator);//Debit the Till Account
                    bool FlexPostCR = UpdateFlexRequest(RequestId, TranStatus, CRIndicator);//Debit the Till Account
                    PostingStatus = true;
                }
               
               
            }
            catch(Exception ex)
            {
                PostingStatus = false;
            }
           
            return PostingStatus;
        }

        public double CreditTill(string AccountNos, double Amount,string UserName)
        {
            double CurrentBal = 0;
            var CustomerAcc = GetAccount(AccountNos);
            CurrentBal = Convert.ToDouble(CustomerAcc.Accountbal - Amount);
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
                Account.Posteddate = DateTime.Now;
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
