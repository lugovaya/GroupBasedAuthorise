using GroupBasedAuthorise.Models.DataModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.DAL
{
    public class IdentityManager
    {
        // Swap ApplicationRole for IdentityRole:
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        private readonly RoleManager<Permission> _permissionManager = new RoleManager<Permission>(
            new RoleStore<Permission>(new ApplicationDbContext()));

        private readonly UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public IdentityResult CreateUser(ApplicationUser user, string password)
        {
            return _userManager.Create(user, password);
        }

        #region Permissions
        public IdentityResult CreatePermission(string name, string description = "")
        {
            // Swap ApplicationRole for IdentityRole:
            return _permissionManager.Create(new Permission(name, description));
        }

        public bool PermissionExists(string name)
        {
            return _permissionManager.RoleExists(name);
        }

        public IdentityResult AddUserToPermission(string userId, string permissionName)
        {
            return _userManager.AddToRole(userId, permissionName);
        }

        public void ClearUserPermissions(string userId)
        {
            ApplicationUser user = _userManager.FindById(userId);
            var currentRoles = new List<IdentityUserRole>();

            currentRoles.AddRange(user.Roles);
            foreach (IdentityUserRole role in currentRoles)
            {
                _userManager.RemoveFromRole(userId, role.RoleId); // TODO: check it
            }
        }

        public void RemoveFromPermission(string userId, string permissionName)
        {
            _userManager.RemoveFromRole(userId, permissionName);
        }

        public void DeletePermission(string permissionId)
        {
            IQueryable<ApplicationUser> roleUsers = _context.Users.Where(u => u.Roles.Any(r => r.RoleId == permissionId));
            var permission = _context.Permissions.Find(permissionId);

            foreach (ApplicationUser user in roleUsers)
            {
                RemoveFromPermission(user.Id, permission.Name);
            }
            _context.Permissions.Remove(permission);
            _context.SaveChanges();
        }

        #endregion

        #region Groups
        public int CreateGroup(string groupName)
        {
            //if (GroupNameExists(groupName))
            //{
            //    throw new GroupExistsException(
            //        "A group by that name already exists in the database. Please choose another name.");
            //}

            var newGroup = new Group(groupName);
            _context.Groups.Add(newGroup);
            _context.SaveChanges();

            return newGroup.Id;
        }

        public int CreateAdminGroup(string userId)
        {
            var adminGroupId = CreateGroup("Admin");

            var adminPermissions = new List<string>
            {
                "Create", 
                "Edit",
                "Delete"
            };

            adminPermissions.ForEach(ad_p =>
                AddPermissionToGroup(adminGroupId, ad_p));

            AddUserToGroup(userId, adminGroupId);

            return adminGroupId;
        }

        public bool GroupNameExists(string groupName)
        {
            return _context.Groups.Any(gr => gr.Name == groupName);
        }

        public void ClearGroupUsers(int groupId)
        {
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);

            foreach (var user in group.Users)
            {
                ClearUserPermissions(user.Id);
                ClearUserCompanies(user.Id);
                ClearUserGroups(user.Id);
            }

            group.Users.Clear();

            _context.SaveChanges();
        }

        public void ClearUserGroups(string userId)
        {
            ClearUserPermissions(userId);
            ApplicationUser user = _context.Users.Find(userId);
            user.Groups.Clear();
            _context.SaveChanges();
        }

        public void AddUserToGroup(string userId, int groupId)
        {
            Group group = _context.Groups.Find(groupId);
            ApplicationUser user = _context.Users.Find(userId);

            var userGroup = new ApplicationUserGroup
            {
                Group = group,
                GroupId = group.Id,
                User = user,
                UserId = user.Id
            };

            foreach (var permission in group.Permissions)
            {
                _userManager.AddToRole(userId, permission.Permission.Name);
            }
            user.Groups.Add(userGroup);
            _context.SaveChanges();
        }

        public void RemoveUserFromGroup(string userId, int groupId)
        {
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            ClearUserPermissions(userId);
            group.Users.Remove(user);

            _context.SaveChanges();
        }

        public void ClearGroupPermissions(int groupId)
        {
            Group group = _context.Groups.Find(groupId);
            IQueryable<ApplicationUser> groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));

            foreach (var permission in group.Permissions)
            {
                var currentRoleId = permission.PermissionId;
                foreach (ApplicationUser user in groupUsers)
                {
                    // Is the user a member of any other groups with this role?
                    int groupsWithRole = user.Groups.Count(g => g.Group.Permissions.Any(r => r.PermissionId == currentRoleId));

                    // This will be 1 if the current group is the only one:
                    if (groupsWithRole == 1)
                    {
                        RemoveFromPermission(user.Id, permission.Permission.Name);
                    }
                }
            }
            group.Permissions.Clear();
            _context.SaveChanges();
        }

        public void AddPermissionToGroup(int groupId, string permissionName)
        {
            var group = _context.Groups.Find(groupId);
            var permission = _context.Permissions.First(r => r.Name == permissionName);

            var newPermission = new GroupPermission
            {
                GroupId = group.Id,
                Group = group,
                PermissionId = permission.Id,
                Permission = permission
            };

            // make sure the groupRole is not already present
            if (!group.Permissions.Contains(newPermission))
            {
                group.Permissions.Add(newPermission);
                _context.SaveChanges();
            }

            // Add all of the users in this group to the new role:
            IQueryable<ApplicationUser> groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));
            foreach (ApplicationUser user in groupUsers)
            {
                if (!(_userManager.IsInRole(user.Id, permissionName)))
                {
                    AddUserToPermission(user.Id, permission.Name);
                }
            }
        }

        public void RemovePermissionFromGroup(int groupId, string permissionId)
        {
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);
            var permission = group.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);

            group.Permissions.Remove(permission);

            _context.SaveChanges();

            // Add all of the users in this group to the new role:
            IQueryable<ApplicationUser> groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));
            foreach (ApplicationUser user in groupUsers)
            {
                if (!(_userManager.IsInRole(user.Id, permissionId)))
                {
                    RemoveFromPermission(user.Id, permissionId); // TODO: check it
                }
            }
        }

        public void DeleteGroup(int groupId)
        {
            Group group = _context.Groups.Find(groupId);

            // Clear the roles from the group:
            ClearGroupPermissions(groupId);
            _context.Groups.Remove(group);
            _context.SaveChanges();
        }
        #endregion

        #region Company
        public void CreateCompany(string companyName, string userId)
        {
            if (CompanyNameExist(companyName))
            {
                throw new GroupExistsException(
                    "A group by that name already exists in the database. Please choose another name.");
            }

            var company = new Company(companyName);
            _context.Companies.Add(company);
            _context.SaveChanges();

            var groupId = CreateAdminGroup(userId);

            AddGroupToCompany(company.Id, groupId);
        }

        public bool CompanyNameExist(string companyName)
        {
            return _context.Companies.Any(company => company.Title == companyName);
        }

        public void ClearCompanyGroups(Guid companyId)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == companyId);

            foreach (var group in company.Groups)
            {
                ClearGroupPermissions(group.Id);
                ClearGroupUsers(group.Id);
            }

            company.Groups.Clear();

            _context.SaveChanges();
        }

        public void AddGroupToCompany(Guid companyId, int groupId)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == companyId);
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);

            company.Groups.Add(group);

            _context.SaveChanges();
        }

        public void RemoveGroupFromCompany(Guid companyId, int groupId)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == companyId);
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);

            ClearGroupPermissions(group.Id);

            company.Groups.Remove(group);

            _context.SaveChanges();
        }

        public void AddUserToCompanyGroup(Guid companyId, int groupId, string userId)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == companyId);
            var user = _context.Users.FirstOrDefault(c => c.Id == userId);

            user.Companies.Add(new ApplicationUserCompany
            {
                Id = Guid.NewGuid(),
                UserId = userId, 
                CompanyId = companyId,
                User = user,
                Company = company
            });

            AddUserToGroup(userId, groupId);

            _context.SaveChanges();
        }

        public void RemoveUserFromCompanyGroup(Guid companyId, int groupId, string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var company = user.Companies.FirstOrDefault(c => c.CompanyId == companyId);

            user.Companies.Remove(company);

            RemoveUserFromGroup(userId, groupId);

            _context.SaveChanges();
        }

        public void DeleteCompany(Guid companyId)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == companyId);

            foreach (var group in company.Groups)
            {
                var groupId = group.Id;

                foreach (var user in group.Users)
                    RemoveUserFromCompanyGroup(companyId, groupId, user.Id);

                ClearGroupPermissions(groupId);
            }

            _context.Companies.Remove(company);

            _context.SaveChanges();
        }

        public void ClearUserCompanies(string userId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            user.Companies.Clear();

            _context.SaveChanges();
        }
        #endregion
    }
}