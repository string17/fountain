using BLL.ApplicationLogic;
using DAL.CustomObjects;
using DataAccessLayer.CustomObjects;
using FountainContext.Data.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace PureFountain.Controllers
{
    public class FountainController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string ipaddress = AuditService.DetermineIPAddress();
        private readonly UserManagement _userMgt;
        private readonly AccountManagement _account;
        private readonly TransactionManagement _trans;
        private readonly SequenceManager _sequence;
        private readonly ReportManagement _report;

        public FountainController()
        {
            _userMgt = new UserManagement();
            _account =new AccountManagement();
            _trans = new TransactionManagement();
            _sequence = new SequenceManager();
            _report = new ReportManagement();
        }

        //private string ComputerDetails = AuditService.DetermineCompName(ipaddress);
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
            return View();
        }


        public ActionResult CreateTill()
        {
            ViewBag.Message = "Till Account";
            return View();
        }

        [HttpPost]
        public ActionResult CreateTill(TillManager account)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string MethodName = Constants.AuditActionType.CreateAccount.ToString();
            string AccountNo = _sequence.GenerateAccountNos();
           try
            {
                var accNos = new PureAccountNo();
                accNos.Accountno = AccountNo;
                accNos.Accountclass = "BranchTill";
                accNos.Userbvn = WebConfigurationManager.AppSettings["UniversalToken"];
                bool _success = _account.NewAccountNos(accNos);
                if (_success)
                {
                    var Till = new PureTillAccount();
                    Till.Accountname = account.AccountName;
                    Till.Accountno = AccountNo;
                    Till.Accountbal = account.AccountBal;
                    Till.Currencycode = "NGN";
                    Till.Tellerid = "";
                    Till.Amountdebited = 0;
                    Till.Accountstatus = false;
                    Till.Drcrindicator = "CR";
                    Till.Createdon = DateTime.Now;
                    Till.Creditedby = User.Identity.Name;

                    bool NewTill = _account.CreateTill(Till);
                    if (NewTill)
                    {
                        Log.InfoFormat(MethodName, "Account Successful");
                        InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                        ViewBag.SuccessMsg = "Account Successfully created";
                    }
                    else
                    {
                        Log.InfoFormat(MethodName, "Account Unsuccessful");
                        InsertAudit(Constants.AuditActionType.CustomerAccount, "Account not Created", User.Identity.Name);
                        ViewBag.ErrorMsg = "Account not Successfully";
                    }
                }
                else
                {

                    Log.InfoFormat(MethodName, "Account Unsuccessful");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account not Created", User.Identity.Name);
                    ViewBag.ErrorMsg = "Account not Successfully";
                }
                
            }
            catch (Exception EX)
            {
                ErrorLogManager.LogError(MethodName, EX);
                InsertAudit(Constants.AuditActionType.CustomerAccount, EX.Message, User.Identity.Name);
                ViewBag.SuccessMsg = "Account Successfully created";
            }

            return View();
        }

        public ActionResult ViewTill()
        {
            ViewBag.Message = "Till Account";
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            ViewBag.Account = _account.GetTillAccount();
            return View();
        }


        public ActionResult TillHistory()
        {
            ViewBag.Message = "Till Account";
            ViewBag.Account = _account.GetTillHistory();
            return View();
        }


        [HttpGet]
        [Route("ModifyTill/{Id}")]
        public ActionResult ModifyTill(int? Id)
        {
            ViewBag.Message = "Till Account";
            int TillId = Id.GetValueOrDefault();
            ViewBag.Till = _account.GetTillAccountDetails(TillId);
            ViewBag.Teller = _userMgt.getAllTeller();
            return View();
        }


        [Route("ModifyTill/{Id}")]
        public ActionResult ModifyTill(TillManager Till, int? Id)
        {
           
            if (!ModelState.IsValid)
            {
                return View();
            }
            ViewBag.Message = "Till Account";
            int TillId = Id.GetValueOrDefault();
            ViewBag.Till = _account.GetTillAccountDetails(TillId);
            string MethodName = Constants.AuditActionType.ModifiedUser.ToString();
            ViewBag.Teller = _userMgt.getAllTeller();
            try
            {
            var _tillDetails =_account.GetTillDetails(TillId);
            string CreatedOn = DateTime.Now.ToString("yyyy-MM-dd");
            var existingTill =_account.ValidateTill(Till.TellerId, CreatedOn);
            
            if(existingTill == 0)
            {
             
                    var account = new PureTillAccount();
                    account.Accountname = Till.AccountName;
                    account.Accountbal = Till.AccountBal;
                    account.Tellerid = Till.TellerId;
                    account.Accountstatus = Till.AccountStatus;
                    account.Drcrindicator = "CR";
                    account.Amountdebited = 0;
                    account.Creditedby = User.Identity.Name;
                    account.Createdon = DateTime.Now;
                    bool EditTill =_account.UpdateTill(account.Accountname, account.Accountbal, account.Tellerid, account.Accountstatus, account.Drcrindicator, account.Creditedby, account.Createdon, TillId);

                    if (EditTill)
                    {
                        var NewTill = new PureTellerTill();
                        NewTill.Accountno = _tillDetails.AccountNo;
                        NewTill.Initialbalance = account.Accountbal;
                        NewTill.Tellerid = account.Tellerid;
                        NewTill.Amount = 0;
                        NewTill.Drcrindicator = "CR";
                        NewTill.Debiteddate = DateTime.Now;
                        NewTill.Createdby = User.Identity.Name;
                    
                        bool TellerTill = new TransactionManagement().InsertTellerTill(NewTill);
                        if (TellerTill)
                        {
                            var _till = new PureTillHistory();
                            _till.Accountname = Till.AccountName;
                            _till.Accountno = _tillDetails.AccountNo;
                            _till.Amtcredited = Till.AccountBal;
                            _till.Amtdebited = 0;
                            _till.Closedbal = 0;
                            _till.Createdby = User.Identity.Name;
                            _till.Createddate = DateTime.Now;
                            _till.Currencycode = "NGN";
                            _till.Tellerid =Till.TellerId;
                            _till.Trandate = DateTime.Now;
                            bool _tillHistory = new TransactionManagement().InsertTillHistory(_till);
                            if (_tillHistory)
                            {
                                Log.InfoFormat(MethodName, "Till Update Successful");
                                InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update Successful", User.Identity.Name);
                                TempData["SuccessMsg"] = "Till Update Successful";
                                return RedirectToAction("ViewTill");
                            }
                            else
                            {
                                Log.InfoFormat(MethodName, "Till Update not Successful");
                                InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update not Successful", User.Identity.Name);
                                ViewBag.ErrorMsg = "Till Update not successful";
                            }
                     
                        }
                        else
                        {
                            Log.InfoFormat(MethodName, "Till Update not Successful");
                            InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update not Successful", User.Identity.Name);
                            ViewBag.ErrorMsg = "Till Update not successful";
                        }
                        
                    }
                    else
                    {
                        Log.InfoFormat(MethodName, "Till Update Successful");
                        InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update Successful", User.Identity.Name);
                        ViewBag.ErrorMsg = "Till Update not successful";
                    }
               
            }
            else
            {
                Log.InfoFormat(MethodName, "There is an existing and active Till Account for the selected Teller");
                InsertAudit(Constants.AuditActionType.ModifyTill, "Unable to update Till Account", User.Identity.Name);
                ViewBag.ErrorMsg = "There is an active Till Account for the selected Teller";
                return View();
             
            }

            }
                catch (Exception ex)
            {
                ErrorLogManager.LogError(MethodName, ex);
                InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update Unsuccessful", User.Identity.Name);
                ViewBag.ErrorMsg = "Till Update Unsuccessful";
            }

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
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
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
                    Acc.Createdby = User.Identity.Name;
                    Acc.Createdon = DateTime.Now;

                    UserManagement usermgt = new UserManagement();
                    Acc.Userpwd = usermgt.EncryptPassword(Account.UserPWD);
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
                            return View();

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
            ViewBag.Success = TempData["SuccessMsg"];
            UserManagement Usermgts = new UserManagement();
            ViewBag.Users = Usermgts.getUserByCompany();
            return View();
        }

        [HttpGet]
        [Route("viewuser/{Id}")]
        public ActionResult ViewUser(UserView User)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            ViewBag.Users = new UserManagement().getUserByCompany();
            return View();
        }

        [HttpGet]
        [Route("EditUser/{Id}")]
        public ActionResult EditUser(int? Id)
        {
            ViewBag.Message = "Edit User";
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
            int UserId = Id.GetValueOrDefault();
            var user = new UserManagement().getUserByIds(UserId);
            ViewBag.User = user;
            return View();
        }

        [Route("EditUser/{Id}")]
        public ActionResult EditUser(UserViewModel Account, int? Id, FormCollection c)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var ipaddress = AuditService.DetermineIPAddress();
            string MethodName = Constants.AuditActionType.ModifiedUser.ToString();
            ViewBag.Message = "Modify User";
            int UserId = Id.GetValueOrDefault();
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
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
                    Acc.Userpwd = usermgt.EncryptPassword(Account.UserPWD);
                    Acc.Userimg = usermgt.DoFileUpload(Account.UserImg, c["UserImg1"]);
                    var updateUser = usermgt.UpdateUser(Acc.Firstname, Acc.Middlename, Acc.Lastname, Acc.Username, Acc.Useremail, Acc.Userpwd, Acc.Userimg, Acc.Phonenos, Acc.Roleid, Acc.Userstatus, User.Identity.Name, DateTime.Now, UserId);
                    if (updateUser == true)
                    {
                        InsertAudit(Constants.AuditActionType.Login, "Successfully Login", User.Identity.Name);
                        Log.InfoFormat(MethodName, "Modified a user");
                        TempData["SuccessMsg"] = "Account successfully modified";
                        return RedirectToAction("viewuser");
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
            string MethodName = Constants.AuditActionType.UpdateProfile.ToString();
            UserManagement usermgt = new UserManagement();
            RoleManagement rolemgt = new RoleManagement();
            ViewBag.Roles = rolemgt.getRoleByRoleId();
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
                    editUser.Userpwd = usermgt.EncryptPassword(account.UserPWD);
                    bool UpdateUser = false;
                    UpdateUser = userManagement.UpdateProfile(editUser.Firstname, editUser.Middlename, editUser.Lastname, editUser.Username, editUser.Useremail, editUser.Userpwd, editUser.Userimg, editUser.Phonenos, User.Identity.Name, DateTime.Now, User.Identity.Name);
                    if (UpdateUser == true)
                    {
                        InsertAudit(Constants.AuditActionType.ProfileModified, "Modified a user " + editUser.Username, User.Identity.Name);
                        TempData["ProfileMsg"] = "Profile updated successfully. Please Kindly login";
                        return RedirectToAction("Logout", "Fountain");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Unable to update";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    //Log.InfoFormat(MethodName, ComputerDetails, ex.Message);
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
            string MethodName = Constants.AuditActionType.EditRole.ToString();
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
                ViewBag.SuccessMsg = "Role successfully modified";
                return RedirectToAction("View_role", "Dolphin");
            }
            catch (Exception ex)
            {
                //Log.InfoFormat(MethodName, ComputerDetails, ex.Message);
                ViewBag.ErrorMsg = "Unable to connect to the server";
                return View();
            }

        }


        public ActionResult ViewProfile()
        {
            ViewBag.Message = "Profile";
            ViewBag.UserProfile = new UserManagement().getUserProfileByUsername(User.Identity.Name);
            return View();
        }

        public ActionResult CreateAccount()
        {
            ViewBag.Message = "Account";
            ViewBag.Country = _account.GetAllCountries();
            ViewBag.Referral = _account.GetReferral();
            ViewBag.Banks = _account.GetAllBank();
            ViewBag.Religion = _account.GetAllReligion();
            ViewBag.Card = _account.GetAllCard();
            ViewBag.Account = _account.GetAccountCategory();
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(CustomerAccountView customer)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            string MethodName = Constants.AuditActionType.CustomerAccount.ToString();
            ViewBag.Country = _account.GetAllCountries();
            ViewBag.Referral = _account.GetReferral();
            ViewBag.Banks = _account.GetAllBank();
            ViewBag.Religion = _account.GetAllReligion();
            ViewBag.Card = _account.GetAllCard();
            ViewBag.Account = _account.GetAccountCategory();

            try
            {
                var Acc = new PureCustomerInfo();
                Acc.Firstname = customer.FirstName;
                Acc.Middlename = customer.MiddleName;
                Acc.Lastname = customer.LastName;
                Acc.Useremail = customer.UserEmail;
                Acc.Phoneno1 = customer.PhoneNos1;
                Acc.Phoneno2 = customer.PhoneNos2;
                Acc.Usersex = customer.UserSex;
                Acc.Homeaddress = customer.UserAddress;
                Acc.Officeaddress = customer.OfficeAddress;
                Acc.Stateorigin = customer.StateId;
                Acc.Dob = customer.DOB;
                Acc.Homeaddress = customer.UserAddress;
                Acc.Userlga = customer.UserLGA;
                Acc.Maritalstatus = customer.MaritalStatus;
                Acc.Occupationid = customer.OccupationalId;
                Acc.Jobtitle = customer.JobTitle;
                Acc.Incomerange = customer.IncomeRange;
                Acc.Nationality = customer.CountryCode;
                Acc.Idnos = customer.IdNos;
                Acc.Idissuedate = customer.IdIssueDate;
                Acc.Idexpirydate = customer.IdExpiryDate;
                Acc.Userbvn = customer.UserBVN;
                Acc.Idissueauth = customer.IDIssueAuth;
                Acc.Employmentdate = customer.EmploymentDate;
                Acc.Iddetails = customer.IdDetails;
                Acc.Otherbankid = customer.OtherBankId;
                Acc.Otheraccountnos = customer.OtherAccountNos;
                Acc.Nextofkin = customer.KFName + " " + customer.KMName;
                Acc.Religionid = customer.ReligionNos;
                Acc.Maidenname = customer.MaidenName;
                Acc.Knumber = customer.KNumber;
                Acc.Krelationship = customer.KRelationship;
                Acc.Kaddress = customer.KAddress;
                Acc.Refname = customer.ReferralName;
                Acc.Accounttype = customer.AccountType; ;
                Acc.Reasonforaccount = customer.ReasonForAccount;
                Acc.Accountname = customer.AccountName;
                Acc.Accountno = "";
                Acc.Accountstatus = false;
                Acc.Createdby = User.Identity.Name;
                Acc.Createdon = DateTime.Now;
                Acc.Modifiedby = "";
                Acc.Modifiedon = DateTime.Now;
                Acc.Approvedby = "";
                Acc.Approvedon = null;
                Acc.Accountimg = _userMgt.DoFileUpload(customer.AccountImg);
                Acc.Accountsign = _userMgt.DoFileUpload(customer.AccountSign);
                bool NewAccount = false;
                NewAccount = _account.InsertAccount(Acc);
                if (NewAccount)
                {
                    //Log.InfoFormat(MethodName, ComputerDetails, "Account", User.Identity.Name);
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Successfully Created", User.Identity.Name);
                    ViewBag.SuccessMsg = "Account successfully created";
                    return View();
                }
                else
                {
                    Log.InfoFormat(MethodName, "Unable to create Account");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                    ViewBag.ErrorMsg = "Unable to create Account";
                    return View();
                }
            }
            catch (Exception ex)
            {
                //Log.InfoFormat(MethodName, ComputerDetails, ex.Message);
                InsertAudit(Constants.AuditActionType.CustomerAccount, ex.Message, User.Identity.Name);
                ViewBag.ErrorMsg = ex.Message;
            }
            return View();
        }


        public ActionResult GetStates(string code)
        {
            var codes = _account.GetStatesById(code);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserDetails(int UserId)
        {
            var codes = new UserManagement().getUserById(UserId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        //getLoan Status
        public ActionResult GetLoanDetails(int LoanId)
        {
            var codes = new LoanManagement().GetLoanDetails(LoanId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        //get Till details
        public ActionResult GetTillDetails(int TillId)
        {
            var codes = _account.GetTillAccountDetails(TillId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetchTillDetails(int TillId)
        {
            var codes = _account.GetTillDetails(TillId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetPendingAccount(int CustomerId)
        {
            var codes = _account.GetAccountDetails(CustomerId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAccountDetails(int CustomerId)
        {
            var Customercodes =_account.GetAccountDetails(CustomerId);
            return Json(Customercodes, JsonRequestBehavior.AllowGet);
        }


        //Get customer Account Balance
        public ActionResult GetCustomerBalance(string AccountNos)
        {
            var codes =_account.GetAccountBal(AccountNos);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAccount()
        {
            ViewBag.Message = "Customers";
            ViewBag.Customer = _account.GetCustomerInfo();
            return View();
        }


        [HttpGet]
        [Route("EditAccount/{Id}")]
        public ActionResult EditAccount(int Id)
        {
            ViewBag.Message = "Customers";
            ViewBag.Customer = _account.GetAccountDetails(Id);
            ViewBag.Country = _account.GetAllCountries();
            ViewBag.Referral = _account.GetReferral();
            ViewBag.Banks = _account.GetAllBank();
            return View();
        }

        public ActionResult ViewCustomer()
        {
            ViewBag.Message = "Customers";
            ViewBag.Customer = _account.NewAccount();
            return View();
        }

        public ActionResult ApproveAccount()
        {
            ViewBag.Message = "Approve Account";
            ViewBag.Customer = _account.GetPendingAccount();
            return View();
        }

        public ActionResult ListAccount()
        {
            ViewBag.Message = "View Account";
            ViewBag.Customer = _account.GetAllAccount();
            return View();
        }

        public ActionResult PostDeposit()
        {
            ViewBag.Message = "Post Deposit";
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            return View();
        }


        
        [HttpPost]
        public ActionResult PostDeposit(TransactionViewModel tran)
        {
            string MethodName = Constants.AuditActionType.PostTransaction.ToString();
            string TellerId = User.Identity.Name;
            string RequestId = new SequenceManager().ReturnPostingSequence();
            string Amount = tran.Amount.ToString("#,##.00");
            decimal DepositAmount = Convert.ToDecimal(Amount);
            var CustomerDetails =_account.GetAccountBal(tran.AccountNo.ToString());
            DateTime PostdDate = DateTime.Now;
            string ddate = PostdDate.ToString("yyyy-MM-dd");
            string UserName = User.Identity.Name;
            var TellerTill = new TransactionManagement().GetTillByUserName(UserName, ddate);
            if (TellerTill == null)
            {
                Log.InfoFormat(MethodName, "No Till account", User.Identity.Name);
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                ViewBag.ErrorMsg = "There is no Till Account Assigned";
                return View();
            }
            decimal? TillBal = TellerTill.Initialbalance;
            if (TillBal < Convert.ToDecimal(DepositAmount))
            {
                Log.InfoFormat(MethodName, "Transaction Failed");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                ViewBag.ErrorMsg = "Amount is greater than the Till Balance";
                return View();

            }
            try
            {
                var CreditRem = new PureRemittance();
                CreditRem.Requestid = RequestId;
                CreditRem.Creditedby = TellerId;
                CreditRem.Remamount = tran.Amount;
                CreditRem.Creditedon = DateTime.Now;
                bool UpdateRem = new TransactionManagement().CreditRemAccount(CreditRem);
                if (UpdateRem == false)
                {
                    ViewBag.ErrorMsg = "Unable to Remit the till";
                    return View();
                }

                var ProcessTill =_account.GetTellerTill(TellerId, ddate);
                if (ProcessTill == null)
                {
                    ViewBag.ErrorMsg = "No Till Account Found";
                    return View();
                }
                bool DebitTill = new TransactionManagement().ConfirmTransaction(RequestId, CustomerDetails.AccountNos, DepositAmount, TellerId);
                if (DebitTill == false)
                {
                    ViewBag.ErrorMsg = "Unable to Post the Transaction";
                    return View();
                }

                var acc = new PureTransactionLog();
                acc.Sourceaccount = ProcessTill.AccountNo;
                acc.Destinationaccount = CustomerDetails.AccountNos;
                acc.Narration = RequestId + " | " + tran.DepositorName + " | " + tran.DepositorNo;
                acc.Amount = tran.Amount;
                acc.Transtatus = "P";
                acc.Trantype = "D";
                acc.Customerid = CustomerDetails.CustomerId;
                acc.Trancurrency = "NGN";
                acc.Traninitiator = User.Identity.Name;
                acc.Tranapprover = "";
                acc.Trandate = DateTime.Now;
                acc.Approveddate = null;
                acc.Requestid = RequestId;

                bool firstPost = false;
                firstPost =_account.InsertTransaction(acc);
                if (firstPost)
                {
                    try
                    {

                        var deposit = new PureDeposit();
                        deposit.Requestid = RequestId;
                        deposit.Accounttitle = CustomerDetails.AccountName;
                        deposit.Accountnos = tran.AccountNo;
                        deposit.Depositorname = tran.DepositorName;
                        deposit.Depositorno = tran.DepositorNo;
                        deposit.Narration = RequestId + " | " + tran.DepositorName + " | " + tran.DepositorNo;
                        deposit.Amount = tran.Amount;
                        deposit.Processor = User.Identity.Name;
                        deposit.Status = "P";
                        deposit.Currencyiso = "NGN";
                        deposit.Depositeddate = DateTime.Now;
                        bool newDeposit = false;
                        newDeposit = _account.InsertDeposit(deposit);
                        if (newDeposit == false)
                        {
                            ViewBag.ErrorMsg = "Unable to Post the Transaction";
                            return View();
                        }
                        try
                        {
                            var request = new PurePostRequest();
                            request.Requestid = RequestId;
                            request.Accountname = CustomerDetails.AccountName;
                            request.Accountno = CustomerDetails.AccountNos;
                            request.Drcrindicator = "CR";
                            request.Tranamount = tran.Amount;
                            request.Transtatus = "P";
                            bool DrRequest = false;
                            DrRequest =_account.InsertRequest(request);
                            if (DrRequest == true)
                            {
                                var request1 = new PurePostRequest();
                                request1.Requestid = RequestId;
                                request1.Accountname = ProcessTill.AccountName;
                                request1.Accountno = ProcessTill.AccountNo;
                                request1.Drcrindicator = "DR";
                                request1.Tranamount = tran.Amount;
                                request1.Transtatus = "P";
                                bool CrRequest = false;
                                var request3 = new PureStatement();
                                //decimal custBal= Convert.ToDecimal(CustomerDetails.Balance) + tran.Amount;
                                request3.Referenceid = RequestId;
                                request3.Transactiondetails = RequestId + " | " + tran.DepositorName + " | " + tran.DepositorNo;
                                request3.Accountno = CustomerDetails.AccountNos;
                                request3.Deposit = Convert.ToDecimal(Amount);
                                request3.Withdrawal = 0;
                                request3.Accountbal = Convert.ToDecimal(CustomerDetails.Balance) + tran.Amount;//
                                request3.Valuedate = DateTime.Now;

                                bool CustomerStatement = false;
                                CustomerStatement =_account.InsertStatement(request3);
                                CrRequest = _account.InsertRequest(request1);
                                if (CrRequest && CustomerStatement)
                                {
                                    Log.InfoFormat(MethodName, "Deposit Successful");
                                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                                    ViewBag.SuccessMsg = "Transaction Successful";
                                    return View();

                                }
                                else
                                {
                                    Log.InfoFormat(MethodName, "Transaction Failed");
                                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                                    ViewBag.ErrorMsg = "Transaction Failed";
                                    return View();
                                }
                            }
                            else
                            {
                                Log.InfoFormat(MethodName, "Transaction Failed");
                                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                                ViewBag.ErrorMsg = "Transaction Failed";
                                return View();
                            }

                        }
                        catch (Exception ex)
                        {
                            //Log.InfoFormat(MethodName, ComputerDetails, ex.Message);
                            InsertAudit(Constants.AuditActionType.PostTransaction, ex.Message, User.Identity.Name);
                            ViewBag.ErrorMsg = ex.Message;
                            return View();
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log.InfoFormat(MethodName, ComputerDetails, ex.Message);
                        InsertAudit(Constants.AuditActionType.PostTransaction, ex.Message, User.Identity.Name);
                        ViewBag.ErrorMsg = ex.Message;
                        return View();
                    }
                }

            }
            catch (Exception ex)
            {
                //Log.InfoFormat(MethodName, ComputerDetails, ex.Message);
                InsertAudit(Constants.AuditActionType.PostTransaction, ex.Message, User.Identity.Name);
                ViewBag.ErrorMsg = ex.Message;
                return View();
            }
            return View();
        }

        public ActionResult DepositHistory()
        {
            ViewBag.Message = "Deposit History";
            ViewBag.Deposit = new TransactionManagement().GetDepositHistory(User.Identity.Name);
            return View();
        }

        public ActionResult TransactionHistory()
        {
            ViewBag.Message = "Transaction History";
            ViewBag.Deposit = _account.GetDepositByUserName(User.Identity.Name);
            return View();
        }

        //Receipt
        public ActionResult Receipt()
        {
            ViewBag.Message = "Receipt";
            return View();
        }


        public ActionResult BankStatement()
        {
            ViewBag.Message = "Bank Statement";
            return View();
        }


        public ActionResult LoanHistory()
        {
            ViewBag.Message = "Loan History";
            string UserName = User.Identity.Name;
            ViewBag.Loan = new LoanManagement().GetLoanByUsername(UserName);
            return View();
        }

        public ActionResult AccountBalance(string AccountNos)
        {
            ViewBag.Message = "Account Balance";
            return View();
        }

        [HttpGet]
        public ActionResult CheckAccountBalance(string AccountNos)
        {
            ViewBag.Message = "Account Balance";
            ViewBag.AccountBalance =_account.GetAccountBal(AccountNos);
            if (ViewBag.AccountBalance == null)
            {
                return View();
            }
            else
            {
                return PartialView("_CustomerBalance");
            }

        }


        public ActionResult LoanReport()
        {
            ViewBag.Message = "Loan Report";
            ViewBag.Customer = _account.GetOverallCustomerLoan();
            return View();
        }


        [HttpGet]
        public ActionResult GetTransHistory(string startdate, string enddate, string AccountNos)
        {
            ViewBag.CustomerStatement = new ReportManagement().IndividualStatement(AccountNos, startdate, enddate);
            if (ViewBag.CustomerStatement == null)
            {

                return View();
            }
            else
            {
                return PartialView("_CustomerStatement");
            }

        }


        public ActionResult TransactionReport()
        {
            ViewBag.Message = "Transaction Report";
            ViewBag.Customer = _account.GetPendingAccount();
            return View();
        }

        public ActionResult RepostTransaction()
        {
            ViewBag.Message = "Repost Transaction";
            ViewBag.Customer = _account.GetPendingAccount();
            return View();
        }

        public ActionResult ApproveDeposit()
        {
            ViewBag.Message = "Approve Deposit";
            string TranType = "D";
            ViewBag.Transaction = _account.GetPendingTransaction(TranType);
            return View();
        }

        public ActionResult ApproveWithdrawal()
        {
            ViewBag.Message = "Approve Withdrawal";
            string TranType = "W";
            ViewBag.Transaction = _account.GetPendingTransaction(TranType);
            return View();
        }


        public ActionResult ApproveTransaction(string RequestId)
        {

            TransactionManagement transactionMgt = new TransactionManagement();
            string MethodName = "Approve Transaction";
            var codes = transactionMgt.GetTransactionDetails(RequestId);
            string TranStatus = "S";
            string Approver = User.Identity.Name;
            bool UpdateTransaction = false;
            UpdateTransaction = transactionMgt.ApproveTransaction(RequestId, Approver, TranStatus);//Transaction
            if (UpdateTransaction)
            {
                Log.InfoFormat(MethodName, "Account Successfully created");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Successfully Created", User.Identity.Name);
            }
            else
            {
                Log.InfoFormat(MethodName, " Approval failed");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Approval failed", User.Identity.Name);
            }
            return Json(codes, JsonRequestBehavior.AllowGet);
        }



        public ActionResult RejectTransaction(string RequestId)
        {
            TransactionManagement transactionMgt = new TransactionManagement();
            var codes = transactionMgt.GetTransactionDetails(RequestId);
            string MethodName = "Cancel Transaction";
            string ApproverId = User.Identity.Name;
            string TranStatus = "X";
            bool UpdateTransaction = false;
            UpdateTransaction = transactionMgt.RejectTransaction(RequestId, ApproverId, TranStatus);//Transaction
            if (UpdateTransaction)
            {
                Log.InfoFormat(MethodName, "Transaction Successfully cancelled");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Successfully Cancelled", User.Identity.Name);
            }

            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RejectLoan(int LoanId)
        {
            LoanManagement loanMgt = new LoanManagement();
            string MethodName = "Decline Loan";
            string LoanStatus = "X";
            string Approver = User.Identity.Name;
            bool codes = loanMgt.ApproveLoanStatus(LoanStatus, LoanId, Approver);//Transaction
            Log.InfoFormat(MethodName, "Unable to grant loan");
            InsertAudit(Constants.AuditActionType.CustomerAccount, "Unable to grant loan", User.Identity.Name);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GrantLoan(int LoanId)
        {
            LoanManagement loanMgt = new LoanManagement();
            string Ref = "";
            string MethodName = "Approve Loan";
            string LoanStatus = "A";
            string Approver = User.Identity.Name;
            string loanRef = new SequenceManager().GenerateLoanReference();
            bool codes = loanMgt.ApproveLoanStatus(LoanStatus, LoanId, Approver);//Transaction
            if (codes)
            {
               Ref = loanRef;
            }
            else
            {
                Ref = "Not SuccessFul";
            }
            Log.InfoFormat(MethodName, "Account Successfully created");
            InsertAudit(Constants.AuditActionType.CustomerAccount, "Loan Granted", User.Identity.Name);
            return Json(Ref, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowTransaction(string RequestId)
        {
            TransactionManagement transactionMgt = new TransactionManagement();
            var codes = transactionMgt.GetTransactionDetails(RequestId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ApproveLoan()
        {
            ViewBag.Message = "Approve Loan";
            ViewBag.Loan = new LoanManagement().GetPendingLoan();
            return View();
        }

        public ActionResult ViewAudit()
        {
            ViewBag.Message = "Audit Trail";
            AuditService auditMgt = new AuditService();
            ViewBag.Audit = auditMgt.getAuditById();
            return View();
        }

        public ActionResult AccountTracker()
        {
            ViewBag.Message = "Account Tracking";
            ViewBag.Customer = _account.GetCustomerByReferral(User.Identity.Name);
            return View();
        }

        public ActionResult GenerateAccountNos(int CustomerId)
        {
            try
            {
                var AccountNo = new SequenceManager().GenerateAccountNos().ToString();
                var acc = new PureCustomerInfo();
                acc.Accountno = AccountNo;
                acc.Accountstatus = true;
                bool UpdateAccount = _account.ApproveAccount(CustomerId, acc.Accountno, acc.Accountstatus);
                if (UpdateAccount)
                {
                    var codes = _account.GetAccountDetails(CustomerId);
                    var accNos = new PureAccountNo();
                    accNos.Accountno = AccountNo;
                    accNos.Accountclass = "Customer";
                    accNos.Userbvn = codes.UserBVN;
                    bool InsertAccount = _account.NewAccountNos(accNos);
                    if (InsertAccount)
                    {
                        var Bal = new PureAccountDetail();
                        Bal.Accountbal = Convert.ToDecimal(0.00);
                        Bal.Accountno = AccountNo;
                        Bal.Customerid = CustomerId;
                        Bal.Modifiedby = User.Identity.Name;
                        Bal.Modifiedon = DateTime.Now;
                        bool InsertBal = _account.FreshAccount(Bal);
                        if (InsertBal)
                        {
                            Log.InfoFormat(Constants.AuditActionType.ApproveAccount.ToString(), "Account Successfully created ", AccountNo);
                            InsertAudit(Constants.AuditActionType.ApproveAccount, "Account Successfully Created", User.Identity.Name);
                            return Json(new { success = true });
                            //return Json(codes, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Log.InfoFormat(Constants.AuditActionType.ApproveAccount.ToString(), "Account failed", AccountNo);
                            InsertAudit(Constants.AuditActionType.ApproveAccount, "Account failed", User.Identity.Name);
                            //return View();
                            return Json(new { success = false });
                        }

                    }
                    else
                    {
                        Log.InfoFormat(Constants.AuditActionType.ApproveAccount.ToString(), "Account failed", AccountNo);
                        InsertAudit(Constants.AuditActionType.ApproveAccount, "Account failed", User.Identity.Name);
                        //return View();
                        return Json(new { success = false });
                    }

                }
                else
                {
                    Log.InfoFormat(Constants.AuditActionType.ApproveAccount.ToString(), "Account failed");
                    InsertAudit(Constants.AuditActionType.ApproveAccount, "Account failed", User.Identity.Name);
                    //return View();
                    return Json(new { success = false });

                }
            }
            catch(Exception ex)
            {
                Log.InfoFormat(Constants.AuditActionType.ApproveAccount.ToString(), ex.Message);
                InsertAudit(Constants.AuditActionType.ApproveAccount, "Account failed", User.Identity.Name);
                //return View();
                return Json(new { success = false });
            }
        }


        public ActionResult ModifyAccount()
        {
            ViewBag.Message = "Modify Account";
            ViewBag.Customer = _account.NewAccount();
            return View();
        }


        public ActionResult ConvertToWords(string LoanAmount)
        {
            string codes = new AmountToWordsManager().NumberToNaira(LoanAmount);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateLoan()
        {
            LoanManagement loanMgt = new LoanManagement();
            ViewBag.Loan = loanMgt.GetLoanCategory();
            //ViewBag.Trans=new Tra
            return View();
        }

        [HttpPost]
        public ActionResult CreateLoan(LoanViewModel tran)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string MethodName = "Loan Application";
            LoanManagement loanMgt = new LoanManagement();
            ViewBag.Loan = loanMgt.GetLoanCategory();
            decimal LoanInterest = tran.LoanAmount * Convert.ToDecimal(0.05) * Convert.ToDecimal(tran.LoanTimeline);
            try
            {
                var loan = new PureLoan();
                loan.Loanduration = tran.LoanTimeline;
                loan.Loaninterestweek = LoanInterest;
                loan.Loaninterest = LoanInterest; //* Convert.ToDecimal(tran.LoanTimeline);
                loan.Loanstatus = "P";
                loan.Loancateid = tran.LoanCateId;
                loan.Accountnos = tran.AccountNos;
                loan.Loanamount = tran.LoanAmount;
                loan.Guarantor1name = tran.Guarantor1Name;
                loan.Guarantor1phoneno = tran.Guarantor1Phone;
                loan.Guarantor2name = tran.Guarantor2Name;
                loan.Guarantor2phonenos = tran.Guarantor2Phone;
                loan.Processor = User.Identity.Name;
                loan.Approver = null;
                loan.Repaymentstatus = "Pending";
                loan.Processeddate = DateTime.Now;
                loan.Approveddate = null;
                bool NewLoan = loanMgt.InsertLoan(loan);
                if (NewLoan)
                {
                    Log.InfoFormat(MethodName, "Application Successful");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Application Successful", User.Identity.Name);
                    ViewBag.SuccessMsg = "Application Successful";
                }
                else
                {
                    Log.InfoFormat(MethodName, "Application Unuccessful");
                    InsertAudit(Constants.AuditActionType.LoanApplication, "Application Unuccessful", User.Identity.Name);
                    ViewBag.ErrorMsg = "Application Unuccessful";
                }
            }
            catch (Exception ex)
            {
                Log.InfoFormat(MethodName, "Application not Successful", ex.Message);
                InsertAudit(Constants.AuditActionType.LoanApplication, "Application Unuccessful", User.Identity.Name);
                ViewBag.ErrorMsg = "Application Unuccessful";
            }


            return View();
        }


        public ActionResult WithdrawalHistory()
        {
            ViewBag.Message = "Withdrawal";
            ViewBag.Withdrawal = new TransactionManagement().GetWithdrawalByUsername(User.Identity.Name);
            return View();
        }

        public ActionResult Withdrawal()
        {
            ViewBag.Message = "Withdrawal";
            return View();
        }


        [HttpPost]
        public ActionResult Withdrawal(TransactionViewModel tran)
        {
            string MethodName = Constants.AuditActionType.Withdrawal.ToString();
            string TellerId = User.Identity.Name;
            string RequestId = new SequenceManager().ReturnPostingSequence();
            string Amount = tran.Amount.ToString("#,##.00");
            decimal WithdrawalAmount = Convert.ToDecimal(Amount);
            var CustomerDetails =_account.GetAccountBal(tran.AccountNo.ToString());
            DateTime PostdDate = DateTime.Now;
            string ddate = PostdDate.ToString("yyyy-MM-dd");
            var TellerTill = new TransactionManagement().GetTillByUserName(TellerId, ddate);
            if (TellerTill == null)
            {
                Log.InfoFormat(MethodName, "No Till account");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", TellerId);
                ViewBag.ErrorMsg = "There is no Till Account Assigned";
                return View();
            }
            else
            {
                
                if (CustomerDetails.Balance < Convert.ToDecimal(WithdrawalAmount))
                {
                    Log.InfoFormat(MethodName, "Insufficient Balance");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Insufficient Balance", User.Identity.Name);
                    ViewBag.ErrorMsg = "Insufficient Balance";
                    return View();

                }
                try
                {
                    var withdrawal = new PureWithdrawal();
                    withdrawal.Requestid = RequestId;
                    withdrawal.Accountno = tran.AccountNo;
                    withdrawal.Amount = tran.Amount;
                    withdrawal.Currencyiso = "NGN";
                    withdrawal.Transtatus = tran.TranStatus;
                    withdrawal.Processor = User.Identity.Name;
                    withdrawal.Withdrawaldate = DateTime.Now;
                    bool UpdateRem = new TransactionManagement().InsertWithdrawal(withdrawal);
                    if (UpdateRem ==false)
                    {
                        ViewBag.ErrorMsg = "Unable to Debit the till";
                        return View();
                    }

                    var ProcessTill =_account.GetTellerTill(TellerId, ddate);
                    bool DebitTill = new TransactionManagement().PostWithdrawal(RequestId, CustomerDetails.AccountNos, WithdrawalAmount, TellerId);
                    if (DebitTill == false)
                    {
                        ViewBag.ErrorMsg = "Unable to Post the Transaction";
                        return View();
                    }
                    var acc = new PureTransactionLog();
                    acc.Sourceaccount = CustomerDetails.AccountNos;
                    acc.Destinationaccount = ProcessTill.AccountNo;
                    acc.Narration = RequestId;
                    acc.Amount = tran.Amount;
                    acc.Transtatus = "P";
                    acc.Trantype = "W";
                    acc.Customerid = CustomerDetails.CustomerId;
                    acc.Trancurrency = "NGN";
                    acc.Traninitiator = User.Identity.Name;
                    acc.Tranapprover = "";
                    acc.Trandate = DateTime.Now;
                    acc.Approveddate = null;
                    acc.Requestid = RequestId;
                    bool firstPost =_account.InsertTransaction(acc);
                    if (firstPost)
                    {
                        try
                        {
                            var request = new PurePostRequest();
                            request.Requestid = RequestId;
                            request.Accountname = CustomerDetails.AccountName;
                            request.Accountno = CustomerDetails.AccountNos;
                            request.Drcrindicator = "DR";
                            request.Tranamount = tran.Amount;
                            request.Transtatus = "P";
                            bool DrRequest = false;
                            DrRequest =_account.InsertRequest(request);

                            var request3 = new PureStatement();
                            request3.Referenceid = RequestId;
                            request3.Transactiondetails = RequestId + " | " + CustomerDetails.AccountName;
                            request3.Accountno = CustomerDetails.AccountNos;
                            request3.Deposit = 0;
                            request3.Withdrawal = Convert.ToDecimal(Amount);
                            request3.Accountbal = Convert.ToDecimal(CustomerDetails.Balance) + tran.Amount;
                            request3.Valuedate = DateTime.Now;

                            bool CustomerStatement = _account.InsertStatement(request3);
                            if (DrRequest && CustomerStatement)
                            {
                                var request1 = new PurePostRequest();
                                request1.Requestid = RequestId;
                                request1.Accountname = ProcessTill.AccountName;
                                request1.Accountno = ProcessTill.AccountNo;
                                request1.Drcrindicator = "CR";
                                request1.Tranamount = tran.Amount;
                                request1.Transtatus = "P";
                                bool CrRequest = _account.InsertRequest(request1);
                                if (CrRequest )
                                {
                                    Log.InfoFormat(MethodName, "Login Successful");
                                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                                    ViewBag.SuccessMsg = "Transaction Successful";
                                    return View();

                                }
                                else
                                {
                                    Log.InfoFormat(MethodName, "Transaction Failed");
                                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                                    ViewBag.ErrorMsg = "Transaction Failed";
                                    return View();
                                }
                            }
                            else
                            {
                                Log.InfoFormat(MethodName, "Transaction Failed");
                                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                                ViewBag.ErrorMsg = "Transaction Failed";
                                return View();
                            }

                        }
                        catch (Exception ex)
                        {
                            ErrorLogManager.LogError(MethodName, ex);
                            InsertAudit(Constants.AuditActionType.PostTransaction, ex.Message, User.Identity.Name);
                            ViewBag.ErrorMsg = ex.Message;
                            return View();
                        }

                    }
                }
                catch (Exception ex)
                {
                    ErrorLogManager.LogError(MethodName, ex);
                    InsertAudit(Constants.AuditActionType.PostTransaction, ex.Message, User.Identity.Name);
                    ViewBag.ErrorMsg = ex.Message;
                    return View();
                }

            }

            return View();
        }


        public ActionResult Logout()
        {
            TempData["ProfileMsg"] = TempData["ProfileMsg"];
            InsertAudit(Constants.AuditActionType.Logout, "Successfully Logout", User.Identity.Name);
            new UserManagement().DeleteTracking(User.Identity.Name);
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

    }
}