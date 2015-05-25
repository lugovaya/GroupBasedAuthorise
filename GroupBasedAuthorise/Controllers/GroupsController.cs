using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GroupBasedAuthorise.DAL;
using GroupBasedAuthorise.Models.DataModels;
using GroupBasedAuthorise.Models;

namespace GroupBasedAuthorise.Controllers
{
    public class GroupsController : Controller
    {
        private readonly EntityManager _manager = new EntityManager();

        // GET: Groups
        [Authorize(Roles = "Edit, Create, Delete")]
        public async Task<ActionResult> Index()
        {
            var groups = await GetUserGroups();
            return View(groups);
        }

        private async Task<List<GroupViewModel>> GetUserGroups()
        {
            var userId = User.Identity.GetUserId();
            var user = await _manager.GetUserById(userId);

            var groups = new List<GroupViewModel>();

            foreach (var group in user.Groups)
            {
                var viewGroup = new GroupViewModel
                {
                    GroupId = group.GroupId,
                    GroupName = group.Group.Name,
                    CompanyId = group.Group.CompanyId.ToString(),
                    CompanyName = group.Group.Company.Title
                };

                foreach (var permission in group.Group.Permissions)
                    viewGroup.Permissions.Add(new PermissionViewModel
                        {
                            Name = permission.Permission.Name,
                            Description = permission.Permission.Description,
                            PermissionId = permission.PermissionId,
                            Checked = _manager.HasUserPermission(userId, permission.Permission.Name)
                        });
                groups.Add(viewGroup);
            }

            return groups;
        }

        // GET: Groups/Details/5
        [Authorize(Roles = "Edit, Create, Delete")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var group = (GroupViewModel)await _manager.GetGroupByIdAsync(Convert.ToInt32(id));
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: Groups/CreateCompanyGroup
        [Authorize(Roles = "Create, Delete")]
        public ActionResult CreateCompanyGroup(string id)
        {
            var group = new GroupViewModel
            {
                CompanyId = id
            };

            foreach (var avaliablePermission in _manager.GetAvaliablePermission())
                group.Permissions.Add((PermissionViewModel)avaliablePermission);

            return View("Create", group);
        }

        // GET: Groups/Create
        [Authorize(Roles = "Create, Delete")]
        public ActionResult Create()
        {
            var group = new GroupViewModel();

            var avaliableCompanies = _manager.GetAllCompanies();

            ViewBag.CompanyId = new SelectList(avaliableCompanies, "Id", "Title", group.CompanyId);

            foreach (var avaliablePermission in _manager.GetAvaliablePermission())
                group.Permissions.Add((PermissionViewModel)avaliablePermission);

            return View(group);
        }

        // POST: Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Create, Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(GroupViewModel group)
        {
            if (ModelState.IsValid)
            {
                var companyId = Guid.Parse(group.CompanyId);
                var userId = User.Identity.GetUserId();

                // create new group
                var groupId = await _manager.CreateGroupAsync(group.GroupName, companyId);

                // add to this group yourself
                await _manager.AddUserToCompanyGroupAsync(companyId, groupId, userId);

                // add to the group checked permission
                foreach (var permission in group.Permissions)
                    if (permission.Checked)
                        await _manager.AddPermissionToGroupAsync(groupId, permission.Name);

                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: Groups/Edit/5
        [Authorize(Roles = "Edit, Create, Delete")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var group = (GroupViewModel) await _manager.GetGroupByIdAsync(Convert.ToInt32(id));

            var avaliablePermission = _manager.GetAvaliablePermission();

            // checked permission
            foreach (var permission in group.Permissions)
                permission.Checked = true;

            // unchecked permission
            foreach (var permission in avaliablePermission)
                if (group.Permissions.FirstOrDefault(x => x.PermissionId == permission.Id) == null)
                    group.Permissions.Add((PermissionViewModel)permission);

            if (group == null)
            {
                return HttpNotFound();
            }
            
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Edit, Create, Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(GroupViewModel group)
        {
            if (ModelState.IsValid)
            {
                var domainGroup = await _manager.GetGroupByIdAsync(group.GroupId);
                domainGroup.Name = group.GroupName;
                
                // remain only checked permission
                foreach (var permission in group.Permissions.ToList())
                    if (!permission.Checked)
                        group.Permissions.Remove(permission);

                // remove from domainGroup permissions that are not in viewGroup permission
                foreach (var permission in domainGroup.Permissions.ToList())
                    if (group.Permissions.FirstOrDefault(p => p.PermissionId == permission.PermissionId) == null)
                        await _manager.RemovePermissionFromGroupAsync(group.GroupId, permission.PermissionId);

                // add to domainGroup permissions that haven't been in it yet
                foreach (var permission in group.Permissions)
                    if (domainGroup.Permissions.FirstOrDefault(p => p.PermissionId == permission.PermissionId) == null)
                        await _manager.AddPermissionToGroupAsync(group.GroupId, permission.Name);

                // update new domainGroup in _context
                await _manager.UpdateGroupAsync(domainGroup);

                return RedirectToAction("Index");
            }
            
            return View(group);
        }

        // GET: Groups/Delete/5
        [Authorize(Roles = "Delete")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var group = (GroupViewModel) await _manager.GetGroupByIdAsync(Convert.ToInt32(id));
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [Authorize(Roles = "Delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _manager.DeleteGroupAsync(id);
            return RedirectToAction("Index");
        }

        [Authorize(Roles="AddUsers")]
        public ActionResult AddUsers(int id)
        {
            var usersModel = new AddUserToGroupViewModel();

            usersModel.GroupId = id;

            return View(usersModel);
        }

        [Authorize(Roles = "AddUsers")]
        [HttpPost]
        public async Task<ActionResult> AddUsers(AddUserToGroupViewModel users)
        {
            if (ModelState.IsValid)
            {
                var emails = users.UsersEmails.Split(',');

                foreach (var email in emails)
                {
                    var user = await _manager.GetUserByEmailAsync(email.Trim());

                    await _manager.AddUserToGroupAsync(user.Id, users.GroupId);
                }

                return RedirectToAction("Index");
            }

            return View(users);
        }

        [Authorize(Roles = "Edit, Create, Delete")]
        public ActionResult SendInvites(int id)
        {
            var usersModel = new AddUserToGroupViewModel();

            usersModel.GroupId = id;

            return View(usersModel);
        }

        [Authorize(Roles = "Edit, Create, Delete")]
        [HttpPost]
        public ActionResult SendInvites(AddUserToGroupViewModel users)
        {
            if (ModelState.IsValid)
            {
                var emails = users.UsersEmails.Split(',');

                foreach (var user in emails)
                {
                    // TODO: generate link for user invite from email
                }

                return RedirectToAction("Index");
            }

            return View(users);
        }

        public async Task<ActionResult> InviteUser(int id, string userEmail)
        {
            var user = await _manager.GetUserByEmailAsync(userEmail);

            if (user == null)
                return RedirectToAction("Register", "Account", null);

            await _manager.AddUserToGroupAsync(user.Id, id);

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account", null);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _manager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
