# Hypesoft Challenge X

A modern e-commerce API built with .NET 7, MongoDB, and JWT authentication.

## Features

- 🚀 **RESTful API** built with .NET 7
- 🏗 **MongoDB** for flexible, scalable data storage
- 🔐 **JWT Authentication** for secure API access
- 🧪 **Unit & Integration Tests** with xUnit
- 🔄 **CI/CD** with GitHub Actions
- 📚 **Swagger/OpenAPI** documentation

## Prerequisites

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (v6.0 or later)
- [Git](https://git-scm.com/)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/hypesoft-challengex.git
cd hypesoft-challengex
```

### 2. Configure Environment Variables

Create an `appsettings.Development.json` file in the `src/Hypesoft.API` directory with the following content:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "MongoDBSettings": {
    "DatabaseName": "HypesoftDB_Dev",
    "ConnectionString": "mongodb://localhost:27017"
  },
  "JwtSettings": {
    "Secret": "YOUR_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "Hypesoft.API",
    "Audience": "Hypesoft.Clients",
    "ExpirationInMinutes": 1440
  },
  "DetailedErrors": true,
  "UseInMemoryDatabase": true
}
```

### 3. Run the Application

```bash
cd backend/src/Hypesoft.API
dotnet run
```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

## API Documentation

Once the application is running, you can access the Swagger UI at:
- [https://localhost:5001/swagger](https://localhost:5001/swagger)
- [http://localhost:5000/swagger](http://localhost:5000/swagger)

## Running Tests

To run the unit and integration tests:

```bash
cd backend
dotnet test
```

## CI/CD

This project includes a GitHub Actions workflow that runs on every push to the `main` branch and on pull requests. The workflow:

1. Builds the solution
2. Runs all tests
3. Publishes test results

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `MONGODB_CONNECTION_STRING` | MongoDB connection string | `mongodb://localhost:27017` |
| `JWT_SECRET` | Secret key for JWT token generation | - |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Production` |

## Project Structure

```
backend/
├── src/
│   ├── Hypesoft.API/           # API project
│   ├── Hypesoft.Application/    # Application layer (use cases, DTOs, validators)
│   ├── Hypesoft.Domain/         # Domain models and interfaces
│   └── Hypesoft.Infrastructure/ # Infrastructure (data access, external services)
└── tests/
    ├── Hypesoft.UnitTests/      # Unit tests
    └── Hypesoft.IntegrationTests/ # Integration tests
```

## Contributing

1. Fork the repository
2. Create a new branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
