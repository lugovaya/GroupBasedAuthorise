using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class ApplicationUserGroup
    {
        public virtual string UserId { get; set; }

        public virtual int GroupId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Group Group { get; set; }
    }
}