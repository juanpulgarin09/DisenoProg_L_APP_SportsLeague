# 🏆 SportsLeague API

A RESTful API for managing a Colombian Sports League, built with N-Layer Architecture in .NET 10. Enables full management of teams, players, referees, tournaments, matches, match events, standings, and player lineups.

---

## 📋 Table of Contents

- [General Description](#general-description)
- [Tech Stack](#tech-stack)
- [Project Architecture](#project-architecture)
- [Project History — Phases](#project-history--phases)
- [System Entities](#system-entities)
- [Available Endpoints](#available-endpoints)
- [Business Validations](#business-validations)
- [Installation & Setup](#installation--setup)
- [DataSeeder — Initial Data](#dataseeder--initial-data)
- [Migrations](#migrations)
- [File Structure](#file-structure)
- [Author](#author)

---

## 📖 General Description

SportsLeague API is a backend system that simulates the information system of a Colombian football league. It was built incrementally across 6 phases plus one evaluative event, applying the patterns and principles learned at each stage.

The project implements a **REST API** that exposes HTTP endpoints so any application (web, mobile, Swagger) can manage all league information — from creating teams to calculating the standings table in real time.

### Project Evolution

| Phase | Content | Key Concepts |
|-------|---------|--------------|
| **Phase 1** | Team CRUD | GenericRepository, AutoMapper, Swagger, N-Layers |
| **Phase 2** | Players with FK to Team | Foreign Keys, Navigation Properties, Enums |
| **Phase 3** | Referee, Tournament, TournamentTeam | N:M relationship, intermediate table, state machine |
| **Phase 4** | Match with multiple FKs | DeleteBehavior.Restrict, multiple FKs to same table |
| **Phase 5** | MatchResult, Goal, Card | 1:1 relationship, match events, MatchValidationHelper (DRY) |
| **Phase 5.1** | DataSeeder | Automatic initial data, BetPlay League 2026 |
| **Phase 6** | Standings & Statistics | Advanced LINQ, real-time calculations, no new tables |
| **EV #4** | MatchLineup — Lineups | Cross validations, max starters rule, Phase 5 pattern |

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Main framework |
| C# | — | Programming language |
| ASP.NET Core | 10.0 | Web framework for the REST API |
| Entity Framework Core | 8.0 | ORM — Code First with SQL Server |
| SQL Server | — | Database engine |
| AutoMapper | 12.0.1 | Automatic mapping between Entities and DTOs |
| Swagger / Swashbuckle | 10.1 | Interactive API documentation |

---

## 🏗️ Project Architecture

The solution follows an **N-Layer Architecture** with three independent projects:

```
SportsLeague/
├── SportsLeague.Domain       → Business Layer (the brain)
├── SportsLeague.DataAccess   → Data Layer (the storage)
└── SportsLeague.API          → Presentation Layer (the window)
```

### Each Layer's Responsibility

**SportsLeague.Domain** — Does not depend on any other layer:
- Entities (classes that represent database tables)
- Enums (fixed value lists)
- Interfaces (contracts that define what methods exist)
- Services (business logic and validations)
- Helpers (reusable auxiliary classes)

**SportsLeague.DataAccess** — Depends only on Domain:
- DbContext (EF Core configuration and relationships)
- Repositories (implement interfaces and talk to SQL Server)
- Migrations (database change history)
- Seeders (automatic initial data)

**SportsLeague.API** — Depends on Domain and DataAccess:
- Controllers (receive HTTP requests, return JSON)
- DTOs (define what data goes in and out)
- Mappings (AutoMapper — converts between DTOs and Entities)
- Program.cs (configuration and dependency injection)

### HTTP Request Flow

```
Client (Swagger / Mobile App / Web)
              │
              │  POST /api/Match
              ▼
        MatchController
        ├── Receives MatchRequestDTO
        ├── AutoMapper: DTO → Entity
        └── calls matchService.CreateAsync()
              │
              ▼
        MatchService
        ├── Validates: tournament exists and is InProgress?
        ├── Validates: teams are different?
        ├── Validates: teams are enrolled in tournament?
        ├── Validates: referee exists?
        └── calls matchRepository.CreateAsync()
              │
              ▼
        MatchRepository
        ├── entity.CreatedAt = DateTime.UtcNow
        ├── _dbSet.AddAsync(entity)
        └── _context.SaveChangesAsync()
              │
              ▼
        SQL Server
        INSERT INTO Matches (...)
              │
              ▼ (response goes up)
        MatchController
        ├── AutoMapper: Entity → MatchResponseDTO
        └── return 201 Created + JSON
              │
              ▼
Client receives: { "id": 1, "homeTeamName": "Atlético Nacional", ... }
```

---

## 📚 Project History — Phases

### Phase 1 — Team CRUD
The foundation of the entire project. Establishes the N-Layer architecture, the `GenericRepository<T>` that all entities inherit from, AutoMapper for mappings, and Swagger for documentation. The pattern established here repeats throughout all subsequent phases.

### Phase 2 — Player with FK
Introduces **Foreign Keys** and **Navigation Properties**. A player belongs to a team — the first 1:N relationship in the project. Also introduces **Enums** with `PlayerPosition`. Key validation: jersey number must be unique within the same team.

### Phase 3 — Referee, Tournament and TournamentTeam (N:M)
First **many-to-many relationship** in the project. A tournament can have many teams and a team can be in many tournaments. Solved with the intermediate table `TournamentTeam`. Also introduces the **tournament state machine**: Pending → InProgress → Finished (unidirectional flow).

### Phase 4 — Match
The match introduces the challenge of **multiple Foreign Keys to the same table**. A match has two teams (HomeTeam and AwayTeam), both pointing to the Teams table. This requires using `DeleteBehavior.Restrict` instead of `Cascade` to avoid SQL Server cascade cycles. State machine: Scheduled → InProgress → Finished / Suspended.

### Phase 5 — MatchResult, Goal and Card
Introduces the **1:1 relationship** between Match and MatchResult (a match has exactly one result). Goals and Cards are 1:N relationships. The `MatchValidationHelper` applies the **DRY principle** (Don't Repeat Yourself) — the same validations needed by Goals and Cards are centralized in one auxiliary class.

### Phase 5.1 — DataSeeder
Initial data automation. When the API starts with an empty database, it automatically creates 20 BetPlay 2026 League teams, 80 players, 4 referees, 1 tournament in InProgress state, and all 20 enrollments. Only acts if the database is completely empty.

### Phase 6 — Standings and Statistics
The most advanced phase in LINQ. Calculates in real time the standings table, top scorers, and card statistics. **Creates no new tables** — everything is processed in memory from existing data. Own goals do not count toward the scorers table.

### Evaluative Event #4 — MatchLineup
Player lineups for matches. Allows registering the players called up for a match, indicating whether they are starters or substitutes. Follows the same pattern as Goal and Card. Special validation: maximum 11 starters per team per match. Substitutes have no limit.

---

## 🗃️ System Entities

### AuditBase — Abstract Base Class
All entities inherit from this class:

| Field | Type | Description |
|-------|------|-------------|
| `Id` | int | Auto-incremental PK |
| `CreatedAt` | DateTime | Creation date |
| `UpdatedAt` | DateTime? | Last update date (nullable) |

---

### Team — Teams
| Field | Type | Constraint |
|-------|------|------------|
| Name | string(100) | Required, unique |
| City | string(100) | Required |
| Stadium | string(150) | Required |
| LogoUrl | string(500)? | Optional |
| FoundedDate | DateTime | Required |

---

### Player — Players
| Field | Type | Constraint |
|-------|------|------------|
| FirstName | string(80) | Required |
| LastName | string(80) | Required |
| BirthDate | DateTime | Required |
| Number | int | Unique per team |
| Position | PlayerPosition | Enum |
| TeamId | int | FK → Team |

**Enum PlayerPosition:** `Goalkeeper=0`, `Defender=1`, `Midfielder=2`, `Forward=3`

---

### Referee — Referees
| Field | Type | Constraint |
|-------|------|------------|
| FirstName | string(80) | Required |
| LastName | string(80) | Required |
| Nationality | string(80) | Required |

---

### Tournament — Tournaments
| Field | Type | Constraint |
|-------|------|------------|
| Name | string(150) | Required |
| Season | string(20) | Required |
| StartDate | DateTime | Required |
| EndDate | DateTime | Required |
| Status | TournamentStatus | Enum |

**Enum TournamentStatus:** `Pending=0` → `InProgress=1` → `Finished=2`

---

### TournamentTeam — Team-Tournament Enrollment (N:M)
| Field | Type | Constraint |
|-------|------|------------|
| TournamentId | int | FK → Tournament |
| TeamId | int | FK → Team |
| RegisteredAt | DateTime | Required |

> Unique composite index: `(TournamentId, TeamId)`

---

### Match — Matches
| Field | Type | Constraint |
|-------|------|------------|
| TournamentId | int | FK → Tournament (Cascade) |
| HomeTeamId | int | FK → Team (Restrict) |
| AwayTeamId | int | FK → Team (Restrict) |
| RefereeId | int | FK → Referee (Restrict) |
| MatchDate | DateTime | Required |
| Venue | string(150) | Match venue |
| Matchday | int | Round number in tournament |
| Status | MatchStatus | Enum |

**Enum MatchStatus:** `Scheduled=0` → `InProgress=1` → `Finished=2` / `Suspended=3`

> Why Restrict on Teams and Referee? SQL Server cannot handle two cascade paths to the same table. Using Restrict prevents this cycle and protects historical data.

---

### MatchResult — Match Result (1:1)
| Field | Type | Constraint |
|-------|------|------------|
| MatchId | int | FK → Match (unique — enforces 1:1) |
| HomeGoals | int | Required, ≥ 0 |
| AwayGoals | int | Required, ≥ 0 |
| Observations | string(500)? | Optional |

---

### Goal — Goals
| Field | Type | Constraint |
|-------|------|------------|
| MatchId | int | FK → Match (Cascade) |
| PlayerId | int | FK → Player (Restrict) |
| Minute | int | Between 1 and 120 |
| Type | GoalType | Enum |

**Enum GoalType:** `Normal=0`, `Penalty=1`, `OwnGoal=2`

---

### Card — Cards
| Field | Type | Constraint |
|-------|------|------------|
| MatchId | int | FK → Match (Cascade) |
| PlayerId | int | FK → Player (Restrict) |
| Minute | int | Between 1 and 120 |
| Type | CardType | Enum |

**Enum CardType:** `Yellow=0`, `Red=1`

---

### MatchLineup — Player Lineups (EV #4)
| Field | Type | Constraint |
|-------|------|------------|
| MatchId | int | FK → Match (Cascade) |
| PlayerId | int | FK → Player (Restrict) |
| IsStarter | bool | true = Starter, false = Substitute |
| Position | string(10) | "GK", "CB", "ST", etc. |

> Unique composite index: `(MatchId, PlayerId)` — a player cannot appear twice in the same lineup.

---

## 🌐 Available Endpoints

### Team
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| GET | `/api/Team` | List all teams | 200 |
| GET | `/api/Team/{id}` | Get team by ID | 200 / 404 |
| POST | `/api/Team` | Create team | 201 / 409 |
| PUT | `/api/Team/{id}` | Update team | 204 / 404 / 409 |
| DELETE | `/api/Team/{id}` | Delete team | 204 / 404 |

### Player
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| GET | `/api/Player` | List all players | 200 |
| GET | `/api/Player/{id}` | Get player by ID | 200 / 404 |
| GET | `/api/Player/team/{teamId}` | Players by team | 200 / 404 |
| POST | `/api/Player` | Create player | 201 / 404 / 409 |
| PUT | `/api/Player/{id}` | Update player | 204 / 404 / 409 |
| DELETE | `/api/Player/{id}` | Delete player | 204 / 404 |

### Referee
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| GET | `/api/Referee` | List all referees | 200 |
| GET | `/api/Referee/{id}` | Get referee by ID | 200 / 404 |
| POST | `/api/Referee` | Create referee | 201 |
| PUT | `/api/Referee/{id}` | Update referee | 204 / 404 |
| DELETE | `/api/Referee/{id}` | Delete referee | 204 / 404 |

### Tournament
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| GET | `/api/Tournament` | List all tournaments | 200 |
| GET | `/api/Tournament/{id}` | Get tournament by ID | 200 / 404 |
| POST | `/api/Tournament` | Create tournament | 201 / 400 |
| PUT | `/api/Tournament/{id}` | Update tournament | 204 / 404 / 409 |
| DELETE | `/api/Tournament/{id}` | Delete tournament | 204 / 404 / 409 |
| PATCH | `/api/Tournament/{id}/status` | Change status | 204 / 404 / 409 |
| POST | `/api/Tournament/{id}/teams` | Enroll team | 200 / 404 / 409 |
| GET | `/api/Tournament/{id}/teams` | List enrolled teams | 200 / 404 |

### Match
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| GET | `/api/Match/tournament/{tournamentId}` | Matches by tournament | 200 / 404 |
| GET | `/api/Match/{id}` | Get match by ID | 200 / 404 |
| POST | `/api/Match` | Create match | 201 / 404 / 409 |
| PUT | `/api/Match/{id}` | Update match | 204 / 404 / 409 |
| DELETE | `/api/Match/{id}` | Delete match | 204 / 404 / 409 |
| PATCH | `/api/Match/{id}/status` | Change match status | 204 / 404 / 409 |

### Match Events
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| POST | `/api/match/{id}/result` | Register result | 200 / 404 / 409 |
| GET | `/api/match/{id}/result` | Get result | 200 / 404 |
| POST | `/api/match/{id}/goals` | Register goal | 200 / 404 / 409 |
| GET | `/api/match/{id}/goals` | List goals | 200 / 404 |
| DELETE | `/api/match/{id}/goals/{gId}` | Delete goal | 204 / 404 |
| POST | `/api/match/{id}/cards` | Register card | 200 / 404 / 409 |
| GET | `/api/match/{id}/cards` | List cards | 200 / 404 |
| DELETE | `/api/match/{id}/cards/{cId}` | Delete card | 204 / 404 |

### Match Lineups — EV #4
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| POST | `/api/match/{id}/lineup` | Add player to lineup | 201 / 404 / 409 |
| GET | `/api/match/{id}/lineup` | Get full lineup | 200 / 404 |
| GET | `/api/match/{id}/lineup/team/{teamId}` | Get lineup by team | 200 / 404 |
| DELETE | `/api/match/{id}/lineup/{lineupId}` | Remove player | 204 / 404 |

### Statistics — Phase 6
| Method | Route | Description | HTTP |
|--------|-------|-------------|------|
| GET | `/api/standings?tournamentId=1` | Standings table | 200 / 404 |
| GET | `/api/stats/scorers?tournamentId=1` | Top scorers | 200 / 404 |
| GET | `/api/stats/cards?tournamentId=1` | Card statistics | 200 / 404 |

### HTTP Status Codes Used

| Code | Meaning | When |
|------|---------|------|
| 200 OK | Success with data | Successful GET requests |
| 201 Created | Resource created | Successful POST requests |
| 204 No Content | Success without data | Successful PUT and DELETE |
| 404 Not Found | Resource not found | ID not found in database |
| 409 Conflict | Business rule conflict | Failed validation |

---

## ✅ Business Validations

### Teams
- Team name must be unique across the system.

### Players
- Jersey number must be unique within the same team.
- The team the player belongs to must exist.

### Tournaments
- End date must be later than start date.
- Valid state transitions: `Pending → InProgress → Finished`.
- Only `Pending` tournaments can be edited or deleted.
- Only `Pending` tournaments can enroll teams.
- A team cannot be enrolled in the same tournament twice.

### Matches
- Tournament must exist and be in `InProgress` state.
- Home team and away team must be different.
- Both teams must be enrolled in the tournament.
- The referee must exist.
- Only `Scheduled` matches can be edited or deleted.
- Valid transitions: `Scheduled → InProgress → Finished` / `Scheduled or InProgress → Suspended`.

### Goals and Cards
- Match must be in `InProgress` or `Finished` state.
- Player must belong to one of the two teams in the match.
- Minute must be between 1 and 120.

### MatchResult
- Match must be in `Finished` state.
- A match cannot have two results.
- Goals cannot be negative.

### MatchLineup — EV #4
- Match must exist.
- Player must exist.
- Player must belong to the HomeTeam or AwayTeam of the match.
- A player cannot appear twice in the same match lineup.
- Maximum 11 starters per team per match (substitutes have no limit).
- Match must be in `Scheduled` state.

---

## ⚙️ Installation & Setup

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022
- SQL Server (Express, Developer, or LocalDB)
- SQL Server Management Studio (optional)

### Steps

**1. Clone the repository**
```bash
git clone https://github.com/juanpulgarin09/DisenoProg_L_APP_SportsLeague.git
cd SportsLeague
```

**2. Configure the connection string**

Edit `SportsLeague.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SportsLeagueDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**3. Apply migrations**

In Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console**, select `SportsLeague.DataAccess` as the Default Project and run:
```
Update-Database
```

**4. Run the application**

Press `F5` in Visual Studio. The DataSeeder will automatically populate the database with the BetPlay 2026 League initial data.

**5. Open Swagger**

Navigate to: `https://localhost:7019/swagger`

> The application automatically redirects from `/` to `/swagger`.

---

## 🌱 DataSeeder — Initial Data

When the API starts with an empty database, the following is created automatically:

| What | Count | Detail |
|------|-------|--------|
| Teams | 20 | BetPlay 2026 League — real Colombian teams |
| Players | 80 | 4 per team — real Colombian players |
| Referees | 4 | Real Colombian referees |
| Tournament | 1 | "Liga BetPlay 2026-I" in InProgress state |
| Enrollments | 20 | All 20 teams enrolled in the tournament |

The Seeder only acts if the database is completely empty. If data already exists, it does nothing.

---

## 🗄️ Migrations

| Migration | Tables Created | Phase |
|-----------|----------------|-------|
| `InitialDataBase` | Teams | Phase 1 |
| `AddPlayerEntity` | Players | Phase 2 |
| `New3TablesRefereeTournamentAndTournamentTeam` | Referees, Tournaments, TournamentTeams | Phase 3 |
| `AddMatchEntity` | Matches | Phase 4 |
| `New3TablesMatchResultGoalCard` | MatchResults, Goals, Cards | Phase 5 |
| `AddMatchLineup` | MatchLineups | EV #4 |

To create a new migration:
```
Add-Migration MigrationName
Update-Database
```

---

## 📁 File Structure

```
SportsLeague/
│
├── SportsLeague.API/
│   ├── Controllers/
│   │   ├── TeamController.cs
│   │   ├── PlayerController.cs
│   │   ├── RefereeController.cs
│   │   ├── TournamentController.cs
│   │   ├── MatchController.cs
│   │   ├── MatchEventController.cs
│   │   ├── MatchLineupController.cs
│   │   └── StandingsController.cs
│   ├── DTOs/
│   │   ├── Request/
│   │   │   ├── TeamRequestDTO.cs
│   │   │   ├── PlayerRequestDTO.cs
│   │   │   ├── RefereeRequestDTO.cs
│   │   │   ├── TournamentRequestDTO.cs
│   │   │   ├── RegisterTeamDTO.cs
│   │   │   ├── UpdateTournamentStatusDTO.cs
│   │   │   ├── MatchRequestDTO.cs
│   │   │   ├── UpdateMatchStatusDTO.cs
│   │   │   ├── MatchResultRequestDTO.cs
│   │   │   ├── GoalRequestDTO.cs
│   │   │   ├── CardRequestDTO.cs
│   │   │   └── CreateMatchLineupDTO.cs
│   │   └── Response/
│   │       ├── TeamResponseDTO.cs
│   │       ├── PlayerResponseDTO.cs
│   │       ├── RefereeResponseDTO.cs
│   │       ├── TournamentResponseDTO.cs
│   │       ├── MatchResponseDTO.cs
│   │       ├── MatchResultResponseDTO.cs
│   │       ├── GoalResponseDTO.cs
│   │       ├── CardResponseDTO.cs
│   │       ├── MatchLineupResponseDTO.cs
│   │       ├── StandingDTO.cs
│   │       ├── TopScorerDTO.cs
│   │       └── CardStatsDTO.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── SportsLeague.Domain/
│   ├── Entities/
│   │   ├── AuditBase.cs
│   │   ├── Team.cs
│   │   ├── Player.cs
│   │   ├── Referee.cs
│   │   ├── Tournament.cs
│   │   ├── TournamentTeam.cs
│   │   ├── Match.cs
│   │   ├── MatchResult.cs
│   │   ├── Goal.cs
│   │   ├── Card.cs
│   │   └── MatchLineup.cs
│   ├── Enums/
│   │   ├── PlayerPosition.cs
│   │   ├── TournamentStatus.cs
│   │   ├── MatchStatus.cs
│   │   ├── GoalType.cs
│   │   └── CardType.cs
│   ├── Helpers/
│   │   └── MatchValidationHelper.cs
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IGenericRepository.cs
│   │   │   ├── ITeamRepository.cs
│   │   │   ├── IPlayerRepository.cs
│   │   │   ├── IRefereeRepository.cs
│   │   │   ├── ITournamentRepository.cs
│   │   │   ├── ITournamentTeamRepository.cs
│   │   │   ├── IMatchRepository.cs
│   │   │   ├── IMatchResultRepository.cs
│   │   │   ├── IGoalRepository.cs
│   │   │   ├── ICardRepository.cs
│   │   │   └── IMatchLineupRepository.cs
│   │   └── Services/
│   │       ├── ITeamService.cs
│   │       ├── IPlayerService.cs
│   │       ├── IRefereeService.cs
│   │       ├── ITournamentService.cs
│   │       ├── IMatchService.cs
│   │       ├── IMatchEventService.cs
│   │       ├── IMatchLineupService.cs
│   │       └── IStandingsService.cs
│   └── Services/
│       ├── TeamService.cs
│       ├── PlayerService.cs
│       ├── RefereeService.cs
│       ├── TournamentService.cs
│       ├── MatchService.cs
│       ├── MatchEventService.cs
│       ├── MatchLineupService.cs
│       └── StandingsService.cs
│
└── SportsLeague.DataAccess/
    ├── Context/
    │   └── LeagueDbContext.cs
    ├── Repositories/
    │   ├── GenericRepository.cs
    │   ├── TeamRepository.cs
    │   ├── PlayerRepository.cs
    │   ├── RefereeRepository.cs
    │   ├── TournamentRepository.cs
    │   ├── TournamentTeamRepository.cs
    │   ├── MatchRepository.cs
    │   ├── MatchResultRepository.cs
    │   ├── GoalRepository.cs
    │   ├── CardRepository.cs
    │   └── MatchLineupRepository.cs
    ├── Seeders/
    │   └── DataSeeder.cs
    └── Migrations/
        ├── InitialDataBase.cs
        ├── AddPlayerEntity.cs
        ├── New3TablesRefereeTournamentAndTournamentTeam.cs
        ├── New3TablesMatchResultGoalCard.cs
        └── AddMatchLineup.cs
```

---

## 👤 Author

**Juan Pulgarin**
Software Design Student
Instituto Tecnológico Metropolitano — ITM
Professor: Carlos Diaz
Semester 2026-1
GitHub: [@juanpulgarin09](https://github.com/juanpulgarin09)
