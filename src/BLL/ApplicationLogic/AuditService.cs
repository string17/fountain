using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ApplicationLogic
{
    public class AuditService: BaseService
    {
        private FountainDb context = FountainDb.GetInstance();
        public void insertAudit(PureAuditTrail newuser)
        {
            context.Insert<PureAuditTrail>(newuser);
        }

        public List<PureAuditTrail> getAuditById()
        {
            var actual = context.Fetch<PureAuditTrail>().ToList();
            return actual;
        }
    }
}
