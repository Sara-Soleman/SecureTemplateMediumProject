using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions.CQRS
{
    public interface ICommandHandler<TRequest, TResponse, TUnitOfWork> : IRequestHandler<TRequest, Result<TResponse, IDomainError>>
        where TRequest : ICommand<TResponse>
        where TResponse : notnull
        where TUnitOfWork : IUnitOfWork
    { }

    public interface ICommandHandler<TRequest> : IRequestHandler<TRequest, Result<Unit>>
        where TRequest : ICommand
    { }
}
