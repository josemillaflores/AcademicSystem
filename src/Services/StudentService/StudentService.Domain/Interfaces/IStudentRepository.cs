using StudentService.Domain.Entities;
using StudentService.Domain.Enums;
using System.Linq.Expressions;

namespace StudentService.Domain.Interfaces;

/// <summary>
/// Repository Pattern - Interfaz específica para Student
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetActiveStudentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetStudentsByStatusAsync(EnrollmentStatus status, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository Pattern - Interfaz genérica base
/// </summary>
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

/// <summary>
/// Unit of Work Pattern
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}