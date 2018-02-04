using BLL.ApplicationLogic;
using DAL.CustomObjects;
using DataAccessLayer.CustomObjects;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PureFountain.Controllers
{
    public class FountainController : Controller
    {
        // GET: Fountain
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult CreateUser()
        {
            ViewBag.Message = "New User";
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            //Company company = new Company();
            //ViewBag.Customer = company.getCompanyById();
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(UserViewModel Account)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            ViewBag.Message = "New User";
            string ComputerDetails = "";
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            //Company company = new Company();
            //ViewBag.Customer = company.getCompanyById();
            if (Account.UserPWD.Any("!@#$%^&*".Contains) && Account.UserPWD.Length >= 6)
            {

                try
                {
                    var Acc = new PureUser();
                    Acc.Firstname = Account.FirstName;
                    Acc.Middlename = Account.MiddleName;
                    Acc.Lastname = Account.LastName;
                    Acc.Username = Account.UserName;
                    Acc.Useremail = Account.UserEmail;
                    Acc.Userpwd = Account.UserPWD;
                    Acc.Phonenos = Account.PhoneNos;
                    //Acc.CustomerId = Account.CustomerId;
                    Acc.Roleid = Account.RoleId;
                    Acc.Userstatus = Account.UserStatus;
                    Acc.Createdby = User.Identity.Name;
                    Acc.Createdon = DateTime.Now;

                    UserManagement usermgt = new UserManagement();
                    Acc.Userpwd = usermgt.base64Encode(Account.UserPWD);
                    Acc.Userimg = usermgt.DoFileUpload(Account.UserImg);
                    var validUser = usermgt.getUserByUsername(Account.UserName);
                    if (validUser != null && validUser.Username == Account.UserName)
                    {
                        ViewBag.ErrorMsg = "User already exists";
                        return View();
                    }
                    else
                    {
                        bool NewUser = false;
                        NewUser = usermgt.InsertUser(Acc);
                        if (NewUser == true)
                        {
                            ViewBag.SuccessMsg = "User successfully created";
                            //InsertAudit(BLL.ApplicationLogic.Constants.AuditActionType.CreatedRole, "Created a user " + Acc.UserName, User.Identity.Name);
                            //ErrorLogManager.LogError(ComputerDetails, Constants.AuditActionType.Login.ToString(), User.UserName);
                            //InsertAudit(Constants.AuditActionType.Login, "User Created Successfully", User.UserName);
                            return View();
                            //return RedirectToAction("View_User", "Dolphin");

                        }
                        else
                        {
                            ViewBag.ErrorMsg = "Unable to create the user's account";
                            return View();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMsg = ex.Message;
                    return View();

                }
            }
            else
            {
                ViewBag.ErrorMsg = "The password must contain special and minimum of six characters";
                return View();
            }

        }


        public ActionResult ViewUser()
        {
            ViewBag.Message = "Users";
            TempData["SuccessMsg"] = TempData["SuccessMsg"];
            UserManagement Usermgts = new UserManagement();
            ViewBag.Users = Usermgts.getUserByCompany();
            return View();
        }

        [HttpGet]
        [Route("view_user/{Id}")]
        public ActionResult ViewUser(UserView User)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            UserManagement Usermgts = new UserManagement();
            ViewBag.Users = Usermgts.getUserByCompany();
            return View();
        }

        [HttpGet]
        [Route("Edit_User/{Id}")]
        public ActionResult EditUser(int? Id)
        {
            ViewBag.Message = "Edit User";
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            int UserId = Id.GetValueOrDefault();
            UserManagement existingUser = new UserManagement();
            var user = existingUser.getUserByIds(UserId);
            ViewBag.User = user;
            return View();
        }

        [Route("Edit_User/{Id}")]
        public ActionResult EditUser(UserViewModel Account, int? Id, FormCollection c)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            ViewBag.Message = "Modify User";
            int UserId = Id.GetValueOrDefault();
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            //Company company = new Company();
            //ViewBag.Customer = company.getCompanyById();
            if (Account.UserPWD.Any("!@#$%^&*".Contains) && Account.UserPWD.Length >= 6)
            {

                try
                {
                    var Acc = new PureUser();
                    Acc.Firstname = Account.FirstName;
                    Acc.Middlename = Account.MiddleName;
                    Acc.Lastname = Account.LastName;
                    Acc.Username = Account.UserName;
                    Acc.Useremail = Account.UserEmail;
                    Acc.Userpwd = Account.UserPWD;
                    Acc.Phonenos = Account.PhoneNos;
                    Acc.Roleid = Account.RoleId;
                    Acc.Userstatus = Account.UserStatus;
                    Acc.Modifiedby = User.Identity.Name;
                    Acc.Modifiedon = DateTime.Now;
                    UserManagement usermgt = new UserManagement();
                    Acc.Userpwd = usermgt.base64Encode(Account.UserPWD);
                    Acc.Userimg = usermgt.DoFileUpload(Account.UserImg, c["UserImg1"]);
                    var updateUser = usermgt.UpdateUser(Acc.Firstname, Acc.Middlename, Acc.Lastname, Acc.Username,Acc.Useremail, Acc.Userpwd, Acc.Userimg, Acc.Phonenos, Acc.Roleid, Acc.Userstatus, User.Identity.Name, DateTime.Now, UserId);
                    if (updateUser == true)
                    {
                        //InsertAudit(Constants.AuditActionType.UserModified, "Modified a user " + Acc.Username, User.Identity.Name);
                        TempData["SuccessMsg"] = "Account successfully modified";
                        return RedirectToAction("view_user");
                        //return View();
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Account modification failed";
                        return View();
                    }



                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMsg = ex.Message;
                    return View();

                }
            }
            else
            {
                ViewBag.ErrorMsg = "The password must contain special and minimum of six characters";
                return View();
            }

        }

        public ActionResult EditProfile()
        {
            ViewBag.Message = "Edit Profile";
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            UserManagement existingUser = new UserManagement();
            var user = existingUser.getUserByUsername(User.Identity.Name);
            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        public ActionResult EditProfile(UserViewModel account, FormCollection c)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            UserManagement usermgt = new UserManagement();
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            string Username = User.Identity.Name;
            var editUser = new PureUser();
            if (account.UserPWD.Any("!@#$%^&*".Contains) && account.UserPWD.Length >= 6)
            {
                try
                {

                    editUser.Firstname = account.FirstName;
                    editUser.Middlename = account.MiddleName;
                    editUser.Lastname = account.LastName;
                    editUser.Username = account.UserName;
                    editUser.Useremail = account.UserEmail;
                    editUser.Userpwd = account.UserPWD;
                    editUser.Phonenos = account.PhoneNos;
                    editUser.Modifiedby = User.Identity.Name;
                    editUser.Modifiedon = DateTime.Now;
                    editUser.Userimg = usermgt.DoFileUpload(account.UserImg, c["UserImg1"]);
                    UserManagement userManagement = new UserManagement();
                    editUser.Userpwd = usermgt.base64Encode(account.UserPWD);
                    bool UpdateUser = userManagement.UpdateProfile(editUser.Firstname, editUser.Middlename, editUser.Lastname, editUser.Username,editUser.Useremail, editUser.Userpwd, editUser.Userimg, editUser.Phonenos, User.Identity.Name, DateTime.Now, User.Identity.Name);
                    //InsertAudit(Constants.AuditActionType.ProfileModified, "Modified a user " + editUser.UserName, User.Identity.Name);
                    TempData["ProfileMsg"] = "Profile updated successfully. Please Kindly login";
                    return RedirectToAction("Logout", "Fountain");
                    //ViewBag.SuccessMsg = "Account successfully modified";

                }
                catch (Exception)
                {
                    ViewBag.ErrorMsg = "Unable to connect to the server";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMsg = "The password must contain special and minimum of six characters";
                return View();
            }
        }


        [HttpGet]
        public ActionResult ViewRole()
        {
            ViewBag.Message = "Role";
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            return View();
        }

        public ActionResult ViewRole(PureRole Account)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            try
            {
                //Account.RoleStatus = false;
                Account.Rolename = Account.Rolename.ToUpper();
                Account.Roledesc = Account.Roledesc;

                if (Account.Rolestatus != null)
                {
                    Account.Rolestatus = true;
                }
                else
                {
                    Account.Rolestatus = false;
                }
                Account.Rolestatus = Account.Rolestatus;

                RoleManagement roleManagement = new RoleManagement();
                var validRole = roleManagement.getRoleByRoleName(Account.Rolename);
                if (validRole != null && validRole.Rolename == Account.Rolename)
                {
                    TempData["ErrorMsg"] = "Role Already Exists";
                    return View();
                }
                else
                {
                    bool NewRole = false;
                    NewRole = roleManagement.InsertRole(Account);
                    if (NewRole == true)
                    {
                        TempData["SuccessMsg"] = "Role created successfully";
                        //InsertAudit(Constants.AuditActionType.CreatedRole, "Created a role " + Account.Rolename, User.Identity.Name);
                        return RedirectToAction("View_role");

                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Unable to connect to the database";
                        return View();
                    }
                }
            }
            catch (Exception)
            {
                return View();
            }


        }

        [HttpGet]
        [Route("EditRole/{Id}")]
        public ActionResult EditRole(int? Id)
        {
            ViewBag.Message = "Role";
            int RoleId = Id.GetValueOrDefault();
            RoleManagement existingRole = new RoleManagement();
            var userRole = existingRole.getRoleById(RoleId);
            ViewBag.UserRole = userRole;
            return View();
        }

        [Route("EditRole/{Id}")]
        public ActionResult EditRole(RoleViewModel extRole, int Id)
        {
            int RoleId = Id;
            RoleManagement existingRole = new RoleManagement();
            var userRole = existingRole.getRoleById(RoleId);
            ViewBag.UserRole = userRole;
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                var editRole = new PureRole();
                editRole.Roleid = Convert.ToInt32(Id);
                editRole.Rolename = extRole.RoleName;
                editRole.Roledesc = extRole.RoleDesc;
                editRole.Rolestatus = extRole.RoleStatus;
                RoleManagement roleManagement = new RoleManagement();
                bool UpdateRole = false;
                UpdateRole = roleManagement.UpdateRole(extRole.RoleName, extRole.RoleDesc, extRole.RoleStatus, editRole.Roleid);
                //InsertAudit(Constants.AuditActionType.RoleModified, "Modified a role" + editRole.RoleName, User.Identity.Name);
                ViewBag.SuccessMsg = "Role successfully modified";
                return RedirectToAction("View_role", "Dolphin");
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "Unable to connect to the server";
                return View();
            }

        }


        public ActionResult CreateAccount()
        {
            ViewBag.Message = "Account";
            AccountManagement account = new AccountManagement();
            ViewBag.Country = account.GetAllCountries();
            return View();
        }

        public ActionResult GetStates(string code)
        {
            var codes = new AccountManagement().GetStatesById(code);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserDetails(int UserId)
        {
            var codes = new UserManagement().getUserById(UserId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateLoan()
        {
            return View();
        }



        public ActionResult Logout()
        {
            TempData["ProfileMsg"] = TempData["ProfileMsg"];
            //InsertAudit(Constants.AuditActionType.Logout, "User signed out successfully", User.Identity.Name);
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

    }
}