using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class GroupPermission
    {
        public virtual int GroupId { get; set; }

        public virtual string PermissionId { get; set; }

        public virtual Group Group { get; set; }

        public virtual Permission Permission { get; set; }

        public static explicit operator PermissionViewModel(GroupPermission permission)
        {
            // NOTE: the property "Checked" is empty
            var newPermission = new PermissionViewModel
            {
                Description = permission.Permission.Description,
                Name = permission.Permission.Name,
                PermissionId = permission.PermissionId
            };

            return newPermission;
        }
    }
}