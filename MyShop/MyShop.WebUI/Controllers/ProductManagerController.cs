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
    public class ProductManagerController : Controller
    {
        IRepository<Product> context;
        IRepository<ProductCategory> productCategories;
        IRepository<DiscountInfo> DiscountInfoContext;
        IRepository<ItemDiscountInfo> ItemDiscountInfoContext;

        public ProductManagerController(IRepository<Product> productContext, IRepository<ProductCategory> productCategoryContext, IRepository<DiscountInfo> discountInfoContext,  IRepository<ItemDiscountInfo> Itemdiscountinfocontext) {
            context = productContext;
            productCategories = productCategoryContext;
            DiscountInfoContext = discountInfoContext;
            ItemDiscountInfoContext = Itemdiscountinfocontext;
        }
        // GET: ProductManager
        public ActionResult Index()
        {
            List<Product> products = context.Collection().ToList();
            return View(products);
        }

        public ActionResult Create() {
            ProductManagerViewModel viewModel = new ProductManagerViewModel();

            viewModel.Product = new Product();
            viewModel.ProductCategories = productCategories.Collection();
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(Product product, HttpPostedFileBase file) {
            if (!ModelState.IsValid)
            {
                return View(product);
            }
            else {

                if (file != null) {
                    product.Image = product.Id + Path.GetExtension(file.FileName);
                    file.SaveAs(Server.MapPath("//Content//ProductImages//") + product.Image);
                }

                context.Insert(product);
                context.Commit();

                List<DiscountInfo> discountList = DiscountInfoContext.Collection().Where(s => s.ItemId == product.Id || s.ItemId == product.Category).ToList();

                foreach(var i in discountList)
                {
                    UpdateDiscountData(i.Id);
                }

                return RedirectToAction("Index");
            }

        }

        public ActionResult Edit(string Id) {
            Product product = context.Find(Id);
            if (product == null)
            {
                return HttpNotFound();
            }
            else {
                ProductManagerViewModel viewModel = new ProductManagerViewModel();
                viewModel.Product = product;
                viewModel.ProductCategories = productCategories.Collection();

                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Edit(Product product, string Id, HttpPostedFileBase file) {
            Product productToEdit = context.Find(Id);

            if (productToEdit == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (!ModelState.IsValid) {
                    return View(product);
                }

                if (file != null) {
                    productToEdit.Image = product.Id + Path.GetExtension(file.FileName);
                    file.SaveAs(Server.MapPath("//Content//ProductImages//") + productToEdit.Image);
                }

                productToEdit.Category = product.Category;
                productToEdit.Description = product.Description;
                productToEdit.Name = product.Name;
                productToEdit.Price = product.Price;
                productToEdit.InStock = product.InStock;
                context.Commit();

                //Update discount info

                List<DiscountInfo> discountList = DiscountInfoContext.Collection().Where(s => s.ItemId == product.Id || s.ItemId == product.Category).ToList();

                foreach(var i in discountList)
                {
                    UpdateDiscountData(i.Id);
                }



                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(string Id)
        {
            Product productToDelete = context.Find(Id);

            if (productToDelete == null)
            {
                return HttpNotFound();
            }
            else
            {
                return View(productToDelete);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id) {
            Product productToDelete = context.Find(Id);

            if (productToDelete == null)
            {
                return HttpNotFound();
            }
            else
            {
                context.Delete(Id);
                context.Commit();
                return RedirectToAction("Index");
            }
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

             if (ItemDiscountToEditList.Count < productInCategory.Count )
            {
                var ItemIdList = ItemDiscountToEditList.Select(s => s.ItemId);
                foreach (var productTemp in productInCategory)
                {
                    
                    if (!ItemIdList.Contains(productTemp.Id))
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
}