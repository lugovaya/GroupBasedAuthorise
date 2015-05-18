using GroupBasedAuthorise.Models.DataModels;
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

        public Guid CompanyId { get; set; }

        public string CompanyName { get; set; }

        public List<GroupViewModel> CompanyGroups { get; set; }

        // TODO: check it
        public static explicit operator Company(CompanyViewModel company)
        {
            var newCompany = new Company
            {
                Id = company.CompanyId,
                Title = company.CompanyName
            };

            foreach (var group in company.CompanyGroups)
                newCompany.Groups.Add((Group)group);

            return newCompany;
        }
    }
}