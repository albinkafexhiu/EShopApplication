using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using EShop.Domain.DTO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace EShopAdmin.Controllers
{
    public class OrderController : Controller
    {
        private readonly string _apiBaseUrl = "http://localhost:5161";

        public async Task<IActionResult> Index()
        {
            List<OrderDTO>? orders;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync("/api/OrderApi");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"API error: {response.StatusCode}";
                    return View(new List<OrderDTO>());
                }

                orders = await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            }

            return View(orders ?? new List<OrderDTO>());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            OrderDTO? order;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync($"/api/OrderApi/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                order = await response.Content.ReadFromJsonAsync<OrderDTO>();
            }

            if (order == null)
                return RedirectToAction(nameof(Index));

            return View(order);
        }

        // ===========================
        // GENERATE INVOICE (PDF)
        // ===========================
        public async Task<IActionResult> GenerateInvoice(Guid id)
        {
            OrderDTO? order;

            // 1) Load order data from EShop.Web API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync($"/api/OrderApi/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Cannot load order {id}";
                    return RedirectToAction(nameof(Index));
                }

                order = await response.Content.ReadFromJsonAsync<OrderDTO>();
            }

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var products = order.Products ?? new List<ProductInOrderDTO>();

            // 2) Create PDF document with QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header()
                        .Text("EShop Invoice")
                        .FontSize(24)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Order Id: {order.Id}");
                        col.Item().Text($"Customer: {order.UserEmail}");
                        col.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}");

                        col.Item().LineHorizontal(1);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); // product
                                columns.RelativeColumn(1); // qty
                                columns.RelativeColumn(2); // price
                                columns.RelativeColumn(2); // total
                            });

                            // header row
                            table.Header(header =>
                            {
                                header.Cell().Text("Product").SemiBold();
                                header.Cell().Text("Qty").SemiBold();
                                header.Cell().Text("Price").SemiBold();
                                header.Cell().Text("Line Total").SemiBold();
                            });

                            // product rows
                            foreach (var p in products)
                            {
                                table.Cell().Text(p.ProductName ?? "");
                                table.Cell().Text(p.Quantity.ToString());
                                table.Cell().Text(p.Price.ToString("0.00"));
                                table.Cell().Text(p.LineTotal.ToString("0.00"));
                            }

                            // footer row with total
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(3).AlignRight().Text("Total:");
                                footer.Cell().Text(order.TotalPrice.ToString("0.00"));
                            });
                        });

                        col.Item().LineHorizontal(1);
                        col.Item().Text("Thank you for your purchase!").Italic();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("EShopAdmin • ");
                            text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                        })
                        ;
                });
            });

            var pdfBytes = document.GeneratePdf();
            var fileName = $"Invoice_{order.Id}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
