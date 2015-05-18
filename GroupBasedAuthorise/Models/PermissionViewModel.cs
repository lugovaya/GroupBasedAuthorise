using GroupBasedAuthorise.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models
{
    public class PermissionViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Checked { get; set; }

        // Important: The properties Users and Groups will be empty
        public static explicit operator Permission(PermissionViewModel permission)
        {
            return new Permission
            {
                Id = permission.PermissionId, 
                Name = permission.Name,
                Description = permission.Description                 
            };
        }

        public string PermissionId { get; set; }
    }
}
