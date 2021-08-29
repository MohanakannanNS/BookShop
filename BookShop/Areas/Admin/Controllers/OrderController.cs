using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderDetailsVM
            {
                OrderHeader = _unitOfWork.OrderHeader.FirstOrDefault(i => i.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(i => i.OrderId == id, includeProperties: "Product")
            };

            return View(OrderVM); 
        }

        [Authorize(Roles =StaticDetails.Role_Admin+","+StaticDetails.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(i => i.Id == id);
            orderHeader.OrderStatus = StaticDetails.StatusInProcess;
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public IActionResult Details(string stripeToken)
        {
            var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(i => i.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if (stripeToken != null)
            {
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 30),
                    Currency = "usd",
                    Description = "Order Id:" + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = StaticDetails.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                   orderHeader.PaymentStatus = StaticDetails.PaymentStatusApproved;
                   orderHeader.PaymentDate = DateTime.Now;
                }
                _unitOfWork.Save();
               
            }
            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles =StaticDetails.Role_Admin+","+StaticDetails.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(i => i.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = StaticDetails.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        public IActionResult CancelOrder(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(i => i.Id == id);
            if (orderHeader.OrderStatus == StaticDetails.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 30),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TrackingNumber
                };
                var service = new RefundService();
                Refund refund=service.Create(options);

                orderHeader.OrderStatus = StaticDetails.StatusRefunded;
                orderHeader.PaymentStatus = StaticDetails.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = StaticDetails.StatusCancelled;
                orderHeader.PaymentStatus = StaticDetails.StatusCancelled;
            }
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        #region API_Call
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(StaticDetails.Role_Admin) || User.IsInRole(StaticDetails.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(i=>i.ApplicationUserId==claim.Value,
                                                                 includeProperties: "ApplicationUser"
                                                                 );
            }
            switch (status)
            {
                case "Inprocess":
                    orderHeaderList = orderHeaderList.Where(i => i.OrderStatus==StaticDetails.StatusApproved||
                                                                  i.OrderStatus==StaticDetails.StatusInProcess
                                                                  );
                    break;
                //case "Shipped":
                //    orderHeaderList = orderHeaderList.Where(i => i.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment);
                //    break;
                case "Completed":
                    orderHeaderList = orderHeaderList.Where(i => i.OrderStatus == StaticDetails.StatusShipped);
                    break;
                case "Rejected":
                    orderHeaderList = orderHeaderList.Where(i => i.OrderStatus == StaticDetails.StatusCancelled ||
                                                                  i.OrderStatus == StaticDetails.StatusRefunded ||
                                                                  i.OrderStatus == StaticDetails.PaymentStatusRejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaderList });

           
        }
        #endregion
    }
}
