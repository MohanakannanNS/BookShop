using BookShop.DataAccess.Data;
using BookShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repositories.IRepositories
{
    public class CompanyRepository:Repository<Company>,ICompanyRepository
    {
        private readonly ApplicationDbContext _db;
        public CompanyRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(Company company)
        {
            //var objFromDb = _db.Companies.FirstOrDefault(i => i.Id == company.Id);
            //if (objFromDb != null)
            //{
            //    company.Name = objFromDb.Name;
            //    company.StreetAddress = objFromDb.StreetAddress;
            //    company.City = objFromDb.City;
            //    company.State = objFromDb.State;
            //    company.PostalCode = objFromDb.PostalCode;
            //    company.PhoneNumber = objFromDb.PhoneNumber;
            //    company.IsAuthorizedCompany = objFromDb.IsAuthorizedCompany;
            //}
            _db.Update(company);

        }
    }
}
