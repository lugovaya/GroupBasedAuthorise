using GroupBasedAuthorise.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string CompanyId { get; set; }

        [Required]
        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Company group")]
        public List<GroupViewModel> CompanyGroups { get; set; }

        // TODO: check it
        public static explicit operator Company(CompanyViewModel company)
        {
            var newCompany = new Company
            {
                Id = Guid.Parse(company.CompanyId),
                Title = company.CompanyName
            };

            foreach (var group in company.CompanyGroups)
                newCompany.Groups.Add((Group)group);

            return newCompany;
        }
    }
}