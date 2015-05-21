using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class Permission : IdentityRole
    {
        public Permission()
            : base()
        {
            this.Groups = new HashSet<Group>();
        }

        public Permission(string name, string description)
            : base(name)
        {
            this.Description = description;
            this.Groups = new HashSet<Group>();
        }

        public virtual string Description { get; set; }

        public virtual ICollection<Group> Groups { get; set; }

        public static explicit operator PermissionViewModel(Permission permission)
        {
            var viewPermission = new PermissionViewModel
            {
                PermissionId = permission.Id,
                Name = permission.Name,
                Description = permission.Description
            };

            return viewPermission;
        }
    }
}
