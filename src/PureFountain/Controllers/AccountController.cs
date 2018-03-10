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
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace PureFountain.Controllers
{
    public class AccountController : BaseController
    {
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
                UserManagement userManagement = new UserManagement();
                if (!userManagement.DoesUsernameExists(_Username))
                {
                    ErrorMsg = "Invalid Username";
                    ErrorLogManager.LogWarning(MethodName, ErrorMsg);
                    ViewBag.ErrorMsg = "Invalid Username";
                    return View();
                }
                _Password = userManagement.EncryptPassword(_Password);// Encode password

                if (!userManagement.DoesPasswordExists(_Username, _Password))
                {
                    ViewBag.ErrorMsg = "Invalid Password";
                    ErrorLogManager.LogWarning(MethodName, ErrorMsg);
                    return View();
                }

                var ActiveUser = userManagement.getUserByUsername(_Username);
                if (ActiveUser != null)
                {
                    var AccountStatus = ActiveUser.Userstatus;
                    if (AccountStatus == true)
                    {
                        var newUser = userManagement.getFreshUser(_Username);
                        if (newUser != 0)
                        {
                            var ExtLogin = userManagement.TrackLogin(_Username);
                            if (ExtLogin == null)
                            {
                                FormsAuthentication.SetAuthCookie(ActiveUser.Username, false);
                                ErrorLogManager.LogWarning(MethodName, "Login Successful");
                                InsertAudit(Constants.AuditActionType.Login, "Successfully Login", _Username);
                                InsertTracking(_Username, ipaddress, ComputerDetails);
                                return RedirectToAction("Dashboard", "Fountain");
                            }

                            else if (ExtLogin.Username == _Username && ExtLogin.Systemname == ComputerDetails && ExtLogin.Systemip == ipaddress)
                            {
                                FormsAuthentication.SetAuthCookie(ActiveUser.Username, false);
                                ErrorLogManager.LogWarning(MethodName, "Login Successful");
                                InsertAudit(Constants.AuditActionType.Login, "Successfully Login", _Username);
                                InsertTracking(_Username, ipaddress, ComputerDetails);
                                ViewBag.ErrorMsg = "You didn't logout properly last time";
                                return View();
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "You didn't logout properly last time";
                                ErrorLogManager.LogWarning(MethodName, ErrorMsg);
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

                ErrorLogManager.LogError(MethodName, ex);
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
            User.Username = User.Username.ToUpper();
            UserManagement usermgt = new UserManagement();
            var extAccount = usermgt.getUserByUsername(User.Username);
            if (extAccount != null)
            {
                EmailFormModel emailModel = new EmailFormModel();
                long Id = extAccount.Userid;
                string txtRecipient = extAccount.Username.ToLower();
                string PasswordUrl = WebConfigurationManager.AppSettings["BaseURL"];
                var body = "Kindly click on this link  to reset your password. </br>" + PasswordUrl + "Account/Reset_password?Id=" + Id;
                var message = new MailMessage();
                message.To.Add(new MailAddress(txtRecipient));  // replace with valid value 
                message.From = new MailAddress(WebConfigurationManager.AppSettings["SenderEmailAddress"]);  // replace with valid value
                message.Subject = "Update your Password";
                message.Body = string.Format(body, emailModel.FromEmail, emailModel.Message);
                message.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    try
                    {
                        smtp.Host = WebConfigurationManager.AppSettings["EmailHost"];
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential();
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["EmailPort"]);
                        smtp.Send(message);
                        ViewBag.SuccessMsg = "Email sent.";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMsg = ex.Message;
                        return View();
                    }

                }

                ViewBag.SuccessMsg = "Email sent.";
            }
            else
            {
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
            UserManagement existingUser = new UserManagement();
            var userDetails = existingUser.modifyPassword(UserId);
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
                    UserManagement userService = new UserManagement();
                    var ExtDetails = userService.getUserById(editUser.Userid);
                    editUser.Userpwd = userService.EncryptPassword(editUser.Userpwd);
                    if (ExtDetails.Userpwd != editUser.Userpwd)
                    {
                        try
                        {
                            editUser.Userid = Convert.ToInt32(Id);
                            bool validatePassword = userService.UpdatePassword(editUser.Userpwd, Id);
                            if (validatePassword==true)
                            {
                                ErrorLogManager.LogWarning(MethodName, ErrorMsg);
                                InsertAudit(Constants.AuditActionType.PasswordChanged, editUser.Username + "Changed password", ExtDetails.Username);
                                TempData["SuccessMsg"] = "Kindly login with the new password";
                            }
              
                            return RedirectToAction("login");

                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorMsg = ex.Message;
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "The new password must not match the old password";
                        return View();
                    }

                }
                else
                {
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
