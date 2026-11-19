import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, qs } from '../api'
import Pager from '../components/Pager'
import { fmtDate, fmtTime, STATUS_TEXT } from '../staff'
import { money } from '../utils'

/** Kõik broneeringud: otsing, filter, sortimine, leheküljed. */
export default function StaffReservations({ renderRowExtra }) {
  const [restaurants, setRestaurants] = useState([])
  const [f, setF] = useState({ restaurantId: '', date: '', status: '', search: '', sort: 'date', desc: true, page: 1, pageSize: 10 })
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
          <h1>Broneeringud</h1>
        </div>
        <p>Otsing nime, e-posti või telefoni järgi. Klõpsake veeru pealkirjal, et sortida.</p>
      </div>
      {error && <p className="alert alert-error">{error}</p>}

      <div className="filters">
        <input className="grow" placeholder="Otsi…" value={f.search} onChange={set('search')} />
        <select value={f.restaurantId} onChange={set('restaurantId')}>
          <option value="">Kõik restoranid</option>
          {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
        <input type="date" value={f.date} onChange={set('date')} />
        <select value={f.status} onChange={set('status')}>
          <option value="">Kõik olekud</option>
          {Object.entries(STATUS_TEXT).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th className="sortable" onClick={() => sortBy('date')}>Aeg{arrow('date')}</th>
              <th>Restoran</th>
              <th className="sortable" onClick={() => sortBy('table')}>Laud{arrow('table')}</th>
              <th className="sortable" onClick={() => sortBy('name')}>Klient{arrow('name')}</th>
              <th className="sortable" onClick={() => sortBy('guests')}>In.{arrow('guests')}</th>
              <th>Eeltellimus</th>
              <th>Olek</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data?.items.map((r) => (
              <tr key={r.id}>
                <td data-label="Aeg">{fmtDate(r.startTime)} {fmtTime(r.startTime)}</td>
                <td data-label="Restoran">{r.restaurantName}</td>
                <td data-label="Laud">{r.tableNumber}</td>
                <td data-label="Klient">{r.customerName}<div className="muted small">{r.customerPhone}</div></td>
                <td data-label="Inimesi">{r.guests}</td>
                <td data-label="Eeltellimus">{r.preOrder.length ? `${money(r.preOrderTotal)}${r.isPaid ? ' ✓' : ''}` : '—'}</td>
                <td data-label="Olek"><span className={`badge badge-${r.status}`}>{STATUS_TEXT[r.status]}</span></td>
                <td className="actions">
                  {r.status === 'Active' && (
                    <>
                      <button className="btn btn-small btn-dark" onClick={() => setStatus(r.id, 'Arrived')}>Saabus</button>
                      <button className="btn btn-small" onClick={() => setStatus(r.id, 'NoShow')}>Ei tulnud</button>
                    </>
                  )}
                  <Link to={`/broneering/${r.id}`} className="btn btn-small">Ava</Link>
                  {renderRowExtra && renderRowExtra(r)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data?.items.length === 0 && <p className="italic muted">Broneeringuid ei leitud.</p>}
      {data && <Pager {...data} onChange={(page) => setF({ ...f, page })} />}
    </div>
  )
}
