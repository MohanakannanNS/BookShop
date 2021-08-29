using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;

        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _host = host;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
                
            if (id != null)
            {
                productVM.Product = _unitOfWork.Product.FirstOrDefault(i => i.Id == id);
                return View(productVM);
            }
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _host.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);
                    var uploadPath = Path.Combine(webRootPath, @"Images\Products");
                    if (productVM.Product.ImageURL != null)
                    {
                        var imagePath = Path.Combine(webRootPath,productVM.Product.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                        
                    }
                    using (var fileStream = new FileStream(Path.Combine(uploadPath,fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageURL = @"\Images\Products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var objFromDb = _unitOfWork.Product.Get(productVM.Product.Id);
                        productVM.Product.ImageURL = objFromDb.ImageURL;
                    }
                   
                }

                if (productVM.Product.Id != 0)
                {

                    _unitOfWork.Product.Update(productVM.Product);                    
                }
                else
                {
                    _unitOfWork.Product.Add(productVM.Product);                    
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                productVM.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                if (productVM.Product.Id != 0)
                {
                    var objFromDb = _unitOfWork.Product.Get(productVM.Product.Id);
                    productVM.Product = objFromDb;
                }
                return View(productVM);
            }

        }

        #region API_CALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var result=_unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = result });
        }

        [HttpDelete]
        public IActionResult Delete(int Id)
        {
            var webRootPath = _host.WebRootPath;
            var objFromDb = _unitOfWork.Product.Get(Id);
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageURL.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitOfWork.Product.Remove(Id);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfull." });
        }
        #endregion
    }
}
