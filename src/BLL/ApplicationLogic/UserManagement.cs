using DAL.CustomObjects;
using DataAccessLayer.CustomObjects;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BLL.ApplicationLogic
{
    public class UserManagement
    {
        private readonly FountainDb context = FountainDb.GetInstance();
        public PureUser getUserByUsername(string Username)
        {
            string SQL = "Select * from Pure_User where UserName =@0";
            var actual = context.FirstOrDefault<PureUser>(SQL, Username);
            return actual;
        }

        // Get Teller
        public List<PureUser> getAllTeller()
        {
            string sql = "select * from Pure_User where RoleId=3";
            var actual = context.Fetch<PureUser>(sql).ToList();
            return actual;
        }
        public PureUser getUserByUsernames(string Username, int? RoleId)
        {
            string SQL = "Select * from Pure_User where UserName =@0 AND RoleId=@1";
            var actual = context.FirstOrDefault<PureUser>(SQL, Username, RoleId);
            return actual;
        }

        public List<UserView> getUserByCompany()
        {
            string sql = "select A.*, B.* from Pure_User A inner join Pure_Role B on  A.RoleId=B.RoleId";
            var actual = context.Fetch<UserView>(sql);
            return actual;
          
        }

        public bool UpdatePassword(string Password, int? Id)
        {
            try
            {
                var users = context.SingleOrDefault<PureUser>("where UserId =@0", Id);
                users.Userpwd = Password;
                context.Update(users);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public PureUser modifyPassword(int Id)
        {
            string sql = "Select * from Pure_User where UserId =@0";
            var actual = context.FirstOrDefault<PureUser>(sql, Id);
            return actual;

        }

        public UserProfileView getUserProfileByUsername(string Username)
        {
            var actual = context.SingleOrDefault<PureUser>("where UserName=@0", Username);
            var userrRole = context.SingleOrDefault<PureRole>("where RoleId=@0", actual.Roleid);
            UserProfileView userView = new UserProfileView()
            {
                FirstName = actual.Firstname,
                MiddleName = actual.Middlename,
                LastName = actual.Lastname,
                UserName = actual.Username,
                UserEmail=actual.Useremail,
                UserPWD = actual.Userpwd,
                RoleId = actual.Roleid,
                PhoneNos = actual.Phonenos,
                UserImg = actual.Userimg,
                RoleName = userrRole.Rolename,
                UserStatus = actual.Userstatus,
                CreatedBy = actual.Createdby,
                CreatedOn = actual.Createdon

            };

            return userView;
        }

        public List<UserView> getUserByIds(int UserId)
        {
            string sql = "select A.*, B.* from Pure_User A inner join Pure_Role B on  A.RoleId=B.RoleId where A.UserId=@0";
            var actual = context.Fetch<UserView>(sql, UserId);
            return actual;
        }

     
        public List<PureUser> getAllUsers()
        {
            var actual = context.Fetch<PureUser>();
            return actual;
        }

        public List<PureUser> getUserById()
        {
            var actual = context.Fetch<PureUser>().ToList();
            return actual;
        }

        public List<PureMenu> getMenuByUsername(string Username)
        {
            try
            {
                string SQL = "select A.* from Pure_Menu A inner join Pure_RoleMenu B on A.MenuId = B.MenuId inner join Pure_User c on c.RoleId = B.RoleId where c.UserName =@0 order by A.RankId ASC";
                var actual = context.Fetch<PureMenu>(SQL, Username).ToList();
                return actual;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool DoesUsernameExists(string Username)
        {
            var rslt = context.Fetch<PureUser>().Where(a => a.Username == Username).FirstOrDefault();
            if (rslt == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public bool InsertUser(PureUser Username)
        {
            try
            {
                context.Insert(Username);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int DeleteTracking(string Username)
        {
            string sql = "delete from Pure_Tracking where UserName =@0";
            int actual= context.Delete<PureTracking>(sql, Username);
            return actual;
        }

        public PureTracking TrackLogin(string username)
        {
            string sql = "select * from Pure_Tracking where UserName =@0";
            var actual = context.FirstOrDefault<PureTracking>(sql,username);
            return actual;
        }

        public bool DoesPasswordExists(string Username, string Password)
        {
            var rslt = context.Fetch<PureUser>().Where(a => a.Username == Username).FirstOrDefault();
            if (rslt.Userpwd == Password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int getFreshUser(string Username)
        {
            string sql = "Select COUNT(*) from Pure_AuditTrail where UserName = @0";
            var actual = context.ExecuteScalar<int>(sql, Username);
            return Convert.ToInt32(actual);
        }

        public PureUser getUserById(decimal UserId)
        {
            var actual = context.SingleById<PureUser>(UserId);
            return actual;
        }



        public bool UpdateUser(string FirstName, string MiddleName, string LastName, string UserName,string UserEmail, string UserPWD, string UserImg, string PhoneNos, int? RoleId, bool? UserStatus, string ModifiedBy, DateTime ModifiedOn, int? UserId)
        {
            try
            {
                var users = context.SingleOrDefault<PureUser>("WHERE UserId=@0", UserId);
                users.Firstname = FirstName;
                users.Middlename = MiddleName;
                users.Lastname = LastName;
                users.Userimg = UserImg;
                users.Username = UserName;
                users.Useremail = UserEmail;
                users.Userpwd = UserPWD;
                users.Phonenos = PhoneNos;
                users.Roleid = RoleId;
                users.Userstatus = UserStatus;
                users.Modifiedby = ModifiedBy;
                users.Modifiedon = ModifiedOn;
                context.Update(users);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool UpdateProfile(string FirstName, string MiddleName, string LastName, string UserName,string UserEmail, string UserPWD, string UserImg, string PhoneNos, string ModifiedBy, DateTime ModifiedOn, string Username)
        {
            try
            {
                var users = context.SingleOrDefault<PureUser>("WHERE UserName=@0", Username);
                users.Firstname = FirstName;
                users.Middlename = MiddleName;
                users.Lastname = LastName;
                users.Userimg = UserImg;
                users.Username = UserName;
                users.Useremail = UserEmail;
                users.Userpwd = UserPWD;
                users.Phonenos = PhoneNos;
                //users.RoleId = RoleId;
                //users.UserStatus = UserStatus;
                users.Modifiedby = ModifiedBy;
                users.Modifiedon = ModifiedOn;
                context.Update(users);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string DoFileUpload(HttpPostedFileBase pic, string filename = "")
        {
            if (pic == null && string.IsNullOrWhiteSpace(filename))
            {
                return "";
            }
            if (!string.IsNullOrWhiteSpace(filename) && pic == null) return filename;

            string result = DateTime.Now.Millisecond + "UserPics.jpg";
            pic.SaveAs(HttpContext.Current.Server.MapPath("~/UserImg/") + result);
            return result;
        }


        public string EncryptPassword(string UserPWD)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(UserPWD);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    UserPWD = Convert.ToBase64String(ms.ToArray());
                }
            }
            return UserPWD;
        }

        public string DecryptPassword(string UserPWD)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(UserPWD);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    UserPWD = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return UserPWD;
        }
    }
}
