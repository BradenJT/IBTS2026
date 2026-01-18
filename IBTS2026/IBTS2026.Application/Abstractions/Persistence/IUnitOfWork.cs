using System;
using System.Collections.Generic;
using System.Text;

namespace IBTS2026.Application.Abstractions.Persistence
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
