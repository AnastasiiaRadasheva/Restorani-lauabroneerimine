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

## Test users

| Role | Email | Password |
|---|---|---|
| Admin | admin@restoran.ee | Admin123! |
| Waiter | waiter@restoran.ee | Waiter123! |
| Customer | klient@restoran.ee | Klient123! |
| Service (worker) | service@restoran.ee | Service123! |

Login with `POST /api/auth/login`, then press **Authorize** in Swagger and paste the token.
Creating, updating and deleting restaurants, tables and menu items requires the `Admin` role.

## API endpoints

| Method | URL | Description |
|---|---|---|
| GET | `/api/restaurants?lat=&lng=` | Restaurants, nearest first (with distance) |
| GET/POST/PUT/DELETE | `/api/restaurants/{id}` | Restaurant CRUD |
| GET | `/api/restaurants/{id}/tables` | Tables of a restaurant |
| GET/POST/PUT/DELETE | `/api/tables/{id}` | Table CRUD (seats, hall or terrace, position on the floor plan) |
| GET | `/api/menu?restaurantId=&category=&search=&sort=&page=` | Menu with filter, search, sorting, paging |
| GET/POST/PUT/DELETE | `/api/menu/{id}` | Menu CRUD |

| POST | `/api/auth/register` | Register a customer |
| POST | `/api/auth/login` | Login, returns a JWT token |
| GET | `/api/auth/me` | Current user |

Errors are returned as `ProblemDetails` (400, 404, 409, 500).
