using BookShop.DataAccess.Data;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API_Call
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _db.ApplicationUsers.Include(i => i.Company).ToList();//note this *******
            var roles = _db.Roles.ToList();
            var userRole = _db.UserRoles.ToList();
            foreach (var item in userList)
            {
                var roleId = userRole.FirstOrDefault(i => i.UserId == item.Id).RoleId;
                item.Role = roles.FirstOrDefault(i => i.Id == roleId).Name;//********
                if (item.Company == null)
                {
                    item.Company = new Company() { Name = "" };
                }
            }
            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(i => i.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }
            else
            {
                if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
                {
                    //user is in locked status
                    objFromDb.LockoutEnd = DateTime.Now;
                }
                else
                {
                    objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
                }
                _db.SaveChanges();
                return Json(new { success = true, message = "Operation successfull" });
            }
        }
        #endregion
    }
}
