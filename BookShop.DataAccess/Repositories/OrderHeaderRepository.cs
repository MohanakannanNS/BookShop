using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repositories
{
    public class OrderHeaderRepository:Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader header)
        {
            _db.Update(header);
        }
    }
}
