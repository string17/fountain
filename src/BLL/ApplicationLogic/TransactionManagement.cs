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
        public double DoCredit(double AccountBal,double Amount)
        {
            //string sql = "select * from Pure_AccountNos where AccountNos=@0";
            double accountBal = 0;
            try
            {
                //var actual = context.FirstOrDefault<PureAccountDetail>(sql, AccountNos);
                accountBal = AccountBal + Amount;
            }
            catch (Exception ex)
            {
                accountBal = Convert.ToInt32(DateTime.Now.ToString("HHmmssffff"));
                ErrorLogManager.LogError("GetAccountSequence", ex);
            }

            return accountBal;
        }
    }
}
