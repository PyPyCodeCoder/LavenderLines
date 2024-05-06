using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BlogInfrastructure.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private record CountByYearResponseItem(string Year, int Count);
        private record CountByCategoryResponseItem(string Category, int Count);
        
        private record CountByArticleResponseItem(string ArticleId, int Count);
        
        private readonly LldbContext _context;
        
        public ChartsController(LldbContext context)
        {
            _context = context;
        }
 
        [HttpGet("countByYear")]
        public async Task<JsonResult> GetCountByYearAsync(CancellationToken cancellationToken)
        {
            var responseItems = await _context
                .Articles
                .GroupBy(article => article.Data.Year)
                .Select(group => new CountByYearResponseItem(group.Key.ToString(), group.Count()))
                .ToListAsync(cancellationToken);
 
            return new JsonResult(responseItems);     
        }
        
        [HttpGet("countByCategory")]
        public async Task<JsonResult> GetCountByCategoryAsync(CancellationToken cancellationToken)
        {
            var responseItems = await _context
                .Articles
                .GroupBy(article => article.Category)
                .Select(group => new CountByCategoryResponseItem(group.Key.Name, group.Count()))
                .ToListAsync(cancellationToken);
 
            return new JsonResult(responseItems);     
        }
        
        [HttpGet("countCommentsByArticle")]
        public async Task<JsonResult> GetCountCommentsByArticleAsync(CancellationToken cancellationToken)
        {
            var responseItems = await _context
                .Comments
                .GroupBy(comment => comment.ArticleId)
                .Select(group => new CountByArticleResponseItem(group.Key.ToString(), group.Count()))
                .ToListAsync(cancellationToken);

            return new JsonResult(responseItems);
        }
    }
}
