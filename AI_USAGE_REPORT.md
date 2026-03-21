# AI Usage Report

**Project:** ExpenseManager API
**Stack:** .NET 8 Web API · Clean Architecture · PostgreSQL · EF Core

## 1. Prompts Used

### Project Initialization

> **Honest note:** The project was initialized with the help of ChatGPT, Claude, and official documentation. I used these tools as a sounding board to explore clean architecture patterns, compare structural options, and validate my thinking — not to generate the project blindly.

The specific areas I researched during initialization:

- Clean architecture layer responsibilities in the context of a .NET 8 Web API
- Whether and how Domain, Application, Infrastructure, and API layers should reference each other
- What project structure conventions look like for fintech-focused APIs

**Prompts used during initialization:**

> "I want to build a .NET 8 Web API for a fintech app using clean architecture. The app handles transactions and idempotency. Can you explain what each layer — Domain, Application, Infrastructure, API — should and should not contain, and what the project reference rules should be?"

> "What are the risks of letting the Domain layer depend on EF Core or any infrastructure concern? How do other teams handle shared base classes like audit fields?"

> "What does a clean architecture .NET solution folder structure typically look like? What goes in each project?"

**My decisions from this phase — owned entirely by me:**

| Decision                                             | Rationale                                                     |
| ---------------------------------------------------- | ------------------------------------------------------------- |
| Domain has zero external dependencies                | Keeps it fully unit-testable; no EF Core, no NuGet            |
| EF Core strictly in Infrastructure only              | Prevents domain model corruption by persistence concerns      |
| Idempotency at middleware level, not inside handlers | Cross-cutting concern; handlers should be pure business logic |
| `FluentValidation.AspNetCore` registered in API      | Pipeline registration is an HTTP-layer responsibility         |

These were validated through AI conversation and web research, but the final calls were mine.

---

### Phase 1 — Architecture & Project Setup

**Prompt 1**

> "I am building a .NET 8 Web API using clean architecture with layers: API, Application, Domain, Infrastructure. The project is a fintech app focusing on transactions and idempotency. What should each layer's responsibility be and what NuGet packages belong where?"

- **Why I asked this:** To validate my own understanding of clean architecture boundaries before writing code.
- **What I got:** A layer breakdown with package suggestions (MediatR in Application, EF Core in Infrastructure, FluentValidation in Application).
- **What I did:** Confirmed it matched my design. Moved `FluentValidation.AspNetCore` to the API layer instead — the AI placed it in Application, but registration belongs closer to the HTTP pipeline.

---
