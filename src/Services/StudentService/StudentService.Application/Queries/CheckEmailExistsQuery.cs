using MediatR;
using StudentService.Domain.Interfaces;

namespace StudentService.Application.Queries;

public record CheckEmailExistsQuery(string Email) : IRequest<bool>;

public class CheckEmailExistsQueryHandler : IRequestHandler<CheckEmailExistsQuery, bool>
{
    private readonly IStudentRepository _repository;

    public CheckEmailExistsQueryHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CheckEmailExistsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.EmailExistsAsync(request.Email, cancellationToken);
    }
}