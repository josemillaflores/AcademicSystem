using TeacherService.Domain.Entities;
using System.Linq.Expressions;
using AcademicSystem.Common;

namespace TeacherService.Domain.Interfaces;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<Teacher?> GetByTeacherNumberAsync(string teacherNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Teacher>> GetActiveTeachersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Teacher>> GetTeachersBySpecialtyAsync(Guid specialtyId, CancellationToken cancellationToken = default);
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