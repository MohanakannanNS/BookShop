using BookShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repositories.IRepositories
{
    public interface IOrderDetailsRepository: IRepository<OrderDetails>
    {
        void Update(OrderDetails details);
    }
}
