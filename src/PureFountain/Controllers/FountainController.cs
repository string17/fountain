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
    public class FountainController : BaseController
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
            string AccountNos = new SequenceManager().GenerateAccountNos();
            try
            {
                var Till = new PureTillAccount();
                Till.Accountname = account.AccountName;
                Till.Accountnos = AccountNos;
                Till.Accountbal = 0;
                Till.Currencycode = "NGN";
                Till.Tellerid = "";
                Till.Amountdebited = 0;
                Till.Accountstatus = false;
                Till.Drcrindicator = "CR";
                Till.Createdon = DateTime.Now;
                Till.Creditedby = User.Identity.Name;
                bool NewTill = new AccountManagement().CreateTill(Till);
                if (NewTill == true)
                {
                    ErrorLogManager.LogWarning(MethodName, "Account Successful");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                    ViewBag.SuccessMsg = "Account Successfully created";
                }
                {
                    ErrorLogManager.LogWarning(MethodName, "Account Unsuccessful");
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
            AccountManagement rolemgt = new AccountManagement();
            ViewBag.Account = new AccountManagement().GetTillAccount();
            return View();
        }

        [HttpGet]
        [Route("ModifyTill/{Id}")]
        public ActionResult ModifyTill(int? Id)
        {
            ViewBag.Message = "Till Account";
            int TillId = Id.GetValueOrDefault();
            ViewBag.Till = new AccountManagement().GetTillAccountDetails(TillId);
            ViewBag.Teller = new UserManagement().getAllTeller();
            return View();
        }


        [Route("ModifyTill/{Id}")]
        public ActionResult ModifyTill(TillManager Till, int? Id)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string MethodName = Constants.AuditActionType.ModifiedUser.ToString();
            ViewBag.Teller = new UserManagement().getAllTeller();
            int TillId = Id.GetValueOrDefault();

            try
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
                bool EditTill = new AccountManagement().UpdateTill(account.Accountname, account.Accountbal, account.Tellerid, account.Accountstatus, account.Drcrindicator, account.Creditedby, account.Createdon, TillId);

                if (EditTill == true)
                {
                    var NewTill = new PureTellerTill();
                    NewTill.Accountnos = account.Accountnos;
                    NewTill.Initialbalance = account.Accountbal;
                    NewTill.Tellerid = account.Tellerid;
                    NewTill.Amount = 0;
                    NewTill.Drcrindicator = "CR";
                    NewTill.Debiteddate = DateTime.Now;
                    NewTill.Createdby = User.Identity.Name;
                    bool TellerTill = new TransactionManagement().InsertTellerTill(NewTill);
                    ErrorLogManager.LogWarning(MethodName, "Till Update Successful");
                    InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update Successful", User.Identity.Name);
                    TempData["SuccessMsg"] = "Till Update Successful";
                    return RedirectToAction("ViewTill");
                }
                else
                {
                    ErrorLogManager.LogWarning(MethodName, "Till Update Successful");
                    InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update Successful", User.Identity.Name);
                    ViewBag.SuccessMsg = "Till Update Unsuccessful";
                }
            }
            catch (Exception ex)
            {
                ErrorLogManager.LogError(MethodName, ex);
                InsertAudit(Constants.AuditActionType.ModifyTill, "Till Update Unsuccessful", User.Identity.Name);
                ViewBag.SuccessMsg = "Till Update Unsuccessful";
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
            TempData["SuccessMsg"] = TempData["SuccessMsg"];
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
                        ErrorLogManager.LogWarning(MethodName, "Modified a user");
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
                    editUser.Userpwd = usermgt.EncryptPassword(account.UserPWD);
                    bool UpdateUser = userManagement.UpdateProfile(editUser.Firstname, editUser.Middlename, editUser.Lastname, editUser.Username, editUser.Useremail, editUser.Userpwd, editUser.Userimg, editUser.Phonenos, User.Identity.Name, DateTime.Now, User.Identity.Name);
                    InsertAudit(Constants.AuditActionType.ProfileModified, "Modified a user " + editUser.Username, User.Identity.Name);
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
            catch (Exception)
            {
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
            AccountManagement accountmgt = new AccountManagement();
            ViewBag.Country = accountmgt.GetAllCountries();
            ViewBag.Referral = accountmgt.GetReferral();
            ViewBag.Banks = accountmgt.GetAllBank();
            ViewBag.Religion = accountmgt.GetAllReligion();
            ViewBag.Card = accountmgt.GetAllCard();
            ViewBag.Account = accountmgt.GetAccountCategory();
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(CustomerAccountView customer)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }
            var ipaddress = AuditService.DetermineIPAddress();
            string MethodName = Constants.AuditActionType.CustomerAccount.ToString();
            AccountManagement accountmgt = new AccountManagement();
            ViewBag.Country = accountmgt.GetAllCountries();
            ViewBag.Referral = accountmgt.GetReferral();
            ViewBag.Banks = accountmgt.GetAllBank();
            ViewBag.Religion = accountmgt.GetAllReligion();
            ViewBag.Card = accountmgt.GetAllCard();
            ViewBag.Account = accountmgt.GetAccountCategory();
            DateTime NewTime = DateTime.Now;

            try
            {
                var Acc = new PureCustomerInfo();
                Acc.Firstname = customer.FirstName;
                Acc.Middlename = customer.MiddleName;
                Acc.Lastname = customer.LastName;
                Acc.Useremail = customer.UserEmail;
                Acc.Phonenos1 = customer.PhoneNos1;
                Acc.Phonenos2 = customer.PhoneNos2;
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
                Acc.Accountnos = "";
                Acc.Accountstatus = false;
                Acc.Createdby = User.Identity.Name;
                Acc.Createdon = DateTime.Now;
                Acc.Modifiedby = "";
                Acc.Modifiedon = DateTime.Now;
                Acc.Approvedby = "";
                Acc.Approvedon = null;
                AccountManagement CustomerAccount = new AccountManagement();
                Acc.Accountimg = new UserManagement().DoFileUpload(customer.AccountImg);
                Acc.Accountsign = new UserManagement().DoFileUpload(customer.AccountSign);
                bool NewAccount = false;
                NewAccount = CustomerAccount.InsertAccount(Acc);
                if (NewAccount == true)
                {
                    ErrorLogManager.LogWarning(MethodName, "Account Successfully created");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Successfully Created", User.Identity.Name);
                    ViewBag.SuccessMsg = "Account successfully created";
                    return View();
                }
                else
                {
                    ErrorLogManager.LogWarning(MethodName, "Unable to create Account");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                    ViewBag.ErrorMsg = "Unable to create Account";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ErrorLogManager.LogError(MethodName, ex);
                InsertAudit(Constants.AuditActionType.CustomerAccount, ex.Message, User.Identity.Name);
                ViewBag.ErrorMsg = ex.Message;
            }
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

        //getLoan Status
        public ActionResult GetLoanDetails(int LoanId)
        {
            var codes = new LoanManagement().GetLoanDetails(LoanId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        //get Till details
        public ActionResult GetTillDetails(int TillId)
        {
            var codes = new AccountManagement().GetTillAccountDetails(TillId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPendingAccount(int CustomerId)
        {
            var codes = new AccountManagement().GetAccountDetails(CustomerId);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAccountDetails(int CustomerId)
        {
            var Customercodes = new AccountManagement().GetAccountDetails(CustomerId);
            return Json(Customercodes, JsonRequestBehavior.AllowGet);
        }


        //Get customer Account Balance
        public ActionResult GetCustomerBalance(string AccountNos)
        {
            var codes = new AccountManagement().GetAccountBal(AccountNos);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAccount()
        {
            ViewBag.Message = "Customers";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetCustomerInfo();
            return View();
        }


        [HttpGet]
        [Route("EditAccount/{Id}")]
        public ActionResult EditAccount(int Id)
        {
            ViewBag.Message = "Customers";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetAccountDetails(Id);
            ViewBag.Country = accountMgt.GetAllCountries();
            ViewBag.Referral = accountMgt.GetReferral();
            ViewBag.Banks = accountMgt.GetAllBank();
            return View();
        }

        public ActionResult ViewCustomer()
        {
            ViewBag.Message = "Customers";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.NewAccount();
            return View();
        }

        public ActionResult ApproveAccount()
        {
            ViewBag.Message = "Approve Account";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetPendingAccount();
            return View();
        }

        public ActionResult ListAccount()
        {
            ViewBag.Message = "View Account";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetPendingAccount();
            return View();
        }

        public ActionResult PostDeposit()
        {
            ViewBag.Message = "Post Deposit";
            ViewBag.SuccessMsg = TempData["SuccessMsg"];
            AccountManagement accountMgt = new AccountManagement();
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
            var CustomerDetails = new AccountManagement().GetAccountBal(tran.AccountNos.ToString());
            //var DoTransaction = new TransactionManagement().CreditCustomer(tran.AccountNos, tran.Amount, User.Identity.Name);
            DateTime PostdDate = DateTime.Now;
            string ddate = PostdDate.ToString("yyyy-MM-dd");
            string UserName = User.Identity.Name;
            var TellerTill = new TransactionManagement().GetTillByUserName(UserName, ddate);
            if (TellerTill == null)
            {
                ErrorLogManager.LogWarning(MethodName, "No Till account");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                ViewBag.ErrorMsg = "There is no Till Account Assigned";
                return View();
            }
            decimal? TillBal = TellerTill.Initialbalance;
            if (TillBal < Convert.ToDecimal(DepositAmount))
            {
                ErrorLogManager.LogWarning(MethodName, "Transaction Failed");
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

                var ProcessTill = new AccountManagement().GetTellerTill(TellerId, ddate);
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
                acc.Sourceaccount = ProcessTill.AccountNos;
                acc.Destinationaccount = CustomerDetails.AccountNos;
                acc.Narration = RequestId + " | " + tran.DepositorName + " | " + tran.DepositorNos;
                acc.Amount = tran.Amount;
                acc.Transtatus = "P";
                acc.Customerid = CustomerDetails.CustomerId;
                acc.Trancurrency = "NGN";
                acc.Traninitiator = User.Identity.Name;
                acc.Tranapprover = "";
                acc.Trandate = DateTime.Now;
                acc.Approveddate = null;
                acc.Requestid = RequestId;

                bool firstPost = false;
                firstPost = new AccountManagement().InsertTransaction(acc);
                if (firstPost == true)
                {
                    try
                    {

                        var deposit = new PureDeposit();
                        deposit.Requestid = RequestId;
                        deposit.Accounttitle = CustomerDetails.AccountName;
                        deposit.Accountnos = tran.AccountNos;
                        deposit.Depositorname = tran.DepositorName;
                        deposit.Depositornos = tran.DepositorNos;
                        deposit.Narration = RequestId + " | " + tran.DepositorName + " | " + tran.DepositorNos;
                        deposit.Amount = tran.Amount;
                        deposit.Processor = User.Identity.Name;
                        deposit.Status = "P";
                        deposit.Currencyiso = "NGN";
                        deposit.Depositeddate = DateTime.Now;
                        bool newDeposit = false;
                        newDeposit = new AccountManagement().InsertDeposit(deposit);
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
                            request.Accountnos = CustomerDetails.AccountNos;
                            request.Drcrindicator = "CR";
                            request.Tranamount = tran.Amount;
                            request.Transtatus = "P";
                            bool DrRequest = false;
                            DrRequest = new AccountManagement().InsertRequest(request);
                            if (DrRequest == true)
                            {
                                var request1 = new PurePostRequest();
                                request1.Requestid = RequestId;
                                request1.Accountname = ProcessTill.AccountName;
                                request1.Accountnos = ProcessTill.AccountNos;
                                request1.Drcrindicator = "DR";
                                request1.Tranamount = tran.Amount;
                                request1.Transtatus = "P";
                                bool CrRequest = false;
                                CrRequest = new AccountManagement().InsertRequest(request1);
                                if (CrRequest == true)
                                {
                                    ErrorLogManager.LogWarning(MethodName, "Deposit Successful");
                                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                                    ViewBag.SuccessMsg = "Transaction Successful";
                                    return View();
                                    
                                }
                                else
                                {
                                    ErrorLogManager.LogWarning(MethodName, "Transaction Failed");
                                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                                    ViewBag.ErrorMsg = "Transaction Failed";
                                    return View();
                                }
                            }
                            else
                            {
                                ErrorLogManager.LogWarning(MethodName, "Transaction Failed");
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
            return View();
        }
        


        public ActionResult TransactionHistory()
        {
            ViewBag.Message = "Transaction History";
            ViewBag.Deposit = new AccountManagement().GetDepositByUserName(User.Identity.Name);
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
            //ViewBag.Customer = new ReportManagement().CustomerBankStatement();
            return View();
        }


        public ActionResult LoanHistory()
        {
            ViewBag.Message = "Loan History";
            string UserName = User.Identity.Name;
            ViewBag.Loan = new LoanManagement().GetLoanByUsername(UserName);
            return View();
        }

        public ActionResult LoanReport()
        {
            ViewBag.Message = "Loan Report";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetOverallCustomerLoan();
            return View();
        }

        [HttpGet]
        public ActionResult GetTransHistory(string startdate, string enddate, string AccountNos)
        {
            ViewBag.deposits = new ReportManagement().CustomerBankStatement(startdate, enddate, AccountNos);
            return PartialView("_CustomerStatement");
        }


        public ActionResult TransactionReport()
        {
            ViewBag.Message = "Transaction Report";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetPendingAccount();
            return View();
        }

        public ActionResult RepostTransaction()
        {
            ViewBag.Message = "Repost Transaction";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.GetPendingAccount();
            return View();
        }

        public ActionResult ApproveDeposit()
        {
            ViewBag.Message = "Approve Deposit";
            ViewBag.Transaction = new AccountManagement().GetPendingTransaction();
            return View();
        }


       
        public ActionResult ApproveTransaction(string RequestId)
        {
               
            TransactionManagement transactionMgt = new TransactionManagement();
            string MethodName = "Approve Transaction";
            var codes = transactionMgt.GetTransactionDetails(RequestId);
            string TranStatus = "S";
            string Approver = User.Identity.Name;
            string CustomerAccount = codes.Destinationaccount;
          
            bool UpdateTransaction = transactionMgt.ApproveTransaction(RequestId, Approver,TranStatus);//Transaction
            ErrorLogManager.LogWarning(MethodName, "Account Successfully created");
            InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Successfully Created", User.Identity.Name);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }
            


        public ActionResult RejectTransaction(string RequestId)
        {
            TransactionManagement transactionMgt = new TransactionManagement();
            var codes = transactionMgt.GetTransactionDetails(RequestId);
            string MethodName = "Cancel Transaction";
            string ApproverId = User.Identity.Name;
            string TranStatus = "F";
            var UpdateTransaction = transactionMgt.RejectTransaction(RequestId, ApproverId, TranStatus);//Transaction
            ErrorLogManager.LogWarning(MethodName, "Transaction Successfully cancelled");
            InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Successfully Cancelled", User.Identity.Name);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RejectLoan(int LoanId)
        {
            LoanManagement loanMgt = new LoanManagement();
            string MethodName = "Decline Loan";
            string LoanStatus = "F";
            string Approver = User.Identity.Name;
            bool codes = loanMgt.ApproveLoanStatus(LoanStatus, LoanId, Approver);//Transaction
            ErrorLogManager.LogWarning(MethodName, "Unable to grant loan");
            InsertAudit(Constants.AuditActionType.CustomerAccount, "Unable to grant loan", User.Identity.Name);
            return Json(codes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GrantLoan(int LoanId)
        {
            LoanManagement loanMgt = new LoanManagement();
            string MethodName = "Approve Loan";
            string LoanStatus = "A";
            string Approver = User.Identity.Name;
            bool codes = loanMgt.ApproveLoanStatus(LoanStatus,LoanId, Approver);//Transaction
            ErrorLogManager.LogWarning(MethodName, "Account Successfully created");
            InsertAudit(Constants.AuditActionType.CustomerAccount, "Loan Granted", User.Identity.Name);
            return Json(codes, JsonRequestBehavior.AllowGet);
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
            ViewBag.Customer = new AccountManagement().GetCustomerByReferral(User.Identity.Name);
            return View();
        }

        public ActionResult GenerateAccountNos(int CustomerId)
        {
            var AccountNos = new SequenceManager().GenerateAccountNos().ToString();
            var acc= new PureCustomerInfo();
            acc.Accountnos = AccountNos;
            acc.Accountstatus = true;
            AccountManagement accountMgt = new AccountManagement();
            //var ExistingNos = accountMgt.GetAccountNos(AccountNos);
            bool UpdateAccount = accountMgt.ApproveAccount(CustomerId, acc.Accountnos, acc.Accountstatus);
            var codes = new AccountManagement().GetAccountDetails(CustomerId);
            var accNos = new PureAccountNo();
            accNos.Accountnos = AccountNos;
            accNos.Customerid = CustomerId;
            bool InsertAccount = accountMgt.NewAccountNos(accNos);
            var Bal = new PureAccountDetail();
            Bal.Accountbal = Convert.ToDecimal(0.00);
            Bal.Accountnos = AccountNos;
            Bal.Customerid = CustomerId;
            Bal.Modifiedby = User.Identity.Name;
            Bal.Modifiedon = DateTime.Now;
            bool InsertBal = accountMgt.FreshAccount(Bal);
            ErrorLogManager.LogWarning(Constants.AuditActionType.ApproveAccount.ToString(), "Account Successfully created");
            InsertAudit(Constants.AuditActionType.ApproveAccount, "Account Successfully Created", User.Identity.Name);
            return Json(codes, JsonRequestBehavior.AllowGet);
                      
        }

    
        public ActionResult ModifyAccount()
        {
            ViewBag.Message = "Modify Account";
            AccountManagement accountMgt = new AccountManagement();
            ViewBag.Customer = accountMgt.NewAccount();
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

            //DateTime ProcessedDate = DateTime.Now;
            DateTime Processed = DateTime.Now;
            //DateTime dt1 = DateTime.Now.ParseExact(DateTime, "yyyy-MM-dd", null);
            //DateTime dt1 = (DateTime.Now).Sql.Types.SqlDateTime;
            LoanManagement loanMgt = new LoanManagement();
            ViewBag.Loan = loanMgt.GetLoanCategory();
            //DateTime dt2 = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            decimal LoanInterest =tran.LoanAmount * Convert.ToDecimal(0.05) * Convert.ToDecimal(tran.LoanTimeline);
            try
            {
                var loan = new PureLoan();
                loan.Loanduration = tran.LoanTimeline;
                loan.Loaninterestweek = LoanInterest;
                loan.Loaninterest = LoanInterest * Convert.ToDecimal(tran.LoanTimeline);
                loan.Loanstatus = "P";
                loan.Loancateid = tran.LoanCateId;
                loan.Accountnos = tran.AccountNos;
                loan.Loanamount = tran.LoanAmount;
                loan.Guarantor1name = tran.Guarantor1Name;
                loan.Guarantor1phonenos = tran.Guarantor1Phone;
                loan.Guarantor1occupation = tran.Guarantor1Occupation;
                loan.Guarantor2name = tran.Guarantor2Name;
                loan.Guarantor2phonenos = tran.Guarantor2Phone;
                loan.Guarantor2occupation = tran.Guarantor2Occupation;
                loan.Cheque1nos = loanMgt.DoFileUpload(tran.cheque1);
                loan.Cheque2nos = loanMgt.DoFileUpload(tran.cheque2);
                loan.Guarantoridnos = loanMgt.DoFileUpload(tran.Guarantor1IdCard);
                loan.Guarantor2idnos = loanMgt.DoFileUpload(tran.Guarantor2IdCard);
                loan.Nepabill = loanMgt.DoFileUpload(tran.NEPABill);
                loan.Processor = User.Identity.Name;
                loan.Approver = null;
                loan.Repaymentstatus = "Pending";
                loan.Processeddate = DateTime.Now;
                loan.Approveddate = null;
                bool NewLoan = loanMgt.InsertLoan(loan);
                if (NewLoan == true)
                {
                    ErrorLogManager.LogWarning(MethodName, "Application Successful");
                    InsertAudit(Constants.AuditActionType.CustomerAccount, "Application Successful", User.Identity.Name);
                    ViewBag.SuccessMsg = "Application Successful";
                }
                else
                {
                    ErrorLogManager.LogWarning(MethodName, "Application Unuccessful");
                    InsertAudit(Constants.AuditActionType.LoanApplication, "Application Unuccessful", User.Identity.Name);
                    ViewBag.ErrorMsg = "Application Unuccessful";
                }
            }
            catch(Exception ex)
            {
                ErrorLogManager.LogError(MethodName, ex);
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
            var CustomerDetails = new AccountManagement().GetAccountBal(tran.AccountNos.ToString());
            //var DoTransaction = new TransactionManagement().CreditCustomer(tran.AccountNos, tran.Amount, User.Identity.Name);
            DateTime PostdDate = DateTime.Now;
            string ddate = PostdDate.ToString("yyyy-MM-dd");
            //string UserName = User.Identity.Name;
            var TellerTill = new TransactionManagement().GetTillByUserName(TellerId, ddate);
            if (TellerTill == null)
            {
                ErrorLogManager.LogWarning(MethodName, "No Till account");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", TellerId);
                ViewBag.ErrorMsg = "There is no Till Account Assigned";
                return View();
            }
            decimal? TillBal = TellerTill.Initialbalance;
            if (CustomerDetails.Balance < Convert.ToDecimal(WithdrawalAmount))
            {
                ErrorLogManager.LogWarning(MethodName, "Insufficient Balance");
                InsertAudit(Constants.AuditActionType.CustomerAccount, "Insufficient Balance", User.Identity.Name);
                ViewBag.ErrorMsg = "Insufficient Balance";
                return View();

            }
            try
            {
                var withdrawal = new PureWithdrawal();
                withdrawal.Requestid = RequestId;
                withdrawal.Accountnos = tran.AccountNos;
                withdrawal.Amount = tran.Amount;
                withdrawal.Currencyiso = tran.CurrencyISO;
                withdrawal.Transtatus = tran.TranStatus;
                withdrawal.Processor = User.Identity.Name;
                withdrawal.Withdrawaldate = DateTime.Now;
                bool UpdateRem = new TransactionManagement().InsertWithdrawal(withdrawal);
                if (UpdateRem == false)
                {
                    ViewBag.ErrorMsg = "Unable to Remit the till";
                    return View();
                }

                var ProcessTill = new AccountManagement().GetTellerTill(TellerId, ddate);
                bool DebitTill = new TransactionManagement().PostWithdrawal(RequestId, CustomerDetails.AccountNos, WithdrawalAmount, TellerId);
                if (DebitTill == false)
                {
                    ViewBag.ErrorMsg = "Unable to Post the Transaction";
                    return View();
                }
                var acc = new PureTransactionLog();
                acc.Sourceaccount = CustomerDetails.AccountNos;
                acc.Destinationaccount = ProcessTill.AccountNos;
                acc.Narration = RequestId;
                acc.Amount = tran.Amount;
                acc.Transtatus = "P";
                acc.Customerid = CustomerDetails.CustomerId;
                acc.Trancurrency = "NGN";
                acc.Traninitiator = User.Identity.Name;
                acc.Tranapprover = "";
                acc.Trandate = DateTime.Now;
                acc.Approveddate = null;
                acc.Requestid = RequestId;
                bool firstPost = false;
                firstPost = new AccountManagement().InsertTransaction(acc);
                if (firstPost == true)
                {


                    try
                    {
                        var request = new PurePostRequest();
                        request.Requestid = RequestId;
                        request.Accountname = CustomerDetails.AccountName;
                        request.Accountnos = CustomerDetails.AccountNos;
                        request.Drcrindicator = "DR";
                        request.Tranamount = tran.Amount;
                        request.Transtatus = "P";
                        bool DrRequest = false;
                        DrRequest = new AccountManagement().InsertRequest(request);
                        if (DrRequest == true)
                        {
                            var request1 = new PurePostRequest();
                            request1.Requestid = RequestId;
                            request1.Accountname = ProcessTill.AccountName;
                            request1.Accountnos = ProcessTill.AccountNos;
                            request1.Drcrindicator = "CR";
                            request1.Tranamount = tran.Amount;
                            request1.Transtatus = "P";
                            bool CrRequest = false;
                            CrRequest = new AccountManagement().InsertRequest(request1);
                            if (CrRequest == true)
                            {
                                ErrorLogManager.LogWarning(MethodName, "Login Successful");
                                InsertAudit(Constants.AuditActionType.CustomerAccount, "Account Created", User.Identity.Name);
                                ViewBag.SuccessMsg = "Transaction Successful";
                                return View();

                            }
                            else
                            {
                                ErrorLogManager.LogWarning(MethodName, "Transaction Failed");
                                InsertAudit(Constants.AuditActionType.CustomerAccount, "Transaction Failed", User.Identity.Name);
                                ViewBag.ErrorMsg = "Transaction Failed";
                                return View();
                            }
                        }
                        else
                        {
                            ErrorLogManager.LogWarning(MethodName, "Transaction Failed");
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