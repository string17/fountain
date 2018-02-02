using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DataAccessLayer.CustomObjects
{
    public class CompanyViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerRespTime { get; set; }
        public string CustomerRestTime { get; set; }
        public string CustomerRest1Time { get; set; }
        public HttpPostedFileBase CustomerBanner { get; set; }
        public string CustomerAlias { get; set; }
        public bool? CustomerStatus { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
