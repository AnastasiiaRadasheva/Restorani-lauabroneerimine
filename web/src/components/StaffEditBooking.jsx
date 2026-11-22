import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { useTr } from '../staff'
import { money } from '../utils'
import { ALLERGIES } from './booking/AllergyBlock'
import { useExtrasCatalog } from './booking/EveningDetails'
import { useMenu } from './booking/GastronomySection'

const OCCASIONS = ['None', 'Birthday', 'Anniversary', 'Proposal', 'Business']
const TABS = [
  ['time', 'Aeg ja laud', 'Время и стол', 'Time and table'],
  ['food', 'Eeltellimus', 'Предзаказ', 'Pre-order'],
  ['extras', 'Lisateenused', 'Доп. услуги', 'Extras'],
  ['wishes', 'Soovid ja allergiad', 'Пожелания и аллергии', 'Wishes and allergies'],
]

/**
 * Personal muudab broneeringut külalise palvel: aeg ja laud, eeltellimus, lisateenused, soovid.
 * Hiljem lisatud road ja teenused makstakse kohapeal (kaarti pole vaja).
 */
export default function StaffEditBooking({ r, onSaved, onClose }) {
  const tr = useTr()
  const [tab, setTab] = useState('time')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function save(request, okText) {
    setBusy(true)
    setError('')
    try {
      const updated = await request()
      onSaved(updated, okText)
    } catch (e) {
      setError(e.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="paper white details-card staff-edit">
      <div className="row-between">
        <p className="eyebrow">{tr('Muuda külalise palvel', 'Изменить по просьбе гостя', "Change at the guest's request")}</p>
        <button type="button" className="btn btn-small" onClick={onClose}>{tr('Sulge', 'Закрыть', 'Close')}</button>
      </div>
      <div className="chips">
        {TABS.map(([key, et, ru, en]) => (
          <button key={key} type="button" className={tab === key ? 'chip caps on' : 'chip caps'} onClick={() => { setTab(key); setError('') }}>{tr(et, ru, en)}</button>
        ))}
      </div>
      {tab === 'time' && <TimeTab r={r} busy={busy} save={save} />}
      {tab === 'food' && <FoodTab r={r} busy={busy} save={save} />}
      {tab === 'extras' && <ExtrasTab r={r} busy={busy} save={save} />}
      {tab === 'wishes' && <WishesTab r={r} busy={busy} save={save} />}
      {error && <p className="alert alert-error">{error}</p>}
    </div>
  )
}

function TimeTab({ r, busy, save }) {
  const tr = useTr()
  const [date, setDate] = useState(r.startTime.slice(0, 10))
  const [time, setTime] = useState(r.startTime.slice(11, 16))
  const [guests, setGuests] = useState(r.guests)
  const [tableId, setTableId] = useState(r.tableId)
  const [slots, setSlots] = useState([])
  const [tables, setTables] = useState([])

  useEffect(() => {
    api('/api/tables/slots' + qs({ restaurantId: r.restaurantId, from: date, days: 1, guests }))
      .then((d) => setSlots(d[0]?.slots ?? []))
      .catch(() => setSlots([]))
  }, [r.restaurantId, date, guests])
  useEffect(() => {
    api('/api/tables/available' + qs({ restaurantId: r.restaurantId, date, time, guests }))
      .then((a) => setTables(a.tables.filter((t) => t.fitsGuests && (t.isFree || t.id === r.tableId))))
      .catch(() => setTables([]))
  }, [r.restaurantId, r.tableId, date, time, guests])

  const times = [...new Set([time, ...slots.map((s) => s.time)])].sort()
  return (
    <div className="se-form">
      <div className="field-row">
        <label className="field">{tr('Kuupäev', 'Дата', 'Date')}<input type="date" value={date} onChange={(e) => setDate(e.target.value)} /></label>
        <label className="field">{tr('Kellaaeg', 'Время', 'Time')}
          <select value={time} onChange={(e) => setTime(e.target.value)}>
            {times.map((t) => {
              const s = slots.find((x) => x.time === t)
              return <option key={t} value={t}>{t}{s ? ` · ${s.freeTables} ${tr('vaba', 'своб.', 'free')}` : ''}</option>
            })}
          </select>
        </label>
        <label className="field">{tr('Külalisi', 'Гостей', 'Guests')}<input type="number" min={1} max={20} value={guests} onChange={(e) => setGuests(Number(e.target.value))} /></label>
        <label className="field">{tr('Laud', 'Стол', 'Table')}
          <select value={tableId ?? ''} onChange={(e) => setTableId(e.target.value ? Number(e.target.value) : null)}>
            <option value="">{tr('Vali automaatselt', 'Подобрать автоматически', 'Pick automatically')}</option>
            {tables.map((t) => <option key={t.id} value={t.id}>{tr('Laud', 'Стол', 'Table')} {t.number} · {t.seats} {tr('kohta', 'мест', 'seats')}{t.id === r.tableId ? tr(' (praegune)', ' (текущий)', ' (current)') : ''}</option>)}
          </select>
        </label>
      </div>
      <button type="button" className="btn btn-dark" disabled={busy} onClick={() => save(
        () => api(`/api/reservations/${r.id}`, { method: 'PUT', body: { date, time, guests, tableId, specialRequest: r.specialRequest } }),
        tr('Aeg ja laud muudetud.', 'Время и стол изменены.', 'Time and table changed.'),
      )}>{tr('Salvesta', 'Сохранить', 'Save')}</button>
    </div>
  )
}

function FoodTab({ r, busy, save }) {
  const tr = useTr()
  const menu = useMenu(r.restaurantId)
  const [qty, setQty] = useState(() => Object.fromEntries(r.preOrder.map((p) => [p.menuItemId, p.quantity])))
  const change = (id, d) => setQty((q) => ({ ...q, [id]: Math.max(0, Math.min(50, (q[id] ?? 0) + d)) }))
  const total = menu.reduce((s, m) => s + (qty[m.id] ?? 0) * m.price, 0)
  const before = r.preOrderTotal
  const diff = total - before
  return (
    <div className="se-form">
      <div className="se-menu">
        {menu.map((m) => (
          <div key={m.id} className={qty[m.id] ? 'pm-item on' : 'pm-item'}>
            {m.imageUrl && <img src={m.imageUrl} alt="" />}
            <span className="name">{m.name}<span className="small muted d-block">{money(m.price)}</span></span>
            <div className="stepper round">
              <button type="button" onClick={() => change(m.id, -1)}>−</button>
              <span className="value">{qty[m.id] ?? 0}</span>
              <button type="button" onClick={() => change(m.id, 1)}>+</button>
            </div>
          </div>
        ))}
      </div>
      <p className="small">
        {tr('Eeltellimus', 'Предзаказ', 'Pre-order')}: <strong>{money(total)}</strong>
        {diff > 0 && <span className="ox"> · +{money(diff)} {tr('makstakse kohapeal', 'оплата на месте', 'paid on site')}</span>}
        {diff < 0 && <span> · {money(diff)}</span>}
      </p>
      <button type="button" className="btn btn-dark" disabled={busy || diff === 0} onClick={() => save(
        () => api(`/api/reservations/${r.id}/preorder`, { method: 'POST', body: { items: Object.entries(qty).filter(([, q]) => q > 0).map(([id, q]) => ({ menuItemId: Number(id), quantity: q })) } }),
        tr('Eeltellimus muudetud.', 'Предзаказ изменён.', 'Pre-order changed.'),
      )}>{tr('Salvesta', 'Сохранить', 'Save')}</button>
    </div>
  )
}

function ExtrasTab({ r, busy, save }) {
  const tr = useTr()
  const catalog = useExtrasCatalog()
  const [codes, setCodes] = useState(() => r.extras.map((x) => x.code))
  const toggle = (c) => setCodes(codes.includes(c) ? codes.filter((x) => x !== c) : [...codes, c])
  return (
    <div className="se-form">
      <div className="chips">
        {catalog.map((x) => (
          <button key={x.code} type="button" className={codes.includes(x.code) ? 'chip on' : 'chip'} onClick={() => toggle(x.code)}>
            {tr.t(`extra.${x.code}`, {}, x.name)} · {x.price ? money(x.price) : tr('tasuta', 'бесплатно', 'free')}
          </button>
        ))}
      </div>
      <p className="small muted">{tr('Lisatud teenused makstakse kohapeal.', 'Добавленные услуги оплачиваются на месте.', 'Added extras are paid on site.')}</p>
      <button type="button" className="btn btn-dark" disabled={busy} onClick={() => save(
        () => api(`/api/reservations/${r.id}/extras`, { method: 'PUT', body: { extras: codes } }),
        tr('Lisateenused muudetud.', 'Доп. услуги изменены.', 'Extras changed.'),
      )}>{tr('Salvesta', 'Сохранить', 'Save')}</button>
    </div>
  )
}

function WishesTab({ r, busy, save }) {
  const tr = useTr()
  const [allergies, setAllergies] = useState(r.allergies ?? [])
  const [note, setNote] = useState(r.allergyNote ?? '')
  const [request, setRequest] = useState(r.specialRequest ?? '')
  const [occasion, setOccasion] = useState(r.occasion ?? 'None')
  const toggle = (a) => setAllergies(allergies.includes(a) ? allergies.filter((x) => x !== a) : [...allergies, a])
  return (
    <div className="se-form">
      <label className="field">{tr('Tähtpäev', 'Повод', 'Occasion')}
        <select value={occasion} onChange={(e) => setOccasion(e.target.value)}>
          {OCCASIONS.map((o) => <option key={o} value={o}>{o === 'None' ? '—' : tr.t(`occasion.${o}`)}</option>)}
        </select>
      </label>
      <p className="eyebrow">{tr('Allergiad', 'Аллергии', 'Allergies')}</p>
      <div className="chips">
        {ALLERGIES.map((a) => (
          <button key={a} type="button" className={allergies.includes(a) ? 'chip on' : 'chip'} onClick={() => toggle(a)}>{tr.t(`allergy.${a}`)}</button>
        ))}
      </div>
      <label className="field">{tr('Märkus köögile', 'Заметка для кухни', 'Note for the kitchen')}<textarea rows={2} maxLength={300} value={note} onChange={(e) => setNote(e.target.value)} /></label>
      <label className="field">{tr('Soovid', 'Пожелания', 'Requests')}<textarea rows={2} maxLength={500} value={request} onChange={(e) => setRequest(e.target.value)} /></label>
      <button type="button" className="btn btn-dark" disabled={busy} onClick={() => save(
        () => api(`/api/reservations/${r.id}/details`, { method: 'PUT', body: { allergies, allergyNote: note, specialRequest: request, occasion } }),
        tr('Soovid salvestatud.', 'Пожелания сохранены.', 'Wishes saved.'),
      )}>{tr('Salvesta', 'Сохранить', 'Save')}</button>
    </div>
  )
}
