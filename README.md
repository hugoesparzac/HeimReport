<h1 align="center">HeimReport — HR Retention & Survey Platform</h1>

<div align="center">

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

---

![.NET 10](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Angular 22](https://img.shields.io/badge/Angular_22-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white)
![Vitest](https://img.shields.io/badge/Vitest-FCC72B?style=for-the-badge&logo=vitest&logoColor=1E1E20)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Cloudinary](https://img.shields.io/badge/Cloudinary-3448C5?style=for-the-badge&logo=cloudinary&logoColor=white)

---

</div>

HeimReport (HR) is an employee retention analysis platform designed to capture honest workforce feedback and detect turnover risks early.

The ecosystem features two distinct components: a strategic **Web Dashboard for HR Teams** to manage the workforce and evaluate metrics, and a frictionless **Mobile-Friendly Client for Employees** to rapidly complete continuous pulse surveys.

---

## 🚧 Project Status

HeimReport is currently under active development.

The platform is designed to serve as a portfolio architecture piece focused on:
- End-to-end employee lifecycle management, survey distribution, and metric processing.
- Clean separation of concerns between Web API and Frontend Client.
- Modern Angular patterns (Zoneless + Signals + Vitest).
- Clean developer onboarding experience.

**Currently implemented (backend):**
- Catalog management (Countries, Departments, Positions) with soft delete, bulk operations, and recovery ("trash") views.
- Employee lifecycle management: hiring, job changes, promotions, termination, and reactivation, with full job history tracking.
- User accounts with role-based access (Employee, HR, Admin), self-registration, admin/HR provisioning, JWT authentication with refresh token rotation, and email verification.
- System-wide audit logging for all mutating operations.
- Employee profile photo storage via Cloudinary.
- Transactional email delivery via Mailgun (account verification, temporary passwords).

---

## ✨ Technical Highlights

- **Dual-Experience System:** Strategic desktop management console for HR administrators alongside an optimized interface for employee survey taking.
- **Full Employee Lifecycle Tracking:** Every job change, promotion, termination, and reactivation is recorded in a dedicated history trail, separate from general system audit logs.
- **Role-Based Provisioning:** Admins can grant any role; HR can only provision Employee/HR accounts — enforced consistently across validation and business logic.
- **Continuous Pulse Surveys:** Lightweight, automated feedback cycles designed to monitor organizational sentiment in real time.
- **System-Wide Audit Trail:** Every create/update/delete/reactivate action across the platform is logged with actor, timestamp, and before/after state — while never persisting sensitive fields (password hashes, tokens).
- **Pragmatic Layered Monolith:** Clean separation of presentation, business logic, and data access within a unified backend.
- **Angular Signals & Zoneless:** Full integration with Angular's latest reactivity paradigm for robust performance.
- **Dockerized Data Storage:** Isolated, high-availability local database containerization with robust automated healthchecks.

---

## 🏗️ Architectural Vision

HeimReport balances enterprise maintainability with pragmatic architecture. It utilizes a clean **Controller-Service-Repository** pattern within a structured mono-repo layout, backed by DTO-level validation and a dedicated mapping layer.

### Core Architectural Principles

- **Layered Isolation**
  - **Controllers:** Manage strict HTTP contracts, routing, authorization policies, and OpenAPI metadata.
  - **Validators:** FluentValidation rules — including async uniqueness checks, cross-entity existence checks, and context-dependent rules (e.g. circular manager hierarchies, role-assignment policies).
  - **Services (Business Logic):** Orchestrate entity mutations, job history transitions, audit logging, and external integrations.
  - **Repositories (Data Access):** Handle dedicated Entity Framework Core actions and data mapping to PostgreSQL.
  - **Mappers:** Pure, dependency-free translation between entities and DTOs.

- **Mono-repo Strategy**
  Backend API and Frontend Web Client assets coexist inside a single repository, fostering:
  - Atomic changes across API contracts and UI views.
  - Simplified package auditing and dependency updates.
  - Accelerated local developer workspace initialization.

---

## 🚀 Tech Stack

### Backend (.NET 10)
- **Framework:** ASP.NET Core 10 Web API
- **Language:** C# 14
- **Database:** PostgreSQL 17 with Entity Framework Core 10
- **Validation:** FluentValidation
- **Authentication:** JWT bearer tokens with refresh token rotation and reuse detection
- **File Storage:** Cloudinary (employee profile photos)
- **Email Delivery:** Mailgun (account verification, temporary passwords)
- **API Documentation:** Scalar (interactive OpenAPI reference)
- **Testing:** xUnit, Moq, MockQueryable, Bogus (unit tests) — integration tests planned

### Frontend (Angular)
- **Framework:** Angular (Modern Standalone Component Architecture)
- **Language:** TypeScript
- **Reactivity Model:** Angular Signals (Zoneless Execution)
- **Styling:** Tailwind CSS Utility Framework
- **Testing:** Vitest Testing Suite

### DevOps & Infrastructure
- Docker & Docker Compose (Isolated local Database tier)
- Volume persistence and local loopback exposure management

---

## 📁 Project Structure

```text
src/
 ├── HeimReport.Api/                  # ASP.NET Core Web API
 │    ├── Controllers/                # Presentation Layer & HTTP Endpoints
 │    ├── Data/                       # EF Core DbContext, Configurations, Migrations & Seeding
 │    ├── DTOs/                       # Core Request & Response Contracts
 │    ├── Entities/                   # Domain Models (Employees, Users, Surveys, Audit Logs, etc.)
 │    ├── Enums/                      # Shared domain enumerations
 │    ├── Exceptions/                 # Domain & not-found exception types
 │    ├── ExceptionHandlers/          # Global exception handling & ProblemDetails mapping
 │    ├── Email/                      # Mailgun-based transactional email sending
 │    ├── Storage/                    # Cloudinary-based photo storage
 │    ├── Security/                   # JWT provider, password/token hashing, role policies
 │    ├── Mappers/                    # Entity <-> DTO translation
 │    ├── Repositories/               # Data Access Layer Implementations
 │    ├── Services/                   # Business Logic & Orchestration
 │    └── Validators/                 # FluentValidation rules per DTO
 │
 ├── HeimReport.Api.UnitTests/        # xUnit unit test suite (Services, Validators, Mappers)
 │
 └── HeimReport.Client/                # Angular Web Application
      └── src/app/
           ├── core/                  # Global Interceptors, Guards, and Core Services
           ├── shared/                # Reusable Presentational UI Components
           └── features/              # Feature domains (HR Dashboard, Survey View)

```

---

## 🛠️ Getting Started (Hybrid Local Workflow)

Local development follows a hybrid workflow: **PostgreSQL runs inside an isolated Docker container**, while the .NET API and Angular UI execute natively on your host machine for maximum performance and hot-reloading efficiency.

### Prerequisites

* .NET 10 SDK
* Node.js 22+ & Angular CLI
* Docker & Docker Compose
* A [Cloudinary](https://cloudinary.com/) account (free tier is sufficient for local development)
* A [Mailgun](https://www.mailgun.com/) account (free tier is sufficient for local development)

### 1. Clone the Repository

```bash
git clone https://github.com/hugoesparzac/HeimReport.git
cd HeimReport

```

### 2. Configure Environment Secrets

Create a local `.env` file in the repository root directory to provision local development database credentials:

```bash
cp .env.example .env

```

*Note: Your local `.env` is already configured in `.gitignore` to protect local credentials from version control.*

### 3. Spin Up the PostgreSQL Database Container

Initialize the containerized development database. It will map exclusively to your local loopback address (`127.0.0.1:5432`) to ensure network safety.

```bash
docker compose up database -d

```

### 4. Configure Local Secrets (Database, Mailgun, Cloudinary)

Sensitive configuration — the database connection string, Mailgun API key, and Cloudinary credentials — is never committed to source control. It's managed locally via the .NET Secret Manager. See the [User Secrets Management](#-user-secrets-management) section below for the exact commands.

### 5. Run the Backend API

Navigate to the API folder, restore dependencies, and start the hot-reloading development engine. On first run in a `Development` environment, the API automatically seeds the database with sample catalogs, employees, and test user accounts (see [Database Seeding](#-database-seeding) below).

```bash
cd src/HeimReport.Api
dotnet restore
dotnet watch

```

### 6. Run the Angular Frontend

Open a secondary terminal workspace to install dependencies and run the client server:

```bash
cd src/HeimReport.Client
npm install
ng serve

```

---

## 🖥️ Local Application URLs

| Service | Address | Target Audience |
| --- | --- | --- |
| **Frontend UI** | `http://localhost:4200` | HR Dashboard & Employee Survey Views |
| **Backend API Gateway** | `http://localhost:5156` | Native REST Endpoint Base |
| **API Reference (Scalar)** | `http://localhost:5156/scalar` | Interactive OpenAPI Documentation |

---

## 🔧 User Secrets Management

For secure local development configuration outside of `.csproj`/`appsettings.json` tracking, this project uses the .NET Secret Manager. Run the following commands inside `src/HeimReport.Api`:

**Database connection string:**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=HeimReportDb;Username=your_env_user;Password=your_env_password"

```

**Mailgun (transactional email):**

```bash
dotnet user-secrets set "Mailgun:ApiKey" "your-mailgun-api-key"

```

**Cloudinary (employee photo storage):**

```bash
dotnet user-secrets set "Cloudinary:CloudName" "your-cloud-name"
dotnet user-secrets set "Cloudinary:ApiKey" "your-cloudinary-api-key"
dotnet user-secrets set "Cloudinary:ApiSecret" "your-cloudinary-api-secret"

```

Non-sensitive configuration for these providers (domain, from-email, folder names, etc.) lives in `appsettings.Development.json` and is safe to keep in source control.

---

## 🌱 Database Seeding

When running in the `Development` environment, the API automatically seeds the database on startup — **only if it's currently empty**, making the process idempotent and safe to run repeatedly.

The seeder provisions:
- A base catalog of countries, departments, and positions.
- ~50 sample employees per seeded country, with a coherent management hierarchy and placeholder profile photos.
- One Admin account and two HR accounts per country, ready to log in immediately.

All seeded user accounts share a single fixed password for convenience during local testing. Credentials for every seeded account are printed to the console output once seeding completes — look for the `=== Seeded Users (Development Only) ===` block after the API starts.

> ⚠️ Seeding is strictly gated behind `IsDevelopment()` and will never run against a production environment.

---

## 🧪 Testing

- **Unit tests** (`src/HeimReport.Api.UnitTests`) cover Services, Validators, and Mappers using xUnit, Moq, MockQueryable (for `IQueryable` mocking), and Bogus (for fake data generation).
- **Integration tests** are planned, targeting real database interactions (repositories, EF Core-specific query translation) and end-to-end API flows.

Run the unit test suite from the repository root:

```bash
dotnet test

```

---

## 🤝 Contributing

Contributions are welcome! Please review our [CONTRIBUTING.md](CONTRIBUTING.md) file for more details regarding our Git branching patterns, naming conventions, and standalone frontend structure rules.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.