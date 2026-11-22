# Testkontod

Need kontod luuakse automaatselt esimesel käivitamisel (`DbSeeder`). Kui andmebaas on juba olemas, kustuta `restoran.db` ja käivita API uuesti.

## Personal

| Roll | E-post | Parool | Mida näeb |
|---|---|---|---|
| Admin | admin@restoran.ee | Admin123! | Töölaud (mis vajab tähelepanu, otsing), laudade ja menüü haldus, plaan, kalender, broneeringud, telefonibroneering, küsimused |
| Teenindaja | waiter@restoran.ee | Waiter123! | Plaan, kalender, broneeringud, telefonibroneering, küsimused |
| Teenus (Worker) | service@restoran.ee | Service123! | Ainult taustateenus (meeldetuletused, "ei tulnud", maksmata broneeringute tühistamine) |

## Kliendid (parool kõigil `Klient123!`)

| Nimi | E-post |
|---|---|
| Kati Klient | klient@restoran.ee |
| Jaan Tamm | jaan@example.com |
| Liis Kask | liis@example.com |
| Peeter Saar | peeter@example.com |
| Anna Mägi | anna@example.com |
| Mihkel Saar | mihkel@example.com |
| Sofia Lepp | sofia@example.com |
| Kristjan Kuusk | kristjan@example.com |
| Elena Petrova | elena@example.com |
| Marko Rebane | marko@example.com |
| Laura Tamm | laura@example.com |

Igal kliendil on erinevatel päevadel erinev arv broneeringuid (nädalavahetusel rohkem), mõned on tähtpäevaga (◆).

## Makse test

Kaart `4242 4242 4242 4242`, aegumine `12/28`, CVC `123` (demo, päris raha ei liigu).
