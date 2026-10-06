using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions
{
    //public interface IUnitOfWork : IDisposable
    //{
    //    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    //}

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
