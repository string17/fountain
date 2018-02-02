using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ApplicationLogic
{
    public class Constants
    {
        
        public enum AuditActionType
        {

            Login,
            Logout,
            CreatedUser,
            CreatedRole,
            CustomerCreated,
            ModifiedUser,
            RequestedNews,
            CreatedMenu,
            ModifiedMenu,
            AssignedPermissionToRole,
            CreatedPermission,
            AssignedMenuToRole,
            ModifiedPermission,
            DeletedUser,
            General,
            ApprovedUser,
            RejectedUser,
            RoleModified,
            UserModified,
            CustomerProfileModified,
            ProfileModified,
            PasswordChanged,
            UploadUser
        }
    }
}
