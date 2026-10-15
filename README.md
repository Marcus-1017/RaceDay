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

placeholder screenshot of github actions

