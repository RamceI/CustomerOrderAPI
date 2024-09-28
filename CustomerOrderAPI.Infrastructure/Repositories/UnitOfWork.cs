using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Infrastructure.Data;


namespace CustomerOrderAPI.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Customer> Customers => new Repository<Customer>(_context);
        public IRepository<Order> Orders => new Repository<Order>(_context);
        public IRepository<Item> Items => new Repository<Item>(_context);
        public IRepository<Product> Products => new Repository<Product>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
