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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(Category category)
        {
            Category entity = _db.Categories.FirstOrDefault(s => s.Id == category.Id);
            if(entity != null)
            {
                entity.Name = category.Name;
               // _db.SaveChanges();
            }           
        }
    }
}
