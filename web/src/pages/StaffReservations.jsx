import { useCallback, useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { api, qs } from '../api'
import Pager from '../components/Pager'
import { fmtDate, fmtTime, STATUS_TEXT, useTr } from '../staff'
import { addDays, money, toIsoDate } from '../utils'

/** Kõik broneeringud: otsing, filter, sortimine, leheküljed. */
/** Tänase päeva lõpp (saabumist saab märkida ainult tänaseni). */
const endOfToday = () => { const d = new Date(); d.setHours(23, 59, 59, 999); return d }

export default function StaffReservations({ renderRowExtra }) {
  const tr = useTr()
  const [params] = useSearchParams()
  const [restaurants, setRestaurants] = useState([])
  // ?date=… tuleb kalendrist ("Päeva broneeringud nimekirjas")
  const [f, setF] = useState(() => {
    const date = params.get('date') ?? ''
    return { restaurantId: '', date, status: '', search: '', sort: 'date', desc: !date, page: 1, pageSize: 10 }
  })
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api('/api/restaurants').then(setRestaurants).catch((e) => setError(e.message))
  }, [])

  const load = useCallback(() => {
    api('/api/reservations' + qs(f))
      .then((d) => {
        setData(d)
        setError('')
      })
      .catch((e) => setError(e.message))
  }, [f])

  useEffect(() => {
    const t = setTimeout(load, 250)
    return () => clearTimeout(t)
  }, [load])

  const set = (field) => (e) => setF({ ...f, [field]: e.target.value, page: 1 })
  const sortBy = (field) => setF({ ...f, sort: field, desc: f.sort === field ? !f.desc : false, page: 1 })
  const arrow = (field) => (f.sort === field ? (f.desc ? ' ↓' : ' ↑') : '')

  async function setStatus(id, status) {
    try {
      await api(`/api/reservations/${id}/status`, { method: 'PATCH', body: { status } })
      load()
    } catch (e) {
      setError(e.message)
    }
  }

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{tr('Broneeringud', 'Брони', 'Bookings')}</h1>
        </div>
        <p>{tr('Otsing nime, e-posti või telefoni järgi. Klõpsake veeru pealkirjal, et sortida.', 'Поиск по имени, e-mail или телефону. Нажмите на заголовок столбца для сортировки.', 'Search by name, e-mail or phone. Click a column header to sort.')}</p>
      </div>
      {error && <p className="alert alert-error">{error}</p>}

      <div className="chips">
        {[[tr('Täna', 'Сегодня', 'Today'), 0], [tr('7 päeva', '7 дней', '7 days'), 6], [tr('30 päeva', '30 дней', '30 days'), 29]].map(([label, n]) => (
          <button
            key={label}
            type="button"
            className={f.from === toIsoDate(new Date()) && f.to === toIsoDate(addDays(new Date(), n)) ? 'chip caps on' : 'chip caps'}
            onClick={() => setF({ ...f, date: '', from: toIsoDate(new Date()), to: toIsoDate(addDays(new Date(), n)), sort: 'date', desc: false, page: 1 })}
          >
            {tr('Lähimad', 'Ближайшие', 'Upcoming')}: {label}
          </button>
        ))}
        <button type="button" className="chip caps" onClick={() => setF({ ...f, from: '', to: '', desc: true, page: 1 })}>{tr('Kõik', 'Все', 'All')}</button>
        <Link className="chip caps chip-link chip-right" to={'/personal/kalender' + (f.date ? `?date=${f.date}` : '')}>
          {tr('Vaata kalendris', 'Открыть в календаре', 'Open in calendar')} →
        </Link>
      </div>

      <div className="filters">
        <input className="grow" placeholder={tr('Otsi…', 'Поиск…', 'Search…')} value={f.search} onChange={set('search')} />
        <select value={f.restaurantId} onChange={set('restaurantId')}>
          <option value="">{tr('Kõik restoranid', 'Все рестораны', 'All restaurants')}</option>
          {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
        <input type="date" value={f.date} onChange={set('date')} />
        <select value={f.status} onChange={set('status')}>
          <option value="">{tr('Kõik olekud', 'Все статусы', 'All statuses')}</option>
          {Object.keys(STATUS_TEXT).map((k) => <option key={k} value={k}>{tr.status(k)}</option>)}
        </select>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th className="sortable" onClick={() => sortBy('date')}>{tr('Aeg', 'Время', 'Time')}{arrow('date')}</th>
              <th>{tr('Restoran', 'Ресторан', 'Restaurant')}</th>
              <th className="sortable" onClick={() => sortBy('table')}>{tr('Laud', 'Стол', 'Table')}{arrow('table')}</th>
              <th className="sortable" onClick={() => sortBy('name')}>{tr('Klient', 'Гость', 'Guest')}{arrow('name')}</th>
              <th className="sortable" onClick={() => sortBy('guests')}>{tr('In.', 'Гост.', 'Gst.')}{arrow('guests')}</th>
              <th>{tr('Eeltellimus', 'Предзаказ', 'Pre-order')}</th>
              <th>{tr('Olek', 'Статус', 'Status')}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data?.items.map((r) => (
              <tr key={r.id}>
                <td data-label="Aeg">{fmtDate(r.startTime)} {fmtTime(r.startTime)}</td>
                <td data-label="Restoran">{r.restaurantName}</td>
                <td data-label="Laud">{r.tableNumber}</td>
                <td data-label="Klient">
                  {r.occasion !== 'None' && <span className="ox">◆ </span>}<Link to={`/personal/klient?q=${encodeURIComponent(r.customerEmail)}`}>{r.customerName}</Link>
                  <div className="muted small">{r.customerPhone} · {r.customerEmail}</div>
                  {r.allergies?.length > 0 && <div className="small ox">{tr('Allergiad', 'Аллергии', 'Allergies')}: {r.allergies.map((a) => tr.t(`allergy.${a}`)).join(', ')}</div>}
                </td>
                <td data-label="Inimesi">{r.guests}</td>
                <td data-label="Eeltellimus">{r.preOrder.length ? `${money(r.preOrderTotal)}${r.isPaid ? ' ✓' : ''}` : '—'}</td>
                <td data-label="Olek"><span className={`badge badge-${r.status}`}>{tr.status(r.status)}</span></td>
                <td className="actions">
                  <div className="row-actions">
                  {/* Saabus / Ei tulnud – saab ümber vahetada, kui märgiti valesti (ainult külastuse päeval või hiljem) */}
                  {['Active', 'Arrived', 'NoShow'].includes(r.status) && new Date(r.startTime) <= endOfToday() && (
                    <>
                      {r.status !== 'Arrived' && <button className="btn btn-small btn-dark" onClick={() => setStatus(r.id, 'Arrived')}>{tr('Saabus', 'Пришли', 'Arrived')}</button>}
                      {r.status !== 'NoShow' && <button className="btn btn-small" onClick={() => setStatus(r.id, 'NoShow')}>{tr('Ei tulnud', 'Не пришли', 'No-show')}</button>}
                    </>
                  )}
                  <Link className="btn btn-small" to={`/broneering/${r.id}`}>{tr('Ava', 'Открыть', 'Open')}</Link>
                  {renderRowExtra && renderRowExtra(r)}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data?.items.length === 0 && <p className="italic muted">{tr('Broneeringuid ei leitud.', 'Брони не найдены.', 'No bookings found.')}</p>}
      {data && <Pager {...data} onChange={(page) => setF({ ...f, page })} />}
    </div>
  )
}
