using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.Models
{
    public class DiscountInfo:BaseEntity
    {
        public string Name { get; set; }
        public int Code { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Amount { get; set; }
        public decimal Percentage { get; set; }
        public int AppliedType { get; set; }
        public string ItemId { get; set; }
        public int ItemType { get; set; }

        public int Priority { get; set; }

        public DiscountInfo()
        {
            this.ExpiryDate = DateTime.Now;
        }
    }

   

    public enum ItemType {
        Product,
        productCategory
    }

}
