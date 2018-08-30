using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Core.ViewModels
{
    public class PaymentInfoViewModel
    {
        [Required]
        [MinLength(16,ErrorMessage ="Enter Valid Number")]
        [MaxLength(19)]
        public string Number { get; set; }


        [Required]
        [Range(1, 12, ErrorMessage = "Please enter valid Expiry Month")]
        public int ExpiryMonth { get; set; }


        [Required]
        [Range(2018, int.MaxValue, ErrorMessage = "Please enter valid Expiry Year")]
        public int ExpiryYear { get; set; }


        [Required]
        [RegularExpression("^[0-9]{3,3}", ErrorMessage = "Please enter valid CVV")]
        public int CVV { get; set; }


        [Required]
        public string Name { get; set; }


        [Required]
        public string CardType { get; set; }
      
        public string  OrderID { get; set; }

    }
}
