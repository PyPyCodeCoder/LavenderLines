using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogDomain.Model;
using BlogInfrastructure;
using BlogInfrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace BlogInfrastructure.Controllers
{
    [Authorize(Roles = "writer, reader")]
    public class CategoriesController : Controller
    {
        private readonly LldbContext _context;
        private readonly ArticlesDataPortServiceFactory _articlesDataPortServiceFactory;
        public CategoriesController(LldbContext context, ArticlesDataPortServiceFactory articlesDataPortServiceFactory)
        {
            _context = context;
            _articlesDataPortServiceFactory = articlesDataPortServiceFactory;
        }

        [Authorize(Roles = "reader")]
        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        [Authorize(Roles = "reader")]
        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            
            return RedirectToAction("Index", "Articles", new { id = category.Id, categoryName = category.Name });
        }

        [Authorize(Roles = "writer")]
        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "writer")]
        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [Authorize(Roles = "writer")]
        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [Authorize(Roles = "writer")]
        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        [Authorize(Roles = "writer")]
        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "writer")]
        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
            var articles = await _context.Articles
                .Where(a => a.CategoryId == id)
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
            
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize(Roles = "writer")]
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
        
        [Authorize(Roles = "writer")]
        [HttpGet]
        public IActionResult Import()
        {                                 
            return View();
        }

        [Authorize(Roles = "writer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel, CancellationToken cancellationToken = default)
        {                                 
            var importService = _articlesDataPortServiceFactory.GetImportService(fileExcel.ContentType);

            using var stream = fileExcel.OpenReadStream();
            
            await importService.ImportFromStreamAsync(stream, cancellationToken);

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize(Roles = "writer")]
        [HttpGet]
        public async Task<IActionResult> Export(int Id, [FromQuery] string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            CancellationToken cancellationToken = default)
        {
            var exportService = _articlesDataPortServiceFactory.GetExportService(contentType);

            var memoryStream = new MemoryStream();

            await exportService.WriteToAsync(Id, memoryStream, cancellationToken);

            await memoryStream.FlushAsync(cancellationToken);
            memoryStream.Position = 0;
            
            return new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = $"categories_{DateTime.UtcNow.ToShortDateString()}.xlsx"
            };
        }
    }
}
