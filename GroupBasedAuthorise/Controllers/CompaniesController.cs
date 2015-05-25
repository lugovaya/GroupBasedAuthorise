using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GroupBasedAuthorise.DAL;
using GroupBasedAuthorise.Models.DataModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using GroupBasedAuthorise.Models;

namespace GroupBasedAuthorise.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly EntityManager _manager = new EntityManager();

        // GET: /Companies
        public async Task<ActionResult> Index()
        {
            List<CompanyViewModel> companies = await GetUserCompanies();

            if (companies.Count == 0)
                return RedirectToAction("Create", "Companies");

            return View(companies);
        }

        // GET: /Companies/Create
        [Authorize(Roles="Create, Delete")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Create, Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CompanyViewModel company)
        {
            if (ModelState.IsValid)
            {
                await _manager.CreateCompanyAsync(company.CompanyName, User.Identity.GetUserId());
                return RedirectToAction("Index");
            }

            return View(company);
        }


        // GET: /Companies/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var company = (CompanyViewModel) await _manager.GetCompanyByIdAsync(Guid.Parse(id));

            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // GET: Company/Edit/5
        [Authorize(Roles = "Edit, Create, Delete")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();

            var domainCompany = await _manager.GetCompanyByIdAsync(Guid.Parse(id));

            var company = (CompanyViewModel) domainCompany;

            foreach (var group in company.CompanyGroups)
                foreach (var permission in group.Permissions)
                    permission.Checked = await _manager.HasUserPermissionAsync(userId, permission.Name);

            if (company == null)
            {
                return HttpNotFound();
            }

            return View(company);
        }

        // POST: Company/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Edit, Create, Delete")]        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CompanyViewModel company)
        {
            if (ModelState.IsValid)
            {
                var domainCompany = await _manager.GetCompanyByIdAsync(Guid.Parse(company.CompanyId));

                domainCompany.Title = company.CompanyName;

                await _manager.UpdateCompanyAsync(domainCompany);

                return RedirectToAction("Index");
            }
            return View(company);
        }

        // GET: Company/Delete/5
        [Authorize(Roles = "Delete")]        
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var company = await _manager.GetCompanyByIdAsync(Guid.Parse(id));

            if (company == null)
            {
                return HttpNotFound();
            }
            
            return View(company);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            await _manager.DeleteCompanyAsync(Guid.Parse(id));

            return RedirectToAction("Index");
        }
        
        private async Task<List<CompanyViewModel>> GetUserCompanies()
        {
            var userId = User.Identity.GetUserId();
            var user = await _manager.GetUserById(userId);

            var companies = new List<CompanyViewModel>();

            foreach (var comp in user.Companies)
            {
                var company = new CompanyViewModel();

                var domainCompany = _manager.GetCompanyById(comp.CompanyId);

                company.CompanyId = domainCompany.Id.ToString();
                company.CompanyName = domainCompany.Title;

                foreach (var group in domainCompany.Groups)
                {
                    var permissions = new List<PermissionViewModel>();

                    foreach (var perm in group.Permissions)
                    {
                        var domainPermission = _manager.GetPermissionById(perm.PermissionId);

                        permissions.Add(new PermissionViewModel
                        {
                            Name = domainPermission.Name,
                            Description = domainPermission.Description,
                            Checked = _manager.HasUserPermission(userId, domainPermission.Name)
                        });
                    }

                    company.CompanyGroups.Add(new GroupViewModel
                    {
                        GroupName = group.Name,
                        Permissions = permissions
                    });
                }
                companies.Add(company);
            }

            return companies;
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
