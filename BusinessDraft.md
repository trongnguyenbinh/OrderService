# Order WebAPI - POC Design Document

## 1. Project Overview

A .NET Core WebAPI proof-of-concept for an e-commerce system with three core entities: Product, Customer, and Order. The system implements basic CRUD operations for Product and Customer, while Order contains sophisticated business logic integrating both entities.

## 2. System Architecture

### 2.1 Architecture Pattern
- **Clean Architecture / Onion Architecture**
  - API Layer (Controllers)
  - Application Layer (Business Logic, Services)
  - Domain Layer (Entities, Interfaces)
  - Infrastructure Layer (Data Access, External Services)

### 2.2 Technology Stack
- .NET Core 8.0
- Entity Framework Core
- SQL Server / PostgreSQL
- AutoMapper (for DTO mapping)
- FluentValidation (for input validation)
- Scalar/OpenAPI (for API documentation)
- OpenAI API (for AI chat support)
- HttpClient (for API calls)

## 3. Domain Models

### 3.1 Product Entity
```
Product
├── Id (Guid)
├── Name (string)
├── Description (string)
├── SKU (string, unique)
├── Price (decimal)
├── StockQuantity (int)
├── Category (string)
├── IsActive (bool)
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime)
```

### 3.2 Customer Entity
```
Customer
├── Id (Guid)
├── FirstName (string)
├── LastName (string)
├── Email (string, unique)
├── PhoneNumber (string)
├── CustomerType (enum: Regular, Premium, VIP)
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime)
```

### 3.3 Order Entity
```
Order
├── Id (Guid)
├── OrderNumber (string, unique, auto-generated)
├── CustomerId (Guid)
├── Customer (navigation)
├── OrderItems (List<OrderItem>)
├── OrderStatus (enum: Pending, Completed, Cancelled)
├── SubTotal (decimal, calculated)
├── DiscountAmount (decimal)
├── TotalAmount (decimal, calculated)
├── OrderDate (DateTime)
└── UpdatedAt (DateTime)

OrderItem
├── Id (Guid)
├── OrderId (Guid)
├── ProductId (Guid)
├── Product (navigation)
├── Quantity (int)
├── UnitPrice (decimal, snapshot at order time)
└── LineTotal (decimal, calculated)
```

## 4. API Endpoints

### 4.1 Product Endpoints
```
GET    /api/products              - Get all products (with pagination, filtering)
GET    /api/products/{id}         - Get product by ID
POST   /api/products              - Create new product
PUT    /api/products/{id}         - Update product
DELETE /api/products/{id}         - Delete product (soft delete)
GET    /api/products/search       - Search products by name, category, SKU
```

### 4.2 Customer Endpoints
```
GET    /api/customers             - Get all customers (with pagination)
GET    /api/customers/{id}        - Get customer by ID
POST   /api/customers             - Create new customer
PUT    /api/customers/{id}        - Update customer
DELETE /api/customers/{id}        - Delete customer (soft delete)
GET    /api/customers/{id}/orders - Get customer order history
```

### 4.3 Order Endpoints
```
POST   /api/orders                - Create new order
GET    /api/orders/{id}           - Get order by ID
GET    /api/orders                - Get all orders (with pagination)
PUT    /api/orders/{id}/complete  - Mark order as completed
PUT    /api/orders/{id}/cancel    - Cancel order (only if Pending)
```

### 4.4 AI Chat Support Endpoints
```
POST   /api/chat/ask              - Send a question to AI assistant
GET    /api/chat/history/{sessionId}  - Get chat history for a session (optional)
```

## 5. Complex Business Logic for Orders

### 5.1 Order Creation Workflow 
1. **Validation Phase**
   - Validate customer exists
   - Validate all products exist
   - Check product stock availability (simple check: quantity available >= quantity ordered)

2. **Pricing Calculation Phase**
   - Capture current product prices
   - Calculate subtotal (sum of all line items)
   - Apply ONE discount type: customer-type discount only
   - Calculate final total

3. **Inventory Management**
   - Reduce product stock quantity

4. **Order Persistence**
   - Generate unique order number (format: ORD-{timestamp}-{random})
   - Save order with all items
   - Set initial status to "Pending"

### 5.2 Discount Rules 
- **Customer Type Discounts ONLY**
  - Regular: No discount
  - Premium: 5% off total
  - VIP: 10% off total

### 5.3 Order Status Transitions
```
State Machine:
Pending → Completed
   ↓
Cancelled
```

**Business Rules:**
- Can cancel only from Pending status
- Once Completed, cannot change status
- Stock is NOT returned on cancellation (simplification)

### 5.4 Inventory Check
- **Simple Stock Check**: Verify available quantity before order creation
- If insufficient stock, return error message
- No reservation system
- No backorders

## 5A. AI Chat Support Logic

### 5A.1 Chat Request Flow
1. **User sends question** via POST /api/chat/ask
2. **Intent Detection** - Determine what user is asking:
   - Stock inquiry (e.g., "How many laptops do you have?")
   - Product recommendation (e.g., "Best phone under $500?")
   - Product information (e.g., "Tell me about Product X")
   - Order status (e.g., "Where is my order?")
   - General questions

3. **Context Building** - Gather relevant data:
   - Depend on user's question -> Redirect to correct tool for getting data
   - Query products from database
   - Get customer info 
   - Get order history

4. **AI Processing** - Send to Claude API with:
   - User question
   - Retrieved product/customer/order data
   - System instructions
   - Keep chat history maximum 5 messages 

5. **Response Generation** - Return formatted answer

### 5A.2 Supported Query Types

**Stock Inquiries:**
- "How many [product name] are left?"
- "Is [product] in stock?"
- "When will [product] be available?"

**Product Recommendations:**
- "Best laptop for gaming under $1000?"
- "Recommend phone for photography"
- "What products are on sale?"
- "Compare Product A vs Product B"

**Product Information:**
- "Tell me about [product name]"
- "What's the price of [product]?"
- "Product specifications for [product]"

### 5A.3 AI Integration Architecture

```
User Question
     ↓
API Endpoint (/api/chat/ask)
     ↓
ChatService (Business Logic)
     ↓
├─→ Intent Analyzer (determine query type)
├─→ Query Router (redirect to correct tool)
├─→ Data Retriever (get products/orders from DB)
├─→ Context Builder (format data for AI)
└─→ AI Service (call OpenAI API)
     ↓
Format Response
     ↓
Return to User
```

## 6. Core Features

### 6.1 Order Validation Service
- Customer exists check
- Product exists check
- Stock availability check
- Basic duplicate prevention (check by customer + same items within 1 minute)

### 6.2 Pricing Service
- Calculate subtotal (quantity × unit price for each item)
- Apply customer type discount
- Return final total

## 7. Data Relationships

```
Customer (1) ──────< (N) Order
Order (1) ──────< (N) OrderItem
OrderItem (N) >────── (1) Product
```

## 8. Design Patterns to Implement

### 8.1 Repository Pattern
- Abstract data access layer
- Unit of Work for transaction management

### 8.2 Service Layer
- OrderService (handles business logic)
- PricingService (discount calculation)
- InventoryService (stock management)
- ChatService (AI chat support)
- AIService (OpenAI API integration)

## 9. Error Handling Strategy

### 9.1 Custom Exceptions (Keep Only Essential)
- `NotFoundException` (for Product/Customer/Order)
- `InsufficientStockException`
- `InvalidOrderStatusException`

### 9.2 Response Format
```json
{
  "success": false,
  "message": "Product 'Laptop XYZ' has only 3 items in stock",
  "errorCode": "INSUFFICIENT_STOCK"
}
```
