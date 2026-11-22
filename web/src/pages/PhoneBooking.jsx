import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, qs } from '../api'
import { useTr } from '../staff'
import { money, toIsoDate, ZONE_KEY } from '../utils'
import { useMenu } from '../components/booking/GastronomySection'
import { useExtrasCatalog } from '../components/booking/EveningDetails'
import { StaffSignup } from './AdminEvents'

/**
 * Personal: broneering telefoni või vestluse põhjal.
 * 1) küsi kliendilt andmed, 2) vali olemasolev klient või lisa uus, 3) broneeri.
 * Broneering ootab makset – klient saab makselingi (SMS + e-kiri) ja pärast makset kinnitub see ise.
 */
export default function PhoneBooking() {
  const tr = useTr()
  const [search, setSearch] = useState('')
  const [found, setFound] = useState([])
  const [customer, setCustomer] = useState(null)
  const [isNew, setIsNew] = useState(false)
  const [newCustomer, setNewCustomer] = useState({ name: '', email: '', phone: '', language: tr.lang })
  const [restaurants, setRestaurants] = useState([])
  const [b, setB] = useState({ restaurantId: '', date: toIsoDate(new Date()), time: '', guests: 2, tableId: '', specialRequest: '' })
  const [slots, setSlots] = useState([])
  const [tables, setTables] = useState([])
  const [result, setResult] = useState(null)
  const [error, setError] = useState('')
  // Mida kliendile broneeritakse: tavaline laud või gastronoomiline õhtu
  const [mode, setMode] = useState('table')
  // Eeltellimus (menüüst) ja lisateenused – nagu kliendi enda broneerimisel
  const [qty, setQty] = useState({})
  const [extras, setExtras] = useState([])
  const menu = useMenu(b.restaurantId)
  const catalog = useExtrasCatalog()

  useEffect(() => {
    api('/api/restaurants').then((l) => {
      setRestaurants(l)
      setB((x) => ({ ...x, restaurantId: l[0]?.id ?? '' }))
    })
  }, [])

  useEffect(() => {
    const timer = setTimeout(() => api('/api/customers' + qs({ search })).then(setFound).catch(() => {}), 250)
    return () => clearTimeout(timer)
  }, [search])

  useEffect(() => {
    if (!b.restaurantId) return
    api('/api/tables/slots' + qs({ restaurantId: b.restaurantId, from: b.date, days: 1, guests: b.guests }))
      .then((d) => setSlots(d[0]?.slots ?? []))
      .catch(() => setSlots([]))
  }, [b.restaurantId, b.date, b.guests])

  useEffect(() => {
    if (!b.restaurantId || !b.time) return setTables([])
    api('/api/tables/available' + qs({ restaurantId: b.restaurantId, date: b.date, time: b.time, guests: b.guests }))
      .then((a) => setTables(a.tables.filter((t) => t.isFree && t.fitsGuests)))
      .catch(() => setTables([]))
  }, [b.restaurantId, b.date, b.time, b.guests])

  const set = (f) => (e) => setB({ ...b, [f]: e.target.value })

  async function save(e) {
    e.preventDefault()
    setError('')
    if (!customer && !isNew) return setError(tr('Valige klient või lisage uus.', 'Выберите гостя или добавьте нового.', 'Choose a guest or add a new one.'))
    if (!b.time) return setError(tr('Valige kellaaeg.', 'Выберите время.', 'Choose a time.'))
    try {
      const r = await api('/api/reservations/for-customer', {
        method: 'POST',
        body: {
          restaurantId: Number(b.restaurantId),
          date: b.date,
          time: b.time,
          guests: Number(b.guests),
          tableId: b.tableId ? Number(b.tableId) : null,
          chosenByGuest: !!b.tableId, // laud valiti kliendi soovil -> laua valiku tasu
          specialRequest: b.specialRequest || null,
          items: Object.entries(qty).filter(([, q]) => q > 0).map(([menuItemId, quantity]) => ({ menuItemId: Number(menuItemId), quantity })),
          extras,
          customerId: isNew ? null : customer.id,
          newCustomer: isNew ? { ...newCustomer, phone: newCustomer.phone || null } : null,
        },
      })
      setResult(r)
    } catch (err) {
      setError(err.message)
    }
  }

  if (result) {
    const link = `${window.location.origin}/maksa/${result.paymentToken}`
    const pending = result.status === 'PendingPayment'
    return (
      <div className="staff container">
        <div className="paper white phone-done">
          <p className="eyebrow ox">{pending ? tr('Ootab makset', 'Ожидает оплаты', 'Awaiting payment') : tr('Kinnitatud', 'Подтверждена', 'Confirmed')}</p>
          <p className="card-title">{result.customerName} · {new Date(result.startTime).toLocaleString(tr.lang, { weekday: 'short', day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit' })}</p>
          <p>{result.restaurantName} · {tr('laud', 'стол', 'table')} {result.tableNumber} ({tr.t(ZONE_KEY[result.tableLocation])}) · {result.guests}</p>
          {!pending && <p className="small">{tr('Maksta polnud midagi – broneering kinnitati kohe ja kliendile saadeti kinnitus (SMS ja e-kiri).', 'Оплачивать нечего — бронь сразу подтверждена, гостю отправлено подтверждение (SMS и письмо).', 'Nothing to pay – the booking is confirmed and the guest got a confirmation (SMS and e-mail).')}</p>}
          {pending && <p className="small">{tr(
            'Kliendile saadeti SMS ja e-kiri makselingiga. Pärast makset kinnitub broneering automaatselt ja klient saab kinnituse. Maksmata broneering tühistatakse 24 tunni pärast.',
            'Гостю отправлены SMS и письмо со ссылкой на оплату. После оплаты бронь подтвердится автоматически, и гость получит подтверждение. Неоплаченная бронь отменяется через 24 часа.',
            'An SMS and e-mail with a payment link were sent. After payment the booking is confirmed automatically. Unpaid bookings are cancelled after 24 hours.')}</p>}
          {pending && <p className="small">{tr('Makselink', 'Ссылка на оплату', 'Payment link')}: <a href={link}>{link}</a></p>}
          <div className="actions">
            <button className="btn btn-dark" onClick={() => { setResult(null); setCustomer(null); setIsNew(false); setQty({}); setExtras([]) }}>{tr('Uus broneering', 'Новая бронь', 'New booking')}</button>
            <Link className="btn" to={`/broneering/${result.id}`}>{tr('Ava broneering', 'Открыть бронь', 'Open booking')}</Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{tr('Broneering telefoni teel', 'Бронь по звонку', 'Phone booking')}</h1>
        </div>
        <p>{tr('Klient helistas või kirjutas? Täitke andmed tema eest.', 'Гость позвонил или написал? Оформите бронь за него.', 'A guest called or wrote? Book for them.')}</p>
      </div>
      <div className="chips phone-mode">
        <button type="button" className={mode === 'table' ? 'chip caps on' : 'chip caps'} onClick={() => setMode('table')}>{tr('Laud', 'Столик', 'Table')}</button>
        <button type="button" className={mode === 'event' ? 'chip caps on' : 'chip caps'} onClick={() => setMode('event')}>♪ {tr('Gastronoomiline õhtu', 'Гастрономический вечер', 'Gastronomic evening')}</button>
      </div>
      {mode === 'event' && <EventMode />}
      {mode === 'table' && error && <p className="alert alert-error">{error}</p>}

      {mode === 'table' && (
      <form className="phone-form" onSubmit={save}>
      <div className="phone-grid">
        <div className="paper white phone-col">
          <p className="script">
            {tr(
              '„Tere! Palun öelge oma ees- ja perekonnanimi, telefoninumber ja e-post – saadame Teile broneeringu kinnituse ja makselingi.“',
              '«Здравствуйте! Назовите, пожалуйста, имя и фамилию, номер телефона и e-mail — мы отправим Вам подтверждение и ссылку на оплату.»',
              '“Hello! Please tell me your full name, phone number and e-mail – we will send you the confirmation and a payment link.”')}
          </p>
          <div className="chips">
            <button type="button" className={!isNew ? 'chip on' : 'chip'} onClick={() => setIsNew(false)}>{tr('Olemasolev klient', 'Существующий гость', 'Existing guest')}</button>
            <button type="button" className={isNew ? 'chip on' : 'chip'} onClick={() => setIsNew(true)}>{tr('Uus klient', 'Новый гость', 'New guest')}</button>
          </div>

          {!isNew && (
            <>
              <label className="field">
                {tr('Otsi nime, e-posti või telefoni järgi', 'Поиск по имени, e-mail или телефону', 'Search by name, e-mail or phone')}
                <input value={search} onChange={(e) => setSearch(e.target.value)} />
              </label>
              <div className="cust-list">
                {found.map((c) => (
                  <button key={c.id} type="button" className={customer?.id === c.id ? 'thread on' : 'thread'} onClick={() => setCustomer(c)}>
                    <strong>{c.name}</strong>
                    <span className="small muted">{c.email}{c.phone ? ` · ${c.phone}` : ''}</span>
                  </button>
                ))}
              </div>
            </>
          )}
          {isNew && (
            <>
              <label className="field">{tr('Ees- ja perekonnanimi', 'Имя и фамилия', 'Full name')}<input value={newCustomer.name} onChange={(e) => setNewCustomer({ ...newCustomer, name: e.target.value })} /></label>
              <label className="field">E-mail<input type="email" value={newCustomer.email} onChange={(e) => setNewCustomer({ ...newCustomer, email: e.target.value })} /></label>
              <label className="field">{tr('Telefon', 'Телефон', 'Phone')}<input value={newCustomer.phone} onChange={(e) => setNewCustomer({ ...newCustomer, phone: e.target.value })} placeholder="+372 …" /></label>
              <label className="field">{tr('Kliendi keel (SMS ja e-kiri)', 'Язык гостя (SMS и письма)', 'Guest language (SMS and e-mail)')}
                <select value={newCustomer.language} onChange={(e) => setNewCustomer({ ...newCustomer, language: e.target.value })}>
                  <option value="et">Eesti</option><option value="ru">Русский</option><option value="en">English</option>
                </select>
              </label>
            </>
          )}
        </div>

        <div className="paper white phone-col">
          <label className="field">
            {tr('Restoran', 'Ресторан', 'Restaurant')}
            <select value={b.restaurantId} onChange={set('restaurantId')}>
              {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
            </select>
          </label>
          <div className="field-row">
            <label className="field">{tr('Kuupäev', 'Дата', 'Date')}<input type="date" value={b.date} onChange={set('date')} /></label>
            <label className="field">{tr('Külalisi', 'Гостей', 'Guests')}<input type="number" min={1} max={12} value={b.guests} onChange={set('guests')} /></label>
          </div>
          <p className="eyebrow">{tr('Saabumise aeg', 'Время прихода', 'Arrival time')}</p>
          <div className="chips">
            {slots.map((s) => (
              <button key={s.time} type="button" disabled={s.freeTables === 0} className={b.time === s.time ? 'chip small on' : 'chip small'} onClick={() => setB({ ...b, time: s.time, tableId: '' })}>{s.time}</button>
            ))}
          </div>
          <label className="field">
            {tr('Laud (tühi = metrdotel valib)', 'Стол (пусто = выберет метрдотель)', 'Table (empty = maître d’ chooses)')}
            <select value={b.tableId} onChange={set('tableId')}>
              <option value="">—</option>
              {tables.map((t) => <option key={t.id} value={t.id}>{t.number} · {t.seats} · {tr.t(ZONE_KEY[t.location])}{t.spot ? ` · ${t.spot}` : ''}</option>)}
            </select>
          </label>
          <label className="field">{tr('Soovid', 'Пожелания', 'Requests')}<textarea rows={2} value={b.specialRequest} onChange={set('specialRequest')} /></label>
          <p className="small muted">{tr(
            'Personal ei maksa. Kui valite laua kliendi soovil, on laua valik 30 € (tipptunnil 18–20 40 €) – klient saab makselingi. Kui laua valib metrdotel, kinnitub broneering kohe.',
            'Админ не платит. Если выбрать стол по просьбе гостя — выбор стола 30 € (в час пик 18–20 — 40 €), гость получит ссылку на оплату. Если стол выберет метрдотель, бронь подтвердится сразу.',
            'Staff does not pay. Choosing a table for the guest costs €30 (€40 at peak 18–20) – the guest gets a payment link. If the maître d’ chooses, the booking is confirmed at once.')}</p>
        </div>
      </div>

      <div className="paper white phone-extra">
        <p className="eyebrow">{tr('Eeltellimus menüüst', 'Предзаказ из меню', 'Pre-order from the menu')}</p>
        <div className="phone-menu">
          {menu.map((m) => (
            <div key={m.id} className={qty[m.id] ? 'pm-item on' : 'pm-item'}>
              {m.imageUrl && <img src={m.imageUrl} alt="" />}
              <span className="name">{m.name}<span className="small muted d-block">{money(m.price)}</span></span>
              <div className="stepper round">
                <button type="button" onClick={() => setQty({ ...qty, [m.id]: Math.max(0, (qty[m.id] ?? 0) - 1) })}>−</button>
                <span className="value">{qty[m.id] ?? 0}</span>
                <button type="button" onClick={() => setQty({ ...qty, [m.id]: Math.min(50, (qty[m.id] ?? 0) + 1) })}>+</button>
              </div>
            </div>
          ))}
        </div>
        <p className="eyebrow">{tr('Lisateenused', 'Дополнительные услуги', 'Extras')}</p>
        <div className="chips">
          {catalog.map((x) => (
            <button key={x.code} type="button" className={extras.includes(x.code) ? 'chip on' : 'chip'}
              onClick={() => setExtras(extras.includes(x.code) ? extras.filter((c) => c !== x.code) : [...extras, x.code])}>
              {tr.t(`extra.${x.code}`)} · {x.price ? money(x.price) : tr('tasuta', 'бесплатно', 'free')}
            </button>
          ))}
        </div>
        <p className="small">
          <strong>{tr('Kokku', 'Итого', 'Total')}: {money(
            menu.reduce((s, m) => s + (qty[m.id] ?? 0) * m.price, 0) + catalog.filter((x) => extras.includes(x.code)).reduce((s, x) => s + x.price, 0),
          )}</strong>
          {b.tableId ? ` + ${tr('laua valik', 'выбор стола', 'table choice')} 30/40 €` : ''} · {tr('klient maksab lingi kaudu', 'гость оплатит по ссылке', 'the guest pays via link')}
        </p>
        <button className="btn btn-dark btn-block">{tr('Broneeri ja saada makselink', 'Забронировать и отправить ссылку', 'Book and send payment link')}</button>
      </div>
      </form>
      )}
    </div>
  )
}

/** Kliendi registreerimine gastronoomilisele õhtule (telefoni teel). */
function EventMode() {
  const tr = useTr()
  const [events, setEvents] = useState([])
  const [eventId, setEventId] = useState('')
  const [done, setDone] = useState(false)

  const load = () => api('/api/events/all').then((l) => setEvents(l.filter((e) => !e.registrationClosed && new Date(e.startTime) > new Date()))).catch(() => {})
  useEffect(() => {
    load()
  }, [])
  const e = events.find((x) => x.id === Number(eventId))

  return (
    <div className="paper white phone-event">
      <label className="field">{tr('Õhtu', 'Вечер', 'Evening')}
        <select value={eventId} onChange={(x) => { setEventId(x.target.value); setDone(false) }}>
          <option value="">—</option>
          {events.map((x) => (
            <option key={x.id} value={x.id} disabled={x.seatsLeft === 0}>
              {tr.t(`event.${x.code}.title`, {}, x.title)} · {new Date(x.startTime).toLocaleDateString(tr.lang, { day: 'numeric', month: 'short' })} · {x.restaurantName} · {x.seatsLeft} {tr('vaba kohta', 'мест свободно', 'seats left')}
            </option>
          ))}
        </select>
      </label>
      {e && !done && <StaffSignup e={e} onDone={() => { setDone(true); load() }} onCancel={() => setEventId('')} />}
      {done && <p className="alert alert-ok">{tr('Klient on registreeritud – makselink saadeti.', 'Гость записан — ссылка на оплату отправлена.', 'The guest is registered – payment link sent.')}</p>}
    </div>
  )
}
