using GroupBasedAuthorise.Validators;
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
            //UsersEmails = new List<UserEmail>();

            //for (var i = 0; i < 10; i++)
            //    UsersEmails.Add(new UserEmail());
        }

        public int GroupId { get; set; }

        [Required]
        //[ValidateEnumerable(1, 10, ErrorMessage = "List of emails would have been less than 10 but more than 1")]
        public string UsersEmails { get; set; }
    }

    public class UserEmail
    {
        //[Required]
        //[EmailAddress]
        public string Email { get; set; }
    }
}