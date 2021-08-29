using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repositories.IRepositories
{
    public interface IUnitOfWork:IDisposable
    {
        ICategoryRepository Category { get; }
        ISP_Call SP_Call { get; }
        IProductRepository Product { get; }
        ICoverTypeRepository CoverType { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IOrderHeaderRepository OrderHeader { get; }
        void Save();
    }
}
