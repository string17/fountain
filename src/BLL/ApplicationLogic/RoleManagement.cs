using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ApplicationLogic
{
    public class RoleManagement
    {
        private FountainDb _db = FountainDb.GetInstance();
        public List<PureRole> getAllRoles()
        {
            var actual = _db.Fetch<PureRole>();
            return actual;
        }

        public PureRole getRoleById(decimal Id)
        {
            var actual = _db.SingleById<PureRole>(Id);
            return actual;
        }

        public PureRole getRoleByRoleName(string RoleName)
        {
            string SQL = "Select * from Pure_Role where RoleName =@0";
            var actual = _db.FirstOrDefault<PureRole>(SQL, RoleName);
            return actual;
        }
        public List<PureRole> getRoleByRoleId()
        {
            var actual = _db.Fetch<PureRole>().ToList();
            return actual;
        }

        public List<PureRoleMenu> getRoleMenuByRoleId(decimal Id)
        {
            var actual = _db.Fetch<PureRoleMenu>("Where ID = " + Id);
            return actual;
        }

        public bool InsertRole(PureRole RoleName)
        {
            try
            {
                _db.Insert(RoleName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateRole(string RoleName, string RoleDesc, bool RoleStatus, int RoleId)
        {
            try
            {
                var roles = _db.SingleOrDefault<PureRole>("WHERE RoleId=@0", RoleId);
                roles.Rolename = RoleName;
                roles.Roledesc = RoleDesc;
                roles.Rolestatus = RoleStatus;
                _db.Update(roles);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
