using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class GlobalGroup
    {
        private ICollection<ApplicationUser> _users;
        private ICollection<GlobalGroupPermission> _permisson;

        public GlobalGroup(string groupName)
        {
            this.Name = groupName;
        }

        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual ICollection<ApplicationUser> Users
        {
            get { return _users ?? (_users = new HashSet<ApplicationUser>()); }
            set { _users = value; }
        }

        public virtual ICollection<GlobalGroupPermission> Permissions
        {
            get { return _permisson ?? (_permisson = new HashSet<GlobalGroupPermission>()); }
            set { _permisson = value; }
        }
    }
}