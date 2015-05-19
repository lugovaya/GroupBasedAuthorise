using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class Group
    {
        private ICollection<ApplicationUser> _users;
        private ICollection<GroupPermission> _permisson;

        //public Group()
        //{
        //    this.Users = new HashSet<ApplicationUser>();
        //    this.Permissions = new HashSet<GroupPermission>();
        //}

        public Group(string groupName)
        //: this()
        {
            this.Name = groupName;
        }

        public Group(string groupName, Guid companyId)
            : this(groupName)
        {
            this.CompanyId = companyId;
        }

        public Group()
        {
            // TODO: Complete member initialization
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid CompanyId { get; set; }

        public virtual ICollection<ApplicationUser> Users
        {
            get { return _users ?? (_users = new HashSet<ApplicationUser>()); }
            set { _users = value; }
        }

        public virtual ICollection<GroupPermission> Permissions
        {
            get { return _permisson ?? (_permisson = new HashSet<GroupPermission>()); }
            set { _permisson = value; }
        }

        public virtual Company Company { get; set; }

        public static explicit operator GroupViewModel(Group group)
        {
            var newGroup = new GroupViewModel
            {
                GroupId = group.Id,
                GroupName = group.Name
            };

            foreach (var permission in group.Permissions)
                newGroup.Permissions.Add((PermissionViewModel)permission);

            return newGroup;
        }
    }
}
