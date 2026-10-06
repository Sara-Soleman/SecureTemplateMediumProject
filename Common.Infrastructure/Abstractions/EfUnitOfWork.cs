using Common.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Infrastructure.Abstractions
{
    public sealed class EfUnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
    {
        private readonly TContext _context;

        public EfUnitOfWork(TContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
