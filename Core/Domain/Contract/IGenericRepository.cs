using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contract
{
    public interface IGenericRepository<TEntity,TKey> where TEntity : BaseEntity<TKey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync(bool AsNoTracking=false);
        Task<TEntity?> GetAsync(TKey id);
        Task AddAsync(TEntity entity);
        public void UpdateAsync(TEntity entity);
        public void DeleteAsync(TEntity entity);
        Task<IEnumerable<TEntity>> GetAllAsync(ISpecifications<TEntity, TKey> spec);
        Task<TEntity?> GetAsync(ISpecifications<TEntity, TKey> spec);
        Task<int> CountAsync(ISpecifications<TEntity, TKey> spec);
        Task<bool> AnyAsync(ISpecifications<TEntity, TKey> spec);


    }
}
