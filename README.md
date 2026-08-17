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

- SharedKernel: DomainException,IAggregateRoot,DomainEventBase
- Domain: BaseSpecification,ISpecification,IRepository
- Application: IDomainEventsDispatcher, DomainEventsDispatcher




