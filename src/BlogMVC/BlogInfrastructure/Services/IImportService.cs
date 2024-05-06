using BlogDomain.Model;

namespace BlogInfrastructure.Services
{
    public interface IImportService<TEntity>
    where TEntity : Entity
    {
        Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken);
    }
}
