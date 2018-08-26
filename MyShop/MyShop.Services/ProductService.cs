using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.ViewModels;
using MyShop.Core.Contracts;
using MyShop.Core.Models;

namespace MyShop.Services
{
    public class ProductService : IProductService
    {
        IRepository<Product> productContext;

        public Product getProduct(string productID)
        {
            Product p = productContext.Find(productID);
            return p;
        }
    }
}
