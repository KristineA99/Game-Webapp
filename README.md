# Game-Webapp

## How to start and run the applications

### Prerequisites

Before running the applications, make sure the following are installed:

- .NET SDK 10.0
- Node.js 24.19.0

### Subapp 1 - ASP.NET Core MVC
1. Extract the submitted ZIP file.
2. Open a terminal and navigate to the `Subapp1` folder.
3. Restore the required .NET packages:
    dotnet restore
4. Start the application:
    dotnet run
5. Open the URL displayed in the terminal in a web browser.



## Local database setup
The project uses SQLite with Entity Framework Core migrations.
The local `game.db` file is not tracked by Git. When the application is started,
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

### Subapp 2 — ASP.NET Core API and React SPA

#### Frameworks and dependencies

Subapp2 consists of two separate parts:

- **Backend:** ASP.NET Core Web API targeting .NET 10, with Entity Framework Core, SQLite, ASP.NET Core Identity and Serilog.
- **Frontend:** React 19 with TypeScript, built and served during development using Vite 8.
- **Client libraries:** React Router for navigation, TanStack Query for API data and caching, and Bootstrap/React Bootstrap for styling.

### prerequisites:

- .NET 10 SDK
- Node.js 24 LTS, including npm

The project's NuGet and npm dependencies are declared in `Subapp2/api/api.csproj` and `Subapp2/client/package.json`. They are restored by the commands below; individual packages do not need to be installed manually.

#### Startup instructions

1. Extract the submitted ZIP file, or clone the repository.

2. Open a terminal in the project root and navigate to the frontend folder:

   ```powershell
   cd Subapp2/client
   ```

3. Install the frontend dependencies using the committed lockfile:

   ```powershell
   npm ci
   ```

4. Restore the backend dependencies:

   ```powershell
   dotnet restore ../api/api.csproj
   ```

5. Start Subapp2:

   ```powershell
   npm run dev
   ```

   This command starts both:
   - The ASP.NET Core API at `http://localhost:5169`.
   - The Vite frontend at `http://localhost:5173`.

   A browser window should open automatically. If it does not, open:

   ```text
   http://localhost:5173
   ```

6. Keep the terminal running while using the application. Press `Ctrl+C` to stop both servers.

Do not start a second API instance while `npm run dev` is running. If a port is already occupied, stop the existing process and try again. If Vite reports a different frontend port, use the address printed in the terminal.

#### Database and accounts

Subapp2 uses its own SQLite database. At API startup, the application automatically applies migrations and seeds initial categories, questions and quizzes when required. No separate database server or manual migration command is needed.

The application also creates the Admin role and a demo administrator using the `AdminUser` settings in `Subapp2/api/appsettings.json`. Use the email and password specified there to log in as Admin.

Existing accounts are preserved across restarts; changing the configured demo password does not reset an existing account's password.

Guests can play quizzes without registering. Registration creates an ordinary player account. Only administrators can access Category, Question and Quiz management through the Admin Dashboard.

#### Configuration

- Backend settings and the SQLite connection string are in `Subapp2/api/appsettings.json`.
- The development API address is configured through `VITE_API_URL` in `Subapp2/client/.env.development`.
- The API's development listening address is configured in `Subapp2/api/Properties/launchSettings.json`.

The supplied development configuration uses the local addresses listed above.

#### Build and lint checks

From `Subapp2/client`, build the frontend:

```powershell
npm run build
```

Check the frontend with ESLint:

```powershell
npm run lint
```

Build the backend:

```powershell
dotnet build ../api/api.csproj
```

`npm run build` creates the frontend production files in `Subapp2/client/dist`. For the local demonstration, use `npm run dev` to start both the API and frontend.

### Gameplay

Select **Play**, choose a quiz, answer each question and submit the completed quiz. The API validates question membership, duplicate answers, completeness and valid answer options before calculating the score.

Results are not saved. Refreshing or leaving the gameplay page discards the current attempt. If questions are added or removed during play, submission may be rejected with instructions to restart.

#### Logging and error handling

The backend uses Serilog to write API logs to `Subapp2/api/Logs/`.

- Successful database changes and quiz scoring are logged as information.
- Rejected operations and invalid gameplay submissions are logged as warnings.
- Database exceptions are logged as errors, including exception details.

API responses distinguish invalid input (`400`), unauthenticated access (`401`), insufficient permissions (`403`), missing records (`404`) and server failures (`500`). Searches with no matches return an empty list rather than an error.

The frontend displays validation and request errors, together with loading and empty states. Unexpected database exception details remain in server logs rather than being included in API error messages.

Generated log files are not tracked by Git 


