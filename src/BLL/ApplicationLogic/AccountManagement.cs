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

        public List<PureCountry> getAllCountries()
        {
            string sql = "select * from Pure_Country";
            var actual = context.Fetch<PureCountry>(sql);
            return actual;
        }
    }
}
