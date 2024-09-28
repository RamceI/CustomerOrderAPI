
using CustomerOrderAPI.Domain.Entities;
using System.Linq.Expressions;

namespace CustomerOrderAPI.Domain.Interface
{
    public interface IRepository<T> where T : class
    {
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate); // Add this method
        IQueryable<T> AsQueryable();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
