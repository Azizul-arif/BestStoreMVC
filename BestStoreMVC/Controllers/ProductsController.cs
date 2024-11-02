using BestStoreMVC.DTO;
using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly int pageSize = 5;
        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index(int pageIndex,string? search,string? column,string? orderBy)
        {
            IQueryable<Product> query = _context.Products;

            //search functionality
            if(search is not null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            //sort functionality
            string[] validColumns = { "Id", "Name", "Brand", "Price", "Category", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };
            if(!validColumns.Contains(column))
            {
                column = "Id";
            }
            if(!validOrderBy.Contains(orderBy))
            {
                orderBy= "desc";
            }
            if(column=="Name")
            {
                if(orderBy== "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
                
            }
            else if(column=="Brand")
            {
                if(orderBy=="asc")
                {
                    query=query.OrderBy(p => p.Brand);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Brand);
                }
            }
            else if(column== "Price")
            {
                if(orderBy=="asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }
            else if (column== "Category")
            {
                if(orderBy=="asc")
                {
                    query=query.OrderBy(p => p.Category);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Category);
                }
            }
            else if (column== "CreatedAt")
            {
                if(orderBy=="asc")
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }
            else
            {
                if(orderBy=="asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }

            //query = query.OrderByDescending(p => p.Id);
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var products = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            ViewData["Search"] = search ?? "";
            ViewData["Coulumn"] = column;
            ViewData["OrderBy"] = orderBy;
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ProductDTO productDTO)
        {
            if (productDTO.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image cant be null");
            }
            if (!ModelState.IsValid)
            {
                return View(productDTO);
            }
            //save the image file
            string newFileName = DateTime.Now.ToString("yyyyMMddhhmmssff");
            newFileName = newFileName + Path.GetExtension(productDTO.ImageFile.FileName);
            string imageFullPath = _webHostEnvironment.WebRootPath + "/products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDTO.ImageFile.CopyTo(stream);
            }

            //save the product in database
            Product product = new Product()
            {
                Name = productDTO.Name,
                Brand = productDTO.Brand,
                Category = productDTO.Category,
                Price = productDTO.Price,
                Description = productDTO.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index", "Products");

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product is null)
            {
                return Content("Product not found");
            }
            var productDTo = new ProductDTO()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };
            ViewData["productId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
            return View(productDTo);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDTO productDTO)
        {
            var product = _context.Products.Find(id);
            if (product is null)
            {
                return Content("Product not found");
            }
            var productDTo = new ProductDTO()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };
            if (!ModelState.IsValid)
            {
                ViewData["productId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                return View(productDTo);
            }
            //update the image file if we have a new image file
            string newFileName = product.ImageFileName;
            if (productDTo.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssff");
                newFileName = newFileName + Path.GetExtension(productDTo.ImageFile.FileName);
                string ImageFullPath = _webHostEnvironment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(ImageFullPath))
                {
                    productDTO.ImageFile.CopyTo(stream);
                }
                //delete the old image
                string oldImageFullPath = _webHostEnvironment.EnvironmentName + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }
            //update product in database
            product.Name = productDTo.Name;
            product.Brand = productDTo.Brand;
            product.Category = productDTo.Category;
            product.Price = productDTo.Price;
            product.Description = productDTo.Description;
            product.ImageFileName = newFileName;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product is null)
            {
                return Content("Product Not Found");
            }
            string imageFullPath = _webHostEnvironment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }
    }


}
