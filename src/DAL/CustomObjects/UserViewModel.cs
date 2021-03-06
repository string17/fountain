﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAL.CustomObjects
{
    public class UserViewModel
    {
     
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPWD { get; set; }
        public string PhoneNos { get; set; }
        public HttpPostedFileBase UserImg { get; set; }
        public int CustomerId { get; set; }
        public int? RoleId { get; set; }
        public int FlexId { get; set; }
        public bool UserStatus { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

    }
}