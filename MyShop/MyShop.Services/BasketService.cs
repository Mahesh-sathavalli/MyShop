﻿using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
    public class BasketService : IBasketService
    {
        IRepository<Product> productContext;
        IRepository<Basket> basketContext;
        IRepository<DiscountInfo> discountInfoContext;
        IRepository<ItemDiscountInfo> itemDiscountInfoContext;

        public const string BasketSessionName = "eCommerceBasket";

        public BasketService(IRepository<Product> ProductContext, IRepository<Basket> BasketContext, IRepository<DiscountInfo> DiscountInfoContext, IRepository<ItemDiscountInfo> ItemDiscountInfoContext) {
            this.basketContext = BasketContext;
            this.productContext = ProductContext;
            this.discountInfoContext = DiscountInfoContext;
            this.itemDiscountInfoContext = ItemDiscountInfoContext;
        }

        private Basket GetBasket(HttpContextBase httpContext, bool createIfNull) {
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);

            Basket basket = new Basket();

            if (cookie != null)
            {
                string basketId = cookie.Value;
                if (!string.IsNullOrEmpty(basketId))
                {
                    basket = basketContext.Find(basketId);
                }
                else
                {
                    if (createIfNull)
                    {
                        basket = CreateNewBasket(httpContext);
                    }
                }
            }
            else {
                if (createIfNull)
                {
                    basket = CreateNewBasket(httpContext);
                }
            }

            return basket;
           
        }

        private Basket CreateNewBasket(HttpContextBase httpContext) {
            Basket basket = new Basket();
            basketContext.Insert(basket);
            basketContext.Commit();

            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContext.Response.Cookies.Add(cookie);

            return basket;
        }

        public bool AddToBasket(HttpContextBase httpContext, string productId, int quantity = 1) {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);
            bool isOkaytoAdd = true;


            if (item == null)
            {
                item = new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quanity = 1
                };
                var product = productContext.Find(item.ProductId);

                if(product != null)
                {
                    if (product.InStock == 0)
                    {
                        isOkaytoAdd = false;
                    }
                    else if(product.InStock > quantity || product.Category == "Sports, Fitness and Outdoors")
                    {
                        UpdateStock(product, "Add");
                        basket.BasketItems.Add(item);
                        isOkaytoAdd = true;
                    }
                }
                
            }
            else {
                var product = productContext.Find(item.ProductId);
                if(product != null)
                {
                    if (product.InStock == 0 )
                    {
                        isOkaytoAdd = false;
                    }else if((product.InStock + item.Quanity) >= quantity || product.Category == "Sports, Fitness and Outdoors")
                    {
                        product.InStock = product.InStock + item.Quanity;
                        UpdateStock(product, "Add",quantity);
                        item.Quanity = quantity;
                        basket.BasketItems.FirstOrDefault(i => i.ProductId == productId).Quanity = item.Quanity;
                    }
                    
                }
                
            }
            productContext.Commit();
            basketContext.Commit();
            return isOkaytoAdd;
        }

        private void UpdateStock(Product product,string operation,int quantity = 1)
        {
            if(operation =="Add")
            {
                product.InStock = product.InStock - quantity;
                productContext.Update(product);
            }
            
        }

        public void RemoveFromBasket(HttpContextBase httpContext, string itemId) {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.Id == itemId);

            if (item != null) {
                basket.BasketItems.Remove(item);
                basketContext.Commit();
            }
        }

        public List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext) {
            Basket basket = GetBasket(httpContext, false);
            if (basket != null)
            {
                var results = (from b in basket.BasketItems
                               join p in productContext.Collection() on b.ProductId equals p.Id
                               select new BasketItemViewModel()
                               {
                                   Id = b.Id,
                                   Quanity = b.Quanity,
                                   ProductName = p.Name,
                                   ProductID = p.Id,
                                   Image = p.Image,
                                   Price = p.Price,
                                   CurrentStock = p.InStock,
                                   DiscountedPrice = p.Price              
                               }
                              ).ToList();

                foreach(var item in results)
                {
                    ItemDiscountInfo IDI = getPriorityItemDiscount(item.ProductID);

                    if(IDI != null && IDI.ItemId == item.ProductID)
                    {
                        item.DiscountedPrice = IDI.discountedPrice;
                    }
                }
                return results;
            }
            else {
                return new List<BasketItemViewModel>();
            }
        }

        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContext) {
            Basket basket = GetBasket(httpContext, false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0, 0);
            if (basket != null)
            {
                int? basketCount = (from item in basket.BasketItems
                                    select item.Quanity).Sum();

                decimal? basketTotal = (from item in basket.BasketItems
                                        join p in productContext.Collection() on item.ProductId equals p.Id
                                        select item.Quanity * p.Price).Sum();

                model.BasketCount = basketCount ?? 0;
                model.BasketTotal = basketTotal ?? decimal.Zero;

                return model;
            }
            else {
                return model;
            }
        }

        public void ClearBasket(HttpContextBase httpContext) {
            Basket basket = GetBasket(httpContext, false);
            basket.BasketItems.Clear();
            basketContext.Commit();
        }

        public bool CheckItemQuantity(HttpContextBase httpContext, string productID)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productID);



            return true;

        }


        public ItemDiscountInfo getPriorityItemDiscount(string Id)
        {
            List<ItemDiscountInfo> prodDiscount = itemDiscountInfoContext.Collection().Where(s => s.ItemId == Id).ToList();
            List<DiscountInfo> discountList = new List<DiscountInfo>();
            DiscountInfo discount = new DiscountInfo();
            foreach (var pd in prodDiscount)
            {
                var temp = discountInfoContext.Collection().Where(s => s.Id == pd.DiscountId && s.ExpiryDate >= DateTime.Now).FirstOrDefault();
                if (temp != null)
                {
                    discountList.Add(temp);
                }
                
            }
            if(discountList.Count >0)
            {
                discount = discountList.OrderByDescending(s => s.Priority).FirstOrDefault();
            }
            

            if(discount == null)
            {
                return null;
            }
            return prodDiscount.Where(a => a.DiscountId == discount.Id).FirstOrDefault();
        }
    }
}
