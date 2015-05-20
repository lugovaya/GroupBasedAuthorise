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
        private readonly IdentityManager _manager = new IdentityManager();


        // GET: Groups
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
                    CompanyId = group.Group.CompanyId.ToString()
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
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: Groups/Create
        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Title");
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,CompanyId")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Groups.Add(group);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            //ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Title", group.CompanyId);
            return View(group);
        }

        // GET: Groups/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            //ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Title", group.CompanyId);
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,CompanyId")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Title", group.CompanyId);
            return View(group);
        }

        // GET: Groups/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Group group = await db.Groups.FindAsync(id);
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
