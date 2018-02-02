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
        private FountainDb context = FountainDb.GetInstance();
        public List<PureRole> getAllRoles()
        {
            var actual = context.Fetch<PureRole>();
            return actual;
        }

        public PureRole getRoleById(decimal Id)
        {
            var actual = context.SingleById<PureRole>(Id);
            return actual;
        }

        public PureRole getRoleByRoleName(string RoleName)
        {
            string SQL = "Select * from Pure_Role where RoleName =@0";
            var actual = context.FirstOrDefault<PureRole>(SQL, RoleName);
            return actual;
        }
        public List<PureRole> getRoleByRoleId()
        {
            var actual = context.Fetch<PureRole>().ToList();
            return actual;
        }

        public List<PureRoleMenu> getRoleMenuByRoleId(decimal Id)
        {
            var actual = context.Fetch<PureRoleMenu>("Where ID = " + Id);
            return actual;
        }

        public bool InsertRole(PureRole RoleName)
        {
            try
            {
                context.Insert(RoleName);
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
                var roles = context.SingleOrDefault<PureRole>("WHERE RoleId=@0", RoleId);
                roles.Rolename = RoleName;
                roles.Roledesc = RoleDesc;
                roles.Rolestatus = RoleStatus;
                context.Update(roles);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
