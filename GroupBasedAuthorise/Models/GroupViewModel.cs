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
    }
}
