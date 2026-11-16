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

API requests for testing: `src/Restoran.Api/Restoran.Api.http` (Visual Studio / VS Code REST Client / Rider).

## API endpoints

| Method | URL | Description |
|---|---|---|
| GET | `/api/restaurants?lat=&lng=` | Restaurants, nearest first (with distance) |
| GET/POST/PUT/DELETE | `/api/restaurants/{id}` | Restaurant CRUD |
| GET | `/api/restaurants/{id}/tables` | Tables of a restaurant |
| GET/POST/PUT/DELETE | `/api/tables/{id}` | Table CRUD (seats, hall or terrace, position on the floor plan) |
| GET | `/api/menu?restaurantId=&category=&search=&sort=&page=` | Menu with filter, search, sorting, paging |
| GET/POST/PUT/DELETE | `/api/menu/{id}` | Menu CRUD |

Errors are returned as `ProblemDetails` (400, 404, 409, 500).
