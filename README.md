# kavyasCreation - Multi-Vendor B2B/B2C Marketplace

An enterprise-grade e-commerce platform built with **Three-Tier Clean Architecture**, featuring advanced multi-vendor marketplace capabilities, secure authentication, and comprehensive business management.

## 🏗️ Architecture

The platform follows **Clean Architecture** principles with three distinct layers:

- **Core (Domain Layer)**: Business entities, interfaces, and constants
- **Infra (Infrastructure Layer)**: Data access, repositories, and external services
- **Web (Presentation Layer)**: ASP.NET Core MVC/Razor Pages UI

📘 **[Read Full Architecture Documentation](THREE_TIER_ARCHITECTURE.md)**

## ✨ Key Features

### Multi-Vendor Marketplace
- **Vendors** can register, get approved, and sell their products
- **Buyer Companies** can purchase from approved vendors in bulk
- **Individual Customers** can shop directly from the platform
- **Admin** controls all approvals and platform management

### Role-Based Access Control (6 Roles)
- **Admin**: Platform administrator with full control
- **VendorAdmin**: Manages vendor company and users
- **VendorUser**: Can manage products and view orders
- **BuyerAdmin**: Manages buyer company and users
- **BuyerUser**: Can browse and purchase from approved vendors
- **Customer**: Individual shoppers

### Business Features
- Product catalog with categories and specifications
- Advanced inventory management with stock reservations
- Shopping cart with session-based storage
- Order processing with payment integration
- User profiles and multiple shipping addresses
- Wishlist functionality
- Product reviews and ratings

### Technical Features
- ✅ Global error handling middleware
- ✅ Rate limiting (100 requests/min per user)
- ✅ Security headers (XSS, clickjacking protection)
- ✅ Response compression
- ✅ In-memory caching
- ✅ Database connection resilience
- ✅ Health check endpoints
- ✅ Approval workflows with audit trails
- ✅ Soft delete pattern

## Recent Improvements (v1.1)

This version includes significant infrastructure, security, and performance enhancements:

### ✨ Key Features
- **Global Error Handling**: Comprehensive exception handling with environment-aware responses
- **Rate Limiting**: Protection against abuse with configurable request limits (100/min per user)
- **Security Headers**: XSS protection, clickjacking prevention, MIME sniffing protection
- **Response Compression**: Reduced bandwidth usage for better performance
- **Caching Service**: Generic in-memory caching for frequently accessed data
- **Database Resilience**: Automatic retry logic for transient database failures
- **Health Checks**: `/health` endpoint for monitoring and DevOps integration
- **CORS Support**: Configurable cross-origin resource sharing
- **Enhanced Configuration**: All timeouts and limits moved to appsettings.json

### 📋 Documentation
- [IMPROVEMENTS_SUMMARY.md](IMPROVEMENTS_SUMMARY.md) - Performance and security improvements
- [SECURITY_SUMMARY.md](SECURITY_SUMMARY.md) - Security audit and best practices
- [THREE_TIER_ARCHITECTURE.md](THREE_TIER_ARCHITECTURE.md) - Complete architecture guide

## Technology Stack
- ASP.NET Core 10.0
- Entity Framework Core
- SQL Server
- ASP.NET Identity + OAuth (Google, Facebook)
- Bootstrap 5
- Clean Architecture Pattern
- Repository Pattern
- Unit of Work Pattern

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone https://github.com/santhoshgedare/kavyasCreation.git
cd kavyasCreation
```

2. Configure the database connection in `Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=kavyasCreation;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

3. Set up User Secrets for OAuth credentials and admin password:
```bash
cd Web
dotnet user-secrets set "SeedAdmin:Password" "YourSecurePassword123!"
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
```

4. Run database migrations:
```bash
dotnet ef database update --project Infra --startup-project Web
```

5. Start the application:
```bash
dotnet run --project Web
```

6. Access the application:
- Application: `https://localhost:5001`
- Health check: `https://localhost:5001/health`
- Admin login: `admin@local` / (password from user secrets)

## Configuration

Key settings in `Web/appsettings.json`:

```json
{
  "StockManagement": {
    "ReservationExpirationMinutes": 15,
    "CleanupIntervalMinutes": 5
  },
  "Session": {
    "IdleTimeoutMinutes": 30,
    "CookieHttpOnly": true
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60
  },
  "Cache": {
    "DefaultExpirationMinutes": 30
  }
}
```

See [THREE_TIER_ARCHITECTURE.md](THREE_TIER_ARCHITECTURE.md) for complete configuration reference.

## Project Structure

```
kavyasCreation/
├── Core/                          # Domain layer (business logic)
│   ├── Entities/                 # Domain entities
│   ├── Interfaces/               # Repository interfaces
│   └── Constants/                # System constants (Roles, etc.)
├── Infra/                        # Infrastructure layer (data access)
│   ├── Data/                     # DbContext and migrations
│   ├── Repositories/             # Repository implementations
│   └── Services/                 # Infrastructure services
├── Web/                          # Presentation layer (UI)
│   ├── Areas/                    # Feature areas
│   │   ├── Admin/               # Admin management
│   │   ├── Store/               # Customer store
│   │   ├── Account/             # User profile
│   │   └── Identity/            # Authentication
│   ├── Controllers/             # MVC controllers
│   ├── Pages/                   # Razor pages
│   ├── Services/                # Application services
│   ├── Middleware/              # Custom middleware
│   └── wwwroot/                 # Static files
└── Documentation/               # Markdown documentation
```

## Entity Relationships

```
Vendor (1) ─────< VendorUser (*)
   │
   │ (*)
   └─────< VendorBuyerRelationship >─────┐
                                         │ (*)
BuyerCompany (1) ─────< BuyerUser (*)    │
   │                                     │
   └─────────────────────────────────────┘
   
Product (*) ───> Vendor (1) [nullable]
Order (*) ───> BuyerCompany (1) [nullable]
```

## API Endpoints

### Health Check
- `GET /health` - Application health status

### Admin Area
- `/Admin/Dashboard` - Admin dashboard
- `/Admin/Products` - Product management
- `/Admin/Inventory` - Stock management
- `/Admin/Categories` - Category management

### Store Area
- `/Store/Catalog` - Product browsing
- `/Store/Cart` - Shopping cart
- `/Store/Payment` - Checkout

### Account Area
- `/Account/Profile` - User profile management
- `/Account/Profile/Addresses` - Address management

## Default Roles

All roles are automatically seeded on application startup:

1. **Admin** - Platform administrator
2. **VendorAdmin** - Vendor company administrator
3. **VendorUser** - Vendor employee
4. **BuyerAdmin** - Buyer company administrator
5. **BuyerUser** - Buyer company employee
6. **Customer** - Individual shopper

## Security Features

- ✅ **Authentication**: ASP.NET Identity + OAuth (Google, Facebook)
- ✅ **Authorization**: Role-based access control
- ✅ **Rate Limiting**: 100 requests/minute per user or IP
- ✅ **Security Headers**: XSS, clickjacking, MIME sniffing protection
- ✅ **HTTPS**: Enforced with HSTS
- ✅ **Session Security**: HttpOnly cookies, configurable timeout
- ✅ **Input Validation**: Data annotations on all entities
- ✅ **CodeQL Scan**: 0 vulnerabilities found

See [SECURITY_SUMMARY.md](SECURITY_SUMMARY.md) for detailed security audit.

## Performance Optimizations

- Response compression for HTTPS
- In-memory caching with configurable expiration
- Database connection retry logic
- Async/await throughout
- Entity Framework query optimization
- Static asset optimization

## Best Practices

✅ **Clean Architecture**: Clear separation of concerns  
✅ **Repository Pattern**: Abstracted data access  
✅ **Unit of Work**: Transaction management  
✅ **Dependency Injection**: Loosely coupled components  
✅ **Async Programming**: Non-blocking operations  
✅ **Soft Deletes**: Data retention for auditing  
✅ **Approval Workflows**: Multi-step verification  
✅ **Comprehensive Logging**: Structured logging with ILogger  
✅ **Error Handling**: Global exception middleware  
✅ **Documentation**: XML comments and markdown docs  

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions:
- Create an issue on GitHub
- Check the documentation in the `/docs` folder
- Review [THREE_TIER_ARCHITECTURE.md](THREE_TIER_ARCHITECTURE.md) for architecture details

## Roadmap

- [ ] Admin UI for vendor management
- [ ] Vendor onboarding and dashboard
- [ ] Buyer company registration and dashboard
- [ ] Advanced reporting and analytics
- [ ] Multi-currency support
- [ ] API layer with Swagger documentation
- [ ] Unit and integration tests
- [ ] Performance monitoring with Application Insights

---

**Current Status:** ✅ Core infrastructure complete | 🚀 Ready for marketplace features

**Build Status:** ✅ Successful (0 errors)  
**Security Scan:** ✅ 0 vulnerabilities (CodeQL)  
**Architecture:** ✅ Three-tier clean architecture implemented