using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.WebUI.Controllers;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System.Web.Mvc;
using MyShop.Core.ViewModels;
using System.Linq;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void IndexPageDoesReturnProducts()
        {
            IRepository<Product> productContext = new Mocks.MockContext<Product>();
            IRepository<ProductCategory> productCatgeoryContext = new Mocks.MockContext<ProductCategory>();
            IRepository<Customer> customers = new Mocks.MockContext<Customer>();
            IRepository<PaymentInfo> paymentInfo = new Mocks.MockContext<PaymentInfo>();
            productContext.Insert(new Product());

            HomeController controller = new HomeController(productContext, productCatgeoryContext, customers, paymentInfo);

            var result = controller.Index() as ViewResult;
            var viewModel = (ProductListViewModel)result.ViewData.Model;

            Assert.AreEqual(1, viewModel.Products.Count());

        }
    }
}
