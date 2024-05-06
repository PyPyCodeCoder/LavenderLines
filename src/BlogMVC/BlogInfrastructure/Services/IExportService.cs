using BlogDomain.Model;

namespace BlogInfrastructure.Services
{
    public interface IExportService<TEntity>
    where TEntity : Entity
    {
        Task WriteToAsync(int clarifier, Stream stream, CancellationToken cancellationToken);
    }

}
