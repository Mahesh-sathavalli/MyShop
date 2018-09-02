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
    public class HomeController : Controller
    {
        IRepository<Customer> customers;
        IRepository<Product> context;
        IRepository<ProductCategory> productCategories;
        IRepository<PaymentInfo> PaymentInfoContext;
        IRepository<DiscountInfo> DiscountInfoContext;
        IRepository<ItemDiscountInfo> ItemDiscountInfoContext;
        public HomeController(IRepository<Product> productContext, IRepository<ProductCategory> productCategoryContext, IRepository<Customer> Customers, IRepository<PaymentInfo> PaymentInfo, IRepository<DiscountInfo> discountInfoContext, IRepository<ItemDiscountInfo> Itemdiscountinfocontext) 
        {
            context = productContext;
            productCategories = productCategoryContext;
            customers = Customers;
            PaymentInfoContext = PaymentInfo;
            DiscountInfoContext = discountInfoContext;
            ItemDiscountInfoContext = Itemdiscountinfocontext;
        }

        public ActionResult Index(string Category=null)
        {
            List<Product> products;
            List<ProductCategory> categories = productCategories.Collection().ToList();
            Customer customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);
            if (Category == null)
            {
                products = context.Collection().ToList();
            }
            else {
                products = context.Collection().Where(p => p.Category == Category).ToList();
            }

            ProductListViewModel model = new ProductListViewModel();
            model.Products = products;
            model.ProductCategories = categories;
            model.Customer = customer;

            return View(model);
        }

        public ActionResult ProductList(string Category = null,string search=null)
        {
            List<Product> products;
            List<ProductCategory> categories = productCategories.Collection().ToList();
            Customer customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);

           
            if (Category == null)
            {
                products = context.Collection().ToList();
                
            }
            else
            {
                products = context.Collection().Where(p => p.Category == Category).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.ToLower().Contains(search)).ToList();
            }

            ProductListViewModel model = new ProductListViewModel();
            model.Products = products;
            model.ProductCategories = categories;
            model.Customer = customer;
            return View(model);
        }

        public ActionResult Details(string Id) {
            Product product = context.Find(Id);
            List<ProductCategory> categories = productCategories.Collection().ToList();
            Customer customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);
            
            List<ItemDiscountInfo> prodDiscount = ItemDiscountInfoContext.Collection().Where(s => s.ItemId == Id).ToList();
            //DiscountInfo discount = DiscountInfoContext.Collection().Where(s => prodDiscount.Select(a => a.ItemId).Contains(s.ItemId)).OrderByDescending(s=>s.Priority).FirstOrDefault();

            List<DiscountInfo> discountList = new List<DiscountInfo>();
            DiscountInfo discount = new DiscountInfo();
            foreach(var pd in prodDiscount)
            {
                var temp = DiscountInfoContext.Collection().Where(s => s.Id == pd.DiscountId && s.ExpiryDate >= DateTime.Now).FirstOrDefault();
                if(temp!=null)
                {
                    discountList.Add(temp);
                }
                
            }

            if(discountList.Count>0)
            {
                discount = discountList.OrderByDescending(s => s.Priority).FirstOrDefault();
            }
            

            ProductDetailViewModel model = new ProductDetailViewModel();
            model.Product = product;
            model.ProductCategories = categories;
            model.Customer = customer;
            model.ProductDiscount = prodDiscount.Where(s => s.DiscountId == discount.Id).FirstOrDefault();




            if (product == null)
            {
                return HttpNotFound();
            }
            else {
                return View(model);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}