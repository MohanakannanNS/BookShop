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
    public class CoverTypeRepository: Repository<CoverType>,ICoverTypeRepository
    {
        private readonly ApplicationDbContext _db;
        public CoverTypeRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(CoverType coverType)
        {
            var entity = _db.CoverTypes.FirstOrDefault(m => m.Id == coverType.Id);
            entity.Name = coverType.Name;
        }
    }
}
