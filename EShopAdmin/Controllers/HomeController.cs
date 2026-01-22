using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EShopAdmin.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using EShop.Domain.DTO;
namespace EShopAdmin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ImportUsers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportUsers(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Please select an Excel file.";
                return View();
            }

            var users = new List<UserDTO>();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
            {
                int row = 0;

                do
                {
                    while (reader.Read())
                    {
                        if (row == 0)
                        {
                            row++;
                            continue;
                        }

                        var email = reader.GetValue(0)?.ToString();
                        var password = reader.GetValue(1)?.ToString();

                        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
                        {
                            users.Add(new UserDTO
                            {
                                Email = email,
                                Password = password,
                                ConfirmPassword = password
                            });
                        }

                        row++;
                    }
                }
                while (reader.NextResult());
            }

            if (users.Count == 0)
            {
                ViewBag.Error = "No valid users found in the file.";
                return View();
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5161");

                var response = await client.PostAsJsonAsync("/api/Admin/ImportUsers", users);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = $"Successfully processed {users.Count} users.";
                }
                else
                {
                    ViewBag.Error = $"API error: {response.StatusCode}";
                }
            }

            return View();
        }


        public async Task<IActionResult> ExportOrders()
        {
            List<OrderDTO>? orders;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5161");

                // This calls your Web API: /api/OrderApi
                var response = await client.GetAsync("/api/OrderApi");

                if (!response.IsSuccessStatusCode)
                {
                    // simple fallback: go back home with error
                    TempData["Error"] = $"Cannot load orders from API. Status: {response.StatusCode}";
                    return RedirectToAction("Index", "Order");
                }

                orders = await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            }

            if (orders == null || orders.Count == 0)
            {
                TempData["Error"] = "No orders to export.";
                return RedirectToAction("Index", "Order");
            }

            // figure out max products per order for dynamic columns
            var maxProducts = orders.Max(o => o.Products?.Count ?? 0);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Orders");

                // header
                int col = 1;
                ws.Cell(1, col++).Value = "Order Id";
                ws.Cell(1, col++).Value = "User Email";

                for (int i = 0; i < maxProducts; i++)
                {
                    ws.Cell(1, col++).Value = $"Product-{i + 1}";
                }

                ws.Cell(1, col).Value = "Total Price";

                // data
                int row = 2;
                foreach (var order in orders)
                {
                    col = 1;
                    ws.Cell(row, col++).Value = order.Id.ToString();
                    ws.Cell(row, col++).Value = order.UserEmail;

                    var products = order.Products ?? new List<ProductInOrderDTO>();
                    for (int i = 0; i < maxProducts; i++)
                    {
                        if (i < products.Count)
                        {
                            var p = products[i];
                            ws.Cell(row, col++).Value =
                                $"{p.ProductName} x{p.Quantity} = {p.LineTotal}";
                        }
                        else
                        {
                            col++; // skip empty cell
                        }
                    }

                    ws.Cell(row, col).Value = order.TotalPrice;
                    row++;
                }

                // some basic styling
                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    var fileName = $"Orders_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                    const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    return File(content, contentType, fileName);
                }
            }
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
