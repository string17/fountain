using FountainContext.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ApplicationLogic
{
    public class MenuManagement
    {
        private readonly FountainDb _db = FountainDb.GetInstance();


        public List<PureMenu> getMenuByRoleId(decimal RoleId)
        {
            string SQL = "SELECT A.MenuId,A.MenuName,A.MenuURL,A.LinkIcon,A.ExternalUrl, B.MenuId,A.ParentId,A.RankId,RoleId from Pure_Menu A INNER JOIN Pure_RoleMenu B ON A.MenuId=B.MenuId order by A.RankId WHERE B.RoleId=@0  order by A.RankId ASC";
            var actual = _db.Fetch<PureMenu>(SQL,RoleId).ToList();
            return (actual);
        }


        public List<PureMenu> getMenuByMenuId()
        {
            var actual = _db.Fetch<PureMenu>().ToList();
            return actual;
        }

        public PureMenu getMenuByName(string MenuName)
        {
            string SQL = "Select * from DolMenu where MenuName =@0";
            var actual = _db.FirstOrDefault<PureMenu>(SQL, MenuName);
            return actual;
        }

        public List<PureMenu> getMenuByUsername(string Username)
        {
            try
            {
                string SQL = "select A.* from DolMenu A inner join DolRole_Menu B on A.MenuId = B.MenuId inner join DolUser c on c.RoleId = B.RoleId where c.UserName =@0";
                var actual = _db.Fetch<PureMenu>(SQL, Username).ToList();
                return actual;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<PureMenu> getMenuById()
        {
            var actual = _db.Fetch<PureMenu>().ToList();
            return actual;
        }

        public bool InsertMenu(PureMenu MenuName)
        {
            try
            {
                _db.Insert(MenuName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
