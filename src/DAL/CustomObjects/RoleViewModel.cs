using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAccessLayer.CustomObjects

{
    public class RoleViewModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string RoleDesc { get; set; }
        public bool RoleStatus { get; set; }
    }
}