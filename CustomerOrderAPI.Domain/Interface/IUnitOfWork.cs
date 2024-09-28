using CustomerOrderAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerOrderAPI.Domain.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Customer> Customers { get; }
        IRepository<Order> Orders { get; }
        IRepository<Item> Items { get; }
        IRepository<Product> Products { get; }
        Task<int> SaveChangesAsync();
    }
}
