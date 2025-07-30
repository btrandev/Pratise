# Admin Service - ASP.NET Microservice

A comprehensive ASP.NET microservice solution for managing tenants and users with vertical slice architecture, featuring JWT authentication, ABAC authorization, Redis caching, and monitoring with Prometheus and Grafana.

## Common.Middleware Package

This project includes a reusable middleware package that extracts common middleware components from the main application. The package can be referenced by other projects for code reuse.

### Azure Integrations

The Common.Middleware package now includes several Azure-specific integrations for better cloud integration:

- **Azure Application Insights**: For monitoring and telemetry
- **Azure Key Vault**: For secure secret management
- **Azure Blob Storage**: For log storage
- **Azure App Service**: For optimal integration with Azure App Service

See [Azure Integration Documentation](src/Common.Middleware/Azure/README.md) for detailed usage instructions.

## 🏗️ Architecture

This solution follows **Vertical Slice Architecture** with the following layers:

- **Domain**: Core entities and repository interfaces
- **Application**: Business logic with MediatR handlers and CQRS pattern
- **Infrastructure**: Data access, authentication, and external services
- **API**: REST API controllers and cross-cutting concerns
- **SQL Migrations**: Flyway database migrations (separate deployment)

## 🛠️ Tech Stack

- **.NET 9.0** - Latest .NET framework
- **Entity Framework Core** - ORM for data access
- **PostgreSQL** - Primary database
- **Flyway** - Database migrations
- **Redis** - Caching layer
- **MediatR** - CQRS and mediator pattern
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **JWT Bearer** - Authentication and authorization
- **Serilog** - Structured logging
- **Prometheus** - Metrics collection
- **Grafana** - Metrics visualization
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Azure Integration** - Application Insights, Key Vault, Blob Storage, App Service

## 🚀 Features

### Core Features
- ✅ **Tenant Management**: CRUD operations with pagination
- ✅ **User Management**: CRUD operations with pagination
- ✅ **JWT Authentication**: Secure token-based authentication
- ✅ **ABAC Authorization**: Attribute-based access control
- ✅ **Token Refresh/Revoke**: JWT token management
- ✅ **Redis Caching**: Performance optimization
- ✅ **Structured Logging**: Comprehensive logging with Serilog
- ✅ **Health Checks**: Application and infrastructure monitoring
- ✅ **Metrics**: Prometheus integration for observability

### Cross-Cutting Concerns
- ✅ **Validation**: FluentValidation for input validation
- ✅ **Error Handling**: Consistent error responses
- ✅ **Logging**: Structured logging with correlation IDs
- ✅ **Caching**: Redis-based caching strategy
- ✅ **Monitoring**: Health checks and metrics
- ✅ **Security**: JWT authentication and authorization

## 📁 Project Structure

```
AdminService/
├── src/
│   ├── AdminService.API/              # REST API layer
│   ├── AdminService.Application/       # Business logic layer
│   ├── AdminService.Domain/           # Domain entities and interfaces
│   ├── AdminService.Infrastructure/   # Data access and external services
│   └── AdminService.Migrations/       # Database migrations
├── grafana/                           # Grafana configuration
├── Dockerfile                         # Main service container
├── Dockerfile.migrations              # Migrations container
├── docker-compose.yml                 # Multi-container setup
├── prometheus.yml                     # Prometheus configuration
└── README.md                          # This file
```

## 🚀 Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8.0 SDK (for local development)

### Using Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd AdminService
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **Access the services**
   - **API**: http://localhost:5000
   - **Swagger UI**: http://localhost:5000/swagger
   - **Grafana**: http://localhost:3000 (admin/admin)
   - **Prometheus**: http://localhost:9090

### Local Development

1. **Setup database**
   ```bash
   # Start PostgreSQL and Redis
   docker-compose up postgres redis -d
   
   # Run migrations
   dotnet ef database update --project src/AdminService.Migrations
   ```

2. **Run the API**
   ```bash
   cd src/AdminService.API
   dotnet run
   ```

## 📚 API Documentation

### Authentication

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "password123",
  "tenantId": "tenant-guid"
}
```

#### Response
```json
{
  "accessToken": "jwt-token",
  "refreshToken": "refresh-token",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "user-guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "admin@example.com",
    "username": "admin",
    "role": "Admin",
    "tenantId": "tenant-guid"
  }
}
```

### Tenants API

#### Get All Tenants (Paginated)
```http
GET /api/tenants?page=1&pageSize=10
Authorization: Bearer <jwt-token>
```

#### Get Tenant by ID
```http
GET /api/tenants/{id}
Authorization: Bearer <jwt-token>
```

#### Create Tenant
```http
POST /api/tenants
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "name": "Example Tenant",
  "code": "EXAMPLE",
  "description": "Example tenant description",
  "domain": "example.com",
  "isActive": true,
  "subscriptionPlan": "Premium"
}
```

#### Update Tenant
```http
PUT /api/tenants/{id}
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "name": "Updated Tenant Name",
  "description": "Updated description",
  "domain": "updated-example.com",
  "isActive": true,
  "subscriptionPlan": "Enterprise"
}
```

#### Delete Tenant
```http
DELETE /api/tenants/{id}
Authorization: Bearer <jwt-token>
```

### Users API

#### Get All Users (Paginated)
```http
GET /api/users?page=1&pageSize=10&tenantId={tenantId}
Authorization: Bearer <jwt-token>
```

#### Get User by ID
```http
GET /api/users/{id}
Authorization: Bearer <jwt-token>
```

#### Create User
```http
POST /api/users
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "username": "johndoe",
  "password": "securepassword123",
  "phoneNumber": "+1234567890",
  "role": "User",
  "tenantId": "tenant-guid"
}
```

#### Update User
```http
PUT /api/users/{id}
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890",
  "isActive": true,
  "role": "Manager"
}
```

#### Delete User
```http
DELETE /api/users/{id}
Authorization: Bearer <jwt-token>
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Database=adminservice;Username=postgres;Password=password` |
| `ConnectionStrings__Redis` | Redis connection string | `localhost:6379` |
| `Jwt__Secret` | JWT signing secret | `your-super-secret-key-with-at-least-32-characters` |
| `Jwt__Issuer` | JWT issuer | `AdminService` |
| `Jwt__Audience` | JWT audience | `AdminService` |
| `Jwt__AccessTokenExpirationMinutes` | Token expiration time | `60` |

### Database Configuration

The application uses PostgreSQL with the following default settings:
- **Host**: localhost
- **Port**: 5432
- **Database**: adminservice
- **Username**: postgres
- **Password**: password

## 📊 Monitoring

### Health Checks
- **Application Health**: `GET /health`
- **Database Health**: Included in health check
- **Redis Health**: Included in health check

### Metrics
- **Prometheus Metrics**: `GET /metrics`
- **HTTP Metrics**: Request duration, count, etc.
- **Custom Metrics**: Business-specific metrics

### Grafana Dashboards
Access Grafana at http://localhost:3000 with:
- **Username**: admin
- **Password**: admin

## 🔒 Security

### JWT Authentication
- Secure token-based authentication
- Configurable token expiration
- Token refresh mechanism
- Token revocation support

### ABAC Authorization
- Attribute-based access control
- User claims for fine-grained permissions
- Role-based access control (RBAC) support
- Tenant isolation

### Security Headers
- CORS configuration
- HTTPS redirection
- Security headers middleware

## 🧪 Testing

### API Testing
Use the Swagger UI at http://localhost:5000/swagger for interactive API testing.

### Load Testing
```bash
# Using Apache Bench
ab -n 1000 -c 10 http://localhost:5000/api/health

# Using Artillery
artillery quick --count 100 --num 10 http://localhost:5000/api/health
```

## 🚀 Deployment

### Production Deployment

1. **Update configuration**
   ```bash
   # Update appsettings.json with production values
   # Set secure JWT secret
   # Configure production database connection
   ```

2. **Build and deploy**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

3. **Run migrations**
   ```bash
   docker-compose run --rm migrations
   ```

### Kubernetes Deployment

1. **Create namespace**
   ```bash
   kubectl create namespace admin-service
   ```

2. **Apply configurations**
   ```bash
   kubectl apply -f k8s/
   ```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

## 🔄 Changelog

### v1.0.0
- Initial release
- Tenant and User management
- JWT authentication
- Redis caching
- Prometheus metrics
- Grafana dashboards
- Docker support 

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report with ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open the coverage report
start coveragereport/index.html