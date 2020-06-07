using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebLab6.Data;
using WebLab6.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace WebLab6.Controllers
{
    [Authorize]
    public class FoldersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;

        public FoldersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }



        // GET: Folders
   
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var index = _context.Folders.Where(e => e.FolderId == null && e.ApplicationUserId == userId).ToList();
            var admin_index = _context.Folders.Where(e => e.FolderId == null).ToList();
            if (User.IsInRole(ApplicationRoles.Administrators))
                return View(admin_index);
            return View(index);
        }

        // GET: Folders/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var folder = _context.Folders.Include(e=> e.Files).Include(e => e.Folders).SingleOrDefault(e => e.Id == id);
            ViewBag.Path = GetPath(id);
            ViewBag.Can = Conform(id);
            return View(folder);
        }

        // GET: Folders/Create
        public IActionResult Create()
        {
            var model = new FolderViewModel();
            return View(model);
        }

        // POST: Folders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FolderViewModel model)
        {
            if (!ModelState.IsValid)
            { return View(model); }

            var folder = new Folder {
                Name = model.Name,
                ApplicationUserId = _userManager.GetUserId(User)
            };
                _context.Add(folder);
                _context.SaveChanges();
                return RedirectToAction("Index");     
        }

        // GET: Folders/Create
        public IActionResult Create2(Guid? id)
        {
            var model = new FolderViewModel { FolderId = id};
            return View(model);
        }

        // POST: Folders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(FolderViewModel model, Guid? id)
        {
            if (!ModelState.IsValid)
            { return View(model); }

            var folder = new Folder
            {
                FolderId = id,
                Name = model.Name,
                ApplicationUserId = _userManager.GetUserId(User)
            };
            _context.Add(folder);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Folders/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.Id = id;
            return View(new FolderViewModel());
        }

        // POST: Folders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FolderViewModel model, Guid? id)
        {
            if (!ModelState.IsValid)
            { return View(model); }

            var folder = _context.Folders.SingleOrDefault(e => e.Id == id);
            folder.Name = model.Name;
            _context.SaveChanges();
            if (folder.FolderId != null)
            return RedirectToAction("Details",new { id = folder.FolderId});
            return RedirectToAction("Index");
        }

        // GET: Folders/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var folder = _context.Folders.Include(e => e.Folders).Include(e => e.Files).SingleOrDefault(e => e.Id == id);
            return View(folder);
        }

        // POST: Folders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid? id)
        {
            var folder = _context.Folders.Include(e => e.Folders).Include(e => e.Files).SingleOrDefault(e => e.Id == id);
            _context.Folders.Remove(folder);
            _context.SaveChanges();
            if(folder.FolderId != null) return RedirectToAction("Details", new { id = folder.FolderId });
            return RedirectToAction("Index");
        }

        private bool FolderExists(Guid id)
        {
            return _context.Folders.Any(e => e.Id == id);
        }

        private List<Tuple<Guid?, String>> GetPath(Guid? id)
        {
               var folder = _context.Folders.SingleOrDefault(e => e.Id == id);
            var kor = new List<Tuple<Guid?, String>> { new Tuple<Guid?, String>(id, folder.Name) };
            while (folder.FolderId != null)
            {
                folder = _context.Folders.SingleOrDefault(e => e.Id == folder.FolderId);
                kor.Add(new Tuple<Guid?, String>(folder.Id, folder.Name)); 
            }
            kor.Reverse();
            return kor;
        }

        private bool Conform(Guid? id)
        {
            var folder = _context.Folders.SingleOrDefault(e => e.Id == id);
            int count = 0;
            while (folder.FolderId != null)
            {
                folder = _context.Folders.SingleOrDefault(e => e.Id == folder.FolderId);
                count++;
            }
            if (count == 0) return true;
            return false;
        }

    }
}
