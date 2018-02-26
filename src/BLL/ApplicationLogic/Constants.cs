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
            LoanApplication,
            CreatedUser,
            Withdrawal,
            CreatedRole,
            ApproveAccount,
            CustomerCreated,
            ModifyTill,
            PostTransaction,
            ModifiedUser,
            RequestedNews,
            CreatedMenu,
            ModifiedMenu,
            CreateAccount,
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
            UploadUser,
            CustomerAccount
        }
    }
}
