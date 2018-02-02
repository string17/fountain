using DAL.CustomObjects;
using DataAccessLayer.CustomObjects;
using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BLL.ApplicationLogic
{
    public class UserManagement
    {
        private FountainDb context = FountainDb.GetInstance();
        public PureUser getUserByUsername(string Username)
        {
            string SQL = "Select * from Pure_User where UserName =@0";
            var actual = context.FirstOrDefault<PureUser>(SQL, Username);
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
            UserManagement company = new UserManagement();
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

        //public EngineerViewModel getEngineerTerminal(string roleName)
        //{
        //    var term = context.SingleOrDefault<DolRole>("where RoleName=@0", roleName);
        //    var actual = context.SingleOrDefault<PureUser>("Where RoleId=@0", term.RoleId);
        //    var termNum = context.ExecuteScalar<int>("select Count(*) from DolTerminal where UserId =@0", actual.UserId);
        //    EngineerViewModel engineerView = new EngineerViewModel()
        //    {
        //        FirstName = actual.FirstName,
        //        MiddleName = actual.MiddleName,
        //        LastName = actual.LastName,
        //        UserName = actual.UserName,
        //        UserPWD = actual.UserPWD,
        //        RoleId = actual.RoleId,
        //        PhoneNos = actual.PhoneNos,
        //        RoleName = term.RoleName,
        //        //UserImg = actual.UserImg

        //    };
        //    return engineerView;
        //}


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

        public string base64Encode(string UserPWD) //Encode
        {
            try
            {
                byte[] encData_byte = new byte[UserPWD.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(UserPWD);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }

        public string base64Decode(string UserPWD) //Decode    
        {
            try
            {
                var encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecodeByte = Convert.FromBase64String(UserPWD);
                int charCount = utf8Decode.GetCharCount(todecodeByte, 0, todecodeByte.Length);
                char[] decodedChar = new char[charCount];
                utf8Decode.GetChars(todecodeByte, 0, todecodeByte.Length, decodedChar, 0);
                string result = new String(decodedChar);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Decode" + ex.Message);
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

        public int Delete(string Username)
        {
            string sql = "WHERE username = " + Username;
            int actual= context.Delete<PureTracking>(sql);
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
    }
}
