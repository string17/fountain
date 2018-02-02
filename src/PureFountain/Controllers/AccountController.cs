using BLL.ApplicationLogic;
using DataAccessLayer.CustomObjects;
using FountainContext.Data.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult Login(string returnUrl,string SystemName,string SystemIP)
        {
            ViewBag.ProfileMsg = TempData["ProfileMsg"];
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            string ComputerDetails = TempData["ComputerDetails"]+", " + TempData["SystemName"];
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(UserLogin User)
        {
            string ErrorMsg = "";
            string ComputerDetails= "";
            try
            {
                User.Username = User.Username;
                User.Password = User.Password;
                UserManagement userManagement = new UserManagement();
                if (!userManagement.DoesUsernameExists(User.Username))
                {
                    ErrorMsg = "Invalid Username";
                    ErrorLogManager.LogError(ComputerDetails, ErrorMsg, User.Username);
                    ViewBag.ErrorMsg = "Invalid Username";

                    return View();
                }
                User.Password = userManagement.base64Encode(User.Password);// Encode password

                if (!userManagement.DoesPasswordExists(User.Username, User.Password))
                {
                    ViewBag.ErrorMsg = "Invalid Password";
                    ErrorLogManager.LogError(ComputerDetails, ErrorMsg, User.Username);
                    return View();
                }
                
                var ActiveUser = userManagement.getUserByUsername(User.Username);
                if (ActiveUser != null)
                {
                    var AccountStatus = ActiveUser.Userstatus;
                    if (AccountStatus == true)
                    {
                        //var userDetails = userManagement.getUserByUsername(User.Username);
                       
                        var newUser = userManagement.getFreshUser(User.Username);
                        if (newUser != 0)
                        {
                            var ExtLogin = userManagement.TrackLogin(User.Username);
                            if (ExtLogin == null)
                            {
                                FormsAuthentication.SetAuthCookie(ActiveUser.Username, false);
                                ErrorLogManager.LogError(ComputerDetails, Constants.AuditActionType.Login.ToString(), User.Username);
                                InsertAudit(Constants.AuditActionType.Login, "Successfully Login", User.Username);
                                return RedirectToAction("Dashboard", "Fountain");
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "You didn't logout properly last time";
                                ErrorLogManager.LogError(ComputerDetails, ErrorMsg, User.Username);
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
                    //else
                    //{
                    //    ViewBag.ErrorMsg = "Your institution is not active";
                    //    return View();
                    //}
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                ErrorLogManager.LogError(ComputerDetails, ex.Message, User.Username);
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
                //EmailFormModel emailModel = new EmailFormModel();
                //long Id = extAccount.Userid;
                //string txtRecipient = extAccount.Username.ToLower();
                //string PasswordUrl = WebConfigurationManager.AppSettings["BaseURL"];
                //var body = "Kindly click on this link  to reset your password. </br>" + PasswordUrl + "Account/Reset_password?Id=" + Id;
                //var message = new MailMessage();
                //message.To.Add(new MailAddress(txtRecipient));  // replace with valid value 
                //message.From = new MailAddress(WebConfigurationManager.AppSettings["SenderEmailAddress"]);  // replace with valid value
                //message.Subject = "Update your Password";
                //message.Body = string.Format(body, emailModel.FromEmail, emailModel.Message);
                //message.IsBodyHtml = true;

                //using (SmtpClient smtp = new SmtpClient())
                //{
                //    try
                //    {
                //        smtp.Host = WebConfigurationManager.AppSettings["EmailHost"];
                //        smtp.EnableSsl = true;
                //        NetworkCredential NetworkCred = new NetworkCredential();
                //        smtp.UseDefaultCredentials = true;
                //        smtp.Credentials = NetworkCred;
                //        smtp.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["EmailPort"]);
                //        smtp.Send(message);
                //        ViewBag.SuccessMsg = "Email sent.";
                //    }
                //    catch (Exception ex)
                //    {
                //        ViewBag.ErrorMsg = ex.Message;
                //        return View();
                //    }

                //}

                //ViewBag.SuccessMsg = "Email sent.";
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
            string ComputerDetails = "";
            var editUser = new PureUser();
            editUser.Userid = Id.GetValueOrDefault();
            editUser.Userpwd = User.Password;

            if (User.Password.Any("!@#$%^&*".Contains) && User.Password.Length >= 6)
            {
                if (User.Password == User.ConfirmPassword)
                {
                    UserManagement userService = new UserManagement();
                    var ExtDetails = userService.getUserById(editUser.Userid);
                    editUser.Userpwd = userService.base64Encode(editUser.Userpwd);
                    if (ExtDetails.Userpwd != editUser.Userpwd)
                    {
                        try
                        {
                            editUser.Userid = Convert.ToInt32(Id);
                            //editUser.UserPWD = User.Password;
                            bool ValidatePassword = false;
                            ValidatePassword = userService.UpdatePassword(editUser.Userpwd, Id);
                            ErrorLogManager.LogError(ComputerDetails, Constants.AuditActionType.PasswordChanged.ToString(), User.Username);
                            InsertAudit(Constants.AuditActionType.PasswordChanged, editUser.Username + "Changed password", ExtDetails.Username);
                            TempData["SuccessMsg"] = "Kindly login with the new password";
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
