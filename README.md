# ExpenseManager API

A robust ASP.NET Core Web API for managing expenses, designed with scalability and security in mind.

## 🚀 Setup & Run Instructions

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- Visual Studio 2022 or VS Code

### Installation

1.  **Clone the repository**:

    ```bash
    git clone <repository-url>
    cd ExpenseManager
    ```

2.  **Restore dependencies**:

    ```bash
    dotnet restore
    ```

3.  **Update Database**:
    Ensure your connection string in `appsettings.json` is correct, then run:
    ```bash
    dotnet ef database update
    ```

### Running the Application

To start the API in development mode:

```bash
dotnet run --project ExpenseManager.API/ExpenseManager.API.csproj
```

The API will be available at `https://localhost:5001` (or the port specified in `launchSettings.json`).

### API Documentation

Once running, you can access the Swagger UI at:
`https://localhost:5001/swagger`
