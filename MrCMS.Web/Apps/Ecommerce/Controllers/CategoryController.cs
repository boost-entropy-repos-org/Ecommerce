﻿using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Ecommerce.Services.Categories;
using MrCMS.Web.Apps.Ecommerce.Services.Products;
using System.Collections.Generic;
using System;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Website;
using MrCMS.Web.Apps.Ecommerce.Settings;
using System.Linq;

namespace MrCMS.Web.Apps.Ecommerce.Controllers
{
    public class CategoryController : MrCMSAppUIController<EcommerceApp>
    {
       private readonly ICategoryService _categoryService;
        private readonly IProductOptionManager _productOptionManager;
        private readonly IProductService _productService;
        private readonly IProductSearchService _productSearchService;

        public CategoryController(ICategoryService categoryService, IProductOptionManager productOptionManager, IProductService productService, IProductSearchService productSearchService)
        {
            _categoryService = categoryService;
            _productOptionManager = productOptionManager;
            _productService = productService;
            _productSearchService = productSearchService;
        }

        public ViewResult Show(Category page)
        {
            ViewBag.ProductOptions = _productOptionManager.GetAllAttributeOptions();
            ViewBag.ProductSpecifications = _productOptionManager.ListSpecificationAttributes();
            ViewBag.ProductPriceRangeMin = 0;
            ViewBag.ProductPriceRangeMax = 5000;
            ViewBag.Categories = _categoryService.GetAll().Where(x => x.Parent != null && x.Parent.Parent == null && x.Products.Count() > 0).ToList();
            if (page.Parent != null && page.Parent.Parent != null)
                ViewBag.IsSubCategory = true;
            else
                ViewBag.IsSubCategory = false;
            return View(page);
        }
        
        [HttpGet]
        public PartialViewResult Results(string searchTerm,string sortBy, string options, string specifications, decimal productPriceRangeMin = 0, decimal productPriceRangeMax = 0, int pageNo = 0, int pageSize = 0, int categoryId = 0)
        {
            List<string> specs = new List<string>();
            if (!String.IsNullOrWhiteSpace(specifications))
            {
                try
                {
                    string[] rawSpecs = specifications.Split(',');
                    foreach (var item in rawSpecs)
                    {
                        if (item != String.Empty)
                            specs.Add(item);
                    }
                }
                catch (Exception)
                {

                }
            }
            List<string> ops = new List<string>();
            if (!String.IsNullOrWhiteSpace(options))
            {
                ops.Add(options);
            }

            ProductPagedList products = new ProductPagedList(_productSearchService.SearchProducts(
                searchTerm,
                sortBy,
                ops,
                specs,
                productPriceRangeMin,
                productPriceRangeMax,
                pageNo == 0 ? 1 : pageNo,
                pageSize == 0 ? Int32.Parse(MrCMSApplication.Get<EcommerceSettings>().CategoryProductsPerPage.Split(',').First()) : pageSize,categoryId), null);

            return PartialView(products);
        }
    }
}