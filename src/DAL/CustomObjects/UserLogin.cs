using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAccessLayer.CustomObjects
{
    public partial class UserLogin
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ComputerDetails { get; set; }
        public string UserEmail { get; set; }
    }
}