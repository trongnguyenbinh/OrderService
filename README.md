# LegacyOrder Service

[![Quality Gate Status](https://sonar.veasy.vn/api/project_badges/measure?project=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464&metric=alert_status&token=sqb_4cadfacd86ece679195e6554aca18abce7a92ea6)](https://sonar.veasy.vn/dashboard?id=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464)
[![Coverage](https://sonar.veasy.vn/api/project_badges/measure?project=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464&metric=coverage&token=sqb_4cadfacd86ece679195e6554aca18abce7a92ea6)](https://sonar.veasy.vn/dashboard?id=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464)
[![Security Rating](https://sonar.veasy.vn/api/project_badges/measure?project=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464&metric=software_quality_security_rating&token=sqb_4cadfacd86ece679195e6554aca18abce7a92ea6)](https://sonar.veasy.vn/dashboard?id=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464)
[![Maintainability Issues](https://sonar.veasy.vn/api/project_badges/measure?project=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464&metric=software_quality_maintainability_issues&token=sqb_4cadfacd86ece679195e6554aca18abce7a92ea6)](https://sonar.veasy.vn/dashboard?id=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464)
[![Technical Debt](https://sonar.veasy.vn/api/project_badges/measure?project=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464&metric=software_quality_maintainability_remediation_effort&token=sqb_4cadfacd86ece679195e6554aca18abce7a92ea6)](https://sonar.veasy.vn/dashboard?id=trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464)

## Project Overview

LegacyOrder Service is a refactor project from the poor architecture of the original project. I refactored the codebase to improve the architecture and make it more maintainable. Draft [BusinessDraft.md](BusinessDraft.md) for implement new features. 

## Technology Stack

### Core Framework
- **.NET 8.0** - Latest LTS version of .NET
- **ASP.NET Core Web API** - RESTful API framework

### Database
- **PostgreSQL 18** - Primary database
- **Entity Framework Core 9.0.1** - ORM for data access
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0.3** - PostgreSQL provider

### API Documentation
- **Swashbuckle.AspNetCore 6.5.0** - OpenAPI/Swagger generation
- **Scalar.AspNetCore 2.11.1** - Modern API documentation UI

### Logging & Monitoring
- **Serilog.AspNetCore 10.0.0** - Structured logging
- **Serilog.Sinks.Console 6.1.1** - Console logging
- **Serilog.Sinks.File 7.0.0** - File-based logging
- **Serilog.Enrichers.Environment 3.0.1** - Environment enrichment

### Object Mapping
- **AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1** - DTO mapping

### AI Integration
- **OpenAI 2.7.0** - AI-powered chat support

### Security & Configuration
- **VaultSharp 1.17.5.1** - HashiCorp Vault integration for secrets management

### Testing
- **xUnit 2.6.6** - Testing framework
- **Moq 4.20.70** - Mocking framework
- **FluentAssertions 6.12.0** - Fluent assertion library
- **AutoFixture 4.18.1** - Test data generation
- **Microsoft.EntityFrameworkCore.InMemory 8.0.0** - In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing 8.0.0** - Integration testing
- **Coverlet** - Code coverage analysis

### Containerization
- **Docker** - Multi-stage Dockerfile with Alpine Linux base images
- **Health Checks** - Built-in container health monitoring

## Architecture

The project follows a clean, layered architecture pattern with clear separation of concerns:

### Layer Structure

```
┌─────────────────────────────────────────────────────────────┐
│                    LegacyOrder (API Layer)                  │
│  - Controllers (OrdersController, CustomersController,      │
│    ProductsController, ChatController)                      │
│  - Middleware (Global Exception Handling)                   │
│  - Module Registrations (DI, Logging, Vault)                │
│  - Composition Root (wires interfaces to implementations)   │
└─────────────────────────────────────────────────────────────┘
                    ↓                       ↓
                    ↓                       ↓ 
                    ↓                       ↓
┌──────────────────────────────┐  ┌──────────────────────────┐
│       Service Layer          │  │    Repository Layer      │
│  - Business Logic            │  │  - Data Access           │
│  - Services (OrderService,   │  │  - Implementations       │
│    CustomerService,          │  │    (OrderRepository,     │
│    ProductService,           │  │    CustomerRepository,   │
│    ChatService,              │  │    ProductRepository,    │
│    OpenAIService)            │  │    ChatRepository)       │
│  - AutoMapper Profiles       │  │  - Query Building        │
└──────────────────────────────┘  └──────────────────────────┘
                    ↓                       ↓
                    ↓                       ↓
                    └───────────┬───────────┘
                                ↓
                ┌───────────────────────────────────────────┐
                │           Domain Layer (Core)             │
                │  - Entity Definitions                     │
                │  - Repository Interfaces                  │
                │    (Domain/Interfaces/Repositories)       │
                │  - Entity Framework DbContext             │
                │  - Database Migrations                    │
                │  - Data Seeders                           │
                │  - Entity Configurations                  │
                └───────────────────────────────────────────┘
                                ↓
                ┌───────────────────────────────────────────┐
                │           Common Layer                    │
                │  - Shared Exceptions                      │
                │    (NotFoundException,                    │
                │    InvalidOperationException)             │
                │  - Enums (OrderStatus, CustomerType)      │
                │  - Cross-cutting Concerns                 │
                └───────────────────────────────────────────┘
                                ↓
                ┌───────────────────────────────────────────┐
                │           Model Layer                     │
                │  - DTOs (Data Transfer Objects)           │
                │  - Request Models (Create/Update)         │
                │  - Response Models (PagedResult)          │
                │  - Enums                                  │
                └───────────────────────────────────────────┘
```

### Layer Dependencies

The architecture follows the **Dependency Inversion Principle (DIP)**:

- **LegacyOrder (API)** → Service + Repository (composition root)
- **Service** → Domain (depends only on abstractions)
- **Repository** → Domain (implements Domain interfaces)
- **Domain** → Common
- **Common** → Model
- **Model** → (No dependencies)

**Key Architectural Principle:**
- Service layer depends **only on Domain abstractions** (interfaces in `Domain.Interfaces.Repositories`)
- Service layer has **no direct dependency** on Repository layer
- Repository layer **implements** the interfaces defined in Domain layer
- API layer acts as the **Composition Root**, wiring interfaces to concrete implementations via Dependency Injection

### Key Design Principles

- **Dependency Inversion Principle (DIP)**: High-level modules (Service) depend on abstractions (Domain interfaces), not on low-level modules (Repository)
- **Dependency Injection**: All services and repositories are registered via DI in the API layer (Composition Root)
- **Separation of Concerns**: Each layer has a specific responsibility with clear boundaries
- **Interface Segregation**: Repository interfaces are defined in the Domain layer (`Domain.Interfaces.Repositories` namespace)
- **Testability**: Service layer can be tested independently with mock repositories; comprehensive unit and integration tests
- **Configuration Management**: Secrets managed via HashiCorp Vault

### Architectural Highlights

#### Clean Architecture with Dependency Inversion

The solution implements **Clean Architecture** principles with proper dependency inversion:

**Repository Interfaces Location:**
- All repository interfaces are defined in `Domain/Interfaces/Repositories/`
- Namespace: `Domain.Interfaces.Repositories`
- Interfaces: `IOrderRepository`, `ICustomerRepository`, `IProductRepository`, `IChatRepository`

**Service Layer Decoupling:**
- Service layer depends **only** on Domain abstractions (no Repository project reference)
- Services use repository interfaces from `Domain.Interfaces.Repositories` namespace
- Complete decoupling from infrastructure concerns (database, data access implementation)

**Repository Layer Implementation:**
- Repository implementations are in `Repository/Implementations/`
- Implements interfaces defined in Domain layer
- Contains all database-specific logic and Entity Framework queries

**Dependency Injection (Composition Root):**
- API layer (`LegacyOrder`) references both Service and Repository projects
- DI container maps interfaces to implementations in `ModuleRegistrations/RepositoryCollection.cs`
- Example: `services.AddScoped<IOrderRepository, OrderRepository>()`

**Benefits:**
- ✅ Service layer is completely independent of infrastructure
- ✅ Easy to swap repository implementations without changing services
- ✅ Better testability - services can be tested with mock repositories
- ✅ Follows SOLID principles (especially DIP and ISP)
- ✅ Clear separation between business logic and data access

## API Endpoints

### Health Check
- **GET** `/api/health` - Service health check endpoint

### Products Management

**Base Route**: `/api/products`

- **GET** `/api/products/search` - Search and filter products with pagination
  - Query parameters: Name, Description, SKU, Category, PageNumber, PageSize, SortBy, SortDirection
  - Returns paginated list of products matching search criteria

- **GET** `/api/products/{id}` - Get product by ID
  - Returns detailed product information

- **POST** `/api/products/create` - Create a new product
  - Request body: Product details (Name, Description, SKU, Price, StockQuantity, Category)
  - Returns created product with generated ID

- **PUT** `/api/products/{id}` - Update an existing product
  - Request body: Updated product details
  - Returns updated product information

- **DELETE** `/api/products/{id}` - Delete a product
  - Soft delete or hard delete based on business rules
  - Returns 204 No Content on success

### Customers Management

**Base Route**: `/api/customers`

- **GET** `/api/customers/search` - Search and filter customers with pagination
  - Query parameters: FirstName, LastName, Email, PhoneNumber, CustomerType, PageNumber, PageSize, SortBy, SortDirection
  - Returns paginated list of customers matching search criteria

- **GET** `/api/customers/{id}` - Get customer by ID
  - Returns detailed customer information

- **POST** `/api/customers/create` - Create a new customer
  - Request body: Customer details (FirstName, LastName, Email, PhoneNumber, Address, CustomerType)
  - Returns created customer with generated ID

- **PUT** `/api/customers/{id}` - Update an existing customer
  - Request body: Updated customer details
  - Returns updated customer information

- **DELETE** `/api/customers/{id}` - Delete a customer
  - Removes customer from the system
  - Returns 204 No Content on success

- **GET** `/api/customers/{id}/orders` - Get customer order history
  - Returns list of all orders placed by the customer
  - Useful for customer service and analytics

### Orders Management

**Base Route**: `/api/orders`

- **GET** `/api/orders/search` - Get all orders with pagination and optional status filter
  - Query parameters: PageNumber, PageSize, OrderStatus (Pending, Completed, Cancelled)
  - Returns paginated list of orders

- **GET** `/api/orders/{id}` - Get order by ID
  - Returns detailed order information including order items

- **POST** `/api/orders/create` - Create a new order
  - Request body: Customer ID, Order items (Product ID, Quantity, Unit Price)
  - Validates inventory availability
  - Calculates total amount and applies pricing rules
  - Returns created order with generated order number

- **PUT** `/api/orders/{id}/complete` - Mark order as completed
  - Changes order status from Pending to Completed
  - Updates inventory levels
  - Returns updated order information

- **PUT** `/api/orders/{id}/cancel` - Cancel order
  - Only available for Pending orders
  - Restores inventory levels
  - Returns updated order information

### AI Chat Support

**Base Route**: `/api/chat`

- **POST** `/api/chat/ask` - Send a question to the AI assistant
  - Request body: UserFingerprint, Message, SessionId (optional)
  - AI assistant provides product information and order support
  - Returns AI-generated response with session ID

- **GET** `/api/chat/history/{sessionId}` - Get chat history for a specific session
  - Returns all messages in the conversation
  - Useful for reviewing past interactions

## CI/CD Pipeline

The project implements a three-stage CI/CD workflow that ensures code quality and reliability.

### Pipeline Flow

```
1. PULL REQUEST VALIDATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Developer
   │
   │ Push feature branch
   ▼
GitHub Pull Request (→ main)
   │
   ▼
GitHub Actions (ci-pr.yml)
   ├─ Restore
   ├─ Build
   ├─ Unit Tests
   │
   ├─ ❌ FAIL → PR BLOCKED (cannot merge)
   └─ ✅ PASS → Allow merge


2. MAIN BRANCH QUALITY GATE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Merge PR → main
   │
   ▼
GitHub Actions (ci-main-sonar.yml)
   ├─ Restore
   ├─ Build (clean)
   ├─ Unit / Integration Tests
   ├─ SonarQube Scan
   ├─ Wait Quality Gate
   │
   ├─ ❌ Quality Gate FAIL
   │     └─ STOP ❌ (Not deploy)
   │
   └─ ✅ Quality Gate PASS
         │
         ▼
     Trigger Jenkins (API call)


3. JENKINS DEPLOYMENT
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Jenkins Pipeline
   │
   ▼
Build Docker Image
   │
   ▼
Run Container
   │
   ▼
Health Check
   │
   ├─ ❌ FAIL → Deploy FAILED (rollback/log)
   └─ ✅ PASS → Deploy SUCCESS 🎉
```

### Stage 1: Pull Request Validation

**Workflow**: `ci-pr.yml` (GitHub Actions)

**Trigger**: Feature branch push & PR creation to main

**Steps**:
- `dotnet restore` - Restore dependencies
- `dotnet build` - Build project
- `dotnet test` - Run unit tests

**Outcome**:
- ✅ **PASS**: PR can be merged to main
- ❌ **FAIL**: PR is blocked, developer must fix issues

---

### Stage 2: Main Branch Quality Gate

**Workflow**: `ci-main-sonar.yml` (GitHub Actions)

**Trigger**: Merge to main branch

**Steps**:
- `dotnet restore` - Restore dependencies
- `dotnet clean build` - Clean build
- `dotnet test` - Run unit & integration tests
- SonarQube scan - Code quality analysis
- Wait for quality gate result

**Outcome**:
- ✅ **PASS**: Quality gate approved → Trigger Jenkins deployment
- ❌ **FAIL**: Quality gate rejected → **STOP, NO DEPLOYMENT** (code does not reach production)

---

### Stage 3: Jenkins Deployment

**Trigger**: API call from Stage 2 (only if quality gate passes)

**Agent**: `local` (deployment server)

**Steps**:
1. Build Docker image (multi-stage, .NET 8.0 SDK Alpine)
2. Run container with Vault secrets & port mapping (127.0.0.1:6868 → 8080)
3. Health check validation (`/api/health` endpoint)

**Outcome**:
- ✅ **PASS**: Deployment successful, application live in production
- ❌ **FAIL**: Deployment failed, rollback to previous version