using BookShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repositories.IRepositories
{
    public interface IShoppingCartRepository:IRepository<ShoppingCart>
    {
        void Update(ShoppingCart cart);
    }
}
