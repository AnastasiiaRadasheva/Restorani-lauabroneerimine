# Argent – Restaurant Table Booking

A restaurant table reservation system (school final project for *Programming* and *Distributed Applications*).
The restaurant is called **Argent** ("silver" in heraldry): three fine-dining restaurants in Tallinn.

Customers pick a restaurant, a date, a time and the number of guests, and the system offers a suitable free table
(or lets them pick one on the floor plan). A reservation can include a food pre-order, an occasion with a free
compliment from the chef, extra services (flowers, champagne, violinist …) and special requests.
Customers can also sign up for **gastronomic evenings** (special dinners with a dress code) and send messages to the restaurant.

## Tech stack

- ASP.NET Core Web API (.NET 10), EF Core, SQLite
- React 19 + Vite + React Router (website, works only through the API)
- Worker Service (`Restoran.Worker`) – separate process, talks to the API over HTTP
- Layers: `Restoran.Api` → `Restoran.Application` (entities, services, DTOs) → `Restoran.Infrastructure` (EF Core, repositories)
- xUnit + Moq tests

## Tests

```bash
dotnet test
```

23 unit tests (xUnit). `ReservationServiceTests` uses **Moq** mocks instead of the database.
Tested: overlap rule, smallest free table, 409 on double booking, opening hours, past dates, 24 h rule, access rights, pre-order validation.

## Logging

`ILogger` logs every request (method, path, status, duration), new / cancelled / paid reservations and all errors.

## Database

Tables: `Users`, `Restaurants`, `Tables`, `Reservations`, `MenuItems`, `PreOrderItems`, `Notifications`.

The database is created automatically on first start (migrations + seed data).

## Run

1. API (creates the database on first start):

```bash
cd src/Restoran.Api
dotnet run
```

Swagger: http://localhost:5000/swagger

2. Website (React + Vite), in a second terminal:

```bash
cd web
npm install
npm run dev
```

Website: http://localhost:5173 (the API allows this address with CORS).

3. Background worker (separate process), in a third terminal:

```bash
cd src/Restoran.Worker
dotnet run
```

API requests for testing: `src/Restoran.Api/Restoran.Api.http` (Visual Studio / VS Code REST Client / Rider).

## Website pages

| Page | Who | What |
|---|---|---|
| `/` | everyone | Home page, gastronomic evenings (sign up, dress code consent) |
| `/broneeri` | everyone (login to confirm) | I. date, time, guests, map with distance · II. floor plan (maître d’ or manual choice, hall / terrace) · III. pre-order · IV. occasion, chef’s compliment, extras, fake payment → receipt |
| `/broneering/{id}` | owner / staff | Details, change date / time / guests, cancel |
| `/minu` | customer | My visits: details, change / cancel, messages to the restaurant, add extras |
| `/menuu` | everyone | Menu by category |
| `/personal/plaan` | Admin, Waiter | Floor plan of the day + timeline, mark arrived / no-show |
| `/personal/broneeringud` | Admin, Waiter | All reservations: search, filter, sort, paging |
| `/admin` | Admin | Tables (with floor plan editor), menu, statistics |

## Background worker (`Restoran.Worker`)

A separate Worker Service process (`BackgroundService` + `PeriodicTimer`, every **15 minutes**).
It logs in to the API with the `Service` role account and uses only `HttpClient` –
it has **no reference** to the database, `DbContext` or other projects. All business logic stays in the API.

Each run:

1. `GET /api/reservations/remind-due` – tomorrow's reservations without a reminder;
2. for each: `POST /api/notifications` and `PATCH /api/reservations/{id}/reminded`;
3. `POST /api/reservations/mark-no-show` – frees tables where the guest did not arrive within 30 minutes.

Customers see the reminders under the bell icon on the website.

## Real-time updates (WebSocket)

- Endpoint in the API project: `ws://localhost:5000/ws/tables?access_token=<JWT>` (the token is required).
- Messages are sent from the **service layer after saving** to the database:
  - `ReservationCreated` – only to staff (Admin, Waiter), includes the customer name;
  - `TableStatusChanged` – to everyone (new, changed, cancelled, arrived, no-show);
  - `TableSelecting` – someone is choosing this table right now (orange on the floor plan).
- The website uses the browser `WebSocket` API (`web/src/realtime.js`) and reconnects automatically.
- Demo: open the staff floor plan in one browser and book a table in another – the plan updates instantly.

## Mobile view

The **same website** is responsive and looks like a mobile app on a phone (no separate client):
language buttons on top, hamburger menu and a user icon, the crest picture above the title,
swipeable date strip, floor plan that scrolls with a finger, rounded visit cards and tables that turn into cards.
Test it in the browser DevTools (device toolbar, e.g. 375 px wide).

## Languages

The website is available in **Estonian, Russian and English** (ET / RU / EN buttons in the header).
All texts are in `web/src/locales/*.js`; plural forms use `Intl.PluralRules`. The staff pages are in Estonian.

## Project structure

```
Restoran.sln
src/
  Restoran.Api/             controllers, JWT, Swagger, ProblemDetails, WebSocket /ws/tables
  Restoran.Application/     entities, DTOs, interfaces, services (business logic)
  Restoran.Infrastructure/  EF Core DbContext, migrations, seed data, repositories
  Restoran.Worker/          background worker (separate process, HTTP only)
tests/
  Restoran.Tests/           xUnit + Moq unit tests
web/                        React website (Vite)
```

## Booking rules

- A reservation lasts **2 hours** and must fit in the restaurant opening hours.
- Two reservations overlap when `newStart < existingEnd && newEnd > existingStart`.
  `Cancelled` and `NoShow` reservations do not occupy a table.
- Automatic table choice: the **smallest free table** with `Seats >= guests`.
- Customers can change or cancel a reservation only **more than 24 hours** before it starts.
  Messages and extra services can be added at any time.
- Choosing the table yourself costs **10 €**, in peak hours **18:00–20:00** it costs **30 €**. The maître d’s choice is free.
- A chef’s compliment (mini cake, champagne or dessert) is free and only available with an occasion.
- Gastronomic evenings with a strict dress code require accepting the rules when signing up; seats are limited.

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
| GET | `/api/tables/slots?restaurantId=&from=&days=&guests=` | Free tables per time slot (crossed-out days and times) |
| GET | `/api/reservations/extras` | Extra services catalogue with prices |
| GET/POST | `/api/reservations/{id}/messages` | Messages between the customer and the restaurant |
| PUT | `/api/reservations/{id}/extras` | Change extra services of a reservation |
| GET | `/api/events` | Upcoming gastronomic evenings with free seats |
| POST/DELETE | `/api/events/{id}/join` | Sign up / cancel (dress code rules must be accepted) |
| GET | `/api/stats?restaurantId=&from=&to=` | Busiest weekdays, hours and dates (Admin) |
| GET | `/api/reservations/remind-due` | Tomorrow's reservations without reminder (Service) |
| PATCH | `/api/reservations/{id}/reminded` | Mark reminder sent (Service) |
| POST | `/api/reservations/mark-no-show` | Free tables of guests 30 min late (Service) |
| POST | `/api/notifications` | Send a notification (Service, Admin) |
| GET | `/api/notifications/my` | My notifications |
| POST | `/api/auth/register` | Register a customer |
| POST | `/api/auth/login` | Login, returns a JWT token |
| GET | `/api/auth/me` | Current user |

Errors are returned as `ProblemDetails` (400, 404, 409, 500).

## Photos

Dish photos in `web/public/img/menu` are from [Unsplash](https://unsplash.com/license) (free to use). The list with links is in `web/public/img/menu/CREDITS.md`.

## SMS and e-mail confirmations

After a booking the customer gets a notification on the website, an SMS (if they have a phone number and "receive SMS" is on in the profile) and an e-mail with the receipt.

- **Demo mode (default):** nothing is sent. The SMS text and the e-mail (HTML receipt) are saved to `src/Restoran.Api/outbox` – open the `.html` file in a browser.
- **Real SMS:** create a Twilio account (free trial), verify your phone number, then fill `Sms:AccountSid`, `Sms:AuthToken` and `Sms:From` in `appsettings.json`. Estonian numbers without a country code (e.g. `58951836`) get `+372`.
- **Real e-mail:** fill `Email:Host`, `Port`, `User`, `Password`, `From` (for Gmail: `smtp.gmail.com`, port 587 and an app password).
