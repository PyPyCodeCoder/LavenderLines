using ClosedXML.Excel;
using BlogDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace BlogInfrastructure.Services
{
    public class ArticlesImportService : IImportService<Category>
    {
        private readonly LldbContext _context;

        public ArticlesImportService(LldbContext context)
        {
            _context = context;
        }

        public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Дані не можуть бути прочитані", nameof(stream));
            }

            using (XLWorkbook workBook = new XLWorkbook(stream))
            {
                foreach (IXLWorksheet worksheet in workBook.Worksheets)
                {
                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        var category_name = row.Cell(5).Value.ToString() ?? String.Empty;
                        Category category = await _context.Categories
                            .FirstOrDefaultAsync(cat => cat.Name == category_name, cancellationToken);
                        if (category == null)
                        {
                            category = new Category();
                            category.Name = category_name;
                            _context.Categories.Add(category);
                        }
                        
                        await AddArticleAsync(row, cancellationToken, category);
                    }
                }
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task AddArticleAsync(IXLRow row, CancellationToken cancellationToken, Category category)
        {
            Article article = new Article();
            
            article.Title = GetArticleTitle(row);
            article.Text = GetArticleText(row);
            article.Data = GetArticleData(row);
            article.Status = GetArticleStatus(row);
            article.Category = category;
            article.Writer = await GetOrCreateArticleWriterAsync(row, cancellationToken);

            _context.Articles.Add(article);
        }
        
        private static string GetArticleTitle(IXLRow row)
        {
            return row.Cell(1).Value.ToString();
        }
        
        private static string GetArticleText(IXLRow row)
        {
            return row.Cell(2).Value.ToString();
        }
        
        private static DateTime GetArticleData(IXLRow row)
        {
            DateTime date = DateTime.Parse(row.Cell(3).Value.ToString());
            return date;
        }
        
        private static int GetArticleStatus(IXLRow row)
        {
            int status = int.Parse(row.Cell(4).Value.ToString());
            return status;
        }
        
        private async Task<Writer> GetOrCreateArticleWriterAsync(IXLRow row, CancellationToken cancellationToken)
        {
            var writer_username = row.Cell(6).Value.ToString();

            Writer writer = await _context.Writers.FirstOrDefaultAsync(wr => wr.Username == writer_username, cancellationToken);
            if (writer is null)
            {
                writer = new Writer();
                writer.Username = writer_username;
                writer.UserId = "142a9e91-995c-43b9-a5e4-3dbfd8851662";//adjust correctly
                
                _context.Writers.Add(writer);
            }

            return writer;
        }
    }
}
