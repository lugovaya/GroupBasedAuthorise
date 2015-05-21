using GroupBasedAuthorise.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models
{
    public class GroupViewModel
    {
        public GroupViewModel()
        {
            this.Permissions = new List<PermissionViewModel>();
        }

        public string GroupName { get; set; }

        public List<PermissionViewModel> Permissions { get; set; }

        // Important: properties Company and Users will be empty
        public static explicit operator Group(GroupViewModel group)
        {
            var newGroup = new Group
            {
                Id = group.GroupId,
                Name = group.GroupName
            };

            foreach (var perm in group.Permissions)
                newGroup.Permissions.Add(new GroupPermission
                {
                    Group = newGroup,
                    GroupId = group.GroupId,
                    Permission = (Permission) perm,
                    PermissionId = perm.PermissionId
                });

            return newGroup;
        }

        public string CompanyId { get; set; }

        public string CompanyName { get; set; }

        public int GroupId { get; set; }
    }
}
