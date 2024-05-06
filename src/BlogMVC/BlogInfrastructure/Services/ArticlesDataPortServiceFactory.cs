using BlogDomain.Model;
namespace BlogInfrastructure.Services;

public class ArticlesDataPortServiceFactory : IDataPortServiceFactory<Category>
{
    private readonly LldbContext _context;
    
    public ArticlesDataPortServiceFactory(LldbContext context)
    {
        _context = context;
    }
    
    public IImportService<Category> GetImportService(string contentType)
    {
        if (contentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            return new ArticlesImportService(_context);
        }
        throw new NotImplementedException($"No import service implemented for articles with content type {contentType}");
    }
    
    public IExportService<Category> GetExportService(string contentType)
    {
        if (contentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            return new ArticlesExportService(_context);
        }
        throw new NotImplementedException($"No export service implemented for articles with content type {contentType}");
    }
}
