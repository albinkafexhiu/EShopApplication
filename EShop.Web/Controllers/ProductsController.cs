using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Service;

namespace EShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _cartService;

        public ProductsController(IProductService productService, IShoppingCartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        // GET: Products
        public IActionResult Index()
        {
            var products = _productService.GetAllProducts();
            return View(products);
        }

        // GET: Products/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var product = _productService.GetProductById(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            product.Id = Guid.NewGuid();
            _productService.CreateNewProduct(product);

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var product = _productService.GetProductById(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid) return View(product);

            _productService.UpdateProduct(product);

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var product = _productService.GetProductById(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _productService.DeleteProduct(id);
            return RedirectToAction(nameof(Index));
        }

        // ================== ADD TO CART ==================

        // GET: Products/AddToCart/5  -> show form (name + quantity)
        [Authorize]
        public IActionResult AddToCart(Guid id)
        {
            var dto = _cartService.GetProductInfo(id);
            return View(dto);
        }

        // POST: Products/AddToCart
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(AddToCartDTO model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge(); // redirect to login
            }

            var userId = Guid.Parse(userIdString);
            _productService.AddToCart(model, userId);

            return RedirectToAction(nameof(Index));
        }
    }
}
