# EShop Application

This project implements a basic e-commerce workflow using ASP.NET Core MVC.

## Features

- Product listing and shopping cart
- User registration and login
- Order creation with email confirmation
- REST API for admin access
- Separate Admin MVC application

## Admin application

The EShop.Admin project communicates with the main application via HTTP API and provides:

- Order listing and order details
- Import users from Excel (.xlsx)
- Export all orders to Excel (.xlsx)
- Generate invoice PDF for a single order

## Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- SQLite (local database)
- Identity for authentication
- HttpClient for API communication
- ClosedXML for Excel export
- ExcelDataReader for Excel import
- QuestPDF for PDF invoices

## Running the solution

1. Run `EShop.Web` first (API + main site)
2. Run `EShop.Admin` second (admin panel)
3. Admin communicates with `/api/OrderApi/...`

