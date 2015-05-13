using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class Group
    {
        public Group()
        {
            //this.Users = new HashSet<ApplicationUser>();
            this.Permissions = new HashSet<GroupPermission>();
        }

        public Group(string groupName)
        {
            this.Name = groupName;
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid CompanyId { get; set; }

        //public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<GroupPermission> Permissions { get; set; }
        public virtual Company Company { get; set; }
    }
}
