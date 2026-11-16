# Restaurant Table Booking

A restaurant table reservation system (school final project for *Programming* and *Distributed Applications*).

Customers pick a restaurant, a date, a time and the number of guests, and the system offers a suitable free table
(or lets them pick one on the floor plan). A reservation can include a food pre-order and special requests.

## Tech stack

- ASP.NET Core Web API (.NET 10), EF Core, SQLite
- Layers: `Restoran.Api` → `Restoran.Application` (entities, services, DTOs) → `Restoran.Infrastructure` (EF Core, repositories)
- xUnit + Moq tests

## Tests

```bash
dotnet test
```

21 unit tests (xUnit). `ReservationServiceTests` uses **Moq** mocks instead of the database.
Tested: overlap rule, smallest free table, 409 on double booking, opening hours, past dates, 24 h rule, access rights, pre-order validation.

## Logging

`ILogger` logs every request (method, path, status, duration), new / cancelled / paid reservations and all errors.

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

## Booking rules

- A reservation lasts **2 hours** and must fit in the restaurant opening hours.
- Two reservations overlap when `newStart < existingEnd && newEnd > existingStart`.
  `Cancelled` and `NoShow` reservations do not occupy a table.
- Automatic table choice: the **smallest free table** with `Seats >= guests`.
- Customers can change or cancel a reservation only **more than 24 hours** before it starts.

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

| GET | `/api/tables/available?restaurantId=&date=&time=&guests=` | Suggested free table + availability of all tables |
| GET | `/api/tables/plan?restaurantId=&date=` | Floor plan of the day (staff) |
| GET | `/api/reservations` | All reservations with filter/search/sort/paging (staff) |
| GET | `/api/reservations/my` | My reservations |
| POST | `/api/reservations` | Create reservation (`409 Conflict` if the table is taken) |
| PUT | `/api/reservations/{id}` | Change reservation (customer: only more than 24 h before) |
| PATCH | `/api/reservations/{id}/cancel` | Cancel (customer: only more than 24 h before) |
| POST | `/api/reservations/{id}/preorder` | Food pre-order |
| POST | `/api/reservations/{id}/pay` | Fake payment (test card `4242 4242 4242 4242`) |
| PATCH | `/api/reservations/{id}/status` | Change status, e.g. `Arrived` (staff) |
| POST | `/api/auth/register` | Register a customer |
| POST | `/api/auth/login` | Login, returns a JWT token |
| GET | `/api/auth/me` | Current user |

Errors are returned as `ProblemDetails` (400, 404, 409, 500).
