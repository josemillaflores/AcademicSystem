using AcademicSystem.Common.Entities;
using CourseService.Domain.Entities;

namespace CourseService.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetAvailableCoursesAsync(CancellationToken cancellationToken = default);
}

 
