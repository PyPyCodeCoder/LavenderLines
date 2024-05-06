using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogDomain.Model;
using BlogInfrastructure;
using Microsoft.AspNetCore.Authorization;

namespace BlogInfrastructure.Controllers
{
    [Authorize(Roles = "writer, reader")]
    public class ArticlesController : Controller
    {
        private readonly LldbContext _context;

        public ArticlesController(LldbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "reader")]
        // GET: Articles
        public async Task<IActionResult> Index(int? id, string? categoryName)
        {
            if(id == null) return RedirectToAction("Index", "Categories");

            ViewBag.Id = id;
            ViewBag.Name = categoryName;
            
            ViewBag.CategoryId = id;
            ViewBag.CategoryName = categoryName;
            var articleByCategory = _context.Articles.Where(a => a.CategoryId == id)
                .Include(a => a.Category)
                .Include(a => a.Writer);

            return View(await articleByCategory.ToListAsync());
        }

        [Authorize(Roles = "reader")]
        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Writer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            ViewBag.Id = article.Id;
            ViewBag.CategoryId = article.CategoryId;
            ViewBag.CategoryName = _context.Categories.Where(c => c.Id == article.CategoryId).FirstOrDefault()?.Name;
            
            return RedirectToAction("Index", "Comments", new { articleId = id});
        }
        
        [Authorize(Roles = "writer")]
        // GET: Articles/Create
        public IActionResult Create(int id)
        {
            ViewBag.CategoryId = id;
            ViewBag.CategoryName = _context.Categories.Where(c => c.Id == id).FirstOrDefault()?.Name;
            ViewData["WriterId"] = new SelectList(_context.Writers, "Id", "Username");
            return View();
        }
        
        [Authorize(Roles = "writer")]
        // POST: Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, [Bind("Title,Text,Data,Status,WriterId,Id")] Article article)
        {
            article.Id = 0;
            article.CategoryId = id;
            
            Category category = _context.Categories.Include(c => c.Articles).FirstOrDefault(c => c.Id == id) ?? throw new InvalidOperationException();
            article.Category = category;
            ModelState.Clear();
            TryValidateModel(article);
            
            Writer writer = _context.Writers.Include(c => c.Articles).FirstOrDefault(c => c.Id == article.WriterId) ?? throw new InvalidOperationException();
            article.Writer = writer;
            ModelState.Clear();
            TryValidateModel(article);
            
            _context.Add(article);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Articles", new {id = id, categoryName = _context.Categories.Where(c => c.Id == id).FirstOrDefault().Name });
        }
        
        [Authorize(Roles = "writer")]
        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            
            ViewBag.CategoryId = article.CategoryId;
            ViewBag.CategoryName = _context.Categories
                .Where(c => c.Id == article.CategoryId)
                .Select(c => c.Name)
                .FirstOrDefault();
            return View(article);
        }

        [Authorize(Roles = "writer")]
        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Text,Data,Status,CategoryId,WriterId,Id")] Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }
            
            try
            {
                _context.Update(article);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(article.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            var categoryId = article.CategoryId;
            var categoryName = _context.Categories
                .Where(c => c.Id == article.CategoryId)
                .Select(c => c.Name)
                .FirstOrDefault();
            
            return RedirectToAction("Index", "Articles", new { id = categoryId, categoryName = categoryName });
        }

        [Authorize(Roles = "writer")]
        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Writer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            
            ViewBag.CategoryId = article.CategoryId;
            ViewBag.CategoryName = _context.Categories.Where(c => c.Id == article.CategoryId).FirstOrDefault()?.Name;

            return View(article);
        }

        [Authorize(Roles = "writer")]
        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            
            var categoryId = article.CategoryId;
            var categoryName = _context.Categories
                .Where(c => c.Id == article.CategoryId)
                .Select(c => c.Name)
                .FirstOrDefault();
            
            var comments = await _context.Comments
                .Where(c => c.ArticleId == id)
                .ToListAsync();
            
            foreach (var comment in comments)
            {
                _context.Comments.Remove(comment);
            }
            
            _context.Articles.Remove(article);
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Articles", new { id = categoryId, categoryName = categoryName });
        }

        [Authorize(Roles = "writer")]
        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
