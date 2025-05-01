## Project Structure & Architecture

**Layered Architecture**
- **API Layer**: Controllers, DTOs, middleware.
- **Application/Core Layer**: Business logic, services, interfaces.
- **Infrastructure Layer**: EF Core, repositories, external services.
- **Domain Layer**: Entities, enums, exceptions.

**Clean Architecture**: Ensure dependencies point inward (Domain → Application → Infrastructure → API) for scalability and maintainability.

---

## Database Access

- **Entity Framework Core**: Use code-first migrations for schema management.
- **DbContext Configuration**: Register with dependency injection (DI).
- **Loading Strategies**: Enable lazy loading or eager loading judiciously to balance performance and predictability.
- **Async/Await**: Use asynchronous methods (`ToListAsync()`, `SaveChangesAsync()`) to avoid thread blocking.
- **Connection Pooling**: Configure `Max Pool Size` in the connection string to ensure scalability.

---

## Security

- **HTTPS Enforcement**: Use `RequireHttps` attribute or middleware.
- **Authentication & Authorization**:
  - Implement JWT Bearer tokens for stateless authentication.
  - Define and apply authorization policies (`[Authorize(Policy = "PolicyName")]`).
- **CORS**: Restrict allowed origins in production with `AllowSpecificOrigins` policy.
- **SQL Injection Prevention**:
  - Prefer EF Core parameterization over raw SQL.
  - If raw SQL is required, use `FromSqlRaw` with caution.
- **Secrets Management**: Store secrets in Azure Key Vault or environment variables; never check into source control.

---

## Validation

- **FluentValidation**: Use for complex validation rules instead of DataAnnotations.
- **Automatic Model Validation**: Apply `[ApiController]` to auto-validate and return `400 Bad Request` for invalid models.

---

## Error Handling

- **Global Exception Middleware**: Use `UseExceptionHandler` to catch exceptions and return standardized `ProblemDetails`.
- **Custom Exceptions**: Map domain exceptions to HTTP status codes (e.g., `NotFoundException` → `404 Not Found`).
- **Structured Logging**: Integrate Serilog with contextual information (`ILogger<T>`).

---

## Performance

- **Caching**: Use `IMemoryCache` or `IDistributedCache` for frequently accessed data.
- **Pagination**: Implement skip/take or keyset pagination for large datasets.
- **DTO Projections**: Use AutoMapper to map entities to DTOs, minimizing payload size.
- **Response Compression**: Enable compression middleware to reduce payload size.

---

## Testing

- **Unit Tests**: Write tests for services and controllers using mocking frameworks (Moq, NSubstitute).
- **Integration Tests**: Use `WebApplicationFactory<TEntryPoint>` to test API endpoints against a test database.
- **End-to-End (E2E) Tests**: Validate workflows with tools like Postman or Newman.

---

## API Design

- **RESTful Conventions**: Use nouns for endpoints (`/users`), HTTP verbs for actions (`GET`, `POST`, `PUT`, `DELETE`), and appropriate status codes.
- **Versioning**: Implement via URL segments (`/api/v1/users`) or headers using `Microsoft.AspNetCore.Mvc.Versioning`.
- **OpenAPI/Swagger**: Document endpoints with XML comments and Swashbuckle (Swashbuckle.AspNetCore).

---

## CI/CD & Configuration

- **Configuration Management**: Use `appsettings.{Environment}.json` and `IConfiguration` for environment-specific settings.
- **CI/CD Pipelines**: Automate build and deployment with GitHub Actions or Azure DevOps.
- **Health Checks**: Add health endpoints (`/health`) and configure database connectivity checks with `AddHealthChecks()`.

---

## Code Quality

- **Static Analysis**: Enforce coding standards with `.editorconfig` and Roslyn analyzers.
- **Code Reviews**: Use pull requests to ensure adherence to team standards.
- **SOLID Principles**: Apply dependency injection, single responsibility, and decoupling.

---

## Monitoring & Logging

- **Structured Logging**: Configure Serilog sinks (Elasticsearch, Application Insights).
- **Metrics & Telemetry**: Track performance using `System.Diagnostics.Metrics` or OpenTelemetry.

---

## Database Best Practices

- **Indexing**: Create indexes on filtered/sorted columns to optimize query performance.
- **Migration Management**: Review and script migrations before applying to production.
- **Backups & Recovery**: Schedule regular backups and periodically test restore procedures.

