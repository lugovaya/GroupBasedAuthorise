﻿using GroupBasedAuthorise.Models.DataModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GroupBasedAuthorise.DAL
{
    public class EntityManager : IDisposable
    {
        // Swap ApplicationRole for IdentityRole:
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        private readonly RoleManager<Permission> _permissionManager = new RoleManager<Permission>(
            new RoleStore<Permission>(new ApplicationDbContext()));

        private readonly UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public async Task<IdentityResult> CreateUser(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
        #region Permissions
        public IdentityResult CreatePermission(string name, string description = "")
        {
            // Swap ApplicationRole for IdentityRole:
            return _permissionManager.Create(new Permission(name, description));
        }

        public async Task<IdentityResult> CreatePermissionAsync(string name, string description = "")
        {
            // Swap ApplicationRole for IdentityRole:
            return await _permissionManager.CreateAsync(new Permission(name, description));
        }

        public bool PermissionExists(string name)
        {
            return _permissionManager.RoleExists(name);
        }

        public bool HasUserPermission(string userId, string permissionName)
        {
            return _userManager.IsInRole<ApplicationUser, string>(userId, permissionName);
        }

        public async Task<bool> HasUserPermissionAsync(string userId, string permissionName)
        {
            return await _userManager.IsInRoleAsync(userId, permissionName);
        }

        public static bool HasCurrentUserPermission(string permissionName)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();

            return new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext())).IsInRole(userId, permissionName);
        }

        public IdentityResult AddUserToPermission(string userId, string permissionName)
        {
            return _userManager.AddToRole(userId, permissionName);
        }

        public async Task<IdentityResult> AddUserToPermissionAsync(string userId, string permissionName)
        {
            return await _userManager.AddToRoleAsync(userId, permissionName);
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

        public async Task ClearUserPermissionsAsync(string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            var currentRoles = new List<IdentityUserRole>();

            currentRoles.AddRange(user.Roles);
            foreach (IdentityUserRole role in currentRoles)
            {
                await _userManager.RemoveFromRoleAsync(userId, role.RoleId); // TODO: check it
            }
        }

        public void RemoveFromPermission(string userId, string permissionName)
        {
            _userManager.RemoveFromRole(userId, permissionName);
        }

        public async Task RemoveFromPermissionAsync(string userId, string permissionName)
        {
            await _userManager.RemoveFromRoleAsync(userId, permissionName);
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

        public Permission GetPermissionById(string permissionId)
        {
            return _context.Permissions.FirstOrDefault(p => p.Id == permissionId);
        }

        public async Task<Permission> GetPermissionByIdAsync(string permissionId)
        {
            return await _context.Permissions.FindAsync(permissionId);
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

        public async Task<int> CreateGroupAsync(string groupName, Guid companyId)
        {
            var newGroup = new Group(groupName, companyId);
            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();

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

        public async Task<int> CreateAdminGroupAsync(Guid companyId, string userId)
        {
            var adminGroupId = await CreateGroupAsync("Admin", companyId);

            var adminPermissions = new List<string>
            {
                "Create", 
                "Edit",
                "Delete"
            };

            foreach (var ad_p in adminPermissions)
                await AddPermissionToGroupAsync(adminGroupId, ad_p);

            await AddUserToCompanyGroupAsync(companyId, adminGroupId, userId);

            //await AddUserToGroupAsync(userId, adminGroupId);

            return adminGroupId;
        }

        public Group GetGroupById(int gropId)
        {
            return _context.Groups.FirstOrDefault(g => g.Id == gropId);
        }

        public async Task<Group> GetGroupByIdAsync(int gropId)
        {
            return await _context.Groups.FindAsync(gropId);
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

        public async Task ClearGroupUsersAsync(int groupId)
        {
            var group = await GetGroupByIdAsync(groupId);

            foreach (var user in group.Users)
            {
                await ClearUserPermissionsAsync(user.Id);
                await ClearUserCompaniesAsync(user.Id);
                await ClearUserGroupsAsync(user.Id);
            }

            group.Users.Clear();

            await _context.SaveChangesAsync();
        }

        public void ClearUserGroups(string userId)
        {
            ClearUserPermissions(userId);
            ApplicationUser user = _context.Users.Find(userId);
            user.Groups.Clear();
            _context.SaveChanges();
        }

        public async Task ClearUserGroupsAsync(string userId)
        {
            await ClearUserPermissionsAsync(userId);
            ApplicationUser user = await ((DbSet<ApplicationUser>) _context.Users).FindAsync(userId);
            user.Groups.Clear();
            await _context.SaveChangesAsync();
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

        public async Task AddUserToGroupAsync(string userId, int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var user = await ((System.Data.Entity.DbSet<ApplicationUser>)_context.Users).FindAsync(userId);

            var userGroup = new ApplicationUserGroup
            {
                Group = group,
                GroupId = group.Id,
                User = user,
                UserId = user.Id
            };

            foreach (var permission in group.Permissions)
                await _userManager.AddToRoleAsync(userId, permission.Permission.Name);

            user.Groups.Add(userGroup);
            await _context.SaveChangesAsync();
        }

        public void RemoveUserFromGroup(string userId, int groupId)
        {
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            ClearUserPermissions(userId);
            group.Users.Remove(user);

            _context.SaveChanges();
        }

        public async Task RemoveUserFromGroupAsync(string userId, int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var user = group.Users.FirstOrDefault(u => u.Id == userId);

            await ClearUserPermissionsAsync(userId);
            group.Users.Remove(user);

            await _context.SaveChangesAsync();
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

        public async Task ClearGroupPermissionsAsync(int groupId)
        {
            var group = _context.Groups.Find(groupId);
            var groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));

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
                        await RemoveFromPermissionAsync(user.Id, permission.Permission.Name);
                    }
                }
            }
            group.Permissions.Clear();
            await _context.SaveChangesAsync();
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

        public async Task<string> AddPermissionToGroupAsync(int groupId, string permissionName)
        {
            var group = _context.Groups.Find(groupId);

            if (_context.Permissions.Count() == 0)
                await CreatePermissionAsync(permissionName);
            else if (!PermissionExists(permissionName))
                await CreatePermissionAsync(permissionName);

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
                await _context.SaveChangesAsync();
            }

            // Add all of the users in this group to the new role:
            IQueryable<ApplicationUser> groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));
            foreach (ApplicationUser user in groupUsers)
            {
                if (!(_userManager.IsInRole(user.Id, permissionName)))
                {
                    await AddUserToPermissionAsync(user.Id, permission.Name);
                }
            }

            return permission.Id;
        }

        public void RemovePermissionFromGroup(int groupId, string permissionId)
        {
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);
            var permission = group.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);

            group.Permissions.Remove(permission);

            _context.SaveChanges();

            // Remove all of the users in this group to the role:
            IQueryable<ApplicationUser> groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));
            foreach (ApplicationUser user in groupUsers)
            {
                if (!(_userManager.IsInRole(user.Id, permissionId)))
                {
                    RemoveFromPermission(user.Id, permissionId); // TODO: check it
                }
            }
        }

        public async Task RemovePermissionFromGroupAsync(int groupId, string permissionId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var groupPermission = group.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);

            group.Permissions.Remove(groupPermission);

            await _context.SaveChangesAsync();

            var permission = await GetPermissionByIdAsync(permissionId);

            // Remove all of the users in this group to the role:
            IQueryable<ApplicationUser> groupUsers = _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));
            foreach (ApplicationUser user in groupUsers)
            {
                await RemoveFromPermissionAsync(user.Id, permission.Name); // TODO: check it
            }
        }

        public async Task UpdateGroupAsync(Group group)
        {
            _context.Entry(group).State = EntityState.Modified;

            await _context.SaveChangesAsync();                
        }

        public void DeleteGroup(int groupId)
        {
            var group = _context.Groups.Find(groupId);

            // Clear the roles from the group:
            ClearGroupPermissions(groupId);

            // remove all user from this group
            foreach (var user in group.Users)
                RemoveUserFromGroup(user.Id, groupId);

            _context.Groups.Remove(group);
            _context.SaveChanges();
        }

        public async Task DeleteGroupAsync(int id)
        {
            var group = await _context.Groups.FindAsync(id);

            // Clear the roles from the group:
            await ClearGroupPermissionsAsync(id);

            // remove all user from this group
            foreach (var user in group.Users)
                await RemoveUserFromGroupAsync(user.Id, id);

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Company
        public void CreateCompany(string companyName, string userId)
        {
            if (CompanyNameExist(companyName))
            {
                throw new GroupExistsException(
                    "A company by that name already exists in the database. Please choose another name.");
            }

            var company = new Company(companyName);
            _context.Companies.Add(company);
            _context.SaveChanges();

            var groupId = CreateAdminGroup(userId);

            AddGroupToCompany(company.Id, groupId);
        }

        public async Task<Guid> CreateCompanyAsync(string companyName, string userId)
        {
            if (CompanyNameExist(companyName))
            {
                throw new GroupExistsException(
                    "A company by that name already exists in the database. Please choose another name.");
            }

            var company = new Company(companyName);
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var groupId = await CreateAdminGroupAsync(company.Id, userId);

            AddGroupToCompanyAsync(company.Id, groupId);

            return company.Id;
        }

        public Company GetCompanyById(Guid companyId)
        {
            return _context.Companies.FirstOrDefault(c => c.Id == companyId);
        }

        public async Task<Company> GetCompanyByIdAsync(Guid companyID)
        {
            return await _context.Companies.FindAsync(companyID);
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

        public async void AddGroupToCompanyAsync(Guid companyId, int groupId)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == companyId);
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);

            company.Groups.Add(group);

            await _context.SaveChangesAsync();
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

        public async Task AddUserToCompanyGroupAsync(Guid companyId, int groupId, string userId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            var user = await ((DbSet<ApplicationUser>)_context.Users).FindAsync(userId);

            user.Companies.Add(new ApplicationUserCompany
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyId = companyId,
                User = user,
                Company = company
            });

            await AddUserToGroupAsync(userId, groupId);

            await _context.SaveChangesAsync();
        }

        public void RemoveUserFromCompanyGroup(Guid companyId, int groupId, string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var company = user.Companies.FirstOrDefault(c => c.CompanyId == companyId);

            user.Companies.Remove(company);

            RemoveUserFromGroup(userId, groupId);

            _context.SaveChanges();
        }

        public async Task RemoveUserFromCompanyGroupAsync(Guid companyId, int groupId, string userId)
        {
            var user = await GetUserById(userId);

            var company = user.Companies.FirstOrDefault(c => c.CompanyId == companyId);

            user.Companies.Remove(company);

            await RemoveUserFromGroupAsync(userId, groupId);

            await _context.SaveChangesAsync();
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

        public async Task DeleteCompanyAsync(Guid companyId)
        {
            var company = await GetCompanyByIdAsync(companyId);

            foreach (var group in company.Groups)
            {
                var groupId = group.Id;

                foreach (var user in group.Users)
                    await RemoveUserFromCompanyGroupAsync(companyId, groupId, user.Id);

                await ClearGroupPermissionsAsync(groupId);
            }

            _context.Companies.Remove(company);

            await _context.SaveChangesAsync();
        }

        public void ClearUserCompanies(string userId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            user.Companies.Clear();

            _context.SaveChanges();
        }

        public async Task ClearUserCompaniesAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            user.Companies.Clear();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            _context.Entry(company).State = EntityState.Modified;

            await _context.SaveChangesAsync(); 
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                GC.SuppressFinalize(this);
            }

        }

        public IEnumerable<Permission> GetAvaliablePermission()
        {
            return _context.Permissions;
        }

        public IEnumerable<Company> GetAllCompanies()
        {
            return _context.Companies;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return user;
        }
    }
}