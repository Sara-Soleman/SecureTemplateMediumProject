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
     : IRequestHandler<GetActiveSessionsQuery, Result<Result<IEnumerable<UserSessionDto>, IDomainError>, IDomainError>>
    {
        private readonly IUserSessionRepository _sessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor; // 1. حقن الـ HttpContextAccessor

        public GetActiveSessionsQueryHandler(
            IUserSessionRepository sessionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _sessionRepository = sessionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<Result<IEnumerable<UserSessionDto>, IDomainError>, IDomainError>> Handle(
            GetActiveSessionsQuery request,
            CancellationToken cancellationToken)
        {
            var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

            // 2. استخراج الـ sid الخاص بالجلسة الحالية من الـ Claims
            var currentSessionIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sid")?.Value;

            Guid.TryParse(currentSessionIdClaim, out var currentSessionGuid);

            // 3. المقارنة الديناميكية لتحديد IsCurrent
            var dtos = sessions.Select(s => new UserSessionDto(
                SessionId: s.Id,
                IpAddress: s.IpAddress,
                UserAgent: s.UserAgent,
                CreatedAt: s.CreatedAt,
                IsCurrent: currentSessionGuid != Guid.Empty && s.Id.Value == currentSessionGuid
            ));

            // تغليف مزدوج للـ Result ليتطابق مع بصمة MediatR الحالية في مشروعك
            Result<IEnumerable<UserSessionDto>, IDomainError> innerResult = Result.Success<IEnumerable<UserSessionDto>, IDomainError>(dtos);

            return Result.Success<Result<IEnumerable<UserSessionDto>, IDomainError>, IDomainError>(innerResult);
        }
    }
}
