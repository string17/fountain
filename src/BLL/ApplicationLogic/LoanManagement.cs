using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
