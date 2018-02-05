using BLL.ApplicationLogic;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PureFountain.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        public ActionResult Index()
        {
            return View();
        }
        

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
           string SystemIP = System.Web.HttpContext.Current.Request.Params["REMOTE_ADDR"] ?? System.Web.HttpContext.Current.Request.UserHostAddress;
            string SystemName = System.Environment.UserName;
            var service = new MenuManagement();
            var userManagement = new UserManagement();
            if (String.IsNullOrEmpty(User.Identity.Name))
            {
                TempData["ProfileMsg"] = TempData["ProfileMsg"];
                FormsAuthentication.SignOut();
                RedirectToAction("Login", "Account", new { SystemIP= SystemIP,SystemName=SystemName });
            }

            ViewBag.LayoutModel = service.getMenuByUsername(User.Identity.Name);
            var userDetails = userManagement.getMenuByUsername(User.Identity.Name);
            ViewBag.UserData = userDetails;
        }

        public void InsertAudit(Constants.AuditActionType action_type, string action, string performed_by)
        {
            AuditService service = new AuditService();
            PureAuditTrail audit = new PureAuditTrail();
            audit.Username = performed_by; //System.Web.HttpContext.Current.User.Identity.Name;
            audit.Useractivity = action_type.ToString();
            audit.Comment = action;
            audit.Datelog = DateTime.Now;
            audit.Systemname = System.Environment.UserName;
            audit.Systemip = System.Web.HttpContext.Current.Request.Params["REMOTE_ADDR"] ?? System.Web.HttpContext.Current.Request.UserHostAddress;
            service.insertAudit(audit);
            return;
        }

        public void InsertTracking(string UserName, string SystemIp, string SystemName)
        {
            AuditService service = new AuditService();
            PureTracking Tracking = new PureTracking();
            string SessionId = Membership.GeneratePassword(12, 1);
            Tracking.Username = UserName; //System.Web.HttpContext.Current.User.Identity.Name;
            Tracking.Sessionid = SessionId;
            Tracking.Logindate = DateTime.Now;
            Tracking.Systemname = SystemName;
            Tracking.Systemip = System.Web.HttpContext.Current.Request.Params["REMOTE_ADDR"] ?? System.Web.HttpContext.Current.Request.UserHostAddress;
            service.insertTracking(Tracking);
            return;
        }

        public string GetIP()
        {
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
        }
    }
}