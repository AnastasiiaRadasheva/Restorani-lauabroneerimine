# Restaurant Table Booking

A restaurant table reservation system (school final project for *Programming* and *Distributed Applications*).

Customers pick a restaurant, a date, a time and the number of guests, and the system offers a suitable free table
(or lets them pick one on the floor plan). A reservation can include a food pre-order and special requests.

## Tech stack

- ASP.NET Core Web API (.NET 10), EF Core, SQLite
- Layers: `Restoran.Api` → `Restoran.Application` (entities, services, DTOs) → `Restoran.Infrastructure` (EF Core, repositories)
- xUnit + Moq tests

## Database

Tables: `Users`, `Restaurants`, `Tables`, `Reservations`, `MenuItems`, `PreOrderItems`, `Notifications`.

The database is created automatically on first start (migrations + seed data).

## Run

```bash
cd src/Restoran.Api
dotnet run
```

Swagger: http://localhost:5000/swagger
