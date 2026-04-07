# Course Management API

Production-style **ASP.NET Core 8 Web API** for a university course management domain. The solution uses **Entity Framework Core**, **SQL Server**, **JWT authentication** with **role-based authorization**, **DTOs** with **data annotations**, a **service layer** with **dependency injection**, **Swagger/OpenAPI** (including JWT support), **refresh tokens**, and **Hangfire** for recurring maintenance jobs.

---

## Features

- JWT access tokens with configurable lifetime; refresh tokens stored in the database with rotation on refresh
- Roles: **Admin**, **Instructor**, **Student** with policy-style rules enforced in services
- CRUD APIs for instructors, students, courses, and enrollments, plus nested reads (instructor courses, student enrollments, course rosters)
- Read queries use **`AsNoTracking()`** and **`Select()`** projections into read DTOs
- Global exception middleware mapping domain exceptions to **404 / 400 / 403** with JSON bodies
- **Hangfire** dashboard at `/hangfire` and a **daily job** that purges expired and revoked refresh tokens
- **BCrypt** password hashing and **seeded** demo data

---

## Technologies

| Technology | Role in this project |
|------------|----------------------|
| **ASP.NET Core 8** | Web host, controllers, authentication pipeline |
| **Entity Framework Core 8** | ORM, migrations, SQL Server provider |
| **SQL Server** | Primary data store (LocalDB connection string by default) |
| **JWT Bearer** | Stateless API authentication |
| **Swashbuckle (Swagger UI)** | Interactive API documentation and testing |
| **Hangfire** | Background processing and recurring jobs (SQL Server storage) |
| **BCrypt.Net** | Secure password hashing |

---

## Project structure

```
CourseManagementApi/
├── Controllers/           # API endpoints (DTOs in/out only)
├── Data/                  # ApplicationDbContext, DbSeeder
├── DTOs/                  # Request/response models with validation attributes
├── Entities/              # Persistence models
├── Enums/
├── Exceptions/            # NotFound, Forbidden, BadRequest
├── Hangfire/              # Dashboard authorization filter
├── Helpers/               # JwtSettings binding
├── Jobs/                  # Hangfire job types
├── Middleware/            # Exception handling
├── Migrations/            # EF Core migrations (after you run `dotnet ef`)
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Program.cs
├── appsettings.json
└── README.md
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or **SQL Server Express LocalDB** (default connection string targets LocalDB)
- EF Core tools (for migrations):

```bash
dotnet tool install --global dotnet-ef
```

---

## Setup

1. Clone or copy this folder.
2. Update **`appsettings.json`** → **`ConnectionStrings:DefaultConnection`** if you are not using LocalDB.
3. **Change `JwtSettings:SecretKey`** to a long random secret before any real deployment.
4. The repository includes an **`InitialCreate`** EF Core migration. On first run, `Program.cs` calls **`Database.MigrateAsync()`**, which creates/updates the database schema automatically.

   To add further schema changes later:

```bash
cd CourseManagementApi
dotnet ef migrations add YourChangeName
dotnet ef database update
```

5. Run the API:

```bash
dotnet run
```

- Swagger UI: `https://localhost:7288/swagger` or `http://localhost:5288/swagger` (see `Properties/launchSettings.json`).
- Hangfire dashboard: `/hangfire` (open access in this template — **lock this down in production**).

---

## Default seeded users

After the first successful migration + seed, the following accounts exist (passwords are case-sensitive):

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@university.edu` | `Admin123!` |
| Instructor | `alice.johnson@university.edu` | `Instructor123!` |
| Instructor | `bob.smith@university.edu` | `Instructor123!` |
| Student | `charlie.brown@student.university.edu` | `Student123!` |
| Student | `dana.lee@student.university.edu` | `Student123!` |

Seeded data also includes instructors (with at least one **InstructorProfile**), students, courses, and enrollments.

---

## Authentication

1. Call **`POST /api/auth/login`** with email and password.
2. Copy the **`accessToken`** from the response.
3. In Swagger, click **Authorize**, choose **Bearer**, paste the token, and confirm.
4. Use **`POST /api/auth/refresh`** with the **`refreshToken`** to obtain a new access token; the previous refresh token is **revoked** (rotation).

Claims in the access token include **subject (user id)**, **email**, and **role** for authorization.

---

## JWT vs HTTP-only cookies (industry context)

**HTTP-only cookies** are widely used for session tokens because the browser does not expose them to JavaScript, which **reduces theft via XSS**. Cookies also participate in browser same-site and secure flags, giving teams another layer of control.

**JWTs in memory + short lifetime** (as in this API) are common for SPAs and mobile clients: the client stores the access token (often in memory) and uses HTTPS; refresh tokens can be rotated and stored more carefully (sometimes in HTTP-only cookies on the server-issued refresh endpoint).

**Trade-offs:** JWTs are easy to scale across services but harder to revoke instantly without extra infrastructure (denylists, short TTLs, refresh rotation). Cookie-based sessions are easier to invalidate server-side but require CSRF protections for browser-based flows. Many production systems combine **short-lived access tokens**, **rotating refresh tokens**, and **secure cookie storage** for the refresh leg.

---

## API summary

| Area | Method | Route | Notes |
|------|--------|-------|--------|
| Auth | POST | `/api/auth/login` | Public |
| Auth | POST | `/api/auth/refresh` | Public; rotates refresh token |
| Users | GET | `/api/users` | Admin |
| Users | GET | `/api/users/{id}` | Admin or self |
| Instructors | GET | `/api/instructors` | Admin (all) / Instructor (self) |
| Instructors | GET | `/api/instructors/{id}` | Admin or self |
| Instructors | GET | `/api/instructors/{id}/courses` | Admin or owning instructor |
| Instructors | POST | `/api/instructors` | Admin |
| Instructors | PUT | `/api/instructors/{id}` | Admin or self |
| Instructors | DELETE | `/api/instructors/{id}` | Admin |
| Students | GET | `/api/students` | Admin (all) / Student (self) |
| Students | GET | `/api/students/{id}` | Admin or self |
| Students | GET | `/api/students/{id}/enrollments` | Admin or self |
| Students | POST | `/api/students` | Admin |
| Students | PUT | `/api/students/{id}` | Admin or self |
| Students | DELETE | `/api/students/{id}` | Admin |
| Courses | GET | `/api/courses` | Admin (all) / Instructor (own) / Student (catalog) |
| Courses | GET | `/api/courses/{id}` | Role-based visibility |
| Courses | GET | `/api/courses/{id}/students` | Admin or course instructor |
| Courses | POST/PUT/DELETE | `/api/courses` … | Admin + Instructor (ownership rules in service) |
| Enrollments | GET | `/api/enrollments` | Filtered by role |
| Enrollments | GET | `/api/enrollments/{id}` | Role-based |
| Enrollments | POST | `/api/enrollments` | Admin or self-enrolling student |
| Enrollments | PUT | `/api/enrollments/{id}` | Admin or instructor (grading) |
| Enrollments | DELETE | `/api/enrollments/{id}` | Admin, owning student, or course instructor |

---

## Bonus features implemented

- **Refresh tokens** with database persistence, rotation on refresh, and revocation of the previous token
- **Hangfire** with **SQL Server** storage and a **daily recurring job** that deletes expired and revoked refresh tokens

---

## Sample usage

### Login (curl)

```bash
curl -s -X POST "https://localhost:7288/api/auth/login" ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@university.edu\",\"password\":\"Admin123!\"}"
```

### Authenticated request

```bash
curl -s "https://localhost:7288/api/courses" ^
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### Swagger flow

1. Open `/swagger`.
2. **Authorize** with the JWT.
3. Try **GET** `/api/courses` or **POST** `/api/enrollments` as a student (self-enrollment).

---

## Assignment checklist (how requirements are met)

| Requirement | Implementation |
|---------------|----------------|
| ASP.NET Core Web API | .NET 8, controller-based API |
| EF Core + SQL Server | `ApplicationDbContext`, SQL Server provider |
| JWT authentication | `JwtBearer` in `Program.cs`, `AuthService` |
| Role-based authorization | `[Authorize(Roles = ...)]` + service checks via `ICurrentUserService` |
| Swagger + JWT button | `AddSecurityDefinition` / `AddSecurityRequirement` |
| DTOs create/update/read | Under `DTOs/` per aggregate |
| Data annotations | `[Required]`, `[MaxLength]`, `[Range]`, etc. on DTOs |
| Service layer + DI | Interfaces + implementations registered in `Program.cs` |
| `AsNoTracking` + `Select` on reads | Applied across read/query services |
| Async/await | Controllers and services use async EF APIs |
| Fluent API relationships | `OnModelCreating` in `ApplicationDbContext` |
| Migrations | Documented commands; `MigrateAsync` on startup |
| Seeded data | `DbSeeder` (users, instructors, profile, students, courses, enrollments) |
| Error handling | `ExceptionHandlingMiddleware` + domain exceptions |
| Refresh tokens (bonus) | `RefreshToken` entity, `/api/auth/refresh`, rotation |
| Hangfire (bonus) | SQL storage, dashboard, daily cleanup job |

---

## License

Provided as a sample for educational submission; adapt as needed for your course policies.
