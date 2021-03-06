﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GroupBasedAuthorise.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Groups = new HashSet<ApplicationUserGroup>();
            this.GlobalGroups = new HashSet<ApplicationUserGlobalGroup>();
            this.Companies = new HashSet<ApplicationUserCompany>();
        }

        public virtual ICollection<ApplicationUserGroup> Groups { get; set; }

        public virtual ICollection<ApplicationUserGlobalGroup> GlobalGroups { get; set; }

        public virtual ICollection<ApplicationUserCompany> Companies { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
