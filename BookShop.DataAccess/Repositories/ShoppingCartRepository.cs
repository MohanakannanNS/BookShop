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
    public class ShoppingCartRepository:Repository<ShoppingCart>,IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart cart)
        {
            _db.Update(cart);
        }
    }
}
