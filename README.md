# 🛒 E-Commerce API

Backend system for browsing products, managing cart, and placing orders.
Built with **ASP.NET Core 9**, **EF Core 9**, **Microsoft Identity**, **JWT**, and **FluentValidation**.

---

## 📐 Architecture

### 3-Layer Architecture (PL → BLL → DAL) + Common

```
┌─────────────────────────────────────────────────────────┐
│                   ECommerce.API (PL)                    │  ← Presentation Layer
│  Controllers | DTOs | Filters | Middleware | Swagger    │
│  (Includes ValidationFilter & ImagesController)          │
└──────────────────────────┬──────────────────────────────┘
                           │ references
┌──────────────────────────▼──────────────────────────────┐
│                  ECommerce.BLL                          │  ← Business Logic Layer
│  Services | Interfaces | Mappings | Validators          │
└──────────────────────────┬──────────────────────────────┘
                           │ references
┌──────────────────────────▼──────────────────────────────┐
│                  ECommerce.DAL                          │  ← Data Access Layer
│  Entities | DbContext | Repositories | UoW | Config      │
└──────────────────────────┬──────────────────────────────┘
                           │ references
┌──────────────────────────▼──────────────────────────────┐
│                  ECommerce.Common                       │  ← Shared Layer
│  Result Pattern | Constants | Helpers | Extensions       │
└─────────────────────────────────────────────────────────┘
```

### Why 3-Layer Architecture?
- **Separation of Concerns**: كل طبقة مسؤولة عن حاجة معينة
- **Testability**: كل طبقة ممكن تُختبر لوحدها
- **Maintainability**: لو حبيت تغير الـ Database، مش هتحتاج تغير غير الـ DAL
- **Reusability**: الـ BLL ممكن تُستخدم مع أي UI (Web, Mobile, Desktop)

### Design Patterns Used

| Pattern | Purpose |
|---------|---------|
| **Repository Pattern** | Generic & Non-Generic repos for data access abstraction |
| **Unit of Work** | Ensures all DB operations in one request are a single transaction |
| **Result Pattern** | Consistent API responses without throwing exceptions for business errors |
| **DTO Pattern** | Separates API request/response from database entities |
| **Factory Method** | Result.Success() / Result.Failure() for clean response creation |

---

## 📁 Project Structure

```
ECommerceAPI/
├── ECommerce.Common/              ← Shared Layer
│   ├── Result/
│   │   └── Result.cs              ← Result<T> and Result (Response Wrapper)
│   ├── Constants/
│   │   └── PolicyNames.cs         ← Auth policy & role constants
│   ├── Helpers/
│   │   └── ClaimsPrincipalExtensions.cs ← Extract UserId from JWT Claims
│   └── ECommerce.Common.csproj
│
├── ECommerce.DAL/                 ← Data Access Layer
│   ├── Entities/
│   │   ├── ApplicationUser.cs     ← Extended Identity User
│   │   ├── Category.cs            ← Product Category
│   │   ├── Product.cs             ← Product entity
│   │   ├── Cart.cs                ← Shopping Cart item
│   │   ├── Order.cs               ← Order header
│   │   └── OrderItem.cs           ← Order line items
│   ├── Context/
│   │   └── ApplicationDbContext.cs← EF Core DbContext + Identity
│   ├── Configurations/
│   │   └── EntityConfigurations.cs← Fluent API configurations
│   ├── Repositories/
│   │   ├── Generic/
│   │   │   ├── IGenericRepository.cs ← Generic repo interface
│   │   │   └── GenericRepository.cs  ← Generic repo implementation
│   │   ├── Interfaces/
│   │   │   └── INonGenericRepositories.cs ← Specific repo interfaces (inherited from IGenericRepository)
│   │   └── NonGenericRepositories.cs    ← Product, Cart, Order, Category repos
│   ├── UnitOfWork/
│   │   ├── IUnitOfWork.cs         ← UoW interface
│   │   └── UnitOfWork.cs          ← UoW implementation
│   └── ECommerce.DAL.csproj
│
├── ECommerce.BLL/                 ← Business Logic Layer
│   ├── Services/
│   │   ├── AuthService.cs         ← Register & Login logic
│   │   ├── CategoryService.cs     ← Category CRUD
│   │   ├── ProductService.cs      ← Product CRUD + Search + Pagination
│   │   ├── CartService.cs         ← Cart management
│   │   ├── OrderService.cs        ← Order processing (transactional)
│   │   └── ImageService.cs        ← File upload handling
│   ├── Interfaces/
│   │   └── IServiceInterfaces.cs  ← All service contracts + DTOs
│   ├── Validators/
│   │   └── DtoValidators.cs       ← FluentValidation for all DTOs
│   ├── Mapping/
│   │   └── MappingProfile.cs      ← AutoMapper profiles
│   └── ECommerce.BLL.csproj
│
├── ECommerce.API/                 ← Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs      ← POST /api/auth/register, login
│   │   ├── CategoriesController.cs← GET/POST/PUT/DELETE /api/categories
│   │   ├── ProductsController.cs  ← GET/POST/PUT/DELETE /api/products
│   │   ├── CartController.cs      ← GET/POST/PUT/DELETE /api/cart
│   │   ├── OrdersController.cs    ← GET/POST /api/orders
│   │   └── ImageController.cs     ← POST /api/image/upload
│   ├── Filters/
│   │   └── ValidationFilter.cs    ← Global validation filter for FluentValidation
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs ← Global error handler
│   ├── Extensions/
│   │   └── ServiceExtensions.cs   ← DI registration helpers
│   ├── Helpers/
│   │   └── BaseController.cs      ← Consistent response formatting
│   ├── Program.cs                 ← App entry point + DI setup
│   ├── appsettings.json           ← Config (DB, JWT, etc.)
│   └── ECommerce.API.csproj
│
└── ECommerce.API.sln              ← Solution file
```

---

## 🔐 Authentication & Authorization

### JWT Authentication
- **Token Generation**: عند الـ Login/Registration بنعمل JWT Token يحتوي على:
  - `NameIdentifier` → UserId
  - `Email` → User email
  - `Role` → User role (Admin, Manager, User)
  - `Jti` → Unique token ID
- **Token Extraction**: كل Controller بيستلم الـ Token من الـ `Authorization` header
- **UserId Extraction**: بنستخدم `User.GetUserId()` من `ClaimsPrincipalExtensions` لاستخراج الـ UserId من الـ JWT Claims

### Policy-Based Authorization
| Policy | Required Role(s) | Used On |
|--------|-----------------|---------|
| `AdminOnly` | Admin | Create/Update/Delete Categories, Products, Image Upload |
| `UserOnly` | User, Admin | Cart, Orders (authenticated users) |
| `AdminOrManager` | Admin, Manager | Reserved for future use |

> **IMPORTANT**: UserId is NEVER passed in request body. It's always extracted from JWT Claims.

---

## 🚀 Setup Instructions

### Prerequisites
- **.NET 9 SDK**
- SQL Server (Local DB or Local SQLEXPRESS instance)
- Visual Studio 2022 or VS Code

### Step 1: Clone the Repository
```bash
git clone <your-repo-url>
cd ECommerceAPI
```

### Step 2: Configure Database
Edit `ECommerce.API/appsettings.json`:
```json
"ConnectionStrings": {
  "ASPNETCoreD11": "Server=localhost\\SQLEXPRESS;Database=ASPNET_API_EcommerceAPI;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Step 3: Configure JWT
Edit `ECommerce.API/appsettings.json`:
```json
"Jwt": {
  "SecretKey": "YourSecretKeyHere_MustBeAtLeast32Characters",
  "Issuer": "ECommerceAPI",
  "Audience": "ECommerceUsers",
  "DurationInMinutes": 1440
}
```

### Step 4: Run Migrations
Generate and apply Code First database migrations:
```bash
# Execute from solution root
dotnet ef migrations add InitialCreate --project ECommerce.DAL --startup-project ECommerce.API
dotnet ef database update --project ECommerce.DAL --startup-project ECommerce.API
```

### Step 5: Seed Roles & Run
```bash
dotnet run --project ECommerce.API --launch-profile http
```

### Step 6: Access Scalar Docs
Open your browser and navigate to:
```
http://localhost:5000/scalar/v1
```

### Step 7: Import Postman Collection
Import the file `ECommerce_API_Postman_Collection.json` located in the root of the project directly into Postman. It contains:
- Pre-configured HTTP methods, headers, and endpoints.
- Automatic extraction and storage of the JWT login token in the `jwt_token` collection variable.
- Separate folders for Categories, Products, Cart, Orders, and Images.

---

## 📡 API Endpoints

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | ❌ | Create new user account |
| POST | `/api/auth/login` | ❌ | Authenticate and get JWT token |

### Categories

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/categories` | ❌ | Get all categories |
| GET | `/api/categories/{id}` | ❌ | Get category by ID |
| POST | `/api/categories` | ✅ Admin | Create category |
| PUT | `/api/categories/{id}` | ✅ Admin | Update category |
| DELETE | `/api/categories/{id}` | ✅ Admin | Delete category |
| POST | `/api/categories/{id}/image` | ✅ Admin | Upload category image |

### Products

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/products?categoryId=&name=&pageNumber=&pageSize=` | ❌ | Get products (filter + search + pagination) |
| GET | `/api/products/{id}` | ❌ | Get product details |
| POST | `/api/products` | ✅ Admin | Create product |
| PUT | `/api/products/{id}` | ✅ Admin | Update product |
| DELETE | `/api/products/{id}` | ✅ Admin | Delete product |
| POST | `/api/products/{id}/image` | ✅ Admin | Upload product image |

### Cart (UserId from JWT)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/cart` | ✅ User | Get user's cart |
| POST | `/api/cart` | ✅ User | Add to cart |
| PUT | `/api/cart` | ✅ User | Update cart item quantity |
| DELETE | `/api/cart/{productId}` | ✅ User | Remove from cart |

### Orders (UserId from JWT)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/orders` | ✅ User | Place order (from cart) |
| GET | `/api/orders` | ✅ User | View order history |
| GET | `/api/orders/{id}` | ✅ User | Get order details |

### File Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/image/upload` | ✅ User/Admin | Upload general image |

---

## 🧪 Testing with Postman

### Step 1: Register a User
```
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "Password123!",
  "address": "123 Main St"
}
```

### Step 2: Login
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123!"
}
```
Response: `{ "isSuccess": true, "data": "<JWT_TOKEN>", "message": "Login successful." }`

### Step 3: Use the Token
For all authenticated endpoints, add this header:
```
Authorization: Bearer <YOUR_JWT_TOKEN>
```

### Step 4: Create a Category (Admin)
*(Note: Promoted user role must be Admin in AspNetUserRoles database table)*
```
POST /api/categories
Authorization: Bearer <ADMIN_JWT_TOKEN>
Content-Type: application/json

{
  "name": "Electronics",
  "description": "Electronic devices and gadgets"
}
```

### Step 5: Get Products with Pagination
```
GET /api/products?pageNumber=1&pageSize=10&categoryId=1&name=laptop
```

### Step 6: Add to Cart
```
POST /api/cart
Authorization: Bearer <YOUR_JWT_TOKEN>
Content-Type: application/json

{
  "productId": 1,
  "quantity": 2
}
```
> Notice: `userId` is NOT in the request body! It's extracted from the JWT token.

### Step 7: Place Order
```
POST /api/orders
Authorization: Bearer <YOUR_JWT_TOKEN>
Content-Type: application/json

{
  "shippingAddress": "123 Main St, Cairo, Egypt",
  "paymentMethod": "CreditCard"
}
```

---

## 📦 Response Format

All API responses follow the **Result Pattern**:

### Success Response
```json
{
  "isSuccess": true,
  "message": "Operation completed successfully",
  "data": { ... },
  "errors": []
}
```

### Error Response
```json
{
  "isSuccess": false,
  "message": "Validation failed.",
  "data": null,
  "errors": [
    "First name is required.",
    "Invalid email format."
  ]
}
```

---

## 🎥 Postman Testing Video

> 🎥 **Record a video** of yourself testing the API with Postman and add the link here:
> 
> `[Video Link: Your YouTube/Drive link here]`

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| **ASP.NET Core** | 9.0 | Web API framework |
| **EF Core** | 9.0 | ORM for database access |
| **SQL Server** | - | Local DB database |
| **Microsoft Identity** | 9.0 | User management & authentication |
| **JWT** | - | Token-based authentication |
| **FluentValidation** | 11.9 | Request validation |
| **AutoMapper** | 12.0.1 | Entity ↔ DTO mapping |
| **Swagger** | 6.5.0 | API documentation |
| **CORS** | - | Cross-origin request support |
