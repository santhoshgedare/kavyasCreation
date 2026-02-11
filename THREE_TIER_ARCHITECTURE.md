# Three-Tier Clean Architecture - Multi-Vendor Marketplace

## Overview

The kavyasCreation platform has been refactored into a three-tier clean architecture to support a multi-vendor B2B marketplace. The system allows:

- **Admins** to manage the platform, approve vendors and buyer companies
- **Vendors** to sell their products to buyer companies
- **Buyer Companies** to purchase from approved vendors
- **Individual Customers** to browse and purchase products

## Architecture Layers

### 1. Core Layer (Domain Layer)
**Location:** `Core/`

The innermost layer containing business entities, interfaces, and business logic. This layer has NO dependencies on other layers.

**Components:**
- **Entities** (`Core/Entities/`): Domain models representing business concepts
  - `Vendor`: Vendor companies selling products
  - `VendorUser`: Users belonging to vendors
  - `BuyerCompany`: Companies purchasing products
  - `BuyerUser`: Users belonging to buyer companies
  - `VendorBuyerRelationship`: Many-to-many relationship between vendors and buyers
  - `Product`: Products sold by vendors
  - `Order`: Purchase orders from buyers
  - `Category`, `UserProfile`, `UserAddress`, etc.

- **Interfaces** (`Core/Interfaces/`): Repository contracts
  - `IVendorRepository`, `IVendorUserRepository`
  - `IBuyerCompanyRepository`, `IBuyerUserRepository`
  - `IVendorBuyerRelationshipRepository`
  - `IProductRepository`, `IOrderRepository`, etc.
  - `IUnitOfWork`: Orchestrates repository operations

- **Constants** (`Core/Constants/`): System-wide constants
  - `Roles`: Defines all role names (Admin, VendorAdmin, VendorUser, BuyerAdmin, BuyerUser, Customer)

**Key Principles:**
- Domain entities use validation attributes
- Entities implement `ISoftDelete` for soft deletion pattern
- No infrastructure or UI dependencies

### 2. Infrastructure Layer (Data Access Layer)
**Location:** `Infra/`

Implements data access and external service integrations. Depends only on Core layer.

**Components:**
- **Data** (`Infra/Data/`):
  - `AppDbContext`: EF Core DbContext for application data
  - `DbInitializer`: Seeds initial data
  - `Migrations/`: EF Core migration files

- **Repositories** (`Infra/Repositories/`): Concrete implementations of Core interfaces
  - `VendorRepository`, `VendorUserRepository`
  - `BuyerCompanyRepository`, `BuyerUserRepository`
  - `VendorBuyerRelationshipRepository`
  - `ProductRepository`, `OrderRepository`, etc.
  - `UnitOfWork`: Implements `IUnitOfWork`

- **Services** (`Infra/Services/`):
  - `InventoryService`: Stock management and reservation logic

**Key Principles:**
- Repository pattern for data access
- Unit of Work pattern for transaction management
- Async/await throughout
- Entity Framework Core with SQL Server

### 3. Presentation Layer (Web/UI Layer)
**Location:** `Web/`

ASP.NET Core MVC/Razor Pages application. Depends on Core and Infra layers.

**Components:**
- **Areas**:
  - `Admin/`: Admin dashboard and management
  - `Store/`: Customer-facing store
  - `Account/`: User profile management
  - `Identity/`: Authentication pages

- **Controllers** (`Web/Controllers/`): MVC controllers
- **Pages** (`Web/Pages/`): Razor Pages
- **Services** (`Web/Services/`):
  - `CartService`: Shopping cart management
  - `CacheService`: In-memory caching
  - `StockReservationCleanupService`: Background service

- **Middleware** (`Web/Middleware/`):
  - `GlobalExceptionHandlerMiddleware`: Centralized error handling

**Key Principles:**
- Dependency injection for all services
- Rate limiting for API protection
- Response compression for performance
- Security headers for protection

## Domain Model - Entity Relationships

```
┌─────────────────┐
│     Vendor      │
│  (Company)      │
└────────┬────────┘
         │ 1
         │
         │ *
┌────────▼────────┐         ┌──────────────────────┐
│   VendorUser    │         │  VendorBuyer         │
│                 │         │  Relationship        │
└─────────────────┘         └──────────┬───────────┘
                                       │
                            ┌──────────┴───────────┐
                            │                      │
                      ┌─────▼──────┐      ┌────────▼────────┐
                      │  Vendor    │      │  BuyerCompany   │
                      └────────────┘      └────────┬────────┘
                                                   │ 1
                                                   │
                                                   │ *
                                          ┌────────▼────────┐
                                          │   BuyerUser     │
                                          └─────────────────┘

┌─────────────────┐
│    Product      │◄──────── VendorId (nullable)
└────────┬────────┘
         │ *
         │
         │ 1
┌────────▼────────┐
│     Order       │◄──────── BuyerCompanyId (nullable)
└─────────────────┘
```

### Entity Details

#### Vendor
- Company information (name, contact, address)
- Tax ID and registration number
- Approval workflow (`IsApproved`, `ApprovedBy`, `ApprovedAt`)
- Active status
- Soft delete support

#### VendorUser
- Links to AspNetUsers (Identity)
- Belongs to one Vendor
- Admin flag (VendorAdmin vs VendorUser role)
- Active status
- Soft delete support

#### BuyerCompany
- Company information (name, contact, address)
- Tax ID and registration number
- Approval workflow
- Active status
- Soft delete support

#### BuyerUser
- Links to AspNetUsers (Identity)
- Belongs to one BuyerCompany
- Admin flag (BuyerAdmin vs BuyerUser role)
- Department and job title
- Soft delete support

#### VendorBuyerRelationship
- Many-to-many between Vendor and BuyerCompany
- Approval workflow (relationship must be approved)
- Active status
- Notes field for relationship details

## Role-Based Access Control (RBAC)

### Roles Hierarchy

```
Admin (Platform Administrator)
  └─ Manages entire platform
  └─ Approves vendors and buyer companies
  └─ Manages vendor-buyer relationships
  └─ Can perform any action

VendorAdmin
  └─ Manages their vendor company
  └─ Can add/manage VendorUsers
  └─ Can manage products for their vendor
  └─ Can view orders for their products

VendorUser
  └─ Can manage products for their vendor
  └─ Can view orders for their products
  └─ Limited administrative capabilities

BuyerAdmin
  └─ Manages their buyer company
  └─ Can add/manage BuyerUsers
  └─ Can create purchase orders
  └─ Can select vendors

BuyerUser
  └─ Can browse vendor products
  └─ Can create purchase orders
  └─ Can view company orders

Customer (Individual)
  └─ Can browse all products
  └─ Can make individual purchases
  └─ Not part of any company
```

### Role Assignment

Roles are defined in `Core/Constants/Roles.cs`:

```csharp
public static class Roles
{
    public const string Admin = "Admin";
    public const string VendorAdmin = "VendorAdmin";
    public const string VendorUser = "VendorUser";
    public const string BuyerAdmin = "BuyerAdmin";
    public const string BuyerUser = "BuyerUser";
    public const string Customer = "Customer";
}
```

All roles are automatically seeded during application startup in `Program.cs`.

## Approval Workflows

### Vendor Approval
1. Vendor company registers (or Admin creates)
2. `IsApproved = false` initially
3. Admin reviews and approves/rejects
4. Sets `IsApproved = true`, `ApprovedBy = adminUserId`, `ApprovedAt = DateTime.UtcNow`
5. Only approved vendors can sell products

### Buyer Company Approval
1. Buyer company registers (or Admin creates)
2. `IsApproved = false` initially
3. Admin reviews and approves/rejects
4. Only approved buyer companies can purchase

### Vendor-Buyer Relationship Approval
1. Either vendor or buyer requests relationship
2. `IsApproved = false` initially
3. Admin or counterparty approves
4. Both parties can see each other's products/orders

## Database Schema

### New Tables

**Vendors**
- Id (PK)
- CompanyName, Description
- ContactEmail, ContactPhone
- Address, City, State, PostalCode, Country
- TaxId, RegistrationNumber
- IsActive, IsApproved, ApprovedAt, ApprovedBy
- CreatedAt, LastUpdatedAt
- IsDeleted, DeletedAt

**VendorUsers**
- Id (PK)
- VendorId (FK to Vendors)
- UserId (FK to AspNetUsers - string)
- FirstName, LastName, Email, PhoneNumber
- JobTitle
- IsAdmin, IsActive
- CreatedAt, LastUpdatedAt
- IsDeleted, DeletedAt

**BuyerCompanies**
- Id (PK)
- CompanyName, Description
- ContactEmail, ContactPhone
- Address, City, State, PostalCode, Country
- TaxId, RegistrationNumber
- IsActive, IsApproved, ApprovedAt, ApprovedBy
- CreatedAt, LastUpdatedAt
- IsDeleted, DeletedAt

**BuyerUsers**
- Id (PK)
- BuyerCompanyId (FK to BuyerCompanies)
- UserId (FK to AspNetUsers - string)
- FirstName, LastName, Email, PhoneNumber
- JobTitle, Department
- IsAdmin, IsActive
- CreatedAt, LastUpdatedAt
- IsDeleted, DeletedAt

**VendorBuyerRelationships**
- Id (PK)
- VendorId (FK to Vendors)
- BuyerCompanyId (FK to BuyerCompanies)
- IsActive, IsApproved, ApprovedAt, ApprovedBy
- Notes
- CreatedAt, LastUpdatedAt

### Updated Tables

**Products**
- Added: VendorId (nullable FK to Vendors)

**Orders**
- Added: BuyerCompanyId (nullable FK to BuyerCompanies)

## Best Practices Implemented

### 1. Clean Architecture Principles
✅ **Dependency Rule**: Dependencies point inward
- Web → Infra → Core
- Core has no dependencies

✅ **Separation of Concerns**: Each layer has distinct responsibility

✅ **Testability**: Business logic in Core can be unit tested independently

### 2. Repository Pattern
✅ **Abstraction**: Interfaces in Core, implementations in Infra

✅ **Unit of Work**: Single transaction boundary

✅ **Async Operations**: All data access is async

### 3. Security
✅ **Role-Based Access Control**: Six distinct roles

✅ **Approval Workflows**: Multi-step verification for vendors and buyers

✅ **Soft Deletes**: Data retention for audit trails

✅ **Input Validation**: Data annotations on all entities

### 4. Data Integrity
✅ **Foreign Keys**: Proper relationships between entities

✅ **Unique Constraints**: Prevent duplicate company names

✅ **Nullable FKs**: Backward compatibility (Products without vendors, Orders without buyer companies)

### 5. Performance
✅ **Caching**: ICacheService for frequently accessed data

✅ **Lazy Loading**: Navigation properties loaded on demand

✅ **Indexes**: On frequently queried columns (CompanyName, UserId, etc.)

✅ **AsNoTracking**: Read-only queries don't track changes

### 6. Maintainability
✅ **Constants Class**: Centralized role definitions

✅ **XML Documentation**: All public methods documented

✅ **Consistent Naming**: Clear, descriptive names throughout

✅ **DRY Principle**: Reusable repository methods

## Future Enhancements

### 1. Business Logic Layer
- Add domain services for complex business rules
- Implement specification pattern for complex queries
- Add domain events for decoupled communication

### 2. API Layer
- Add RESTful API controllers
- Implement API versioning
- Add Swagger/OpenAPI documentation
- Implement rate limiting per role

### 3. Advanced Features
- Multi-currency support
- Multi-language support
- Vendor commission tracking
- Buyer company budgets and approval limits
- Product catalog sharing between vendors and buyers
- Vendor performance analytics

### 4. Testing
- Unit tests for repositories
- Integration tests for complete workflows
- Performance tests for high-load scenarios

### 5. DevOps
- CI/CD pipelines
- Automated database migrations
- Health checks for all services
- Application insights and monitoring

## Migration Guide

### From Old to New Architecture

1. **Update References**:
   - `kavyasCreation` → `Web`
   - `Infrastructure` → `Infra`

2. **Run Migrations**:
   ```bash
   dotnet ef migrations add AddMultiVendorMarketplace --project Infra --startup-project Web
   dotnet ef database update --project Infra --startup-project Web
   ```

3. **Seed Roles**:
   - Roles are automatically seeded on application startup

4. **Assign Initial Roles**:
   - Default admin account has Admin role
   - Manually assign VendorAdmin/BuyerAdmin to company owners

## Conclusion

The refactored three-tier architecture provides:
- **Scalability**: Easy to add new features and entities
- **Maintainability**: Clear separation of concerns
- **Testability**: Business logic isolated from infrastructure
- **Security**: Role-based access control with approval workflows
- **Flexibility**: Support for B2B, B2C, and hybrid models
- **Best Practices**: Industry-standard patterns and principles

The system is now ready for multi-vendor marketplace functionality with comprehensive vendor and buyer company management.
