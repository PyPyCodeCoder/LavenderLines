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
    [Authorize(Roles = "reader")]
    public class CommentsController : Controller
    {
        private readonly LldbContext _context;

        public CommentsController(LldbContext context)
        {
            _context = context;
        }

        // GET: Comments
        public async Task<IActionResult> Index(int? articleId)
        {
            if(articleId == null) return RedirectToAction("Index", "Articles");

            ViewBag.Id = articleId;
            var commentByArticle = _context.Comments.Where(c => c.ArticleId == articleId)
                .Include(c => c.Article)
                .Include(c => c.Reader);

            return View(await commentByArticle.ToListAsync());
        }
        
        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Article)
                .Include(c => c.Reader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewBag.Id = comment.ArticleId;

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create(int articleId)
        {
            ViewBag.Id = articleId;
            ViewBag.ArticleId = articleId;
            ViewData["ReaderId"] = new SelectList(_context.Readers, "Id", "Username");
            return View();
        }
        
        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int articleId, [Bind("Text,Data,Status,ReaderId,Id")] Comment comment)
        {
            comment.ArticleId = articleId;
            
            Article article = _context.Articles.Include(c => c.Comments).FirstOrDefault(c => c.Id == articleId);
            
            Category category = _context.Categories.Include(c => c.Articles).FirstOrDefault(c => c.Id == article.CategoryId);
            article.Category = category;
            ModelState.Clear();
            TryValidateModel(article);
            
            Writer writer = _context.Writers.Include(c => c.Articles).FirstOrDefault(c => c.Id == article.WriterId);
            article.Writer = writer;
            ModelState.Clear();
            TryValidateModel(article);
            
            comment.Article = article;
            ModelState.Clear();
            TryValidateModel(comment);
            
            Reader reader = _context.Readers.Include(c => c.Comments).FirstOrDefault(c => c.Id == comment.ReaderId);
            comment.Reader = reader;
            ModelState.Clear();
            TryValidateModel(comment);
            
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Comments", new {articleId = articleId});
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            
            return View(comment);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Text,Data,Status,ArticleId,ReaderId,Id")] Comment comment)
        {
            try
            {
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index", "Comments", new {articleId = comment.ArticleId});
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Article)
                .Include(c => c.Reader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewBag.Id = comment.ArticleId;

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
