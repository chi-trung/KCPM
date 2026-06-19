namespace WastePlatform.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface — coordinates the persistence of
/// changes made across multiple repositories in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The number of state entries written to the store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
