import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import AdminEvents from './AdminEvents'
import AdminStats from './AdminStats'
import { api, qs } from '../api'
import { useAuth } from '../auth'
import FloorPlan, { spotText } from '../components/FloorPlan'
import Pager from '../components/Pager'
import StaffMessages from '../components/StaffMessages'
import { fmtDate, fmtTime, preview, useTr, ZONE_LIST } from '../staff'
import { addDays, money, pickZone, stageOf, toIsoDate, zonesOf } from '../utils'

const TABS = [
  ['tables', 'Lauad', 'Столы', 'Tables'],
  ['menu', 'Menüü', 'Меню', 'Menu'],
  ['events', 'Gastronoomilised õhtud', 'Гастрономические вечера', 'Gastronomic evenings'],
]

/** Kiirlingid töölaualt personali põhilehtedele. */
const LINKS = [
  ['/personal/kalender', 'Kalender', 'Календарь', 'Calendar'],
  ['/personal/broneeringud', 'Broneeringud', 'Брони', 'Bookings'],
  ['/personal/uus', '+ Uus broneering', '+ Новая бронь', '+ New booking'],
  ['/personal/tugi', 'Vasta küsimustele', 'Ответить на вопросы', 'Answer questions'],
]

/** Admini töölaud: tervitus, kiirlingid, haldus + mis vajab tähelepanu ja otsing. */
export default function Admin() {
  const tr = useTr()
  const { user } = useAuth()
  const greetName = !user?.name || user.name === 'Admin' ? tr('Admin', 'Админ', 'Admin') : user.name.split(' ')[0]
  const [params] = useSearchParams()
  const [tab, setTab] = useState(params.get('tab') ?? '')
  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantId] = useState(1)

  useEffect(() => {
    api('/api/restaurants').then(setRestaurants)
  }, [])

  const restaurant = restaurants.find((r) => r.id === restaurantId)

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{tr('Töölaud', 'Рабочий стол', 'Dashboard')}</h1>
        </div>
        <select value={restaurantId} onChange={(e) => setRestaurantId(Number(e.target.value))}>
          {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
      </div>
      <div className="dash-hello">
        <p className="dash-hello-title">{tr('Tere', 'Привет', 'Hello')}, {greetName}!</p>
        <p className="italic muted">{tr('Mida täna teeme?', 'Что сегодня делаем?', 'What shall we do today?')}</p>
      </div>
      <div className="dash-actions">
        {LINKS.map(([to, et, ru, en]) => (
          <Link key={to} to={to} className="chip caps">{tr(et, ru, en)}</Link>
        ))}
      </div>
      <Attention restaurantId={restaurantId} />
      <p className="eyebrow">{tr('Lisafunktsioonid', 'Доп. функции', 'More tools')}</p>
      <div className="chips admin-tabs">
        <Link to="/personal/plaan" className="chip caps chip-link">{tr('Laudade plaan', 'План столов', 'Table plan')}</Link>
        {TABS.map(([key, et, ru, en]) => (
          <button key={key} className={tab === key ? 'chip caps on' : 'chip caps'} onClick={() => setTab(tab === key ? '' : key)}>
            {tr('Halda', 'Управлять', 'Manage')}: {tr(et, ru, en)}
          </button>
        ))}
        <button className={tab === 'stats' ? 'chip caps on' : 'chip caps'} onClick={() => setTab(tab === 'stats' ? '' : 'stats')}>
          {tr('Statistika', 'Статистика', 'Statistics')}
        </button>
      </div>
      {tab === 'stats' && <AdminStats restaurantId={restaurantId} restaurants={restaurants} />}
      {tab === 'tables' && restaurant && <TablesAdmin restaurant={restaurant} />}
      {tab === 'menu' && <MenuAdmin restaurantId={restaurantId} />}
      {tab === 'events' && restaurants.length > 0 && <AdminEvents restaurants={restaurants} restaurantId={restaurantId} />}
    </div>
  )
}

/* ===== Mis vajab tähelepanu ===== */

function Attention({ restaurantId }) {
  const tr = useTr()
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [found, setFound] = useState([])
  const [threads, setThreads] = useState([])
  const [pending, setPending] = useState([])
  const [today, setToday] = useState([])
  const [week, setWeek] = useState([])
  const [dishes, setDishes] = useState([])

  useEffect(() => {
    const d0 = toIsoDate(new Date())
    const d7 = toIsoDate(addDays(new Date(), 7))
    api('/api/support/threads').then(setThreads).catch(() => {})
    api('/api/reservations' + qs({ status: 'PendingPayment', pageSize: 50, sort: 'date' })).then((p) => setPending(p.items)).catch(() => {})
    api('/api/reservations' + qs({ restaurantId, date: d0, pageSize: 100, sort: 'date' })).then((p) => setToday(p.items.filter((r) => r.status !== 'Cancelled'))).catch(() => {})
    api('/api/reservations' + qs({ restaurantId, from: d0, to: d7, pageSize: 100, sort: 'date' })).then((p) => setWeek(p.items.filter((r) => r.status !== 'Cancelled'))).catch(() => {})
    api('/api/menu' + qs({ restaurantId, pageSize: 100 })).then((p) => setDishes(p.items.filter((m) => !m.isAvailable))).catch(() => {})
  }, [restaurantId])

  useEffect(() => {
    if (search.trim().length < 2) return setFound([])
    const timer = setTimeout(() => api('/api/customers' + qs({ search })).then(setFound).catch(() => {}), 250)
    return () => clearTimeout(timer)
  }, [search])

  const unreadThreads = threads.filter((th) => th.unread > 0)
  const unanswered = week.filter((r) => r.messageCount > 0 && !r.lastMessageFromStaff)
  const important = week.filter((r) => r.occasion !== 'None')
  const guestsToday = today.reduce((s, r) => s + r.guests, 0)
  const card = (r) => `/personal/klient?q=${encodeURIComponent(r.customerEmail)}`

  return (
    <>
      <div className="dash-search paper white">
        <input
          placeholder={tr('Otsi klienti: nimi, e-post või telefon…', 'Найти гостя: имя, e-mail или телефон…', 'Find a guest: name, e-mail or phone…')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && search.trim() && navigate(`/personal/klient?q=${encodeURIComponent(search.trim())}`)}
        />
        {found.length > 0 && (
          <div className="dash-found">
            {found.slice(0, 6).map((c) => (
              <Link key={c.id} to={`/personal/klient?q=${encodeURIComponent(c.email)}`}>
                <strong>{c.name}</strong> <span className="muted small">{c.email}{c.phone ? ` · ${c.phone}` : ''}</span>
              </Link>
            ))}
          </div>
        )}
        <Link className="btn btn-dark" to="/personal/uus">+ {tr('Broneering telefoni teel', 'Бронь по звонку', 'Phone booking')}</Link>
      </div>

      <div className="dash-tiles">
        <Link to="/personal/tugi" className={unreadThreads.length ? 'paper tile alert-tile' : 'paper tile'}>
          <span className="eyebrow">{tr('Vastamata küsimused', 'Неотвеченные вопросы', 'Unanswered questions')}</span>
          <strong>{unreadThreads.length}</strong>
        </Link>
        <Link to="/personal/broneeringud" className={unanswered.length ? 'paper tile alert-tile' : 'paper tile'}>
          <span className="eyebrow">{tr('Sõnumid broneeringutes', 'Сообщения в бронях', 'Booking messages')}</span>
          <strong>{unanswered.length}</strong>
        </Link>
        <a href="#pending" className={pending.length ? 'paper tile alert-tile' : 'paper tile'}>
          <span className="eyebrow">{tr('Ootab makset', 'Ожидают оплаты', 'Awaiting payment')}</span>
          <strong>{pending.length}</strong>
        </a>
        <Link to="/personal/plaan" className="paper tile">
          <span className="eyebrow">{tr('Täna', 'Сегодня', 'Today')}</span>
          <strong>{today.length}</strong>
          <span className="small muted">{guestsToday} {tr('külalist', 'гостей', 'guests')}</span>
        </Link>
        <a href="#dishes" className={dishes.length ? 'paper tile alert-tile' : 'paper tile'}>
          <span className="eyebrow">{tr('Road pole saadaval', 'Блюда недоступны', 'Dishes unavailable')}</span>
          <strong>{dishes.length}</strong>
        </a>
      </div>

      <div className="dash-cols">
        <div className="paper white" id="pending">
          <p className="card-title">{tr('Ootab makset', 'Ожидают оплаты', 'Awaiting payment')}</p>
          {pending.length === 0 && <p className="italic muted small">{tr('Kõik on makstud.', 'Всё оплачено.', 'Everything is paid.')}</p>}
          {pending.map((r) => (
            <div key={r.id} className="dash-row">
              <span><Link to={card(r)}>{r.customerName}</Link> · {fmtDate(r.startTime)} {fmtTime(r.startTime)} · {r.restaurantName}</span>
            </div>
          ))}
          <p className="small muted">{tr('Maksmata broneeringud tühistatakse 24 tunni pärast automaatselt.', 'Неоплаченные брони отменяются автоматически через 24 часа.', 'Unpaid bookings are cancelled automatically after 24 hours.')}</p>
        </div>

        <div className="paper white">
          <p className="card-title">{tr('Vajab vastust', 'Ждут ответа', 'Needs a reply')}</p>
          {unreadThreads.length + unanswered.length === 0 && <p className="italic muted small">{tr('Kõik on vastatud.', 'На всё ответили.', 'All answered.')}</p>}
          {unreadThreads.map((th) => (
            <div key={'t' + (th.userId ?? th.guestKey)} className="dash-row">
              <span><strong>{th.name}</strong> · <span className="small">{preview(th.lastText)}</span></span>
              {/* Avab kohe selle kliendi vestluse */}
              <Link className="btn btn-small btn-ox" to={`/personal/tugi?vestlus=${th.userId != null ? `u${th.userId}` : `g${th.guestKey}`}`}>{tr('Vasta', 'Ответить', 'Reply')}</Link>
            </div>
          ))}
          {unanswered.map((r) => (
            <div key={'r' + r.id} className="dash-row">
              <span><Link to={card(r)}>{r.customerName}</Link> · {fmtDate(r.startTime)} · {tr('broneeringu sõnum', 'сообщение в брони', 'booking message')}</span>
              <StaffMessages r={r} />
            </div>
          ))}
        </div>

        <div className="paper white">
          <p className="card-title">◆ {tr('Olulised 7 päeva jooksul', 'Важные на 7 дней', 'Important in 7 days')}</p>
          {important.length === 0 && <p className="italic muted small">—</p>}
          {important.slice(0, 10).map((r) => (
            <div key={r.id} className="dash-row">
              <span><Link to={card(r)}>{r.customerName}</Link> · {fmtDate(r.startTime)} {fmtTime(r.startTime)} · <span className="ox">{tr.t(`occasion.${r.occasion}`)}</span></span>
            </div>
          ))}
        </div>

        <div className="paper white" id="dishes">
          <p className="card-title">{tr('Road, mis pole saadaval', 'Недоступные блюда', 'Unavailable dishes')}</p>
          {dishes.length === 0 && <p className="italic muted small">{tr('Kõik road on saadaval.', 'Все блюда доступны.', 'All dishes available.')}</p>}
          {dishes.map((m) => <div key={m.id} className="dash-row"><span>{m.name}</span><span className="small ox">{m.category}</span></div>)}
        </div>
      </div>
    </>
  )
}

/* ===== Laudade haldus ===== */

/** Laua suurus plaanil kohtade arvu järgi (protsentides). */
function sizeFor(seats, round) {
  if (round) return seats <= 2 ? { width: 10, height: 16 } : { width: 13, height: 20 }
  if (seats <= 2) return { width: 10, height: 16 }
  if (seats <= 4) return { width: 14, height: 16 }
  if (seats <= 6) return { width: 14, height: 26 }
  if (seats <= 8) return { width: 22, height: 24 }
  return { width: 30, height: 26 }
}

/** Esimene vaba koht saalis (ei kattu laudade, baari, lava ega sissepääsuga). */
function freeSpot(tables, zone, size, restaurant) {
  const blocks = tables.filter((t) => t.location === zone).map((t) => ({ x: t.x - 2, y: t.y - 2, width: t.width + 4, height: t.height + 4 }))
  if (zone === 'Hall' && restaurant.barWidth > 0) blocks.push({ x: restaurant.barX, y: restaurant.barY, width: restaurant.barWidth, height: restaurant.barHeight })
  if (restaurant.stageZone === zone && restaurant.stageWidth > 0) blocks.push({ x: restaurant.stageX, y: restaurant.stageY, width: restaurant.stageWidth, height: restaurant.stageHeight })
  blocks.push({ x: 22, y: 86, width: 16, height: 14 }) // sissepääs
  for (let y = 4; y + size.height <= 98; y += 2) {
    for (let x = 4; x + size.width <= 98; x += 2) {
      const hit = blocks.some((b) => x < b.x + b.width && x + size.width > b.x && y < b.y + b.height && y + size.height > b.y)
      if (!hit) return { x, y }
    }
  }
  return null
}

function TablesAdmin({ restaurant }) {
  const tr = useTr()
  const [tables, setTables] = useState([])
  const [selectedId, setSelectedId] = useState(null)
  const [form, setForm] = useState(null)
  const [zone, setZone] = useState('Hall')
  const [msg, setMsg] = useState(null) // { type: 'ok' | 'err', text }

  const load = useCallback(() => api(`/api/restaurants/${restaurant.id}/tables`).then(setTables).catch((e) => say('err', e.message)), [restaurant.id])

  useEffect(() => {
    load()
    setSelectedId(null)
    setForm(null)
  }, [load])

  // Teade on alati plaani kohal ja kaob mõne sekundi pärast
  function say(type, text) {
    setMsg({ type, text, at: Date.now() })
  }
  useEffect(() => {
    if (!msg) return
    const timer = setTimeout(() => setMsg(null), msg.type === 'err' ? 7000 : 3500)
    return () => clearTimeout(timer)
  }, [msg])

  const toBody = (t) => ({
    restaurantId: restaurant.id, number: Number(t.number), seats: Number(t.seats), location: t.location, isActive: t.isActive,
    x: t.x, y: t.y, width: t.width, height: t.height, spot: t.spot || null, isRound: t.isRound,
  })

  function select(t) {
    setSelectedId(t.id)
    setForm({ ...t, spot: t.spot ?? '' })
  }

  async function addTable() {
    const size = sizeFor(4, false)
    const spot = freeSpot(tables, shownZone, size, restaurant)
    if (!spot) return say('err', tr('Selles saalis pole enam vaba ruumi.', 'В этом зале больше нет свободного места.', 'No free space left in this room.'))
    const number = Math.max(0, ...tables.map((t) => t.number)) + 1
    try {
      const t = await api('/api/tables', { method: 'POST', body: toBody({ number, seats: 4, location: shownZone, isActive: true, ...size, ...spot, spot: '', isRound: false }) })
      await load()
      select(t)
      say('ok', tr(`Laud ${number} lisatud. Lohistage see hiirega õigesse kohta.`, `Стол ${number} добавлен. Перетащите его мышкой на нужное место.`, `Table ${number} added. Drag it to the right spot.`))
    } catch (e) {
      say('err', e.message)
    }
  }

  // Lohistamine: salvestub kohe, viga -> laud läheb tagasi
  async function moveTable(t, x, y) {
    const before = tables
    setTables(tables.map((v) => (v.id === t.id ? { ...v, x, y } : v)))
    try {
      const saved = await api(`/api/tables/${t.id}`, { method: 'PUT', body: toBody({ ...t, x, y }) })
      setTables((l) => l.map((v) => (v.id === t.id ? saved : v)))
      if (selectedId === t.id) setForm((f) => ({ ...f, x, y }))
      say('ok', tr(`Laud ${t.number} – asukoht salvestatud ✓`, `Стол ${t.number} — место сохранено ✓`, `Table ${t.number} – position saved ✓`))
    } catch (e) {
      setTables(before)
      say('err', e.message)
    }
  }

  // Kiirmuudatus (kohad, pööramine, nihutamine, ümmargune, aktiivne) – salvestub kohe
  async function patch(changes, okText) {
    const t = tables.find((x) => x.id === selectedId)
    if (!t) return
    let next = { ...t, ...changes }
    if ('seats' in changes || 'isRound' in changes) {
      const size = sizeFor(next.seats, next.isRound)
      next = { ...next, ...size }
    }
    next.x = Math.max(0, Math.min(100 - next.width, next.x))
    next.y = Math.max(0, Math.min(100 - next.height, next.y))
    try {
      const saved = await api(`/api/tables/${t.id}`, { method: 'PUT', body: toBody(next) })
      setTables((l) => l.map((v) => (v.id === t.id ? saved : v)))
      setForm((f) => ({ ...f, ...saved, spot: f?.spot ?? saved.spot ?? '' }))
      say('ok', okText ?? tr(`Laud ${t.number} salvestatud ✓`, `Стол ${t.number} сохранён ✓`, `Table ${t.number} saved ✓`))
    } catch (err) {
      say('err', err.message)
    }
  }
  const nudge = (dx, dy) => {
    const t = tables.find((x) => x.id === selectedId)
    if (t) patch({ x: t.x + dx, y: t.y + dy })
  }

  // Nooleklahvid liigutavad valitud lauda (kui kursor pole tekstiväljas)
  useEffect(() => {
    function onKey(e) {
      if (!selectedId || ['INPUT', 'TEXTAREA', 'SELECT'].includes(document.activeElement?.tagName)) return
      const d = { ArrowLeft: [-1, 0], ArrowRight: [1, 0], ArrowUp: [0, -1], ArrowDown: [0, 1] }[e.key]
      if (!d) return
      e.preventDefault()
      nudge(d[0] * 2, d[1] * 2)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  })

  async function save(e) {
    e.preventDefault()
    const current = tables.find((t) => t.id === selectedId)
    const seats = Number(form.seats)
    if (!(Number(form.number) >= 1)) return say('err', tr('Laua number peab olema positiivne.', 'Номер стола должен быть положительным.', 'Table number must be positive.'))
    if (!(seats >= 1 && seats <= 20)) return say('err', tr('Kohtade arv peab olema 1 kuni 20.', 'Мест должно быть от 1 до 20.', 'Seats must be 1 to 20.'))
    // Suurus kohtade arvu järgi; teise saali viimisel leitakse seal vaba koht
    const size = sizeFor(seats, form.isRound)
    let pos = { x: Math.min(form.x, 100 - size.width), y: Math.min(form.y, 100 - size.height) }
    if (form.location !== current.location) {
      const spot = freeSpot(tables.filter((t) => t.id !== current.id), form.location, size, restaurant)
      if (!spot) return say('err', tr('Selles saalis pole vaba ruumi.', 'В этом зале нет свободного места.', 'No free space in that room.'))
      pos = spot
    }
    try {
      const saved = await api(`/api/tables/${current.id}`, { method: 'PUT', body: toBody({ ...form, ...size, ...pos }) })
      await load()
      select(saved)
      if (saved.location !== zone) setZone(saved.location)
      say('ok', tr(`Laud ${saved.number} salvestatud ✓`, `Стол ${saved.number} сохранён ✓`, `Table ${saved.number} saved ✓`))
    } catch (err) {
      say('err', err.message)
    }
  }

  async function remove() {
    const t = tables.find((x) => x.id === selectedId)
    if (!window.confirm(`${tr('Kustutada laud', 'Удалить стол', 'Delete table')} ${t.number}?`)) return
    try {
      await api(`/api/tables/${t.id}`, { method: 'DELETE' })
      setSelectedId(null)
      setForm(null)
      await load()
      say('ok', tr('Laud kustutatud.', 'Стол удалён.', 'Table deleted.'))
    } catch (err) {
      say('err', err.message)
    }
  }

  const set = (f) => (e) => setForm({ ...form, [f]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })
  const planTables = tables.map((t) => ({ ...t, state: t.id === selectedId ? 'selected' : t.isActive ? 'free' : 'small', clickable: true }))
  // Ainult saalid, kus on laudu; restorani vahetamisel ei jää plaan tühjaks
  const shownZone = pickZone(tables, zone)

  return (
    <div className="admin-block">
      {msg && <p className={`plan-msg ${msg.type}`} role="status">{msg.text}</p>}

      <div className="plan-wrap">
        <div className="plan-col">
          <div className="zone-tabs" role="tablist">
            {zonesOf(tables).map((z) => (
              <button key={z} type="button" className={shownZone === z ? 'on' : ''} onClick={() => setZone(z)}>{tr.zone(z)}</button>
            ))}
          </div>
          <FloorPlan
            tables={planTables}
            zone={shownZone}
            bar={{ x: restaurant.barX, y: restaurant.barY, width: restaurant.barWidth, height: restaurant.barHeight }}
            stage={stageOf(restaurant)}
            stageLabel={tr('Lava', 'Сцена', 'Stage')}
            edgeLabel={tr.edge(shownZone)}
            entranceLabel={shownZone === 'Upstairs' ? tr('Trepp', 'Лестница', 'Stairs') : tr('Sissepääs', 'Вход', 'Entrance')}
            barLabel={tr('Baar', 'Бар', 'Bar')}
            seatsLabel={(n) => `${n} ${tr('kohta', 'мест', 'seats')}`}
            onSelect={select}
            onMove={moveTable}
          />
          <p className="muted small">{tr(
            'Lohistage lauda hiirega – asukoht salvestub kohe. Klõpsake laual, et muuta numbrit, kohti või kirjeldust.',
            'Перетаскивайте стол мышкой — место сохраняется сразу. Нажмите на стол, чтобы изменить номер, места или описание.',
            'Drag a table with the mouse – the position saves at once. Click a table to change its number, seats or label.')}</p>
        </div>

        <aside className="plan-side paper white side-card">
          <button className="btn btn-dark" onClick={addTable}>+ {tr('Lisa laud', 'Добавить стол', 'Add table')} ({tr.zone(shownZone).toLowerCase()})</button>
          {!form && <p className="italic muted small">{tr('Valige plaanilt laud.', 'Выберите стол на плане.', 'Choose a table on the plan.')}</p>}
          {form && (
            <form className="admin-form" onSubmit={save} noValidate>
              <p className="card-title">{tr('Laud', 'Стол', 'Table')} {form.number}</p>
              {(() => {
                const t = tables.find((x) => x.id === selectedId)
                if (!t) return null
                return (
                  <div className="tbl-quick">
                    <div className="tq-row">
                      <span className="eyebrow">{tr('Kohti', 'Мест', 'Seats')}</span>
                      <div className="stepper round">
                        <button type="button" onClick={() => t.seats > 1 && patch({ seats: t.seats - 1 })}>−</button>
                        <span className="value">{t.seats}</span>
                        <button type="button" onClick={() => t.seats < 20 && patch({ seats: t.seats + 1 })}>+</button>
                      </div>
                    </div>
                    <div className="tq-row">
                      <span className="eyebrow">{tr('Liiguta', 'Сдвинуть', 'Move')}</span>
                      <div className="tq-arrows">
                        <button type="button" className="btn btn-small" onClick={() => nudge(-2, 0)} aria-label="left">←</button>
                        <button type="button" className="btn btn-small" onClick={() => nudge(0, -2)} aria-label="up">↑</button>
                        <button type="button" className="btn btn-small" onClick={() => nudge(0, 2)} aria-label="down">↓</button>
                        <button type="button" className="btn btn-small" onClick={() => nudge(2, 0)} aria-label="right">→</button>
                      </div>
                    </div>
                    <div className="tq-row">
                      {!t.isRound && <button type="button" className="btn btn-small" onClick={() => patch({ width: t.height, height: t.width })}>↻ {tr('Pööra', 'Повернуть', 'Rotate')}</button>}
                      <button type="button" className="btn btn-small" onClick={() => patch({ isRound: !t.isRound })}>{t.isRound ? tr('Kandiline', 'Квадратный', 'Square') : tr('Ümmargune', 'Круглый', 'Round')}</button>
                      <button type="button" className={t.isActive ? 'btn btn-small' : 'btn btn-small btn-ox'} onClick={() => patch({ isActive: !t.isActive })}>
                        {t.isActive ? tr('Peida broneerimisest', 'Скрыть из брони', 'Hide from booking') : tr('Luba broneerida', 'Разрешить бронь', 'Allow booking')}
                      </button>
                    </div>
                    <p className="muted small">{tr('Muudatused salvestuvad kohe. Nooleklahvid liigutavad lauda.', 'Изменения сохраняются сразу. Стрелки на клавиатуре двигают стол.', 'Changes save at once. Arrow keys move the table.')}</p>
                  </div>
                )
              })()}
              <label className="field">{tr('Number', 'Номер', 'Number')}<input type="number" min={1} value={form.number} onChange={set('number')} /></label>
              <label className="field">{tr('Saal', 'Зал', 'Room')}
                <select value={form.location} onChange={set('location')}>
                  {ZONE_LIST.map((z) => <option key={z} value={z}>{tr.zone(z)}</option>)}
                </select>
              </label>
              <label className="field">{tr('Kirjeldus plaanil', 'Описание на плане', 'Label on plan')}<input value={form.spot} onChange={set('spot')} placeholder="akna all" /></label>
              <div className="actions">
                <button className="btn btn-dark">{tr('Salvesta', 'Сохранить', 'Save')}</button>
                <button type="button" className="btn" onClick={() => { setSelectedId(null); setForm(null) }}>{tr('Sulge', 'Закрыть', 'Close')}</button>
                <button type="button" className="btn btn-ox-line" onClick={remove}>{tr('Kustuta', 'Удалить', 'Delete')}</button>
              </div>
            </form>
          )}
        </aside>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead><tr><th>Nr</th><th>{tr('Kohti', 'Мест', 'Seats')}</th><th>{tr('Saal', 'Зал', 'Room')}</th><th>{tr('Kirjeldus', 'Описание', 'Label')}</th><th>{tr('Aktiivne', 'Активен', 'Active')}</th><th /></tr></thead>
          <tbody>
            {tables.map((t) => (
              <tr key={t.id} className={t.id === selectedId ? 'on' : ''}>
                <td data-label="Nr">{t.number}</td>
                <td data-label="Kohti">{t.seats}</td>
                <td data-label="Saal">{tr.zone(t.location)}</td>
                <td data-label="Kirjeldus">{spotText(t.spot, tr.lang)}</td>
                <td data-label="Aktiivne">{t.isActive ? tr('jah', 'да', 'yes') : tr('ei', 'нет', 'no')}</td>
                <td className="actions"><div className="row-actions"><button className="btn btn-small" onClick={() => { setZone(t.location); select(t) }}>{tr('Muuda', 'Изменить', 'Edit')}</button></div></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

/* ===== Menüü haldus ===== */

const CATEGORIES = ['Külmad road', 'Mereannid', 'Setid', 'Pearoad', 'Magustoidud', 'Veinid']
const emptyItem = { name: '', description: '', category: 'Pearoad', price: '', isAvailable: true }

function MenuAdmin({ restaurantId }) {
  const tr = useTr()
  const [f, setF] = useState({ search: '', category: '', sort: 'category', page: 1, pageSize: 8 })
  const [data, setData] = useState(null)
  const [form, setForm] = useState(null)
  const [editId, setEditId] = useState(null)
  const [error, setError] = useState('')

  const load = useCallback(() => {
    // raw: admin muudab andmebaasi teksti, mitte tõlget
    api('/api/menu' + qs({ ...f, restaurantId, raw: true })).then(setData).catch((e) => setError(e.message))
  }, [f, restaurantId])

  useEffect(() => {
    load()
  }, [load])

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })

  async function save(e) {
    e.preventDefault()
    const body = {
      ...form,
      restaurantId,
      price: Number(String(form.price).replace(',', '.')),
      tags: (form.tagsText ?? '').split(',').map((x) => x.trim()).filter(Boolean),
    }
    if (body.name.trim().length < 2) return setError(tr('Nimi on liiga lühike.', 'Название слишком короткое.', 'Name is too short.'))
    if (!body.category.trim()) return setError(tr('Kategooria on kohustuslik.', 'Категория обязательна.', 'Category is required.'))
    if (!(body.price > 0)) return setError(tr('Hind peab olema suurem kui 0.', 'Цена должна быть больше 0.', 'Price must be above 0.'))
    try {
      if (editId) await api(`/api/menu/${editId}`, { method: 'PUT', body })
      else await api('/api/menu', { method: 'POST', body })
      setForm(null)
      setError('')
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  // Kiire nupp: roog selles restoranis ajutiselt saadaval / mitte saadaval
  async function toggleAvailable(m) {
    try {
      await api(`/api/menu/${m.id}`, { method: 'PUT', body: { ...m, restaurantId, isAvailable: !m.isAvailable } })
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function remove(m) {
    if (!window.confirm(`${tr('Kustutada', 'Удалить', 'Delete')} "${m.name}"?`)) return
    try {
      await api(`/api/menu/${m.id}`, { method: 'DELETE' })
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="admin-block">
      {error && <p className="alert alert-error">{error}</p>}
      <div className="filters">
        <input className="grow" placeholder={tr('Otsi…', 'Поиск…', 'Search…')} value={f.search} onChange={(e) => setF({ ...f, search: e.target.value, page: 1 })} />
        <select value={f.category} onChange={(e) => setF({ ...f, category: e.target.value, page: 1 })}>
          <option value="">{tr('Kõik kategooriad', 'Все категории', 'All categories')}</option>
          {CATEGORIES.map((c) => <option key={c}>{c}</option>)}
        </select>
        <select value={f.sort} onChange={(e) => setF({ ...f, sort: e.target.value, page: 1 })}>
          <option value="category">{tr('Kategooria', 'Категория', 'Category')}</option>
          <option value="name">{tr('Nimi', 'Название', 'Name')}</option>
          <option value="price">{tr('Hind', 'Цена', 'Price')}</option>
        </select>
        <button className="btn btn-dark" onClick={() => { setEditId(null); setForm({ ...emptyItem }) }}>+ {tr('Lisa toit', 'Добавить блюдо', 'Add dish')}</button>
      </div>

      {form && (
        <form className="paper white admin-form" onSubmit={save} noValidate>
          <p className="card-title">{editId ? tr('Muuda toitu', 'Изменить блюдо', 'Edit dish') : tr('Uus toit', 'Новое блюдо', 'New dish')}</p>
          <label className="field">{tr('Nimi', 'Название', 'Name')}<input value={form.name} onChange={set('name')} /></label>
          <div className="field-row">
            <label className="field">{tr('Kategooria', 'Категория', 'Category')}<input value={form.category} onChange={set('category')} list="cats" /></label>
            <label className="field">{tr('Hind', 'Цена', 'Price')} €<input value={form.price} onChange={set('price')} inputMode="decimal" /></label>
          </div>
          <datalist id="cats">{CATEGORIES.map((c) => <option key={c} value={c} />)}</datalist>
          <label className="field">{tr('Kirjeldus', 'Описание', 'Description')}<textarea rows={2} value={form.description} onChange={set('description')} /></label>
          <label className="field">{tr('Sildid', 'Метки', 'Tags')} (gluten, milk, egg, nuts, fish, shellfish, vegan, vegetarian)<input value={form.tagsText ?? ''} onChange={set('tagsText')} /></label>
          <label className="field">{tr('Foto aadress', 'Адрес фото', 'Photo URL')}<input value={form.imageUrl ?? ''} onChange={set('imageUrl')} placeholder="/img/menu/austrid.jpg" /></label>
          <label className="check"><input type="checkbox" checked={form.isAvailable} onChange={set('isAvailable')} /> {tr('Saadaval', 'Доступно', 'Available')}</label>
          <div className="actions">
            <button className="btn btn-dark">{tr('Salvesta', 'Сохранить', 'Save')}</button>
            <button type="button" className="btn" onClick={() => setForm(null)}>{tr('Loobu', 'Отмена', 'Cancel')}</button>
          </div>
        </form>
      )}

      <div className="table-wrap">
        <table className="data-table">
          <thead><tr><th>{tr('Nimi', 'Название', 'Name')}</th><th>{tr('Kategooria', 'Категория', 'Category')}</th><th>{tr('Hind', 'Цена', 'Price')}</th><th>{tr('Saadaval', 'Доступно', 'Available')}</th><th /></tr></thead>
          <tbody>
            {data?.items.map((m) => (
              <tr key={m.id}>
                <td data-label="Nimi">
                  <div className="admin-dish">
                    {m.imageUrl && <img src={m.imageUrl} alt="" />}
                    <span>{m.name}<span className="muted small d-block">{m.description}</span></span>
                  </div>
                </td>
                <td data-label="Kategooria">{m.category}</td>
                <td data-label="Hind">{money(m.price)}</td>
                <td data-label="Saadaval">{m.isAvailable ? tr('jah', 'да', 'yes') : <strong className="ox">{tr('POLE SAADAVAL', 'НЕДОСТУПНО', 'UNAVAILABLE')}</strong>}</td>
                <td className="actions">
                  <div className="row-actions">
                  <button className={m.isAvailable ? 'btn btn-small' : 'btn btn-small btn-ox'} onClick={() => toggleAvailable(m)}>
                    {m.isAvailable ? tr('Pole hetkel saadaval', 'Сейчас недоступно', 'Mark unavailable') : tr('Taas saadaval', 'Снова доступно', 'Available again')}
                  </button>
                  <button className="btn btn-small" onClick={() => { setEditId(m.id); setForm({ ...m, tagsText: (m.tags ?? []).join(', ') }) }}>{tr('Muuda', 'Изменить', 'Edit')}</button>
                  <button className="btn btn-small btn-ox-line" onClick={() => remove(m)}>{tr('Kustuta', 'Удалить', 'Delete')}</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data && <Pager {...data} onChange={(page) => setF({ ...f, page })} />}
    </div>
  )
}
