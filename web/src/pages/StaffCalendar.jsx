import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import StaffMessages from '../components/StaffMessages'
import DayBroadcast from '../components/DayBroadcast'
import { Link } from 'react-router-dom'
import { fmtTime, useTr } from '../staff'
import { addDays, money, toIsoDate } from '../utils'


/** Personal: kuu kalender. Päeval klõpsates on näha, kes broneeris, ja neile saab kirjutada. */
export default function StaffCalendar() {
  const tr = useTr()
  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantId] = useState('')
  const [month, setMonth] = useState(() => {
    const d = new Date()
    return new Date(d.getFullYear(), d.getMonth(), 1)
  })
  const [days, setDays] = useState([])
  const [selected, setSelected] = useState(toIsoDate(new Date()))
  const [list, setList] = useState([])
  const [error, setError] = useState('')

  // Kalender algab esmaspäevast
  const first = addDays(month, -((month.getDay() + 6) % 7))

  useEffect(() => {
    api('/api/restaurants').then(setRestaurants).catch(() => {})
  }, [])

  useEffect(() => {
    api('/api/reservations/calendar' + qs({ restaurantId, from: toIsoDate(first), days: 42 }))
      .then(setDays)
      .catch((e) => setError(e.message))
  }, [restaurantId, month]) // eslint-disable-line react-hooks/exhaustive-deps

  const [refresh, setRefresh] = useState(0)
  useEffect(() => {
    api('/api/reservations' + qs({ restaurantId, date: selected, pageSize: 100, sort: 'date', desc: false }))
      .then((p) => setList(p.items.filter((r) => r.status !== 'Cancelled')))
      .catch((e) => setError(e.message))
  }, [restaurantId, selected, refresh])

  const shift = (n) => setMonth(new Date(month.getFullYear(), month.getMonth() + n, 1))

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{tr('Kalender', 'Календарь', 'Calendar')}</h1>
        </div>
        <select value={restaurantId} onChange={(e) => setRestaurantId(e.target.value)}>
          <option value="">{tr('Kõik restoranid', 'Все рестораны', 'All restaurants')}</option>
          {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
      </div>
      {error && <p className="alert alert-error">{error}</p>}

      <div className="cal-wrap">
        <div className="paper white">
          <div className="row-between">
            <button className="btn btn-small" onClick={() => shift(-1)}>‹</button>
            <span className="card-title">{month.toLocaleDateString(tr.lang, { month: 'long', year: 'numeric' })}</span>
            <button className="btn btn-small" onClick={() => shift(1)}>›</button>
          </div>
          <div className="staff-cal">
            {tr(['E', 'T', 'K', 'N', 'R', 'L', 'P'], ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'], ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su']).map((w) => <span key={w} className="wd">{w}</span>)}
            {days.map((d) => {
              const date = new Date(d.date)
              const other = date.getMonth() !== month.getMonth()
              return (
                <button
                  key={d.date}
                  type="button"
                  className={`sc-day${d.date === selected ? ' on' : ''}${other ? ' other' : ''}${d.important ? ' important' : ''}`}
                  onClick={() => setSelected(d.date)}
                >
                  <span className="n">{date.getDate()}</span>
                  {d.reservations > 0 && <span className="c">{d.reservations} {tr('br.', 'бр.', 'bk.')} · {d.guests} {tr('in.', 'гост.', 'gst.')}</span>}
                  {d.important > 0 && <span className="i">◆ {d.important}</span>}
                </button>
              )
            })}
          </div>
          <p className="muted small">{tr('◆ = olulised broneeringud (tähtpäev).', '◆ = важные брони (праздник).', '◆ = important bookings (occasion).')}</p>
        </div>

        <div className="day-list">
          <p className="card-title">{new Date(selected).toLocaleDateString(tr.lang, { weekday: 'long', day: 'numeric', month: 'long' })}</p>
          <DayBroadcast date={selected} restaurantId={restaurantId} count={list.filter((r) => ['Active', 'PendingPayment', 'Arrived'].includes(r.status)).length} onSent={() => setRefresh((x) => x + 1)} />
          {list.length === 0 && <p className="italic muted">{tr('Sel päeval broneeringuid pole.', 'В этот день броней нет.', 'No bookings on this day.')}</p>}
          {list.map((r) => (
            <div key={r.id} className={r.occasion !== 'None' ? 'person important' : 'person'}>
              <div className="row-between">
                <strong>{fmtTime(r.startTime)} · <Link to={`/personal/klient?q=${encodeURIComponent(r.customerEmail)}`}>{r.customerName}</Link></strong>
                <span className={`badge badge-${r.status}`}>{tr.status(r.status)}</span>
              </div>
              <span className="small">
                {r.restaurantName} · {tr('laud', 'стол', 'table')} {r.tableNumber} ({tr.zone(r.tableLocation)}) · {r.guests} {tr('in.', 'гост.', 'gst.')}
                {r.total > 0 ? ` · ${money(r.total)}${r.isPaid ? tr(' makstud', ' оплачено', ' paid') : ''}` : ''}
              </span>
              <span className="small muted">{r.customerPhone ?? '—'} · {r.customerEmail}</span>
              {r.occasion !== 'None' && <span className="small ox">◆ {tr.t(`occasion.${r.occasion}`)}</span>}
              {r.allergies.length > 0 && <span className="small ox">{tr('Allergiad', 'Аллергии', 'Allergies')}: {r.allergies.map((a) => tr.t(`allergy.${a}`)).join(', ')}</span>}
              {r.allergyNote && <span className="small italic">{r.allergyNote}</span>}
              {r.specialRequest && <span className="small italic">{tr('Soovid', 'Пожелания', 'Requests')}: {r.specialRequest}</span>}
              <div className="actions"><StaffMessages r={r} /></div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
