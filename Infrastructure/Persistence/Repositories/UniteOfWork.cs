using Domain;
using Domain.Contract;
using Persistence.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class UniteOfWork : IUniteOfWork
    {
        private readonly ConcurrentDictionary<string ,object>_repositories;
        private readonly AppDbContext _appDbContext;

        public UniteOfWork(AppDbContext appDbContext)
        {
            _repositories = new();
            _appDbContext = appDbContext;
        }
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        => (IGenericRepository<TEntity, TKey>)_repositories.GetOrAdd(typeof(TEntity).Name, (_) => new GenericRepository<TEntity, TKey>(_appDbContext));

        public async Task SaveChangeAsync()
        =>await _appDbContext.SaveChangesAsync();
    }
}
