using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class ApplicationUserCompany
    {
        [Key]
        public Guid Id { get; set; }
        
        public virtual string UserId { get; set; }

        public virtual Guid CompanyId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Company Company { get; set; }
    }
}
