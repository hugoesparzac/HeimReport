# 🤝 Contribution Guidelines for HeimReport

First of all, thank you for investing your time in contributing to **HeimReport (HR)**! To maintain high software quality, ensure a scalable architecture, and provide a smooth collaboration experience, please adhere to the following standards and workflows.

## 🌿 Git Workflow & Branching

We follow a simplified GitHub Flow to ensure continuous integration:

* **Main Branch:** The `main` branch must *always* be in a runnable and stable state.
* **Short-lived Branches:** Create a new branch for every feature or bug fix. Use descriptive prefixes (e.g., `feature/survey-builder` or `bugfix/auth-token`).
* **Pull Requests (PRs):** All changes must be submitted via PRs. Direct commits to `main` are not allowed.
* **Reviews:** A PR must be reviewed and approved by another team member before merging.
* **Quality Gate:** Ensure the code compiles, the application runs locally, and all unit tests pass before requesting a review.

## 📝 Commit Messages

Commit messages are used for future release notes and tracking. We follow the **Conventional Commits** standard. They must be meaningful and descriptive.

🔴 **Bad:**

> fixed stuff
> update code

🟢 **Good:**

> feat(survey): implement anonymous response submission logic
> fix(api): resolve OpenAPI documentation compilation error
> refactor(auth): move JWT validation middleware to security layer

## 🛠️ Coding Standards

Consistency across the codebase is essential for maintainability and long-term scalability. All code (classes, methods, variables, database schemas, and documentation) **must be in English**.

* **The Boy Scout Rule:** "Always leave the code a little cleaner than you found it." If you see a small technical debt while working on a slice, fix it.

### ⚙️ Backend (C# & .NET 10)

We follow a clean, controller-based Web API architecture to maintain a strict separation of concerns:

* **Layer Isolation:**
* `Controllers`: Strictly handle HTTP requests/responses, routing, and OpenAPI annotations.
* `Services / Business Logic`: Encapsulate all core business rules (e.g., score generation, trend calculation, data processing).
* `Data Layer / Repositories`: Manage direct data interactions with PostgreSQL using Entity Framework Core.


* **OpenAPI & Documentation:** Ensure all new controllers and endpoints are properly annotated. Keep dependencies up to date using secure packages (such as `Microsoft.OpenApi 2.9.x`) to prevent security alerts during build pipelines.
* **Naming Conventions:**
* `PascalCase`: For classes, records, method names, public members, and namespaces.
* `camelCase`: For local variables and private fields (prefixed with `_`).
* `Interfaces`: Must always start with "I" (e.g., `ISurveyService`).


* **Clean Code Principles:**
* **SRP (Single Responsibility Principle):** Each class should have only one reason to change.
* **Dependency Injection:** Prefer Constructor Injection as the default method in the backend.
* **Asynchronous Programming:** Use `async/await` for all I/O bound operations (database calls, internal APIs).

### 🅰️ Frontend (Angular & TypeScript)

* **Architecture:** Strictly use **Standalone Components**. Do not introduce `NgModules`.
* **State Management & Zoneless:** Prefer **Angular Signals** for reactive state and data binding to ensure a **Zoneless application** setup, aligning with modern framework standards.
* **Dependency Injection:** Prefer the `inject()` function over traditional constructor injection to maintain a clean and consistent syntax across Standalone Components.
* **Styling:** Use **Tailwind CSS** for layout, spacing, and structural utility classes to build the HR dashboard and mobile client views.
* **Naming Conventions:**
* `kebab-case`: For file names and component selectors (e.g., `survey-form.component.ts`).
* `camelCase`: For variables, functions, and properties.
* `PascalCase`: For classes, interfaces, and types.

## 🧪 Testing & Quality

* **Unit Tests:** Every new feature should include its corresponding unit tests in the appropriate testing directory (using xUnit/Moq for .NET and **Vitest** for Angular).
* **Critical Coverage:** Any logic processing retention scores, survey metric aggregations, or user access control matrices must have exhaustive test coverage.
* **Formatting:** We use a shared `.editorconfig`. Ensure your IDE applies the same indentation and brace rules automatically on save.
* **No Commented-out Code:** Never commit commented-out code blocks. If code is no longer needed, delete it. Git remembers the history.

## 🆘 Seeking Help & Reporting Issues

When you encounter a bug or need help, please open an Issue following these rules:

1. **Be Precise:** Describe the *expected* vs. *actual* behavior clearly.
2. **Provide Context:** Mention the specific file, layer, and line number where the issue occurs.
3. **NO Photos of Screens:** Always copy-paste the raw text of the error log, or provide a high-resolution screenshot directly from your OS. **Prefer copy-pasted logs or proper screenshots instead of low-quality monitor photos.**