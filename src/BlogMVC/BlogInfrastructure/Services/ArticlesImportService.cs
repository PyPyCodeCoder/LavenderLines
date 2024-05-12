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
        
        private static readonly IReadOnlyList<string> HeaderNames =
            new string[]
            {
                "Назва",
                "Текст",
                "Дата",
                "Статус",
                "Категорія",
                "Автор",
            };

        public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Дані не можуть бути прочитані", nameof(stream));
            }
            
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(stream))
                {
                    foreach (IXLWorksheet worksheet in workBook.Worksheets)
                    {
                        
                        CheckHeaders(worksheet);
                        
                        foreach (var row in worksheet.RowsUsed().Skip(1))
                        {
                            var category_name = row.Cell(5).Value.ToString();
                            if (String.IsNullOrEmpty(category_name))
                            {
                                throw new Exception("Категорія не може бути пустою");
                            }
                            
                            Category category = await _context.Categories
                                .FirstOrDefaultAsync(cat => cat.Name == category_name, cancellationToken);
                            
                            if (category == null)
                            {
                                category = new Category();
                                category.Name = category_name;
                                _context.Categories.Add(category);
                            }
                            
                            await AddArticleAsync(row, cancellationToken, category);
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка імпорту: {ex.Message}");
            }
        }
        
        private void CheckHeaders(IXLWorksheet worksheet)
        {
            var headers = worksheet.Row(1).Cells().Select(cell => cell.Value.ToString()).ToList();

            for (int i = 0; i < HeaderNames.Count; i++)
            {
                if (i >= headers.Count || HeaderNames[i] != headers[i])
                {
                    throw new Exception($"Не вдалося знайти на потрібній позиції заголовок '{HeaderNames[i]}' в Excel файлі.");
                }
            }
        }
        
        private async Task AddArticleAsync(IXLRow row, CancellationToken cancellationToken, Category category)
        {
            string title = GetArticleTitle(row);
            string text = GetArticleText(row);
            string writerUsername = row.Cell(6).Value.ToString();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(writerUsername))
            {
                throw new Exception("Усі поля повинні бути заповнені");
            }
            
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Помилка додавання статті: {ex.Message}");
            }
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
            if (writer is null) //we'll use admin's user for that
            {
                var admin = _context.Writers
                    .Include(w => w.User)
                    .FirstOrDefault(w => w.Username == "Toretto");
                
                writer = new Writer();
                writer.Username = writer_username;
                writer.UserId = admin.UserId;
                
                _context.Writers.Add(writer);
            }

            return writer;
        }
    }
}
