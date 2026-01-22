using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EShop.Domain.DomainModels;

namespace EShop.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null);

        T? Get(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null);

        T Insert(T entity);
        T Update(T entity);
        void Delete(T entity);
    }
}