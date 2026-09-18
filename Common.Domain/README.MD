Common.Domain
Overview
Common.Domain is a robust foundational library built for .NET applications following Domain-Driven Design (DDD) principles. It provides a shared set of reusable domain primitives, base classes, strongly-typed identifiers, structured error handling, and fluent validation guards.

Project Structure
The project is organized into the following key directories:

Errors/: Contains domain error definitions (IDomainError, DomainError) and categorization smart enums (ErrorType).

Events/: Houses domain event contracts, base records, handlers, and metadata attributes (IDomainEvent, DomainEvent, IDomainEventHandler).

Exceptions/: Custom domain exceptions for handling validation failures and eventual consistency anomalies.

Services/: Time abstraction utilities (IDateTimeProvider) for testable time management.

Root Files: Core domain building blocks including entities, aggregate roots, strongly-typed IDs, and value objects.

Core Components
Entities & Aggregates: Base classes (Entity<TModel>, AggregateRoot<TModel>) featuring automatic audit tracking (IAuditableEntity) and domain event management (IAggregateRoot).

Strongly-Typed IDs: Type-safe record wrappers (Id<TModel>, IId) that eliminate primitive obsession and prevent accidental ID mix-ups.

Value Objects: Immutable objects like DateRange defined entirely by their structural values rather than unique database identifiers.

Defensive Validators: A rich extension method library (Validators) providing fail-fast guards with automatic argument expression naming.

Error & Event Handling
Domain Errors: Structured failure models categorized via smart enums (ErrorType) for consistent API and business rule feedback.

Domain Events: Decoupled messaging infrastructure integrated with MediatR (INotificationHandler) to propagate state changes asynchronously across the system.