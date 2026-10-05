# Game-Webapp

## How to start and run the applications

### Prerequisites

Before running the applications, make sure the following are installed:

- .NET SDK 10.0
- Node.js 24.19.0

### Subapp 1 - ASP.NER Core MVC
1. Extract the submitted ZIP file.
2. Open a terminal and navigate to the ´Subapp1´ folder.
3. Restore the required .NET packages:
    dotnet restore
4. Start the application:
    dotnet run
5. Open the URL displayed in the terminal in a web browser.



## Local database setup
The project uses SQLite with Entity Framework Core migrations.
The local `game.db`file is not tracked by Git. When the application is started,
the database is created automatically if necessary, pending migrations are applied, 
and initial question data is added if the question database is empty.

No manual database setup is required to run the application.


## Logging and error handling
The application uses Serilog for server-side logging. Log messages are written to the `Logs/` directory.

Database operations in the repository use error handling and structured logging:
- Information is logged for successful create, update, and delete operations.
- Warnings are logged when requested data cannot be found.
- Errors and exceptions are logged when database operations fail.

Generated log files are not tracked by Git.



