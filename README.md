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

---

</div>

HeimReport (HR) is an employee retention analysis platform designed to capture honest workforce feedback and detect turnover risks early. 

The ecosystem features two distinct components: a strategic **Web Dashboard for HR Teams** to evaluate metrics and a frictionless **Mobile-Friendly Client for Employees** to rapidly complete continuous pulse surveys.

---

## 🚧 Project Status

HeimReport is currently under active development. 

The platform is designed to serve as a portfolio architecture piece focused on:
- End-to-end employee survey distribution and metric processing.
- Clean separation of concerns between Web API and Frontend Client.
- Modern Angular patterns (Zoneless + Signals + Vitest).
- Clean developer onboarding experience.

---

## ✨ Technical Highlights

- **Dual-Experience System:** Strategic desktop management console for HR administrators alongside an optimized interface for employee survey taking.
- **Continuous Pulse Surveys:** Lightweight, automated feedback cycles designed to monitor organizational sentiment in real time.
- **Pragmatic Layered Monolith:** Clean separation of presentation, business metrics evaluation, and underlying data structures within a unified backend.
- **Angular Signals & Zoneless:** Full integration with Angular's latest reactivity paradigm for robust performance.
- **Dockerized Data Storage:** Isolated, high-availability local database containerization with robust automated healthchecks.

---

## 🏗️ Architectural Vision

HeimReport balances enterprise maintainability with pragmatic architecture. It utilizes a clean **Controller-Service-Repository** pattern within a structured mono-repo layout.

### Core Architectural Principles

- **Layered Isolation**
  - **Controllers:** Manage strict HTTP contracts, routing, and proper OpenAPI metadata.
  - **Services (Business Logic):** Calculate score aggregations, feedback trends, and retention indices.
  - **Repositories (Data Access):** Handle dedicated Entity Framework Core actions and data mapping to PostgreSQL.

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
- **Documentation:** OpenAPI / Native Swagger Integration
- **Testing:** xUnit & Moq

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
 ├── HeimReport.Api/              # ASP.NET Core Web API 
 │    ├── Controllers/            # Presentation Layer & HTTP Enpoints
 │    ├── Data/                   # EF Core DbContext, Configurations & Migrations
 │    ├── DTOs/                   # Core Request & Response Contracts
 │    ├── Entities/               # Domain Models (Surveys, Questions, Submissions)
 │    ├── Repositories/           # Data Access Layer Implementations
 │    └── Services/               # Business Logic, Metrics & Scoring Engines
 │
 └── HeimReport.Client/           # Angular Web Application
      └── src/app/
           ├── core/              # Global Interceptors, Guards, and Core Services
           ├── shared/            # Reusable Presentational UI Components
           └── features/          # Feature domains (HR Dashboard, Survey View)

```

---

## 🛠️ Getting Started (Hybrid Local Workflow)

Local development follows a hybrid workflow: **PostgreSQL runs inside an isolated Docker container**, while the .NET API and Angular UI execute natively on your host machine for maximum performance and hot-reloading efficiency.

### Prerequisites

* .NET 10 SDK
* Node.js 22+ & Angular CLI
* Docker & Docker Compose

### 1. Clone the Repository

```bash
git clone [https://github.com/hugoesparzac/HeimReport.git](https://github.com/hugoesparzac/HeimReport.git)
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

### 4. Run the Backend API

Navigate to the API folder, restore dependencies, and start the hot-reloading development engine:

```bash
cd src/HeimReport.Api
dotnet restore
dotnet watch

```

### 5. Run the Angular Frontend

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
| **Swagger UI Page** | `http://localhost:5156/swagger` | Interactive API Documentation |

---

## 🔧 User Secrets Management

For secure development connection strings outside of `.csproj` tracking, configure your local workspace using the .NET Secret Manager utility inside `src/HeimReport.Api`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=HeimReportDb;Username=your_env_user;Password=your_env_password"

```

---

## 🤝 Contributing

Contributions are welcome! Please review our [CONTRIBUTING.md](https://www.google.com/search?q=CONTRIBUTING.md) file for more details regarding our Git branching patterns, naming conventions, and standalone frontend structure rules.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](https://www.google.com/search?q=LICENSE) file for more details.