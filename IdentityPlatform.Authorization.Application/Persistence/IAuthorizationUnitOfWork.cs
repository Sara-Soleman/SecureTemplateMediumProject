using Common.Application.Abstractions;

namespace IdentityPlatform.Authorization.Application.Persistence
{
    // Context-specific UnitOfWork interface which extends the common IUnitOfWork
    public interface IAuthorizationUnitOfWork : IUnitOfWork { }
}
