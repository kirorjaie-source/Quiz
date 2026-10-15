using Domain;
using Domain.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        private readonly AppDbContext _appDbContext;

        public GenericRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task AddAsync(TEntity entity)
        =>await _appDbContext.AddAsync(entity);

        public void DeleteAsync(TEntity entity)
        =>_appDbContext.Remove(entity);
        public async Task<IEnumerable<TEntity>> GetAllAsync(bool AsNoTracking = false)
       =>AsNoTracking?await _appDbContext.Set<TEntity>().ToListAsync():
            await _appDbContext.Set<TEntity>().AsNoTracking().ToListAsync();

        public async Task<TEntity?> GetAsync(TKey id)
        => await _appDbContext.Set<TEntity>().FindAsync(id);       

        public void UpdateAsync(TEntity entity)
        =>_appDbContext.Update(entity);
    

        public async Task<IEnumerable<TEntity>> GetAllAsync(ISpecifications<TEntity, TKey> spec) =>
            await SpecificationsEvaluator.GetQuery(_appDbContext.Set<TEntity>(), spec).ToListAsync();

        public async Task<TEntity?> GetAsync(ISpecifications<TEntity, TKey> spec) =>
            await SpecificationsEvaluator.GetQuery(_appDbContext.Set<TEntity>(), spec).FirstOrDefaultAsync();

        public async Task<int> CountAsync(ISpecifications<TEntity, TKey> spec) =>
            await SpecificationsEvaluator.GetQuery(_appDbContext.Set<TEntity>(), spec).CountAsync();

        public async Task<bool> AnyAsync(ISpecifications<TEntity, TKey> spec) =>
            await SpecificationsEvaluator.GetQuery(_appDbContext.Set<TEntity>(), spec).AnyAsync();

    }
}
