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
    }
}