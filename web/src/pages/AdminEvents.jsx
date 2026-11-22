import { useCallback, useEffect, useState } from 'react'
import { api, qs } from '../api'
import { fmtDate, fmtTime, timeSlots, useTr, ZONE_LIST } from '../staff'
import { addDays, money, toIsoDate } from '../utils'

const MIN_DAYS = 14
const empty = (restaurantId) => ({
  restaurantId, title: '', kind: '', description: '', date: '', time: '19:00', pricePerGuest: '', capacity: '',
  zone: '', dressCode: '', rule: '', noChildren: false, imageUrl: '', items: [],
})

/** Admin: gastronoomiliste õhtute loomine ja muutmine, külaliste nimekiri ja kliendi registreerimine. */
export default function AdminEvents({ restaurants }) {
  const tr = useTr()
  const [events, setEvents] = useState([])
  const [form, setForm] = useState(null)
  const [editId, setEditId] = useState(null)
  const [guestsOf, setGuestsOf] = useState(null)
  const [error, setError] = useState('')
  const [ok, setOk] = useState('')

  const load = useCallback(() => api('/api/events/all').then(setEvents).catch((e) => setError(e.message)), [])
  useEffect(() => {
    load()
  }, [load])

  function edit(e) {
    const d = new Date(e.startTime)
    setEditId(e.id)
    setForm({
      restaurantId: e.restaurantId, title: e.title, kind: e.kind, description: e.description,
      date: toIsoDate(d), time: fmtTime(d), pricePerGuest: e.pricePerGuest, capacity: e.capacity, zone: e.zone ?? '',
      dressCode: e.dressCode, rule: e.rule ?? '', noChildren: e.noChildren, imageUrl: e.imageUrl ?? '',
      items: e.items.map((i) => ({ menuItemId: i.menuItemId, name: i.name, description: i.description, price: i.price, imageUrl: i.imageUrl })),
    })
    setError('')
    setOk('')
  }

  async function remove(e) {
    if (!window.confirm(`${tr('Kustutada õhtu', 'Удалить вечер', 'Delete evening')} "${e.title}"?`)) return
    try {
      await api(`/api/events/${e.id}`, { method: 'DELETE' })
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="admin-block">
      {error && <p className="alert alert-error">{error}</p>}
      {ok && <p className="alert alert-ok">{ok}</p>}
      {!form && (
        <button className="btn btn-dark" onClick={() => { setEditId(null); setForm(empty(restaurants[0]?.id ?? 1)); setOk('') }}>
          + {tr('Uus õhtu', 'Новый вечер', 'New evening')}
        </button>
      )}

      {form && (
        <EventForm
          form={form}
          setForm={setForm}
          editId={editId}
          restaurants={restaurants}
          onCancel={() => setForm(null)}
          onSaved={() => {
            setForm(null)
            setOk(editId ? tr('Õhtu salvestatud.', 'Вечер сохранён.', 'Evening saved.') : tr('Õhtu lisatud.', 'Вечер добавлен.', 'Evening added.'))
            load()
          }}
        />
      )}

      <div className="admin-events">
        {events.map((e) => (
          <div key={e.id} className="paper white ae-row">
            {e.imageUrl && <img src={e.imageUrl} alt="" />}
            <div className="ae-text">
              <strong>{tr.t(`event.${e.code}.title`, {}, e.title)}</strong>
              <span className="small">{fmtDate(e.startTime)} {fmtTime(e.startTime)} · {e.restaurantName}{e.zone ? ` · ${tr.zone(e.zone)}` : ''}</span>
              <span className="small muted">{money(e.pricePerGuest)} · {e.capacity - e.seatsLeft}/{e.capacity} {tr('kohta täis', 'мест занято', 'seats taken')} · {e.items.length} {tr('eripakkumist', 'спецпозиций', 'specials')}</span>
            </div>
            <div className="row-actions">
              <button className="btn btn-small btn-dark" onClick={() => setGuestsOf(guestsOf === e.id ? null : e.id)}>{tr('Külalised', 'Гости', 'Guests')}</button>
              <button className="btn btn-small" onClick={() => edit(e)}>{tr('Muuda', 'Изменить', 'Edit')}</button>
              <button className="btn btn-small btn-ox-line" onClick={() => remove(e)}>{tr('Kustuta', 'Удалить', 'Delete')}</button>
            </div>
            {guestsOf === e.id && <EventGuests e={e} onChanged={load} />}
          </div>
        ))}
      </div>
    </div>
  )
}

/* ===== Õhtu vorm ===== */

function EventForm({ form, setForm, editId, restaurants, onCancel, onSaved }) {
  const tr = useTr()
  const [menu, setMenu] = useState([])
  const [tables, setTables] = useState([])
  const [error, setError] = useState('')
  const restaurant = restaurants.find((r) => r.id === Number(form.restaurantId))
  const set = (f) => (e) => setForm({ ...form, [f]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })
  const minDate = toIsoDate(addDays(new Date(), MIN_DAYS))

  useEffect(() => {
    api('/api/menu' + qs({ restaurantId: form.restaurantId, pageSize: 100 })).then((p) => setMenu(p.items)).catch(() => {})
    api(`/api/restaurants/${form.restaurantId}/tables`).then(setTables).catch(() => {})
  }, [form.restaurantId])

  const zones = ZONE_LIST.filter((z) => tables.some((t) => t.location === z && t.isActive))
  const maxSeats = tables.filter((t) => t.isActive && (!form.zone || t.location === form.zone)).reduce((s, t) => s + t.seats, 0)
  const slots = restaurant ? timeSlots(restaurant.opensAt, restaurant.closesAt < restaurant.opensAt ? '23:30' : lastArrival(restaurant.closesAt)) : timeSlots()
  const chosenMenu = new Set(form.items.filter((i) => i.menuItemId).map((i) => i.menuItemId))

  function toggleMenu(m) {
    setForm({
      ...form,
      items: chosenMenu.has(m.id)
        ? form.items.filter((i) => i.menuItemId !== m.id)
        : [...form.items, { menuItemId: m.id, name: m.name, description: m.description, price: m.price, imageUrl: m.imageUrl }],
    })
  }
  const setItem = (idx, f, v) => setForm({ ...form, items: form.items.map((i, k) => (k === idx ? { ...i, [f]: v } : i)) })

  async function photo(e) {
    const file = e.target.files?.[0]
    if (!file) return
    try {
      setForm({ ...form, imageUrl: await shrinkImage(file, 1100) })
    } catch {
      setError(tr('Pilti ei õnnestunud lugeda.', 'Не удалось прочитать фото.', 'Could not read the photo.'))
    }
  }

  async function save(e) {
    e.preventDefault()
    setError('')
    if (!form.date) return setError(tr('Valige kuupäev kalendrist.', 'Выберите дату в календаре.', 'Pick a date in the calendar.'))
    const body = {
      ...form,
      restaurantId: Number(form.restaurantId),
      startTime: `${form.date}T${form.time}:00`,
      pricePerGuest: Number(String(form.pricePerGuest).replace(',', '.')),
      capacity: Number(form.capacity),
      zone: form.zone || null,
      rule: form.rule || null,
      imageUrl: form.imageUrl || null,
      items: form.items.map((i) => ({ ...i, price: Number(String(i.price).replace(',', '.')) })),
    }
    try {
      if (editId) await api(`/api/events/${editId}`, { method: 'PUT', body })
      else await api('/api/events', { method: 'POST', body })
      onSaved()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <form className="paper white ae-form" onSubmit={save} noValidate>
      <p className="card-title">{editId ? tr('Muuda õhtut', 'Изменить вечер', 'Edit evening') : tr('Uus gastronoomiline õhtu', 'Новый гастрономический вечер', 'New gastronomic evening')}</p>

      <div className="ae-grid">
        <div className="ae-col">
          <label className="field">{tr('Restoran', 'Ресторан', 'Restaurant')}
            <select value={form.restaurantId} onChange={(e) => setForm({ ...form, restaurantId: Number(e.target.value), zone: '', items: form.items.filter((i) => !i.menuItemId) })}>
              {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
            </select>
          </label>
          <label className="field">{tr('Pealkiri', 'Название', 'Title')}<input value={form.title} onChange={set('title')} placeholder={tr('nt Trühvliõhtu', 'например, Трюфельный вечер', 'e.g. Truffle night')} /></label>
          <label className="field">{tr('Õhtu liik', 'Тип вечера', 'Kind')}<input value={form.kind} onChange={set('kind')} placeholder={tr('nt Degustatsiooniõhtusöök', 'например, Дегустационный ужин', 'e.g. Tasting dinner')} /></label>
          <label className="field">{tr('Kirjeldus', 'Описание', 'Description')}<textarea rows={3} value={form.description} onChange={set('description')} /></label>
          <div className="field-row">
            <label className="field">{tr('Hind külalise kohta €', 'Цена за гостя €', 'Price per guest €')}<input value={form.pricePerGuest} onChange={set('pricePerGuest')} inputMode="decimal" /></label>
            <label className="field">{tr('Kohti', 'Мест', 'Seats')} ({tr('max', 'макс.', 'max')} {maxSeats})<input type="number" min={1} max={maxSeats} value={form.capacity} onChange={set('capacity')} /></label>
          </div>
          <label className="field">{tr('Saal', 'Зал', 'Room')}
            <select value={form.zone} onChange={set('zone')}>
              <option value="">{tr('Kogu restoran', 'Весь ресторан', 'Whole restaurant')}</option>
              {zones.map((z) => <option key={z} value={z}>{tr.zone(z)}{restaurant?.stageZone === z ? ` · ♪ ${tr('lava', 'сцена', 'stage')}` : ''}</option>)}
            </select>
          </label>
          <div className="field-row">
            <label className="field">{tr('Riietus', 'Дресс-код', 'Dress code')}<input value={form.dressCode} onChange={set('dressCode')} placeholder="smart elegant" /></label>
          </div>
          <label className="field">{tr('Range reegel (külaline peab nõustuma)', 'Строгое правило (гость должен согласиться)', 'Strict rule (guest must accept)')}<textarea rows={2} value={form.rule} onChange={set('rule')} /></label>
          <label className="check"><input type="checkbox" checked={form.noChildren} onChange={set('noChildren')} /> {tr('Ainult täiskasvanutele', 'Только для взрослых', 'Adults only')}</label>
        </div>

        <div className="ae-col">
          <p className="eyebrow">{tr('Kuupäev', 'Дата', 'Date')} · {tr('kõige varem', 'не раньше', 'not before')} {fmtDate(minDate)}</p>
          <MiniCalendar value={form.date} min={minDate} onChange={(d) => setForm({ ...form, date: d })} />
          <p className="eyebrow">{tr('Algus', 'Начало', 'Start')}</p>
          <div className="chips">
            {slots.map((s) => <button key={s} type="button" className={form.time === s ? 'chip small on' : 'chip small'} onClick={() => setForm({ ...form, time: s })}>{s}</button>)}
          </div>
          <p className="eyebrow">{tr('Foto', 'Фото', 'Photo')}</p>
          <div className="ae-photo">
            {form.imageUrl ? <img src={form.imageUrl} alt="" /> : <span className="muted small">{tr('Foto puudub', 'Нет фото', 'No photo')}</span>}
            <label className="btn btn-small">{tr('Laadi üles', 'Загрузить', 'Upload')}<input type="file" accept="image/*" hidden onChange={photo} /></label>
            <input className="grow" value={form.imageUrl.startsWith('data:') ? '' : form.imageUrl} onChange={set('imageUrl')} placeholder={tr('või aadress, nt /img/metal.jpg', 'или адрес, например /img/metal.jpg', 'or URL, e.g. /img/metal.jpg')} />
          </div>
        </div>
      </div>

      <p className="eyebrow">{tr('Ainult sel õhtul – mida saab tellida', 'Только в этот вечер — что можно заказать', 'Only this evening – what guests can order')}</p>
      <p className="small muted">{tr('Märkige menüüst sobivad road ja joogid või lisage oma pakkumine (nt viskisaade).', 'Отметьте подходящие блюда и напитки из меню или добавьте свою позицию (например, виски-сет).', 'Tick dishes and drinks from the menu or add your own special (e.g. a whisky flight).')}</p>
      <div className="ae-menu">
        {menu.map((m) => (
          <button key={m.id} type="button" className={chosenMenu.has(m.id) ? 'ae-dish on' : 'ae-dish'} onClick={() => toggleMenu(m)}>
            {m.imageUrl && <img src={m.imageUrl} alt="" />}
            <span>{m.name}<span className="small muted d-block">{money(m.price)}</span></span>
            <span className="tick">{chosenMenu.has(m.id) ? '✓' : '+'}</span>
          </button>
        ))}
      </div>
      {form.items.map((i, idx) => (
        <div key={idx} className="ae-item">
          {i.imageUrl && <img src={i.imageUrl} alt="" />}
          <input value={i.name} onChange={(e) => setItem(idx, 'name', e.target.value)} placeholder={tr('Nimi', 'Название', 'Name')} />
          <input className="grow" value={i.description} onChange={(e) => setItem(idx, 'description', e.target.value)} placeholder={tr('Kirjeldus', 'Описание', 'Description')} />
          <input className="price" value={i.price} onChange={(e) => setItem(idx, 'price', e.target.value)} inputMode="decimal" placeholder="€" />
          <button type="button" className="btn btn-small btn-ox-line" onClick={() => setForm({ ...form, items: form.items.filter((_, k) => k !== idx) })}>✕</button>
        </div>
      ))}
      <button type="button" className="po-add" onClick={() => setForm({ ...form, items: [...form.items, { menuItemId: null, name: '', description: '', price: '', imageUrl: null }] })}>
        <span className="plus">+</span> {tr('Lisa oma pakkumine', 'Добавить свою позицию', 'Add your own special')}
      </button>

      {error && <p className="alert alert-error">{error}</p>}
      <div className="actions">
        <button className="btn btn-dark">{tr('Salvesta', 'Сохранить', 'Save')}</button>
        <button type="button" className="btn" onClick={onCancel}>{tr('Loobu', 'Отмена', 'Cancel')}</button>
      </div>
    </form>
  )
}

/** Viimane saabumine: 2 tundi enne sulgemist. */
function lastArrival(closes) {
  const [h, m] = closes.split(':').map(Number)
  const total = h * 60 + m - 120
  return `${String(Math.floor(total / 60)).padStart(2, '0')}:${String(total % 60).padStart(2, '0')}`
}

/** Pilt väiksemaks (laius kuni maxWidth), et see mahuks andmebaasi. */
function shrinkImage(file, maxWidth) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.onload = () => {
      const scale = Math.min(1, maxWidth / img.width)
      const canvas = document.createElement('canvas')
      canvas.width = Math.round(img.width * scale)
      canvas.height = Math.round(img.height * scale)
      canvas.getContext('2d').drawImage(img, 0, 0, canvas.width, canvas.height)
      resolve(canvas.toDataURL('image/jpeg', 0.8))
      URL.revokeObjectURL(img.src)
    }
    img.onerror = reject
    img.src = URL.createObjectURL(file)
  })
}

/** Ilus kuukalender: varasemad päevad kui "min" pole valitavad. */
function MiniCalendar({ value, min, onChange }) {
  const tr = useTr()
  const start = value || min
  const [month, setMonth] = useState(() => new Date(start.slice(0, 4), Number(start.slice(5, 7)) - 1, 1))
  const first = addDays(month, -((month.getDay() + 6) % 7))
  const days = Array.from({ length: 42 }, (_, i) => addDays(first, i))
  const shift = (n) => setMonth(new Date(month.getFullYear(), month.getMonth() + n, 1))

  return (
    <div className="mini-cal">
      <div className="row-between">
        <button type="button" className="btn btn-small" onClick={() => shift(-1)}>‹</button>
        <span className="card-title">{month.toLocaleDateString(tr.lang, { month: 'long', year: 'numeric' })}</span>
        <button type="button" className="btn btn-small" onClick={() => shift(1)}>›</button>
      </div>
      <div className="mc-grid">
        {tr(['E', 'T', 'K', 'N', 'R', 'L', 'P'], ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'], ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su']).map((w) => <span key={w} className="wd">{w}</span>)}
        {days.map((d) => {
          const iso = toIsoDate(d)
          const disabled = iso < min
          return (
            <button
              key={iso}
              type="button"
              disabled={disabled}
              className={`mc-day${iso === value ? ' on' : ''}${d.getMonth() !== month.getMonth() ? ' other' : ''}`}
              onClick={() => onChange(iso)}
            >
              {d.getDate()}
            </button>
          )
        })}
      </div>
    </div>
  )
}

/* ===== Õhtu külalised ja kliendi registreerimine ===== */

function EventGuests({ e, onChanged }) {
  const tr = useTr()
  const [list, setList] = useState([])
  const [adding, setAdding] = useState(false)
  const [error, setError] = useState('')
  const load = useCallback(() => api(`/api/events/${e.id}/registrations`).then(setList).catch((err) => setError(err.message)), [e.id])

  useEffect(() => {
    load()
  }, [load])

  async function cancel(r) {
    if (!window.confirm(tr('Tühistada registreerimine? Makstud summa tagastatakse.', 'Отменить запись? Оплаченная сумма будет возвращена.', 'Cancel the registration? The paid amount is refunded.'))) return
    try {
      await api(`/api/events/registrations/${r.id}/cancel`, { method: 'PATCH', body: {} })
      load()
      onChanged()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="ae-guests">
      {error && <p className="alert alert-error">{error}</p>}
      {list.length === 0 && <p className="italic muted small">{tr('Registreerunuid veel pole.', 'Записавшихся пока нет.', 'No registrations yet.')}</p>}
      {list.map((r) => (
        <div key={r.id} className={r.status === 'Cancelled' ? 'dash-row muted' : 'dash-row'}>
          <span>
            <a href={`/personal/klient?q=${encodeURIComponent(r.customerEmail)}`}>{r.customerName}</a> · {r.guests} · {r.tableNumber ? `${tr('laud', 'стол', 'table')} ${r.tableNumber}` : '—'} · {money(r.total)}
            {' '}<span className={`badge badge-${r.status}`}>{r.status === 'Active' ? tr('makstud', 'оплачено', 'paid') : tr.status(r.status)}</span>
            {r.status === 'PendingPayment' && r.paymentToken && <span className="small"> · <a href={`/maksa/ohtu/${r.paymentToken}`}>{tr('makselink', 'ссылка на оплату', 'payment link')}</a></span>}
          </span>
          {r.status !== 'Cancelled' && <button className="btn btn-small btn-ox-line" onClick={() => cancel(r)}>{tr('Tühista', 'Отменить', 'Cancel')}</button>}
        </div>
      ))}
      {!adding && !e.registrationClosed && e.seatsLeft > 0 && (
        <button className="btn btn-small btn-dark" onClick={() => setAdding(true)}>+ {tr('Registreeri klient (telefoni teel)', 'Записать гостя (по звонку)', 'Register a guest (by phone)')}</button>
      )}
      {adding && <StaffSignup e={e} onDone={() => { setAdding(false); load(); onChanged() }} onCancel={() => setAdding(false)} />}
    </div>
  )
}

export function StaffSignup({ e, onDone, onCancel }) {
  const tr = useTr()
  const [search, setSearch] = useState('')
  const [found, setFound] = useState([])
  const [customer, setCustomer] = useState(null)
  const [isNew, setIsNew] = useState(false)
  const [nc, setNc] = useState({ name: '', email: '', phone: '', language: tr.lang })
  const [guests, setGuests] = useState(2)
  const [tables, setTables] = useState([])
  const [tableId, setTableId] = useState('')
  const [drinks, setDrinks] = useState({})
  const [result, setResult] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api(`/api/events/${e.id}/tables`).then(setTables).catch(() => {})
  }, [e.id])
  useEffect(() => {
    if (search.trim().length < 2) return setFound([])
    const timer = setTimeout(() => api('/api/customers' + qs({ search })).then(setFound).catch(() => {}), 250)
    return () => clearTimeout(timer)
  }, [search])

  const total = e.pricePerGuest * guests + e.items.reduce((s, i) => s + (drinks[i.id] ?? 0) * i.price, 0)

  async function save() {
    setError('')
    if (!customer && !isNew) return setError(tr('Valige klient või lisage uus.', 'Выберите гостя или добавьте нового.', 'Choose a guest or add a new one.'))
    try {
      const r = await api(`/api/events/${e.id}/join-for-customer`, {
        method: 'POST',
        body: {
          guests, tableId: tableId ? Number(tableId) : null, acceptRules: true,
          drinks: Object.entries(drinks).filter(([, q]) => q > 0).map(([itemId, quantity]) => ({ itemId: Number(itemId), quantity })),
          customerId: isNew ? null : customer.id,
          newCustomer: isNew ? { ...nc, phone: nc.phone || null } : null,
        },
      })
      setResult(r)
    } catch (err) {
      setError(err.message)
    }
  }

  if (result) {
    return (
      <div className="paper ae-signup">
        <p className="eyebrow ox">{tr('Ootab makset', 'Ожидает оплаты', 'Awaiting payment')}</p>
        <p className="small">{tr(
          'Kliendile saadeti SMS ja e-kiri makselingiga. Maksmata registreerimine tühistatakse 24 tundi enne õhtut.',
          'Гостю отправлены SMS и письмо со ссылкой на оплату. Неоплаченная запись отменяется за 24 часа до вечера.',
          'The guest got an SMS and e-mail with a payment link. Unpaid registrations are cancelled 24 hours before the evening.')}</p>
        {result.paymentToken && <p className="small"><a href={`/maksa/ohtu/${result.paymentToken}`}>{`${window.location.origin}/maksa/ohtu/${result.paymentToken}`}</a></p>}
        <button className="btn btn-small btn-dark" onClick={onDone}>OK</button>
      </div>
    )
  }

  return (
    <div className="paper ae-signup">
      <div className="chips">
        <button type="button" className={!isNew ? 'chip on' : 'chip'} onClick={() => setIsNew(false)}>{tr('Olemasolev klient', 'Существующий гость', 'Existing guest')}</button>
        <button type="button" className={isNew ? 'chip on' : 'chip'} onClick={() => setIsNew(true)}>{tr('Uus klient', 'Новый гость', 'New guest')}</button>
      </div>
      {!isNew && (
        <>
          <input value={search} onChange={(x) => setSearch(x.target.value)} placeholder={tr('Otsi nime, e-posti või telefoni järgi', 'Поиск по имени, e-mail или телефону', 'Search by name, e-mail or phone')} />
          <div className="cust-list">
            {found.map((c) => (
              <button key={c.id} type="button" className={customer?.id === c.id ? 'thread on' : 'thread'} onClick={() => setCustomer(c)}>
                <strong>{c.name}</strong> <span className="small muted">{c.email}</span>
              </button>
            ))}
          </div>
        </>
      )}
      {isNew && (
        <div className="field-row">
          <input value={nc.name} onChange={(x) => setNc({ ...nc, name: x.target.value })} placeholder={tr('Nimi', 'Имя', 'Name')} />
          <input value={nc.email} onChange={(x) => setNc({ ...nc, email: x.target.value })} placeholder="E-mail" />
          <input value={nc.phone} onChange={(x) => setNc({ ...nc, phone: x.target.value })} placeholder={tr('Telefon', 'Телефон', 'Phone')} />
          <select value={nc.language} onChange={(x) => setNc({ ...nc, language: x.target.value })} aria-label="language">
            <option value="et">Eesti</option><option value="ru">Русский</option><option value="en">English</option>
          </select>
        </div>
      )}
      <div className="field-row">
        <label className="field">{tr('Külalisi', 'Гостей', 'Guests')}<input type="number" min={1} max={8} value={guests} onChange={(x) => setGuests(Number(x.target.value))} /></label>
        <label className="field">{tr('Laud', 'Стол', 'Table')}
          <select value={tableId} onChange={(x) => setTableId(x.target.value)}>
            <option value="">—</option>
            {tables.filter((t) => !t.isTaken && t.seats >= guests).map((t) => <option key={t.id} value={t.id}>{t.number} · {t.seats} · {tr.zone(t.location)}</option>)}
          </select>
        </label>
      </div>
      {e.items.map((i) => (
        <div key={i.id} className="dash-row">
          <span className="small">{tr.t(`drink.${i.code}`, {}, i.name)} · {money(i.price)}</span>
          <input type="number" min={0} max={20} className="price" value={drinks[i.id] ?? 0} onChange={(x) => setDrinks({ ...drinks, [i.id]: Number(x.target.value) })} />
        </div>
      ))}
      <p className="small"><strong>{tr('Kokku', 'Итого', 'Total')}: {money(total)}</strong> · {tr('klient maksab lingi kaudu', 'гость оплатит по ссылке', 'the guest pays via link')}</p>
      {error && <p className="alert alert-error">{error}</p>}
      <div className="actions">
        <button className="btn btn-small btn-dark" onClick={save}>{tr('Registreeri ja saada makselink', 'Записать и отправить ссылку', 'Register and send link')}</button>
        <button className="btn btn-small" onClick={onCancel}>{tr('Loobu', 'Отмена', 'Cancel')}</button>
      </div>
    </div>
  )
}
