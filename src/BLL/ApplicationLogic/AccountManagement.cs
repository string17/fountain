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
    }
}
