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
    public class ErrorLogManager
    {
    
        public static int LogError(string methodName, Exception ex)
        {
            var ipaddress = AuditService.DetermineIPAddress();
            var ComputerDetails = AuditService.DetermineCompName(ipaddress);
            string MethodName = Constants.AuditActionType.CustomerAccount.ToString();
            int insertError = 0;
            ILog myLog = LogManager.GetLogger(methodName);

            myLog.Debug("Message: " + ex.Message + "|StackTrace: " + ex.StackTrace);

            return insertError;

        }
        public static int LogWarning(string methodName, string errorMessage)
        {
            var ipaddress = AuditService.DetermineIPAddress();
            var ComputerDetails = AuditService.DetermineCompName(ipaddress);
            string MethodName = Constants.AuditActionType.CustomerAccount.ToString();
            int insertError = 0;
            ILog myLog = LogManager.GetLogger(methodName);

            myLog.WarnFormat(errorMessage);

            return insertError;

        }
    }
}
