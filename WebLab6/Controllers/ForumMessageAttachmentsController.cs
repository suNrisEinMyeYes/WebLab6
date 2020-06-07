using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebLab6.Data;
using WebLab6.Models;
using WebLab6.Models.ViewModels;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebLab6.Controllers
{
    public class ForumMessageAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IHostingEnvironment _hostingEnvironment;

        public ForumMessageAttachmentsController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: ForumMessageAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumMessageAttachments.Include(f => f.ForumMessage);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ForumMessageAttachments/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments
                .Include(f => f.ForumMessage)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null)
            {
                return NotFound();
            }

            return View(forumMessageAttachment);
        }

        // GET: ForumMessageAttachments/Create
        public async Task<IActionResult> Create(Guid? Id)
        {
            if (Id == null)
            {
                return this.NotFound();
            }

            var message = await this._context.ForumMessages
                .SingleOrDefaultAsync(x => x.Id == Id);

            if (message == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Message = message;
            return this.View(new ForumAttachmentCreateModel());
        }


        // POST: ForumMessageAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? Id, ForumAttachmentCreateModel model)
        {
            if (Id == null)
            {
                return this.NotFound();
            }

            var message = await this._context.ForumMessages
                .Include(w => w.ForumMessageAttachments)
                .SingleOrDefaultAsync(x => x.Id == Id);

            if (message == null)
            {
                return this.NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.FilePath.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            if (this.ModelState.IsValid)
            {
                var attachment = new ForumMessageAttachment
                {
                    Created = DateTime.Now,
                };


                var path = Path.Combine(_hostingEnvironment.WebRootPath, "attachments", attachment.Id.ToString("N") + fileExt);
                attachment.FilePath = $"/attachments/{attachment.Id:N}{fileExt}";
                attachment.FileName = fileName;
                using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.FilePath.CopyToAsync(fileStream);
                }


                message.ForumMessageAttachments.Add(attachment);

                this._context.Add(attachment);
                await this._context.SaveChangesAsync();
                return this.RedirectToAction("Index", new { forumMessageId = message.Id });
            }

            this.ViewBag.Hospital = message;
            return this.View(model);
        }


        // GET: ForumMessageAttachments/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments.SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null)
            {
                return NotFound();
            }
            ViewData["ForumMessageId"] = new SelectList(_context.ForumMessages, "Id", "ApplicationUserId", forumMessageAttachment.ForumMessageId);
            return View(forumMessageAttachment);
        }

        // POST: ForumMessageAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ForumMessageId,Created,FileName,FilePath")] ForumMessageAttachment forumMessageAttachment)
        {
            if (id != forumMessageAttachment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(forumMessageAttachment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ForumMessageAttachmentExists(forumMessageAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ForumMessageId"] = new SelectList(_context.ForumMessages, "Id", "ApplicationUserId", forumMessageAttachment.ForumMessageId);
            return View(forumMessageAttachment);
        }

        // GET: ForumMessageAttachments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments
                .Include(f => f.ForumMessage)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null)
            {
                return NotFound();
            }

            return View(forumMessageAttachment);
        }

        // POST: ForumMessageAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumMessageAttachment = await _context.ForumMessageAttachments.SingleOrDefaultAsync(m => m.Id == id);
            _context.ForumMessageAttachments.Remove(forumMessageAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ForumMessageAttachmentExists(Guid id)
        {
            return _context.ForumMessageAttachments.Any(e => e.Id == id);
        }
    }
}
