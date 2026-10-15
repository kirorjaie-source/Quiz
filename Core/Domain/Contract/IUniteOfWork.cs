using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contract
{
    public interface IUniteOfWork
    {
        IGenericRepository<TEntity, TKey>GetRepository<TEntity, TKey>()where TEntity : BaseEntity<TKey>;
        public Task SaveChangeAsync();
    }
}
