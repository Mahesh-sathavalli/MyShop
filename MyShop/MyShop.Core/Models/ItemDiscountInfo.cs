using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.Models
{
    public class ItemDiscountInfo:BaseEntity
    {
        public string DiscountId { get; set; }
        public string ItemId { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal discountedPrice { get; set; }       
    }
}
