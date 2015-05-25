using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class ApplicationUserGlobalGroup
    {
        public virtual string UserId { get; set; }

        public virtual int GlGroupId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual GlobalGroup GlGroup { get; set; }
    }
}