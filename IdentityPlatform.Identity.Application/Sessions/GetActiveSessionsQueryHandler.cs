using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions
{
    public sealed class GetActiveSessionsQueryHandler
        : IQueryHandler<GetActiveSessionsQuery, IEnumerable<UserSessionDto>>
    {
        private readonly IUserSessionRepository _sessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetActiveSessionsQueryHandler(
            IUserSessionRepository sessionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _sessionRepository = sessionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<IEnumerable<UserSessionDto>, IDomainError>> Handle(
            GetActiveSessionsQuery request,
            CancellationToken cancellationToken)
        {
            var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);
            if (sessions == null)
            {
                return Result.Failure<IEnumerable<UserSessionDto>, IDomainError>(DomainError.NotFound("Sessions not found."));
            }

            // استخراج الـ sid الخاص بالجلسة الحالية من الـ Claims
            var currentSessionIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sid")?.Value;
            Guid.TryParse(currentSessionIdClaim, out var currentSessionGuid);

            // المقارنة لتحديد IsCurrent وبناء الـ DTOs
            var dtos = sessions.Select(s => new UserSessionDto(
                SessionId: s.Id,
                IpAddress: s.IpAddress,
                UserAgent: s.UserAgent,
                CreatedAt: s.CreatedAt,
                IsCurrent: currentSessionGuid != Guid.Empty && s.Id.Value == currentSessionGuid
            )).ToList();

            // إرجاع النتيجة بتغليف واحد نظيف تماماً مثل GetBasketQueryHandler
            return Result.Success<IEnumerable<UserSessionDto>, IDomainError>(dtos);
        }
    }
}
