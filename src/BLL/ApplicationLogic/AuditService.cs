using FountainContext.Data.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BLL.ApplicationLogic
{
    public class AuditService: BaseService
    {
        private FountainDb context = FountainDb.GetInstance();

        public static string DetermineIPAddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        public static string DetermineCompName(string IP)
        {
            List<string> compName = null;
            string ocompName = "";
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                compName = GetIPHost.HostName.ToString().Split('.').ToList();
                ocompName = compName.First();

            }
            catch (Exception ex)
            {
                //log.WarnFormat("retrieval of DNS entry for {0} failed", IP);
                ocompName = IP;
            }

            return ocompName;
        }


        public static string GetMacAddress()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true");
            IEnumerable<ManagementObject> objects = searcher.Get().Cast<ManagementObject>();
            string mac = (from o in objects orderby o["IPConnectionMetric"] select o["MACAddress"].ToString()).FirstOrDefault();
            return mac;
        }


        public static int LogError(string computerDetails, string methodName, string errorMessage)
        {
            int insertError = 0;
            ILog myLog = LogManager.GetLogger(methodName);

            myLog.Info(errorMessage);

            return insertError;

        }

        public void insertAudit(PureAuditTrail newuser)
        {
            context.Insert<PureAuditTrail>(newuser);
        }

        public List<PureAuditTrail> getAuditById()
        {
            string sql = "select * from Pure_AuditTrail order by Id desc";
            var actual = context.Fetch<PureAuditTrail>(sql).ToList();
            return actual;
        }

        public bool insertTracking(PureTracking UserName)
        {
            try
            {
                context.Insert(UserName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
