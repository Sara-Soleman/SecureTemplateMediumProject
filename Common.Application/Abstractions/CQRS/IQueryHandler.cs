using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions.CQRS
{
    public interface IQueryHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse, IDomainError>>
       where TRequest : IQuery<TResponse>
       where TResponse : notnull
    { }
}
