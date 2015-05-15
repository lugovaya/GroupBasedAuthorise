using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models
{
    public class CompanyViewModel
    {
        public CompanyViewModel()
        {
            this.CompanyGroups = new List<GroupViewModel>();
        }

        public string CompanyName { get; set; }

        public List<GroupViewModel> CompanyGroups { get; set; }
    }
}