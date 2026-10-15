using Domain;
using Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public abstract class BaseSpecifications<TEntity, TKey> : ISpecifications<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    {
        public Expression<Func<TEntity, bool>>? Criteria { get; }
        public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = new();
        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
        public int Skip { get; private set; }
        public int Take { get; private set; }
        public bool IsPagingEnabled { get; private set; }

        protected BaseSpecifications(Expression<Func<TEntity, bool>>? criteria) => Criteria = criteria;

        protected void AddInclude(Expression<Func<TEntity, object>> include) => IncludeExpressions.Add(include);
        protected void AddOrderBy(Expression<Func<TEntity, object>> orderBy) => OrderBy = orderBy;
        protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderBy) => OrderByDescending = orderBy;
        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}
