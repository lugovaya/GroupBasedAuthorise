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

        public bool PermissionExists(string name)
        {
            return _permissionManager.RoleExists(name);
        }

        public IdentityResult CreatePermission(string name, string description = "")
        {
            // Swap ApplicationRole for IdentityRole:
            return _permissionManager.Create(new Permission(name, description));
        }

        public IdentityResult CreateUser(ApplicationUser user, string password)
        {
            return _userManager.Create(user, password);
        }

        public IdentityResult AddUserToPermission(string userId, string roleName)
        {
            return _userManager.AddToRole(userId, roleName);
        }


        public void ClearUserRoles(string userId)
        {
            ApplicationUser user = _userManager.FindById(userId);
            var currentRoles = new List<IdentityUserRole>();

            currentRoles.AddRange(user.Roles);
            foreach (IdentityUserRole role in currentRoles)
            {
                _userManager.RemoveFromRole(userId, role.RoleId); // TODO: check it
            }
        }

        public void RemoveFromRole(string userId, string roleName)
        {
            _userManager.RemoveFromRole(userId, roleName);
        }

        public void DeleteRole(string roleId)
        {
            IQueryable<ApplicationUser> roleUsers = _context.Users.Where(u => u.Roles.Any(r => r.RoleId == roleId));
            var permission = _context.Permissions.Find(roleId);

            foreach (ApplicationUser user in roleUsers)
            {
                RemoveFromRole(user.Id, permission.Name);
            }
            _context.Permissions.Remove(permission);
            _context.SaveChanges();
        }

        public void CreateGroup(string groupName)
        {
            if (GroupNameExists(groupName))
            {
                throw new GroupExistsException(
                    "A group by that name already exists in the database. Please choose another name.");
            }

            var newGroup = new Group(groupName);
            _context.Groups.Add(newGroup);
            _context.SaveChanges();
        }

        public bool GroupNameExists(string groupName)
        {
            return _context.Groups.Any(gr => gr.Name == groupName);
        }


        public void ClearUserGroups(string userId)
        {
            ClearUserRoles(userId);
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

        public void ClearGroupRoles(int groupId)
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
                        RemoveFromRole(user.Id, permission.Permission.Name);
                    }
                }
            }
            group.Permissions.Clear();
            _context.SaveChanges();
        }

        public void AddRoleToGroup(int groupId, string roleName)
        {
            var group = _context.Groups.Find(groupId);
            var permission = _context.Permissions.First(r => r.Name == roleName);

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
                if (!(_userManager.IsInRole(user.Id, roleName)))
                {
                    AddUserToPermission(user.Id, permission.Name);
                }
            }
        }

        public void DeleteGroup(int groupId)
        {
            Group group = _context.Groups.Find(groupId);

            // Clear the roles from the group:
            ClearGroupRoles(groupId);
            _context.Groups.Remove(group);
            _context.SaveChanges();
        }
    }
}