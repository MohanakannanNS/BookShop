using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> product=_unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count);
            }
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var objFromDb = _unitOfWork.Product.FirstOrDefault(i => i.Id == id,includeProperties:"Category,CoverType");
            var shoppingCart = new ShoppingCart()
            {
                Product = objFromDb,
                ProductId = objFromDb.Id
            };
            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ShoppingCart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claim.Value;

                var cartFromDb = _unitOfWork.ShoppingCart.FirstOrDefault
                    (i => i.ApplicationUserId == cart.ApplicationUserId && i.ProductId == cart.ProductId, includeProperties: "Product");

                if (cartFromDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(cart);
                }
                else
                {
                    cartFromDb.Count += cart.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();
                var count = _unitOfWork.ShoppingCart.GetAll(i=>i.ApplicationUserId==cart.ApplicationUserId).ToList().Count;
                //HttpContext.Session.SetValue(StaticDetails.ssShoppingCart, count);
                HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(cart);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
