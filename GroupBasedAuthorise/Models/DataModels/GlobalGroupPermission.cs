using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class GlobalGroupPermission
    {
        public virtual int GlGroupId { get; set; }

        public virtual string GlPermissionId { get; set; }

        public virtual GlobalGroup GlGroup { get; set; }

        public virtual Permission GlPermission { get; set; }
    }
}
