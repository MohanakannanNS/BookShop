using BookShop.DataAccess.Repositories;
using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]   
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(int pageNumber =1)
        {
            CategoryVM categoryVM = new CategoryVM()
            {
                Category = _unitOfWork.Category.GetAll()
            };
            var count = categoryVM.Category.Count();
            categoryVM.Category = categoryVM.Category.OrderBy(p => p.Name).Skip((pageNumber - 1) * 2).Take(2).ToList();
            categoryVM.PageInfo = new PageInfo
            {
                CurrentPage = pageNumber,
                ItemsPerPage =2,
                TotalItem = count,
                URLParam = "/Admin/Category/Index?pageNumber=:"
            };
            return View(categoryVM);
        }

        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
            {
                return View(category);
            }
            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _unitOfWork.Category.Add(category);
                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(category);
            }
        }

        #region API Call
        [HttpGet]

        public IActionResult GetAll()
        {
            var result = _unitOfWork.Category.GetAll();
            return Json(new { data = result });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            _unitOfWork.Category.Remove(id);
            _unitOfWork.Save();
            TempData["success"] = "Delete Successfull.";
            return Json(new { success=true, message="Delete successfull" });

        }
        #endregion
    }
}
