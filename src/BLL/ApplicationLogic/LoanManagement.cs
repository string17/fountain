using DAL.CustomObjects;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BLL.ApplicationLogic
{
    public class LoanManagement
    {
        private FountainDb context = FountainDb.GetInstance();

        public List<PureLoanCategory> GetLoanCategory()
        {
            string sql = "select * from Pure_Loan_Category";
            var actual = context.Fetch<PureLoanCategory>(sql);
            return actual;
        }

        public string DoFileUpload(HttpPostedFileBase pic, string filename = "")
        {
            if (pic == null && string.IsNullOrWhiteSpace(filename))
            {
                return "";
            }
            if (!string.IsNullOrWhiteSpace(filename) && pic == null) return filename;

            string result = DateTime.Now.Millisecond + "LoanImg.jpg";
            pic.SaveAs(HttpContext.Current.Server.MapPath("~/LoanImg/") + result);
            return result;
        }

        public bool InsertLoan(PureLoan Loan)
        {
            try
            {
                context.Insert(Loan);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogManager.LogError("Insert Loan", ex);
                return false;
            }
        }

        public List<LoanStatement> GetLoanByUsername(string UserName)
        {
            string sql = "SELECT A.*,B.LoanName,C.AccountBal,D.AccountImg,D.AccountName,D.AccountStatus,D.DOB,D.HomeAddress,D.IncomeRange,D.RefName FROM Pure_Loan A INNER JOIN Pure_Loan_Category B ON B.LoanCateId=A.LoanCateId INNER JOIN Pure_Account_Details C ON C.AccountNos=A.AccountNos INNER JOIN Pure_Customer_Info D ON D.AccountNos=A.AccountNos WHERE A.Processor=@0";
            var actual = context.Fetch<LoanStatement>(sql, UserName).ToList();
            return actual;
        }

        public LoanStatement GetLoanDetails(int LoanId)
        {
            string sql = "SELECT A.*,B.LoanName,C.AccountBal,D.AccountImg,D.AccountName,D.AccountStatus,D.DOB,D.HomeAddress,D.IncomeRange,D.RefName FROM Pure_Loan A INNER JOIN Pure_Loan_Category B ON B.LoanCateId=A.LoanCateId INNER JOIN Pure_Account_Details C ON C.AccountNos=A.AccountNos INNER JOIN Pure_Customer_Info D ON D.AccountNos=A.AccountNos WHERE A.LoanId=@0 AND A.LoanStatus='P'";
            var actual = context.SingleOrDefault<LoanStatement>(sql, LoanId);
            return actual;
        }


        public List<LoanStatement> GetPendingLoan()
        {
            string sql = "SELECT A.*,B.LoanName,C.AccountBal,D.AccountImg,D.AccountName,D.AccountStatus,D.DOB,D.HomeAddress,D.IncomeRange,D.RefName FROM Pure_Loan A INNER JOIN Pure_Loan_Category B ON B.LoanCateId=A.LoanCateId INNER JOIN Pure_Account_Details C ON C.AccountNos=A.AccountNos INNER JOIN Pure_Customer_Info D ON D.AccountNos=A.AccountNos WHERE A.LoanStatus='P'";
            var actual = context.Fetch<LoanStatement>(sql).ToList();
            return actual;
        }

        public bool ApproveLoanStatus(string LoanStatus, int? LoanId,string Approver)
        {
            try
            {
                var Loan = context.SingleOrDefault<PureLoan>("where LoanId =@0", LoanId);
                Loan.Loanstatus = LoanStatus;
                Loan.Approver = Approver;
                Loan.Approveddate = DateTime.Now;
                context.Update(Loan);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
