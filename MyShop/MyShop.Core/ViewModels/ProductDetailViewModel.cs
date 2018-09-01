using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.Models;

namespace MyShop.Core.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<ProductCategory> ProductCategories { get; set; }
        public Customer Customer { get; set; }

        public ItemDiscountInfo ProductDiscount { get; set; }
    }
}
