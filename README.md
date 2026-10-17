# RaceDay
PROG6212 Portfolio of Evidence

## RaceDay is an event management system built for the running, walking, and cycling communities in south africa

## User Roles
### Organiser
organisers can:
- create, edit, and delete events
- see the participants of any events they manage
- capture results of participants
- manage event categories

### Participant
participants can:
- create an account and log into the system
- see and enter a list of events to choose from by selecting by category
- view the details of each event they have enrolled in
- track their personal race results over time.

## Part 1
- ERD
- API Endpoint Plan
- SQL Database Script

## Repository Structure
RaceDay/docs/ contains:
|
| -- `RaceDay_API_Endpoint_Plan.pdf`
| -- `RaceDay_ERD.pdf`
| -- `RaceDay_Database.sql`

## Database Setup
to run the sql script:
1. Install SQL server and SQL Server Management Studio (SSMS)
2. Open SSMS and connect to your desired SQL server (or localhost) and run docs/RaceDay_Database.sql
3. Execute the script

## CI/CD
`workflows/part1-ci.yml` checks for a /docs/ directory and the above mentioned contents.

## Part 2 – RESTful API

### Technologies Used
ASP.NET Core Web API (.NET 10), Entity Framework Core (Code First), SQL Server Express, BCrypt password hashing, server-side sessions, Swagger (Swashbuckle), xUnit, GitHub Actions.

### Project Structure
- `Part2/RaceDay.Api` – controllers, models, DbContext, migrations
- `Part2/RaceDay.Contracts` – request and response DTOs
- `Part2/RaceDay.Api.Tests` – xUnit tests
- `docs` – Part 1 ERD, endpoint plan and SQL script

### Database Setup
1. Install SQL Server Express and SSMS.
2. In `Part2/RaceDay.Api/appsettings.json`, check the connection string server name matches your instance (default `localhost\SQLEXPRESS`).
3. From `Part2/RaceDay.Api`, run `dotnet ef database update`. This creates `RaceDayApiDB` from the models (Code First). The schema matches the Part 1 ERD and SQL script.

### How to Run the API
From `Part2/RaceDay.Api`: `dotnet run --launch-profile http`, then open `http://localhost:<port>/swagger` (the port is printed in the terminal).

### Authentication
Users register as an Organiser or a Participant. Passwords are hashed with BCrypt and never stored in plain text. Login creates a server-side session storing the UserID and Role. Every protected endpoint checks the session: 401 if not logged in, 403 if the role is wrong. Organisers can only change their own events.

### Differences from the Part 1 plan
- Added `POST /api/auth/logout` to end the session.

### Swagger
All endpoints, their descriptions and response codes are visible and testable at `/swagger`.

### Unit Testing
Run `dotnet test Part2/RaceDay.slnx`. Tests cover registration, login, protected endpoints, event management, wrong-role rejection and enrolments, using an in-memory database.

### CI/CD
The Part 2 workflow restores, builds and tests the solution on every push.
