﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.ViewModels
{
    public class BasketItemViewModel
    {
        public string Id { get; set; }
        public int Quanity { get; set; }
        public string ProductName { get; set; }

        public string ProductID { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        public int CurrentStock { get; set; }

        public decimal DiscountedPrice { get; set; }
    }
}
