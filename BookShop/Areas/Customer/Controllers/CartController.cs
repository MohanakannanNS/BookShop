using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db
            )
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _db = db;
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var shoppingCartVM = new ShoppingCartVM
            {
                orderHeader = new Models.OrderHeader(),
                shoppingCart = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == claims.Value, includeProperties: "Product")

            };
            shoppingCartVM.orderHeader.OrderTotal = 0;
            shoppingCartVM.orderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(i =>
                                                                            i.Id == claims.Value);
            shoppingCartVM.orderHeader.ApplicationUser.Company = _unitOfWork.Company.
                                                      FirstOrDefault(i => i.Id == shoppingCartVM.orderHeader.ApplicationUser.CompanyId);    

            foreach (var item in shoppingCartVM.shoppingCart)
            {
                item.Price = StaticDetails.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                shoppingCartVM.orderHeader.OrderTotal += (item.Count * item.Price);
                item.Product.Description = StaticDetails.ConvertToRawHtml(item.Product.Description);
                if (item.Product.Description.Length > 100)
                {
                    item.Product.Description = item.Product.Description.Substring(0, 99) + "...";
                }
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Index")]

        public async Task<IActionResult> IndexPost()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty!");
            }
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code},
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent.Please check your email.");
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(i => i.Id == cartId, includeProperties:"Product");
            cart.Count += 1;
            cart.Price = StaticDetails.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(i => i.Id == cartId, includeProperties: "Product");
            if (cart.Count == 1)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                _unitOfWork.ShoppingCart.Remove(cart);
                HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = StaticDetails.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(i => i.Id == id, includeProperties: "Product");
            var count = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count - 1);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                orderHeader = new Models.OrderHeader(),
                shoppingCart = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == claim.Value, includeProperties: "Product")
            };
            ShoppingCartVM.orderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
            ShoppingCartVM.orderHeader.ApplicationUser.Company = _unitOfWork.Company.FirstOrDefault(i => i.Id == ShoppingCartVM.orderHeader.ApplicationUser.CompanyId);

            foreach (var item in ShoppingCartVM.shoppingCart)
            {
                item.Price = StaticDetails.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.orderHeader.OrderTotal += (item.Price * item.Count);
            }

            ShoppingCartVM.orderHeader.Name = ShoppingCartVM.orderHeader.ApplicationUser.Name;
            ShoppingCartVM.orderHeader.PhoneNuber = ShoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.orderHeader.StreetAddress = ShoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.orderHeader.City = ShoppingCartVM.orderHeader.ApplicationUser.City;
            ShoppingCartVM.orderHeader.State = ShoppingCartVM.orderHeader.ApplicationUser.State;
            ShoppingCartVM.orderHeader.PostalCode = ShoppingCartVM.orderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM);
        }
        //[HttpPost]
        //[ActionName("Summary")]
        //[ValidateAntiForgeryToken]
        //public IActionResult SummaryPost(string stripeToken)
        //{
        //    var claimIdentity = (ClaimsIdentity)User.Identity;
        //    var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

        //    ShoppingCartVM.orderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
        //    ShoppingCartVM.orderHeader.ApplicationUser.Company = _unitOfWork.Company.
        //                                                     FirstOrDefault(i => i.Id == ShoppingCartVM.orderHeader.ApplicationUser.CompanyId);

        //    ShoppingCartVM.shoppingCart = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == claim.Value, includeProperties: "Product");

        //    ShoppingCartVM.orderHeader.OrderStatus = StaticDetails.StatusPending;
        //    ShoppingCartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
        //    ShoppingCartVM.orderHeader.ApplicationUserId = claim.Value;
        //    ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;

        //    _unitOfWork.OrderHeader.Add(ShoppingCartVM.orderHeader);
        //    _unitOfWork.Save();

        //    var orderDetailsList = new List<OrderDetails>();
        //    foreach (var item in ShoppingCartVM.shoppingCart)
        //    {
        //        item.Price = StaticDetails.GetPriceBasedOnQuantity(item.Count, item.Product.Price,
        //                                                  item.Product.Price50, item.Product.Price100);
        //        var orderDetails = new OrderDetails()
        //        {
        //            OrderId = ShoppingCartVM.orderHeader.Id,
        //            ProductId = item.ProductId,
        //            Count = item.Count,
        //            Price = item.Price
        //        };
        //        ShoppingCartVM.orderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
        //        _unitOfWork.OrderDetails.Add(orderDetails);
        //    }
        //    _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.shoppingCart);
        //    _unitOfWork.Save();
        //    HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, 0);

        //    if (stripeToken == null)
        //    {
        //        delayed payment for authorized user
        //        ShoppingCartVM.orderHeader.OrderStatus = StaticDetails.StatusApproved;
        //        ShoppingCartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
        //        ShoppingCartVM.orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
        //    }
        //    else
        //    {
        //        var options = new ChargeCreateOptions
        //        {
        //            Amount = Convert.ToInt32(ShoppingCartVM.orderHeader.OrderTotal * 30),
        //            Currency = "INR",
        //            Customer = ShoppingCartVM.orderHeader.Name,
        //            Shipping = ShoppingCartVM.orderHeader.StreetAddress,
        //            Description = "Order Id:" + ShoppingCartVM.orderHeader.Id,
        //            Source = stripeToken,
        //        };

        //        var service = new ChargeService();
        //        Charge charge = service.Create(options);

        //        if (charge.Id == null)
        //        {
        //            ShoppingCartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusRejected;
        //        }
        //        else
        //        {
        //            ShoppingCartVM.orderHeader.TransactionId = charge.Id;
        //        }
        //        if (charge.Status.ToLower() == "succeeded")
        //        {
        //            ShoppingCartVM.orderHeader.OrderStatus = StaticDetails.StatusApproved;
        //            ShoppingCartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusApproved;
        //            ShoppingCartVM.orderHeader.PaymentDate = DateTime.Now;
        //        }
        //        _unitOfWork.Save();
        //    }

        //    return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.orderHeader.Id });

        //}
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.orderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
            ShoppingCartVM.orderHeader.ApplicationUser.Company = _unitOfWork.Company.
                                                             FirstOrDefault(i => i.Id == ShoppingCartVM.orderHeader.ApplicationUser.CompanyId);

            ShoppingCartVM.shoppingCart = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == claim.Value, includeProperties: "Product");

            ShoppingCartVM.orderHeader.OrderStatus = StaticDetails.StatusApproved;
            ShoppingCartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusCOD;
            ShoppingCartVM.orderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.orderHeader);
            _unitOfWork.Save();

            var orderDetailsList = new List<OrderDetails>();
            foreach (var item in ShoppingCartVM.shoppingCart)
            {
                item.Price = StaticDetails.GetPriceBasedOnQuantity(item.Count, item.Product.Price,
                                                          item.Product.Price50, item.Product.Price100);
                var orderDetails = new OrderDetails()
                {
                    OrderId = ShoppingCartVM.orderHeader.Id,
                    ProductId = item.ProductId,
                    Count = item.Count,
                    Price = item.Price
                };
                ShoppingCartVM.orderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitOfWork.OrderDetails.Add(orderDetails);
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.shoppingCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, 0);
            if (ShoppingCartVM.orderHeader.ApplicationUser.CompanyId!=null || ShoppingCartVM.orderHeader.ApplicationUser.Company.IsAuthorizedCompany)
            {
                ShoppingCartVM.orderHeader.OrderStatus = StaticDetails.StatusApproved;
                ShoppingCartVM.orderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
                ShoppingCartVM.orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                
            }

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.orderHeader.Id });
        }

    
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
