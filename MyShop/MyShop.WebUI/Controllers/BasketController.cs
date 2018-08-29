using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyShop.WebUI.Controllers
{
    public class BasketController : Controller
    {
        IRepository<Customer> customers;
        IBasketService basketService;
        IOrderService orderService;
        IRepository<PaymentInfo> PaymentInfoRepo;

        public BasketController(IBasketService BasketService, IOrderService OrderService, IRepository<Customer> Customers, IRepository<PaymentInfo> Paymentinfo) {
            this.basketService = BasketService;
            this.orderService = OrderService;
            this.customers = Customers;
            this.PaymentInfoRepo = Paymentinfo;
        }
        // GET: Basket2
        public ActionResult Index()
        {
            var model = basketService.GetBasketItems(this.HttpContext);
            return View(model);
        }

        public ActionResult AddToBasket(string Id)
        {
            
            basketService.AddToBasket(this.HttpContext, Id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult IncreaseQuantity(string ID,int quantity)
        {
            
            basketService.AddToBasket(this.HttpContext, ID,quantity);

            return RedirectToAction("Index");
        }
        public ActionResult RemoveFromBasket(string Id)
        {
            basketService.RemoveFromBasket(this.HttpContext, Id);

            return RedirectToAction("Index");
        }

        public PartialViewResult BasketSummary() {
            var basketSummary = basketService.GetBasketSummary(this.HttpContext);
            
            return PartialView(basketSummary);
        }

        [Authorize]
        public ActionResult Checkout() {
            Customer customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);

            if (customer != null)
            {
                Order order = new Order()
                {
                    Email = customer.Email,
                    City = customer.City,
                    State = customer.State,
                    Street = customer.Street,
                    FirstName = customer.FirstName,
                    Surname = customer.LastName,
                    ZipCode = customer.ZipCode
                };

                return View(order);
            }
            else {
                return RedirectToAction("Error");
            }
            
        }

        [HttpPost]
        [Authorize]
        public ActionResult Checkout(Order order) {

            var basketItems = basketService.GetBasketItems(this.HttpContext);
            order.OrderStatus = "Order Created";
            order.Email = User.Identity.Name;
            orderService.CreateOrder(order,basketItems);
            var paymentInfoViewModel = new PaymentInfoViewModel() { OrderID = order.Id};
            //process payment
            return View("PaymentInfo",paymentInfoViewModel);            
        }

        public ActionResult PaymentInfo()
        {
            return Redirect("PaymentInfo");            
        }

        [HttpPost]
        [Authorize]
        public ActionResult PaymentInfo(PaymentInfoViewModel PaymentInfoViewModel)
        {
            var paymentInfo = new PaymentInfo()
            {
                Number= PaymentInfoViewModel.Number,
                ExpiryMonth = PaymentInfoViewModel.ExpiryMonth,
                ExpiryYear = PaymentInfoViewModel.ExpiryYear,
                CVV = PaymentInfoViewModel.CVV,
                Name = PaymentInfoViewModel.Name,
                CardType = PaymentInfoViewModel.CardType
            };
            
            var order = orderService.GetOrder(PaymentInfoViewModel.OrderID);
            order.Payment = paymentInfo;

            orderService.UpdateOrder(order);
           // return RedirectToAction("Thankyou", new { OrderId = order.Id });
            return RedirectToAction("Thankyou",new { OrderId = order.Id});
        }

        public ActionResult ThankYou(string OrderId) {
            ViewBag.OrderId = OrderId;
            return View();
        }
    }
}