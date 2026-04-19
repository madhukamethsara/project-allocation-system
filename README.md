# BlindMatch PAS — PUSL2020 Coursework

A secure, web-based **Project Approval System** with blind-matching logic.  
Built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **SQL Server**.

---

## Quick Setup (Step-by-Step)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 **or** VS Code + C# extension

---

### Step 1 — Clone / Open the project

```bash
cd BlindMatchPAS
```

---

### Step 2 — Restore NuGet packages

```bash
dotnet restore
```

---

### Step 3 — Configure your connection string

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlindMatchPAS;Trusted_Connection=True;"
}
```

> For full SQL Server: `Server=YOUR_SERVER;Database=BlindMatchPAS;User Id=sa;Password=yourpassword;`

---

### Step 4 — Apply EF Core Migrations

```bash
# From the BlindMatchPAS folder:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> The app also auto-applies migrations on startup via `db.Database.Migrate()` in `Program.cs`.

---

### Step 5 — Run the application

```bash
dotnet run
```

Navigate to: `https://localhost:5001`

---

### Step 6 — Run Unit Tests

```bash
cd ../BlindMatchPAS.Tests
dotnet test
```

---

## Project Structure

```
BlindMatchPAS/
├── Controllers/
│   ├── AccountController.cs      # Login / Register / Logout
│   ├── StudentController.cs      # Submit, Edit, Withdraw projects
│   ├── SupervisorController.cs   # Blind review + Match
│   ├── AdminController.cs        # Oversight dashboard
│   └── HomeController.cs         # Landing page
│
├── Models/
│   ├── ApplicationUser.cs        # User entity (Student/Supervisor/Admin)
│   ├── Project.cs                # Project entity with status FSM
│   ├── ResearchArea.cs           # Research area entity
│   └── ViewModels/
│       └── ViewModels.cs         # All view-specific models
│
├── Data/
│   └── ApplicationDbContext.cs   # EF Core DbContext + seed data
│
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── IProjectService.cs / ProjectService.cs   ← BLIND MATCH LOGIC HERE
│   └── ResearchAreaService.cs
│
├── Views/
│   ├── Shared/_Layout.cshtml     # Master layout (black/white theme)
│   ├── Account/                  # Login, Register
│   ├── Student/                  # Dashboard, Submit, Edit
│   ├── Supervisor/               # Blind dashboard + match
│   └── Admin/                    # Overview, projects, matches, areas
│
├── Program.cs                    # DI registration + pipeline
└── appsettings.json

BlindMatchPAS.Tests/
└── ProjectServiceTests.cs        # 8 unit tests covering all core logic
```

---

## Blind Matching Logic — How It Works

The core business rule is in `ProjectService.MapToBlindViewModel()`:

```csharp
// IDENTITY REVEAL: only shown AFTER this specific supervisor matches the project
RevealedStudentName  = isMatchedByThisSupervisor ? p.Student?.Name  : null,
RevealedStudentEmail = isMatchedByThisSupervisor ? p.Student?.Email : null
```

### State Machine

```
[Student Submits]
      │
      ▼
  [Pending]
      │
      │  Supervisor views → sees Title, Abstract, TechStack
      │  Student identity: ◼◼◼ HIDDEN ◼◼◼
      │
      │  Supervisor clicks "Confirm Match"
      ▼
  [Matched]  ──→  Identity REVEALED to both parties
      │
      └──→ Student sees: Supervisor name + email
      └──→ Supervisor sees: Student name + email
```

---

## User Roles & Test Accounts

Register accounts via `/Account/Register` and select the role.

| Role       | Dashboard URL          | Can Do                              |
|------------|------------------------|-------------------------------------|
| Student    | `/Student/Dashboard`   | Submit, Edit, Withdraw, Track status|
| Supervisor | `/Supervisor/Dashboard`| Blind review, Confirm match         |
| Admin      | `/Admin/Dashboard`     | View all, manage areas              |

---

## Git Commit Strategy (for coursework marks)

Follow this commit structure:

```bash
git init
git add .
git commit -m "chore: initial project scaffold with ASP.NET Core 8 MVC"

git commit -m "feat: add ApplicationUser, Project, ResearchArea models"
git commit -m "feat: configure EF Core DbContext with seed data"
git commit -m "feat: add AuthService with BCrypt password hashing"
git commit -m "feat: implement blind matching logic in ProjectService"
git commit -m "feat: add StudentController – submit, edit, withdraw"
git commit -m "feat: add SupervisorController with identity reveal on match"
git commit -m "feat: add AdminController – oversight and area management"
git commit -m "feat: add Razor views with black/white theme"
git commit -m "test: add 8 unit tests covering all core matching scenarios"
git commit -m "chore: add EF Core migration InitialCreate"
```

---

## Unit Tests Summary

| Test | Covers |
|------|--------|
| `SubmitProject_ShouldCreateProjectWithPendingStatus` | Project creation |
| `GetBlindProjects_ShouldHideStudentIdentityBeforeMatch` | Blind phase |
| `MatchProject_ShouldRevealStudentIdentityAfterMatch` | Identity reveal |
| `MatchProject_AlreadyMatched_ShouldReturnFalse` | Double-match guard |
| `WithdrawProject_ShouldSetStatusToWithdrawn` | Withdrawal flow |
| `WithdrawProject_WhenMatched_ShouldReturnFalse` | Guard on matched projects |
| `UpdateProject_WhenPending_ShouldSucceed` | Edit while pending |
| `UpdateProject_WhenMatched_ShouldReturnFalse` | Guard on matched edits |

---

## Technology Decisions

| Concern | Choice | Reason |
|---------|--------|--------|
| Auth | Session + BCrypt | Simple, no extra libs needed for coursework scope |
| ORM | EF Core + Migrations | Required by spec, schema versioning built-in |
| DI | ASP.NET Core built-in | Clean separation of service/controller concerns |
| Tests | xUnit + InMemory EF | Fast, no SQL Server needed for CI |
| UI | Razor + inline CSS | No JS build step, matches spec's Razor requirement |
