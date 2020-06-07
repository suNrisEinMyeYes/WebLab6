using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebLab6.Data;
using WebLab6.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebLab6.Controllers
{
    public class MockupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MockupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return this.View();
        }
     
        public async Task<IActionResult> AllForums()
        {
            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators)) ViewBag.Can = true;
            else ViewBag.Can = false;
            return this.View(await this._context.ForumCategorys.Include(f => f.Forums)
                /*.Include("Forums.ForumTopics")*/
                .ThenInclude(f => f.ForumTopics)
                .ToListAsync());
        }

        public IActionResult SingleForum()
        {
            return this.View();
        }

        public IActionResult SingleTopic()
        {
            return this.View();
        }
    }
}
