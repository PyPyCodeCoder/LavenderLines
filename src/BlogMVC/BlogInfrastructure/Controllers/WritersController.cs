using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogDomain.Model;
using BlogInfrastructure;

namespace BlogInfrastructure.Controllers
{
    public class WritersController : Controller
    {
        private readonly LldbContext _context;

        public WritersController(LldbContext context)
        {
            _context = context;
        }

        // GET: Writers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Writers.ToListAsync());
        }

        // GET: Writers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var writer = await _context.Writers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (writer == null)
            {
                return NotFound();
            }

            return View(writer);
        }

        // GET: Writers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Writers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Id")] Writer writer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(writer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(writer);
        }

        // GET: Writers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var writer = await _context.Writers.FindAsync(id);
            if (writer == null)
            {
                return NotFound();
            }
            return View(writer);
        }

        // POST: Writers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Username,Id")] Writer writer)
        {
            if (id != writer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(writer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WriterExists(writer.Id))
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
            return View(writer);
        }

        // GET: Writers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var writer = await _context.Writers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (writer == null)
            {
                return NotFound();
            }

            return View(writer);
        }

        // POST: Writers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var writer = await _context.Writers.FindAsync(id);
            if (writer == null)
            {
                return NotFound();
            }
            
            var articles = await _context.Articles
                .Where(a => a.WriterId == id)
                .ToListAsync();

            foreach (var article in articles)
            {
                var comments = await _context.Comments
                    .Where(c => c.ArticleId == article.Id)
                    .ToListAsync();
                foreach (var comment in comments)
                {
                    _context.Comments.Remove(comment);
                }
                
                _context.Articles.Remove(article);
            }
            
            _context.Writers.Remove(writer);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool WriterExists(int id)
        {
            return _context.Writers.Any(e => e.Id == id);
        }
        
        public async Task<IActionResult> AddWriter(string username, string userId)
        {
            var existingWriter = await _context.Writers.FirstOrDefaultAsync(w => w.UserId == userId);
            if (existingWriter == null)
            {
                Writer writer = new Writer { Username = username, UserId = userId };
                _context.Writers.Add(writer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> RemoveWriter(string userId)
        {
            var writer = await _context.Writers.FirstOrDefaultAsync(w => w.UserId == userId);
            if (writer != null)
            {
                var articles = await _context.Articles.Where(a => a.WriterId == writer.Id).ToListAsync();
                foreach (var article in articles)
                {
                    var comments = await _context.Comments.Where(c => c.ArticleId == article.Id).ToListAsync();
                    
                    foreach (var comment in comments)
                    {
                        _context.Comments.Remove(comment);
                    }
                    
                    _context.Articles.Remove(article);
                }
                
                _context.Writers.Remove(writer);

                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
