# 🏢 Multi-Tenant Employee API

A production-grade REST API for a multi-tenant employee management system, built with **.NET 8**, **PostgreSQL**, **Clean Architecture**, **CQRS + MediatR**, and **Defense-in-Depth** tenant isolation.

---

## 🏗️ Architecture Overview

The solution follows **Clean Architecture** with strict layer separation:

```
Whizz.Task/
├── Employee.Domain/            # Core domain entities, value objects, enums
│   ├── Entities/               # Employee, Tenant, AuditLog, CustomFieldDefinition
│   └── ValueObjects/           # Money (AmountMinor + CurrencyCode)
│
├── Employee.Application/       # Use cases, CQRS, DTOs, interfaces
│   ├── Features/Employee/
│   │   ├── Commands/           # CreateEmployeeCommand, UpdateEmployeeCommand, DeleteEmployeeCommand
│   │   ├── Queries/            # GetEmployeeByIdQuery, GetEmployeesQuery
│   │   ├── Handlers/           # MediatR request handlers
│   │   ├── Validators/         # FluentValidation validators
│   │   └── DTOs/               # EmployeeDto
│   └── Common/
│       ├── ApiResponse.cs      # Envelope: { data, pagination, error }
│       ├── Services/           # CustomDataValidator
│       └── Interfaces/         # IApplicationDbContext, ITenantContext, ICustomDataValidator
│
├── Employee.Infrastructure/    # EF Core, PostgreSQL, Interceptors, Seeding
│   ├── Persistence/
│   │   ├── Data/               # ApplicationDbContext, DatabaseSeeder
│   │   ├── Configurations/     # EF Core entity type configurations
│   │   ├── Interceptors/       # TenantSessionInterceptor (RLS session)
│   │   └── Migrations/         # EF Core migrations
│   └── Common/                 # TenantContext implementation
│
├── Employee.API/               # ASP.NET Core Web API
│   ├── Controllers/            # EmployeesController
│   └── Middleware/             # TenantMiddleware, ExceptionHandlingMiddleware
│
├── Employee.UnitTests/         # xUnit unit tests (Moq + MockQueryable)
├── Employee.IntegrationTests/  # xUnit integration tests (Testcontainers + real PostgreSQL)
├── Dockerfile                  # Multi-stage Alpine production image
└── docker-compose.yml          # PostgreSQL + API orchestration
```

---

## 🚀 Quick Start (One Command)

```bash
docker compose up
```

This will:
1. Start a PostgreSQL 16 Alpine container with a health check.
2. Build and start the API container.
3. Automatically run EF Core migrations and seed data on first startup.
4. Expose the API at **http://localhost:8080** with Swagger UI at **http://localhost:8080/swagger**.

> **Prerequisites:** Docker Desktop installed and running.

---

## 🔑 Seeded Tenant IDs (Ready to Use)

| Tenant | X-Tenant-Id Header Value | Name |
| :--- | :--- | :--- |
| **Tenant A** | `11111111-1111-1111-1111-111111111111` | Tenant A Corp |
| **Tenant B** | `22222222-2222-2222-2222-222222222222` | Tenant B Industries |

**Every API request requires the `X-Tenant-Id` header.** Without it, the API returns `400 Bad Request`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/api/v1/employees`.

| Method | Path | Description |
| :--- | :--- | :--- |
| `POST` | `/api/v1/employees` | Create a new employee |
| `GET` | `/api/v1/employees` | List employees (paginated + filtered) |
| `GET` | `/api/v1/employees/{id}` | Get employee by ID |
| `PUT` | `/api/v1/employees/{id}` | Update an employee |
| `DELETE` | `/api/v1/employees/{id}` | Soft delete an employee |

### Response Envelope

All responses follow this shape:

```json
{
  "data": {},
  "pagination": null,
  "error": null
}
```

- **`data`**: The result payload (or `null` on error).
- **`pagination`**: Present on list endpoints — `{ totalCount, pageNumber, pageSize, totalPages, hasPreviousPage, hasNextPage }`.
- **`error`**: Human-readable error string (or `null` on success).

### Quick cURL Examples

**Create Employee (Tenant A):**
```bash
curl -X POST http://localhost:8080/api/v1/employees \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "firstName": "Alan",
    "lastName": "Turing",
    "email": "alan.turing@tenanta.com",
    "department": "Engineering",
    "status": 0,
    "salary": { "amountMinor": 1200000, "currencyCode": "USD" },
    "customData": "{}"
  }'
```

**List Employees with Pagination (Tenant A):**
```bash
curl "http://localhost:8080/api/v1/employees?pageNumber=1&pageSize=10" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

**Verify Tenant Isolation (Tenant B cannot see Tenant A employee):**
```bash
curl http://localhost:8080/api/v1/employees/{id-from-above} \
  -H "X-Tenant-Id: 22222222-2222-2222-2222-222222222222"
# Returns 404 Not Found — tenant isolation is working!
```

---

## 🔒 Multi-Tenancy: Defense-in-Depth (3 Layers)

Tenant isolation is enforced at three independent layers. If any one layer has a bug, the other two still protect data.

### Layer 1 — HTTP Middleware (`TenantMiddleware`)
```
HTTP Request → TenantMiddleware
  1. Reads X-Tenant-Id header
  2. Validates it is a well-formed UUID
  3. Sets ITenantContext.TenantId for the entire request scope
  → Returns 400 if missing or malformed
```

### Layer 2 — EF Core Global Query Filters (`ApplicationDbContext`)
```csharp
// Automatically appended to every LINQ query on Employees:
e.TenantId == _tenantContext.TenantId && e.DeletedAt == null
```
This means **every** EF Core query is transparently scoped to the current tenant — without any code in handlers needing to remember to filter.

### Layer 3 — PostgreSQL Row-Level Security (`TenantSessionInterceptor` + RLS Policy)
```sql
-- Database enforces tenant isolation at engine level:
CREATE POLICY employee_tenant_isolation_policy ON "Employees"
FOR ALL
USING (
  current_setting('app.bypass_rls', true) = 'on'
  OR "TenantId" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
);
```
- The `TenantSessionInterceptor` automatically sets `SET ROLE tenant_app_role` and `app.current_tenant_id` before every EF Core command.
- Even if a developer uses `.IgnoreQueryFilters()` or raw SQL **without** a `WHERE TenantId = ...` clause, PostgreSQL **still blocks cross-tenant data access at the database engine level**.
- During database seeding and migrations (no tenant context), `app.bypass_rls = 'on'` is set so that setup operations succeed.

---

## 🗃️ Database Design

### Tables

| Table | Key Columns | Notes |
| :--- | :--- | :--- |
| `Tenants` | `Id`, `Name`, `IsActive` | Fixed 2 seeded tenants |
| `Employees` | `Id`, `TenantId`, `FirstName`, `LastName`, `Email`, `Department`, `Status`, `CustomData` (JSONB), `SalaryAmountMinor`, `SalaryCurrencyCode`, `CreatedAt`, `UpdatedAt`, `DeletedAt` | Soft-deleted via `DeletedAt` |
| `AuditLogs` | `Id`, `TenantId`, `EntityName`, `EntityId`, `Action`, `Timestamp`, `Changes` (JSONB) | Auto-logged on every create/update/delete |
| `CustomFieldDefinitions` | `Id`, `TenantId`, `FieldName`, `DataType`, `IsRequired` | Per-tenant schema rules for `CustomData` |

### Email Uniqueness
Email uniqueness is **tenant-scoped**, not global. The same email address can exist in Tenant A and Tenant B simultaneously. Implemented via a **partial unique index**:
```sql
UNIQUE (TenantId, Email) WHERE "DeletedAt" IS NULL
```

### Money Value Object
Salary is represented as a `Money` value object — **no FLOAT or DECIMAL**:
```
SalaryAmountMinor (INTEGER) + SalaryCurrencyCode (VARCHAR(3))
```
- `850000` + `"USD"` = `$8,500.00` (stored in minor currency units, i.e. cents).
- Enforces: no negative amounts, valid 3-letter ISO currency code, same-currency arithmetic.

### PascalCase Naming Convention
All tables, columns, and indexes use **PascalCase** as required:
- `Id`, `TenantId`, `FirstName`, `LastName`, `Email`, `Department`, `Status`, `CustomData`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, etc.

---

## 📋 CustomData Validation

Each tenant has its own `CustomFieldDefinition` schema. On `Create` and `Update`, the `CustomData` JSON is validated against the tenant's definitions.

Supported field types: `string`, `number`, `boolean`, `date` (ISO-8601 `YYYY-MM-DD`).

Example `CustomData` for **Tenant A** (fields: `skills` as string):
```json
{ "skills": ["C#", ".NET", "PostgreSQL"] }
```

If a required field is missing or has the wrong type, the API returns a `400 Bad Request` with a descriptive error message.

> **Note:** If no `CustomFieldDefinitions` exist for the tenant, `CustomData` validation is skipped and any valid JSON object is accepted.

---

## 📊 List Endpoint Filtering & Pagination

`GET /api/v1/employees` supports:

| Query Param | Type | Description |
| :--- | :--- | :--- |
| `pageNumber` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 10) |
| `department` | string | Filter by department name |
| `status` | int | Filter by status (`0` = Active, `1` = Suspended) |
| `search` | string | Full-text search on FirstName, LastName, Email |

---

## 📝 Audit Logging

Every create, update, and soft-delete automatically writes an entry to `AuditLogs` via EF Core's `SaveChangesAsync` override:

```json
{
  "EntityName": "Employee",
  "EntityId": "...",
  "Action": "Created",
  "TenantId": "11111111-1111-1111-1111-111111111111",
  "Timestamp": "2026-10-10T12:00:00Z",
  "Changes": { "FirstName": "Alan", "Email": "alan@tenanta.com" }
}
```

**Note on "who":** Since this API has no authentication requirement, the audit log captures the `TenantId` and the full JSON diff of what changed. In a production system with authentication, the `UserId` field (present in `AuditLogs`) would be populated from the JWT claims.

---

## 🧪 Running Tests

### Unit Tests (Moq + MockQueryable, no database required)
```bash
dotnet test --filter "FullyQualifiedName~Employee.UnitTests"
```
**30 unit tests** covering:
- Create handler — happy path, duplicate email, CustomData validation failure.
- Update handler — not found, duplicate email on update.
- Delete handler — happy path, not found.
- List handler — pagination metadata, department filtering, search.
- GetById handler — found, not found.
- FluentValidation — required fields, email format, salary constraints.
- CustomDataValidator — all field types (string, number, boolean, date), required fields, type mismatches.

### Integration Tests (Real PostgreSQL via Testcontainers)
```bash
dotnet test --filter "FullyQualifiedName~Employee.IntegrationTests"
```
**7 integration tests** covering:
- Create employee — HTTP 201 Created.
- Tenant isolation — Tenant B cannot access Tenant A's employee (HTTP 404).
- Soft delete — employee disappears after deletion (HTTP 404 on subsequent GET).
- Duplicate email — HTTP 409 Conflict.
- Missing tenant header — HTTP 400 Bad Request.
- Invalid tenant GUID — HTTP 400 Bad Request.
- PostgreSQL RLS — `IgnoreQueryFilters()` bypass still blocked at database engine level.

### All Tests
```bash
dotnet test
```

---

## 🛠️ Local Development (Without Docker)

**Prerequisites:** .NET 8 SDK, PostgreSQL 16 running locally.

```bash
# 1. Set your connection string in appsettings.Development.json
# DefaultConnection: "Host=localhost;Port=5432;Database=EmployeeDb;Username=postgres;Password=postgres"

# 2. Apply migrations + seed data
dotnet run --project Employee.API

# Migrations and seeding run automatically on startup.
```

---

## 🐳 Docker Details

**Dockerfile:** Multi-stage build with `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` base image (~172 MB).
- Runs as non-root user `app` for security.
- `ASPNETCORE_URLS=http://+:8080`.

**docker-compose.yml:**
- `postgres`: PostgreSQL 16 Alpine with `pg_isready` health check.
- `api`: Depends on `postgres` (waits for healthy status before starting).
- All credentials passed via environment variables.
- Persistent volume for database data (`postgres-data`).

```bash
# Start
docker compose up

# Start in background
docker compose up -d

# Stop and remove containers
docker compose down

# Full clean (removes volume/data)
docker compose down -v
```

---

## ⚖️ Design Decisions & Trade-offs

### Why CQRS + MediatR?
Separates read and write concerns explicitly. Each handler has a single responsibility. The `ValidationBehavior` pipeline automatically validates every command before it reaches a handler.

### Why `Money` as an Integer Value Object?
Floating-point arithmetic is inherently imprecise for financial calculations. Storing as `INTEGER` (minor units, e.g., cents) with a separate ISO currency code is the industry standard (used by Stripe, etc.). The `Money` value object enforces invariants: no negative amounts, valid ISO 3-letter currency codes, and prevents adding different currencies.

### Why Tenant-Scoped Email Uniqueness?
The same employee email address (e.g., a contractor) may work at multiple companies (tenants). Global email uniqueness would incorrectly prevent this. The constraint is `UNIQUE (TenantId, Email) WHERE DeletedAt IS NULL`.

### Why PostgreSQL RLS?
Application-level filtering (EF Core Global Filters) protects against bugs in application code. But if a developer accidentally uses `.IgnoreQueryFilters()`, or if a reporting query is added without proper tenant scoping, data leaks across tenants. PostgreSQL RLS is the last line of defense — it enforces isolation at the database engine itself, making tenant data leaks architecturally impossible.

### Why Testcontainers for Integration Tests?
Testcontainers spins up a real, isolated PostgreSQL Docker container per test run. This means:
- No shared state between CI runs.
- Tests run against the exact same database engine as production.
- No EF Core InMemory (which doesn't support many PostgreSQL-specific features like JSONB, partial indexes, and RLS).

---

## 🔧 Technologies

| Layer | Technology |
| :--- | :--- |
| Runtime | .NET 8 (ASP.NET Core Web API) |
| ORM | Entity Framework Core 8 + Npgsql |
| Database | PostgreSQL 16 |
| Messaging | MediatR 12 (CQRS) |
| Validation | FluentValidation 11 |
| Testing | xUnit, Moq, MockQueryable.Moq, FluentAssertions |
| Integration DB | Testcontainers.PostgreSql |
| Containerization | Docker + Docker Compose |
| Mapping | AutoMapper 13 |
