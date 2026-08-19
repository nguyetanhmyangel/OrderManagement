# OrderManagement

```bash
mkdir src
vi .gitignore
vi .gitattributes
vi .editorconfig

dotnet new sln -n OrderManagement

dotnet new packagesprops

dotnet new classlib -n OrderManagement.SharedKernel -o src/OrderManagement.SharedKernel
dotnet new classlib -n OrderManagement.Domain -o src/OrderManagement.Domain
dotnet new classlib -n OrderManagement.Application -o src/OrderManagement.Application
dotnet new classlib -n OrderManagement.Infrastructure -o src/OrderManagement.Infrastructure
dotnet new web -n OrderManagement.Api -o src/OrderManagement.Api

dotnet sln add src/OrderManagement.SharedKernel/OrderManagement.SharedKernel.csproj
dotnet sln add src/OrderManagement.Domain/OrderManagement.Domain.csproj
dotnet sln add src/OrderManagement.Application/OrderManagement.Application.csproj
dotnet sln add src/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj
dotnet sln add src/OrderManagement.Api/OrderManagement.Api.csproj

```

# Use Milan Jovanovic's clean architecture template and add...

- SharedKernel: 
  - Interface/class: IEntity, Entity, DomainException, IAggregateRoot, IDomainEvent , DomainEvent, DomainException, 
    Error, ErrorType, IDateTimeProvider, IDomainEvent, IDomainEventHandler, Result, ValidationError.
    ValueObjects: Email, Money, Address, PhoneNumber, ShippingZone, Slug, ValueObject
  
- Domain: 
  - For aggregates where write operations require optimistic concurrency, child entities typically do not need it; concurrency should be managed at the Aggregate Root level:
    - postgre sql:
      - add row this in aggregate: 
      ```c#
      public uint Version { get; private set; } 
      ```
      - fluent api:
      ```c#
      protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<YourEntity>(entity =>
            {
                entity.Property(e => e.Version)
                      .IsRowVersion();        
            });
        }
      ```
    - sql server:  
    ```c#
    public byte[] RowVersion { get; private set; } = [];
    ```
      
  - Interface/class: BaseSpecification, ISpecification, IRepository, {Entity}Repository, {Entity}Error, {Entity}Events
  
- Application: 
  - dll: 
    - dotnet add package FluentValidation.DependencyInjectionExtensions
    - dotnet add package Microsoft.Extensions.Logging.Abstractions
    - dotnet add package Microsoft.Extensions.DependencyInjection
    - dotnet add package Microsoft.EntityFrameworkCore
    - Scrutor
    
    
  - Interface/class: IDomainEventsDispatcher, DomainEventsDispatcher, ISqlConnectionFactory, IUnitOfWork, ICustomMediator, CustomMediator,
    IQuery, IQueryHandler, ICommandHandler<in TCommand, TResponse>, ICommandHandler<in TCommand>, ICommand, ICommand<TResponse>, ICommandHandler<in TCommand>,
    ICustomMediator, CustomMediator, IApplicationDbContext, ICacheService, ICacheableQuery, CacheQueryDecorator
  
  
- Infrastructure:

  - dll: 
      - Npgsql.EntityFrameworkCore.PostgreSQL   
      - Microsoft.Extensions.Hosting.Abstractions
      - dotnet add package Microsoft.Extensions.Caching.Hybrid
      - dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
      - Npgsql.EntityFrameworkCore.PostgreSQL
      - Dapper
      - Microsoft.Extensions.Hosting.Abstractions
      - Microsoft.Extensions.Options.ConfigurationExtension
  
  - Interface/class: Schemas, ApplicationDbContext, EfUnitOfWork, DomainEventInterceptor, {Entity}Configuration, IEventTypeRegistry, EventTypeRegistry, HybridCacheService, 
    SpecificationEvaluator, OutboxOptions, OutboxStatus, OutboxMessageDto, OutboxProcessor, OutboxProcessor, IdempotentDomainEventHandler
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
- follow:
1. product.UpdatePrice(...) -> RaiseDomainEvent(...)          // only add in memory collection

2. repository.Update(product)
3. unitOfWork.SaveChangesAsync()
      ¦
      +-- DomainEventInterceptor.SavingChangesAsync()
      ¦     +-- Extract Domain Events ? Insert into OutboxMessages
      ¦
      +-- EF Core write into DB (Products + OutboxMessages)
      ¦     +-- same 1 transaction
      ¦
      +-- DomainEventInterceptor.SavedChangesAsync()
            +-- ClearDomainEvents()

4. OutboxProcessor (BackgroundService)
   ? Claim message ? Dispatch Handler (eventual consistency)
   
////
1. Decorator Pattern cho Query Handler d? tách hoàn toàn logic cache kh?i business logic:
API
 ¦
 ?
GetxxxxQuery
 ¦
 ?
CacheQueryDecorator
 ¦
 +-- Cache HIT ------? return Response
 ¦
 +-- Cache MISS
          ¦
          ?
   GetxxxxQueryHandler
          ¦
          ?
   I{Entity}ReadService
          ¦
          ?
       Database
          ¦
          ?
     save int Cache
          ¦
          ?
      return Result


- Sql Script: 






