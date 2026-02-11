# kavyasCreation

An enterprise-grade e-commerce platform built with ASP.NET Core, featuring advanced inventory management, secure authentication, and comprehensive user profile management.

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
See [IMPROVEMENTS_SUMMARY.md](IMPROVEMENTS_SUMMARY.md) for detailed information about the improvements.

## Technology Stack
- ASP.NET Core 10.0
- Entity Framework Core
- SQL Server
- ASP.NET Identity + OAuth (Google, Facebook)
- Bootstrap 5

## Getting Started

1. Configure the database connection in `appsettings.json`
2. Set up User Secrets for OAuth credentials and admin password
3. Run database migrations: `dotnet ef database update`
4. Start the application: `dotnet run`
5. Access the health endpoint: `https://localhost:5001/health`

## Configuration

Key settings in `appsettings.json`:
- **StockManagement**: Reservation timeouts and cleanup intervals
- **Session**: Session timeout and security settings
- **RateLimiting**: Request limits and queue size
- **CORS**: Allowed origins and credentials

See [IMPROVEMENTS_SUMMARY.md](IMPROVEMENTS_SUMMARY.md) for complete configuration reference.