using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Models
{
    public class AddUserToGroupViewModel
    {
        public AddUserToGroupViewModel()
        {
            UsersEmails = new List<UserEmail>();
        }

        public int GroupId { get; set; }

        [Required]
        public List<UserEmail> UsersEmails { get; set; }
    }

    public class UserEmail
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}