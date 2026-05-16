using CourseService.Domain.Entities;
using System.Linq.Expressions;

namespace CourseService.Domain.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetAvailableCoursesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetCoursesByCreditsRangeAsync(int minCredits, int maxCredits, CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}