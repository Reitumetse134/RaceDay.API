# RaceDay - Event Management System

**Author:** REITUMETSE HELEPI st10478134  
**Module:** Programming 2B (PROG6212)  
**Parts:** 1 & 2 - System Planning, Database, and RESTful API  
**Date:** October 2026

---

## 📖 Project Overview

South Africa has a rich road events culture, from the iconic Comrades Marathon to the Cape Town Cycle Tour and hundreds of community park runs. However, many of these events are still managed through paper-based registration and disconnected spreadsheets.

**RaceDay** is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. This repository contains:

- **Part 1**: Planning documents (ERD, API endpoint plan, SQL database script)
- **Part 2**: A fully functional RESTful API built in ASP.NET Core with role-based JWT authentication, Swagger documentation, and comprehensive unit tests
- **Part 3**: (Coming soon) MVC web application, Azure Blob Storage integration, and Docker containerisation

---

## 👥 User Roles

The system supports two distinct user roles, each with specific permissions:

### 🏁 Organiser

- Create, edit, and delete events.
- Define age and distance categories for each event.
- View all participant enrolments for their own events.
- Capture and publish finish times and finishing positions.
- Only able to manage events they created (ownership enforced at API level).

### 🏃 Participant

- Register and manage their personal profile.
- Browse upcoming events with filtering options.
- Enrol in events by selecting a relevant category.
- View their personal enrolment history and race results.
- Withdraw from enrolments they made.

---

## 📂 Repository Structure

```
RaceDayDB/
├── .github/
│   └── workflows/
│       └── dotnet.yml                # CI/CD pipeline (build + test)
├── docs/                             # Planning documents (Part 1)
│   ├── ERD.png                       # Entity Relationship Diagram
│   ├── API_Endpoint_Plan.md          # RESTful API endpoint specification
│   ├── RaceDay_Schema.sql            # SQL Server database script
│   └── ci-screenshot.png             # Green CI build screenshot
├── RaceDay.API/                      # Main API project (Part 2)
│   ├── Controllers/                  # Auth, Users, Events, Categories, Enrolments, Results
│   ├── Data/                         # DbContext
│   ├── DTOs/                         # Data Transfer Objects
│   ├── Migrations/                   # EF Core migrations
│   ├── Models/                       # Entity models
│   ├── Program.cs                    # App configuration
│   └── appsettings.json              # Connection strings & JWT config
├── RaceDay.API.Tests/                # Unit test project (Part 2)
│   ├── AuthTests.cs
│   ├── EventTests.cs
│   ├── EnrolmentTests.cs
│   ├── RoleAccessTests.cs
│   ├── CustomWebApplicationFactory.cs
│   └── TestHelpers.cs
└── README.md                         # This file
```

---

## 🛠️ Setup Instructions (Part 2 - API)

### Prerequisites

- **Visual Studio 2022** with the *ASP.NET and web development* workload
- **.NET 8.0 SDK**
- **SQL Server Express** or **LocalDB**
- **SQL Server Management Studio (SSMS)** (optional, for viewing the database)

### Step 1: Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/poe1raceday.git
cd poe1raceday
```

### Step 2: Open in Visual Studio

Double-click `RaceDay.API.sln` to open the solution in Visual Studio 2022.

### Step 3: Configure the Connection String

Open `RaceDay.API/appsettings.json` and verify the connection string matches your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "ThisIsAVeryLongSecretKeyForJwtSigningThatIsAtLeast32CharsLong!",
    "Issuer": "RaceDayAPI",
    "Audience": "RaceDayClient"
  }
}
```

### Step 4: Apply Database Migrations

In Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console**. Set the **Default project** dropdown to `RaceDay.API`, then run:

```powershell
Update-Database
```

This creates the `RaceDayDB` database with all tables (`Users`, `Events`, `Categories`, `Participants`, `EventEnrolments`, `EventRoutes`, `Results`).

### Step 5: Run the API

Press **F5** in Visual Studio. The API will start and open Swagger UI in your default browser at:

```
https://localhost:7122/swagger
```

### Step 6: Test the API in Swagger

1. **Register an Organiser**: `POST /api/Auth/register`
   ```json
   {
     "fullName": "Thabo",
     "lastName": "Mokoena",
     "email": "thabo@raceday.com",
     "password": "Password123!",
     "role": "Organiser",
     "phone": "0821111111"
   }
   ```

2. **Register a Participant**: `POST /api/Auth/register`
   ```json
   {
     "fullName": "Lerato",
     "lastName": "Dlamini",
     "email": "lerato@raceday.com",
     "password": "Password123!",
     "role": "Participant",
     "phone": "0832222222"
   }
   ```

3. **Login**: `POST /api/Auth/login` with either user's credentials → copy the returned JWT token.

4. **Authorize in Swagger**: Click the **🔓 Authorize** button (top-right), enter `Bearer <your-token>`, and click Authorize.

5. **Create an Event** as Organiser: `POST /api/Events`

6. **Add a Category**: `POST /api/events/{eventId}/categories`

7. **Login as Participant** and enrol: `POST /api/categories/{categoryId}/enrolments`

8. **Capture a Result** as Organiser: `POST /api/enrolments/{enrolmentId}/results`

9. **View Results** (public): `GET /api/events/{eventId}/results`

---

## 🔐 Authentication & Role-Based Access

- **Authentication**: JWT Bearer tokens. Users log in with email + password (BCrypt-hashed), and receive a signed token valid for 2 hours.
- **Roles**: `Organiser` and `Participant`. Roles are stored as claims inside the JWT.
- **Authorization**: Endpoints use `[Authorize]` and `[Authorize(Roles = "...")]` attributes to enforce access.
- **Ownership checks**: An Organiser can only edit/delete their own events and categories. Ownership is verified in every protected action.
- **Password storage**: Passwords are hashed with **BCrypt** before being stored — the original password is never persisted.

---

## 🧪 Unit Tests

The project includes **12 unit tests** covering:

- User registration (success + duplicate email)
- User login (success + wrong password)
- Event management (Organiser allowed, Participant forbidden, unauthenticated blocked)
- Public endpoint access (get all events)
- Enrolment creation (Participant allowed, Organiser forbidden)
- Profile updates

### Running Tests Locally

In Visual Studio: **Test → Test Explorer → Run All Tests**

Or via the terminal:

```bash
dotnet test
```

### Test Results

```
Passed! - Failed: 0, Passed: 12, Skipped: 0, Total: 12
```

Tests use an in-memory EF Core database (`CustomWebApplicationFactory`) so they don't touch the real SQL Server instance.

---

## 🔄 CI/CD Pipeline

The repository uses **GitHub Actions** to automatically build and test the API on every push to `main`. The workflow is located at `.github/workflows/dotnet.yml`.

**Status:** ✅ [Insert screenshot of green build → `docs/ci-screenshot.png`]

---

## 📚 Documentation

Full planning documents are located in the `/docs` folder:

- **`ERD.png`** — Entity Relationship Diagram with 7 entities: Users, Events, Categories, Participants, EventEnrolments, EventRoutes, Results. Includes primary keys, foreign keys, and cardinalities.
- **`API_Endpoint_Plan.md`** — Complete RESTful API specification with methods, routes, roles, request bodies, and expected responses.
- **`RaceDay_Schema.sql`** — SQL Server database creation script with seed data.

---

## 🎥 YouTube Video

A full walkthrough demonstrating the running API, code structure, authentication, and unit tests:

---

## 🗺️ Roadmap

- [x] **Part 1** — ERD, API endpoint plan, SQL database script
- [x] **Part 2** — RESTful API with JWT auth, role-based access, Swagger, unit tests, CI/CD
- [ ] **Part 3** — MVC web application, Azure Blob Storage, Docker containerisation

---

## 📖 References

- Microsoft Learn. (2026). *ASP.NET Core Web API documentation*. Available at: https://learn.microsoft.com/en-us/aspnet/core/ [Accessed 1 October 2026].
- Microsoft Learn. (2026). *Entity Framework Core documentation*. Available at: https://learn.microsoft.com/en-us/ef/core/ [Accessed 1 October 2026].
- W3Schools. (2025). *SQL Tutorial*. Available at: https://www.w3schools.com/sql/ [Accessed 28 August 2026].
- Lucid Software Inc. (2026). *Entity Relationship Diagrams*. Lucidchart. Available at: https://www.lucidchart.com/pages/er-diagrams [Accessed 1 September 2026].

---

## 📄 License

This project is submitted as part of the IIE Programming 2B (PROG6212) PoE and is for academic purposes only.
````
