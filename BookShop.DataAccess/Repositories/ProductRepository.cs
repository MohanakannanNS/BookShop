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
    public class ProductRepository: Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(m => m.Id == product.Id);
            if (objFromDb != null)
            {
                if (product.ImageURL != null)
                {
                    objFromDb.ImageURL = product.ImageURL;
                }
                objFromDb.ISBN = product.ISBN;
                objFromDb.Title = product.Title;
                objFromDb.Description = product.Description;
                objFromDb.Author = product.Author;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Price = product.Price;
                objFromDb.Price50 = product.Price50;
                objFromDb.Price100 = product.Price100;
                objFromDb.CategoryId = product.CategoryId;
                objFromDb.CategoryId = product.CategoryId;
            }
        }
    }
}
