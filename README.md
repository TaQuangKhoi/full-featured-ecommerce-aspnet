# E-Commerce Platform - ASP.NET Core 8 MVC

A production-ready e-commerce platform built with **ASP.NET Core 8 MVC**, **Clean Architecture**, **Tailwind CSS**, and **HTMX/Alpine.js**.

## 🏗️ Architecture

```
ECommerce/
├── src/
│   ├── ECommerce.Web/              # MVC Controllers + Razor Views + Tailwind CSS
│   ├── ECommerce.Application/      # MediatR CQRS Commands/Queries + AutoMapper + FluentValidation
│   ├── ECommerce.Domain/           # Entities + Value Objects + Interfaces
│   ├── ECommerce.Infrastructure/   # EF Core DbContext + Repositories + Identity
│   └── ECommerce.Shared/           # DTOs + Enums
├── tests/
│   └── ECommerce.Tests/            # xUnit + Moq + FluentAssertions
├── docker-compose.yml
└── ECommerce.slnx
```

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or Docker)

### Option 1: Docker (Recommended)
```bash
docker-compose up -d
# App available at http://localhost:5000
```

### Option 2: Local Development
```bash
# Update connection string in src/ECommerce.Web/appsettings.json
dotnet build ECommerce.slnx
dotnet run --project src/ECommerce.Web
# App available at https://localhost:5001
```

### Option 3: Using EF Core InMemory (No SQL Server needed)
Update `src/ECommerce.Infrastructure/DependencyInjection.cs` to use InMemory provider for quick testing.

## 🔑 Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@ecommerce.com | Admin123! |

## 🛒 Features

### Customer Side
- ✅ Product catalog with categories, filters, and search
- ✅ Product details page
- ✅ Shopping cart (localStorage + API sync)
- ✅ Checkout (registered users)
- ✅ Order history
- ✅ User authentication (Register/Login)

### Admin Side
- ✅ Dashboard analytics (orders, revenue, products, customers)
- ✅ Product CRUD management
- ✅ Order management with status tracking
- ✅ Category management

## 🔧 Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | ASP.NET Core 8 MVC, Razor Views, Tailwind CSS 3.4, HTMX, Alpine.js, SweetAlert2 |
| Backend | ASP.NET Core 8, MediatR (CQRS), AutoMapper, FluentValidation |
| Database | Entity Framework Core 8, SQL Server (Code-First) |
| Auth | ASP.NET Identity, Role-based authorization |
| Logging | Serilog (Console + File) |
| API Docs | Swagger/OpenAPI |
| Testing | xUnit, Moq, FluentAssertions |
| DevOps | Docker, docker-compose |

## 🧪 Running Tests
```bash
dotnet test ECommerce.slnx
```

## 📊 Database Schema

- **Products**: Id, Name, Description, Price, Stock, ImageUrl, CategoryId, IsActive
- **Categories**: Id, Name, Slug, ParentId (self-referencing)
- **Orders**: Id, CustomerId, Status, Total, ShippingAddress, OrderDate
- **OrderItems**: Id, OrderId, ProductId, Quantity, UnitPrice
- **AspNetUsers**: ASP.NET Identity user table (Email, PasswordHash, etc.)

## 🔐 Security
- ASP.NET Identity with cookie authentication
- Role-based authorization (Customer/Admin)
- Anti-forgery token validation on all POST forms
- Input validation with FluentValidation
- Rate limiting (100 requests/minute per IP)
- HTTPS redirect
- Serilog structured logging

## 📁 API Documentation
Swagger UI available at `/swagger` in development mode.

## License
MIT