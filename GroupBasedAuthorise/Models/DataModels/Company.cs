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
            this.Groups = new HashSet<Group>();
        }

        public System.Guid Id { get; set; }
        public string Title { get; set; }
        public int GroupId { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
    }
}