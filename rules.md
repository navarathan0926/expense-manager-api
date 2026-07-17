Install all required NuGet packages in the correct layers based on 
strict Clean Architecture rules. Here is the complete package list 
with reasons:

## Core Rule
- API layer: anything that touches the HTTP request pipeline, 
  middleware, or presentation
- Infrastructure layer: anything that talks to external systems 
  (database, file system, encryption, network)
- Application layer: ONLY pure logic libraries with no IO, 
  no network, no file system dependencies

## ExpenseManager.API packages

dotnet add ExpenseManager.API package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
Reason: JwtBearer is ASP.NET Core middleware that intercepts incoming 
HTTP requests and validates the JWT token in the Authorization header. 
This is a request pipeline concern — it belongs in the layer that 
owns the HTTP pipeline, which is API. It does NOT generate tokens, 
it only validates them on arrival.

dotnet add ExpenseManager.API package Swashbuckle.AspNetCore --version 6.5.0
Reason: Swagger generates API documentation and a UI for testing 
endpoints. This is purely a presentation/documentation concern with 
no business logic. It only makes sense in the API layer where 
controllers and routes are defined.

dotnet add ExpenseManager.API package Serilog.AspNetCore --version 8.0.0
Reason: This package hooks Serilog into the ASP.NET Core request 
pipeline so HTTP requests are automatically logged. Since it depends 
on the ASP.NET Core pipeline, it must live in the API layer.

dotnet add ExpenseManager.API package Serilog.Sinks.Console --version 5.0.1
dotnet add ExpenseManager.API package Serilog.Sinks.File --version 5.0.0
Reason: Sinks define where logs are written (console, file). 
These are configured at the application entry point (Program.cs) 
which lives in the API layer. Logging infrastructure is bootstrapped 
at startup, not in business logic.

dotnet add ExpenseManager.API package Microsoft.EntityFrameworkCore.Design --version 8.0.0
Reason: This is a build-time tooling package only. The dotnet ef 
commands (add migration, update database) require this package in 
the startup project (API) to boot the application and discover the 
DbContext. It has no runtime use and is marked as PrivateAssets=all 
automatically by dotnet CLI.

## ExpenseManager.Infrastructure packages

dotnet add ExpenseManager.Infrastructure package BCrypt.Net-Next --version 4.0.3
Reason: BCrypt is an external encryption library that performs CPU-
intensive hashing. It is an implementation detail of how passwords 
are stored. The Application layer defines the IPasswordHasher 
interface — Infrastructure implements it using BCrypt. This way 
Application never depends on BCrypt directly and can be tested 
with a mock hasher.

dotnet add ExpenseManager.Infrastructure package CsvHelper --version 33.0.1
Reason: CsvHelper writes data to a file/stream format. This is an 
external IO concern — writing to a MemoryStream and producing a 
byte[]. The Application layer defines ICsvExportService interface — 
Infrastructure implements it using CsvHelper. Application stays 
clean with no file system dependencies.

dotnet add ExpenseManager.Infrastructure package System.IdentityModel.Tokens.Jwt --version 7.5.1
dotnet add ExpenseManager.Infrastructure package Microsoft.IdentityModel.Tokens --version 7.5.1
Reason: These packages create and sign JWT tokens. Token generation 
is an external security concern that depends on cryptographic 
libraries. Application defines IJwtTokenGenerator interface — 
Infrastructure implements it using these packages. This keeps 
Application free of any JWT library dependency.

Note: This is different from JwtBearer in the API layer.
- System.IdentityModel.Tokens.Jwt (Infrastructure) = GENERATES tokens on login
- JwtBearer (API) = VALIDATES tokens on incoming requests
Two different jobs, two different packages, two different layers.

dotnet add ExpenseManager.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add ExpenseManager.Infrastructure package Microsoft.EntityFrameworkCore --version 8.0.0
Reason: EF Core and the Npgsql provider are database access concerns. 
The DbContext, repositories, and migrations all live in Infrastructure 
because they deal with the physical database. Application layer never 
references EF Core directly — it only uses repository interfaces.

## ExpenseManager.Application packages

dotnet add ExpenseManager.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
Reason: AutoMapper maps DTOs to entities and vice versa. It is a 
pure in-memory transformation with no IO, no network, no file system 
access. It is acceptable in Application because it has no external 
dependencies that would break testability or layer isolation.
Note: AutoMapper.Extensions.Microsoft.DependencyInjection already 
includes AutoMapper core — no need to install AutoMapper separately.

dotnet add ExpenseManager.Application package FluentValidation.AspNetCore --version 11.3.0
Reason: FluentValidation validates DTOs using pure C# logic rules. 
It has no IO or network dependencies. Validators live in Application 
because validation is a business rule concern — it defines what 
valid input looks like before it reaches service logic.

## Golden Rules to Remember

1. Does the package talk to a database? → Infrastructure
2. Does the package talk to the file system? → Infrastructure  
3. Does the package do encryption or hashing? → Infrastructure
4. Does the package generate security tokens? → Infrastructure
5. Does the package validate HTTP requests/middleware? → API
6. Does the package configure the request pipeline? → API
7. Does the package do pure in-memory logic only? → Application

## What NEVER goes in Application layer
- BCrypt (external encryption)
- CsvHelper (file IO)
- JWT generation packages (external security)
- EF Core (database IO)
- Any package that touches IO, network, or file system

## What NEVER goes in Infrastructure layer
- JwtBearer (HTTP middleware, not token generation)
- Serilog.AspNetCore (HTTP pipeline logging)
- Swashbuckle (API documentation)

## What NEVER goes in API layer
- BCrypt (business/infrastructure concern)
- EF Core (data access concern)
- CsvHelper (data export concern)