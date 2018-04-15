using BLL.ApplicationLogic;
using DAL.CustomObjects;
using DataAccessLayer.CustomObjects;
using FountainContext.Data.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace PureFountain.Controllers
{
    public class AccountController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly UserManagement _userMgt;

        public AccountController()
        {
            _userMgt = new UserManagement();
        }
        // GET: Account
        [AllowAnonymous]
        [Route("Login/{SystemName,SystemIP}")]
        public ActionResult Login(string returnUrl, string SystemName, string SystemIP)
        {
            ViewBag.ProfileMsg = TempData["ProfileMsg"];
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(UserLogin _user)
        {
            string ErrorMsg = "";
            var ipaddress = AuditService.DetermineIPAddress();
            var ComputerDetails = AuditService.DetermineCompName(ipaddress);
            string MethodName = Constants.AuditActionType.Login.ToString();
            try
            {
                string _Username = _user.Username;
                string _Password = _user.Password;
             if (!_userMgt.DoesUsernameExists(_Username))
                {
                    ErrorMsg = "Invalid Username";
                    Log.InfoFormat(MethodName, ErrorMsg);
                    ViewBag.ErrorMsg = "Invalid Username";
                    return View();
                }
                _Password = _userMgt.EncryptPassword(_Password);// Encode password

                if (!_userMgt.DoesPasswordExists(_Username, _Password))
                {
                    ViewBag.ErrorMsg = "Invalid Password";
                    Log.InfoFormat(MethodName, ErrorMsg);
                    return View();
                }

                var ActiveUser = _userMgt.getUserByUsername(_Username);
                if (ActiveUser != null)
                {
                    var AccountStatus = ActiveUser.Userstatus;
                    if (AccountStatus == true)
                    {
                        var newUser = _userMgt.getFreshUser(_Username);
                        if (newUser != 0)
                        {
                            var ExtLogin = _userMgt.TrackLogin(_Username);
                            if (ExtLogin == null)
                            {
                                FormsAuthentication.SetAuthCookie(ActiveUser.Username, false);
                                InsertAudit(Constants.AuditActionType.Login, "Successfully Login", _Username);
                                InsertTracking(_Username, ipaddress, ComputerDetails);
                                Log.InfoFormat("Successfully Login",_Username, ipaddress, ComputerDetails);
                                return RedirectToAction("Dashboard", "Fountain");
                            }

                            else if (ExtLogin.Username == _Username && ExtLogin.Systemname == ComputerDetails && ExtLogin.Systemip == ipaddress)
                            {
                                FormsAuthentication.SetAuthCookie(ActiveUser.Username, false);
                                InsertAudit(Constants.AuditActionType.Login, "Successfully Login", _Username);
                                InsertTracking(_Username, ipaddress, ComputerDetails);
                                Log.InfoFormat("You didn't logout properly last time", _Username, ipaddress, ComputerDetails);
                                ViewBag.ErrorMsg = "You didn't logout properly last time";
                                return View();
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "You didn't logout properly last time";
                                Log.InfoFormat("You didn't logout properly last time", _Username, ipaddress, ComputerDetails);
                                return View();
                            }

                        }
                        else
                        {
                            int Id = ActiveUser.Userid;
                            TempData["ChangePassword"] = "Kindly change your passowrd";
                            string NewURL = "http://localhost:7554/account/reset_password?Id=" + Id;
                            Response.Redirect(NewURL, true);
                        }

                    }

                }
            }
            catch (Exception ex)
            {

                Log.InfoFormat(MethodName, ex);
                ViewBag.ErrorMsg = ex.Message;
                return View();
            }

            return View();
        }


        public ActionResult Forgot_password()
        {
            ViewBag.Title = "Forgot Password";

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Forgot_password(UserLogin User)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string _Username = User.Username.ToUpper();
            var extAccount = _userMgt.getUserByUsername(_Username);
            if (extAccount != null)
            {
                EmailFormModel emailModel = new EmailFormModel();
                long Id = extAccount.Userid;
                string txtRecipient = extAccount.Username.ToLower();
                string _domainUsername = WebConfigurationManager.AppSettings["UserName"];
                string _domainPWD = WebConfigurationManager.AppSettings["PWD"];
                string PasswordUrl = WebConfigurationManager.AppSettings["BaseURL"];
                var body = "Kindly click on this link  to reset your password. </br>" + PasswordUrl + "Account/Reset_password?Id=" + Id;
                var message = new MailMessage();
                message.To.Add(new MailAddress(txtRecipient));  // replace with valid value 
                message.From = new MailAddress(WebConfigurationManager.AppSettings["SupportAddress"]);  // replace with valid value
                message.Subject = "Password Update";
                message.Body = string.Format(body, emailModel.FromEmail, emailModel.Message);
                message.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    try
                    {
                        smtp.Host = WebConfigurationManager.AppSettings["EmailHost"];
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential(_domainUsername,_domainPWD);
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["EmailPort"]);
                        smtp.Send(message);
                        Log.InfoFormat("Email sent", _Username);
                        ViewBag.SuccessMsg = "The link has been sent to your Email";
                    }
                    catch (Exception ex)
                    {
                        Log.InfoFormat("Email", ex.Message);
                        ViewBag.ErrorMsg = ex.Message;
                        return View();
                    }

                }

                ViewBag.SuccessMsg = "Email sent.";
            }
            else
            {
                Log.InfoFormat("Email", "This Email does not exist on this system", extAccount.Useremail);
                ViewBag.ErrorMsg = "This Email does not exist on this system";
                return View();
            }
            return View();
        }


        [HttpGet]
        [Route("Reset_password/{Id}")]
        public ActionResult Reset_password(int Id)
        {
            ViewBag.Message = "Reset Password";
            ViewBag.SuccessMsg = TempData["ChangePassword"];
            int UserId = Id;
            var userDetails = _userMgt.modifyPassword(UserId);
            ViewBag.User = userDetails;
            return View();
        }

        [Route("Reset_password/{Id}")]
        public ActionResult Reset_password(UserLogin User, int? Id)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string ErrorMsg = "";
            string MethodName = Constants.AuditActionType.PasswordChanged.ToString();
            var editUser = new PureUser();
            editUser.Userid = Id.GetValueOrDefault();
            editUser.Userpwd = User.Password;

            if (User.Password.Any("!@#$%^&*".Contains) && User.Password.Length >= 6)
            {
                if (User.Password == User.ConfirmPassword)
                {
                    var ExtDetails = _userMgt.getUserById(editUser.Userid);
                    editUser.Userpwd = _userMgt.EncryptPassword(editUser.Userpwd);
                    if (ExtDetails.Userpwd != editUser.Userpwd)
                    {
                        try
                        {
                            editUser.Userid = Convert.ToInt32(Id);
                            bool validatePassword = _userMgt.UpdatePassword(editUser.Userpwd, Id);
                            if (validatePassword==true)
                            {
                                Log.InfoFormat(MethodName, ErrorMsg);
                                InsertAudit(Constants.AuditActionType.PasswordChanged, editUser.Username + "Changed password", ExtDetails.Username);
                                TempData["SuccessMsg"] = "Kindly login with the new password";
                            }
              
                            return RedirectToAction("login");

                        }
                        catch (Exception ex)
                        {
                            Log.InfoFormat(MethodName, ex.Message);
                            ViewBag.ErrorMsg = ex.Message;
                            return View();
                        }
                    }
                    else
                    {
                        Log.InfoFormat(MethodName, ErrorMsg);
                        ViewBag.ErrorMsg = "The new password must not match the old password";
                        return View();
                    }

                }
                else
                {
                    Log.InfoFormat(MethodName, "The confirm password must match the new password");
                    ViewBag.ErrorMsg = "The confirm password must match the new password";
                    return View();
                }

            }
            else
            {
                ViewBag.ErrorMsg = "The password must contain special and minimum of six characters";
                return View();
            }
        }
    }
}
