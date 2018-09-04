using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyShop.Core.Models;
using MyShop.DataAccess.InMemory;
using MyShop.Core.ViewModels;
using MyShop.Core.Contracts;
using System.IO;

namespace MyShop.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiscountManagerController : Controller
    {
        IRepository<Customer> customers;
        IRepository<Product> context;
        IRepository<ProductCategory> productCategories;
        IRepository<DiscountInfo> DiscountInfoContext;
        IRepository<ItemDiscountInfo> ItemDiscountInfoContext;

        public DiscountManagerController(IRepository<Product> productContext, IRepository<ProductCategory> productCategoryContext, IRepository<DiscountInfo> discountInfoContext, IRepository<Customer> Customers, IRepository<ItemDiscountInfo> Itemdiscountinfocontext)
        {
            context = productContext;
            productCategories = productCategoryContext;
            DiscountInfoContext = discountInfoContext;
            customers = Customers;
            ItemDiscountInfoContext = Itemdiscountinfocontext;
        }
        // GET: ProductManager
        public ActionResult Index()
        {
            List<DiscountInfo> Discount = DiscountInfoContext.Collection().ToList();

            DiscountListViewModel DiscountListViewModel = new DiscountListViewModel();
            DiscountListViewModel.Customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);
            DiscountListViewModel.Products = context.Collection().ToList();
            DiscountListViewModel.ProductCategories = productCategories.Collection().ToList();
            DiscountListViewModel.DiscountList = Discount;
            return View(DiscountListViewModel);
            
        }

        public ActionResult Create()
        {
            DiscountListViewModel DiscountListViewModel = new DiscountListViewModel();
            DiscountListViewModel.Customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);
            DiscountListViewModel.Products = context.Collection().ToList();
            DiscountListViewModel.ProductCategories = productCategories.Collection().ToList();
            DiscountListViewModel.DiscountList = new List<DiscountInfo>();
            DiscountListViewModel.Discount = new DiscountInfo();


            return View(DiscountListViewModel);
        }

        [HttpPost]
        public ActionResult Create(DiscountInfo discount)
        {
            if (!ModelState.IsValid)
            {
                return View(discount);
            }
            else
            {
                DiscountInfoContext.Insert(discount);
                DiscountInfoContext.Commit();

                UpdateDiscountData(discount.Id);

                return RedirectToAction("Index");
            }

        }

        public ActionResult Edit(string Id)
        {
            DiscountInfo Discount1 = DiscountInfoContext.Find(Id);
            List<Product> products = context.Collection().ToList();
            List<ProductCategory> categories = productCategories.Collection().ToList();
            Customer customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);

            if (Discount1 == null)
            {
                return HttpNotFound();
            }
            else
            {
                DiscountListViewModel DiscountListViewModel = new DiscountListViewModel();
                DiscountListViewModel.Customer = customer;
                DiscountListViewModel.Products = products;
                DiscountListViewModel.ProductCategories = categories;
                DiscountListViewModel.Discount = Discount1;

                return View(DiscountListViewModel);
            }
        }

        [HttpPost]
        public ActionResult Edit(DiscountListViewModel discountL, string Id, HttpPostedFileBase file)
        {
            DiscountInfo discountToEdit = DiscountInfoContext.Find(Id);

            if (discountToEdit == null)
            {
                return HttpNotFound();
            }
            else
            {

                discountToEdit.Name = discountL.Discount.Name;
                discountToEdit.Code = discountL.Discount.Code;
                discountToEdit.ExpiryDate = discountL.Discount.ExpiryDate;
                discountToEdit.Amount = discountL.Discount.Amount;
                discountToEdit.Percentage = discountL.Discount.Percentage;
                discountToEdit.AppliedType = discountL.Discount.AppliedType;
                discountToEdit.ItemId = discountL.Discount.ItemId;
                discountToEdit.ItemType = discountL.Discount.ItemType;
                discountToEdit.Priority = discountL.Discount.Priority;

                DiscountInfoContext.Commit();

                UpdateDiscountData(Id);


                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(string Id)
        {
            DiscountInfo discountToDelete = DiscountInfoContext.Find(Id);

            if (discountToDelete == null)
            {
                return HttpNotFound();
            }
            else
            {
                return View(discountToDelete);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            DiscountInfo discountToDelete = DiscountInfoContext.Find(Id);

            if (discountToDelete == null)
            {
                return HttpNotFound();
            }
            else
            {
                DiscountInfoContext.Delete(Id);
                

                var itemDiscountList = ItemDiscountInfoContext.Collection().Where(s => s.DiscountId == Id).ToList();
                foreach (var item in itemDiscountList)
                {
                    ItemDiscountInfoContext.Delete(item.Id);
                    
                }
                ItemDiscountInfoContext.Commit();
                DiscountInfoContext.Commit();

                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        public JsonResult getDiscontData()
        {

            List<Product> products = context.Collection().ToList();
            List<ProductCategory> categories = productCategories.Collection().ToList();
            Customer customer = customers.Collection().FirstOrDefault(c => c.Email == User.Identity.Name);


            DiscountListViewModel DiscountListViewModel = new DiscountListViewModel();
            DiscountListViewModel.Customer = customer;
            DiscountListViewModel.Products = products;
            DiscountListViewModel.ProductCategories = categories;
            DiscountListViewModel.Discount = new DiscountInfo();
            
            return Json(DiscountListViewModel, JsonRequestBehavior.AllowGet);

        }


        public void UpdateDiscountData(string Id)
        {
            DiscountInfo discount = DiscountInfoContext.Find(Id);
            Product product = context.Collection().Where(s => s.Id == discount.ItemId && discount.ItemType == 1).FirstOrDefault();
            ItemDiscountInfo ItemDiscountToEdit = ItemDiscountInfoContext.Collection().Where(s => s.DiscountId == discount.Id && discount.ItemType == 1).FirstOrDefault();

            ProductCategory Category = productCategories.Collection().Where(s => s.Id == discount.ItemId && discount.ItemType == 2).FirstOrDefault();

            List<Product> productInCategory = new List<Product>();
            if (Category != null)
            {
                productInCategory = context.Collection().Where(s => s.Category.Equals(Category.Id) && discount.ItemType == 2).ToList();
            }

            List<ItemDiscountInfo> ItemDiscountToEditList = ItemDiscountInfoContext.Collection().Where(s => s.DiscountId == discount.Id && discount.ItemType == 2).ToList();
            // upated ItemDiscountInfo it discountId exist and item Type is product


            if (ItemDiscountToEdit != null && product != null)
            {
                ItemDiscountToEdit.DiscountId = discount.Id;
                ItemDiscountToEdit.ItemId = discount.ItemId;
                ItemDiscountToEdit.ItemPrice = product.Price;

                if (discount.AppliedType == 1)
                {
                    //AppliedTYpe = Amount
                    ItemDiscountToEdit.discountedPrice = product.Price - discount.Amount;
                }
                else if (discount.AppliedType == 2)
                {
                    //AppliedType = Percentage
                    decimal temp = (product.Price * (discount.Percentage / 100));
                    ItemDiscountToEdit.discountedPrice = product.Price - temp;
                }
                ItemDiscountInfoContext.Commit();
            }
            else if (ItemDiscountToEdit == null && product != null)
            {
                ItemDiscountInfo IDI = new ItemDiscountInfo();
                IDI.DiscountId = discount.Id;
                IDI.ItemId = discount.ItemId;
                IDI.ItemPrice = product.Price;

                if (discount.AppliedType == 1)
                {
                    //AppliedTYpe = Amount
                    IDI.discountedPrice = product.Price - discount.Amount;
                }
                else if (discount.AppliedType == 2)
                {
                    //AppliedType = Percentage
                    decimal temp = (product.Price * (discount.Percentage / 100));
                    IDI.discountedPrice = product.Price - temp;
                }
                ItemDiscountInfoContext.Insert(IDI);
                ItemDiscountInfoContext.Commit();
            }
            else if (ItemDiscountToEditList.Count > 0 && productInCategory.Count > 0)
            {
                foreach (var itr in ItemDiscountToEditList)
                {
                    var productTemp = productInCategory.Where(s => s.Id == itr.ItemId).FirstOrDefault();


                    if (productTemp != null)
                    {
                        ItemDiscountToEdit = itr;


                        ItemDiscountToEdit.DiscountId = itr.DiscountId;
                        ItemDiscountToEdit.ItemId = itr.ItemId;
                        ItemDiscountToEdit.ItemPrice = productTemp.Price;

                        if (discount.AppliedType == 1)
                        {
                            //AppliedTYpe = Amount
                            ItemDiscountToEdit.discountedPrice = productTemp.Price - discount.Amount;
                        }
                        else if (discount.AppliedType == 2)
                        {
                            //AppliedType = Percentage
                            decimal temp = (productTemp.Price * (discount.Percentage / 100));
                            ItemDiscountToEdit.discountedPrice = productTemp.Price - temp;
                        }
                        ItemDiscountInfoContext.Commit();
                    }



                }

            }
            else if (ItemDiscountToEditList.Count <= 0 && productInCategory.Count > 0)
            {
                foreach (var productTemp in productInCategory)
                {
                    ItemDiscountInfo IDI = new ItemDiscountInfo();
                    IDI.DiscountId = discount.Id;
                    IDI.ItemId = productTemp.Id;
                    IDI.ItemPrice = productTemp.Price;

                    if (discount.AppliedType == 1)
                    {
                        //AppliedTYpe = Amount
                        IDI.discountedPrice = productTemp.Price - discount.Amount;
                    }
                    else if (discount.AppliedType == 2)
                    {
                        //AppliedType = Percentage
                        decimal temp = (productTemp.Price * (discount.Percentage / 100));
                        IDI.discountedPrice = productTemp.Price - temp;
                    }
                    ItemDiscountInfoContext.Insert(IDI);
                    ItemDiscountInfoContext.Commit();
                }

            }

        }
    }
}