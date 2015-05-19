using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class Company
    {
        public Company()
        {
            this.Id = Guid.NewGuid();
            this.Groups = new HashSet<Group>();
        }

        public Company(string companyTitle)
        {
            this.Id = Guid.NewGuid();
            this.Title = companyTitle;
            this.Groups = new HashSet<Group>();
        }

        public System.Guid Id { get; set; }
        public string Title { get; set; }
        public int GroupId { get; set; }

        public virtual ICollection<Group> Groups { get; set; }

        public static explicit operator CompanyViewModel(Company company)
        {
            var newCompany = new CompanyViewModel
            {
                CompanyId = company.Id.ToString(),
                CompanyName = company.Title
            };

            foreach (var group in company.Groups)
                newCompany.CompanyGroups.Add((GroupViewModel)group);

            return newCompany;
        }
    }
}