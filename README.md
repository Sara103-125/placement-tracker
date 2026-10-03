# Placement Application Tracker

A full-stack web app for keeping track of placement and internship applications: where you've applied, what stage each one is at, and which deadlines are coming up.

Built with an **ASP.NET Core 8 Web API**, **Entity Framework Core** and **SQL Server**, plus an **Angular** front end.

## Screenshots

**Dashboard**: headline numbers, a bar chart of applications by status, a progress funnel, and deadlines in the next 7 days.

![Dashboard](docs/screenshots/dashboard.png)

**Kanban board**: drag a card to another column to change its status.

![Kanban board](docs/screenshots/board.png)

**Applications list**: search, filter by status, sort by any column, with overdue and due-soon deadline alerts.

![Applications list](docs/screenshots/applications.png)

**Add / Edit form** with validation, plus a timeline of every status change.

![Edit form](docs/screenshots/edit-form.png)

## Features

- **Add, edit and delete** applications with company, role, location, job link, date applied, deadline, status and notes
- **Six statuses**: Wishlist, Applied, Online Test, Interview, Offer, Rejected
- **List page** with a search box (company or role), a status filter, and **sorting** by clicking any column heading
- **Deadline alerts**: deadlines that have passed are marked *Overdue*, and ones due within 3 days are marked *Due soon*
- **Kanban board**: drag-and-drop cards between status columns, saved straight to the API
- **Status history**: every status change is recorded with a date and shown as a timeline on the edit page
- **Dashboard** with headline numbers, an *Applications by status* bar chart, a *Progress funnel* (how many applications reached each stage, using the status history), and deadlines in the next 7 days
- **Form validation** in the browser and on the API (required fields, max lengths, valid links)
- **Delete confirmation** so nothing is removed by accident
- **Responsive** layout that works on phones

## Tech stack

| Part | Technology |
|---|---|
| Front end | Angular 22 (standalone components, signals, Reactive Forms), Angular CDK drag-and-drop, plain CSS (charts built with HTML/CSS, no chart library) |
| Back end | ASP.NET Core 8 Web API (controllers) |
| Database | SQL Server LocalDB with Entity Framework Core 8 (code-first migrations) |
| API docs | Swagger / OpenAPI |
| Tests | xUnit with the EF Core in-memory provider |

## Project structure

```
placement-tracker/
├── client/                         Angular app
│   └── src/app/
│       ├── models/                 TypeScript types matching the API
│       ├── services/               ApplicationService: every HTTP call to the API
│       ├── utils/                  Deadline helpers (overdue / due soon)
│       └── pages/
│           ├── dashboard/          Headline numbers, charts, upcoming deadlines
│           ├── application-list/   Table with search, filter and sorting
│           ├── application-form/   Add / Edit form + status history timeline
│           └── board/              Kanban board with drag and drop
├── server/
│   ├── PlacementTracker.Api/       ASP.NET Core Web API
│   │   ├── Controllers/            ApplicationsController, DashboardController, HealthController
│   │   ├── Data/                   AppDbContext (EF Core)
│   │   ├── Dtos/                   Request and response shapes
│   │   ├── Entities/               JobApplication, StatusChange, ApplicationStatus
│   │   └── Migrations/             Database schema history
│   └── PlacementTracker.Api.Tests/ xUnit tests
└── docs/screenshots/
```

## How to run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer, with the .NET 8 runtime installed)
- [Node.js](https://nodejs.org/) 22.22+ or 24.15+
- Angular CLI: `npm install -g @angular/cli`
- SQL Server **LocalDB**, which comes with Visual Studio (or install "SQL Server Express LocalDB")

### 1. Start the API

```powershell
cd server/PlacementTracker.Api
dotnet run --launch-profile http
```

The first run creates the `PlacementTracker` database in LocalDB automatically (EF Core migrations run at startup in Development).
The API runs on http://localhost:5117, and Swagger is at http://localhost:5117/swagger.

### 2. Start the Angular app (in a second terminal)

```powershell
cd client
npm install
ng serve        # or: npm start
```

Open **http://localhost:4200**.

The Angular dev server forwards every `/api/...` request to the API on port 5117 (see `client/proxy.conf.json`), so no CORS setup is needed.

### 3. Run the tests

Stop the API first (Ctrl+C), because a running API locks its build files. Then:

```powershell
cd server
dotnet test
```

## API endpoints

| Method | URL | Description |
|---|---|---|
| GET | `/api/applications?search=&status=` | List applications (optional search and status filter) |
| GET | `/api/applications/{id}` | Get one application |
| POST | `/api/applications` | Create an application |
| PUT | `/api/applications/{id}` | Update an application |
| PATCH | `/api/applications/{id}/status` | Change only the status (used by the Kanban board) |
| GET | `/api/applications/{id}/history` | Status history of one application |
| DELETE | `/api/applications/{id}` | Delete an application (and its history) |
| GET | `/api/dashboard/summary` | Counts per status, progress funnel and deadlines in the next 7 days |
| GET | `/api/health` | Checks the API can reach the database |

## Possible improvements

- User accounts with login, so several people can use it
- Drag to reorder cards within a board column
- Reminders before a deadline
- Deploying to the cloud (for example Azure App Service and Azure SQL)
