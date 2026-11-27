import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { useTr } from '../staff'
import { addDays, toIsoDate } from '../utils'

const PERIODS = [
  ['30', 30, 'Viimased 30 päeva', 'Последние 30 дней', 'Last 30 days'],
  ['90', 90, 'Viimased 90 päeva', 'Последние 90 дней', 'Last 90 days'],
  ['180', 180, 'Viimased 6 kuud', 'Последние 6 месяцев', 'Last 6 months'],
]

/**
 * Admin: statistika – hõivatumad nädalapäevad, kellaajad ja kuupäevad (GET /api/stats).
 * Tulbad näitavad broneeringute arvu; kõige hõivatum päev ja kellaaeg on esile tõstetud.
 */
export default function AdminStats({ restaurantId, restaurants }) {
  const tr = useTr()
  const [all, setAll] = useState(false)
  const [period, setPeriod] = useState('90')
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    const days = PERIODS.find((p) => p[0] === period)[1]
    const today = new Date()
    setError('')
    api('/api/stats' + qs({ restaurantId: all ? undefined : restaurantId, from: toIsoDate(addDays(today, -days)), to: toIsoDate(addDays(today, 14)) }))
      .then(setData)
      .catch((e) => setError(e.message))
  }, [restaurantId, all, period])

  // Nädalapäevade nimed valitud keeles (server annab eesti keeles, esmaspäevast alates)
  const weekday = (i) => new Date(2026, 10, 16 + i).toLocaleDateString(tr.lang, { weekday: 'long' })
  const dateLabel = (iso) => new Date(iso).toLocaleDateString(tr.lang, { weekday: 'short', day: 'numeric', month: 'long' })
  const name = restaurants.find((r) => r.id === restaurantId)?.name

  return (
    <div className="paper white admin-stats">
      <div className="row-between">
        <p className="card-title">{tr('Statistika', 'Статистика', 'Statistics')} · {all ? tr('kõik restoranid', 'все рестораны', 'all restaurants') : name}</p>
        <label className="check small"><input type="checkbox" checked={all} onChange={(e) => setAll(e.target.checked)} /> {tr('Kõik restoranid', 'Все рестораны', 'All restaurants')}</label>
      </div>
      <div className="chips">
        {PERIODS.map(([key, , et, ru, en]) => (
          <button key={key} type="button" className={period === key ? 'chip small on' : 'chip small'} onClick={() => setPeriod(key)}>{tr(et, ru, en)}</button>
        ))}
        <span className="small muted">{tr('+ broneeringud järgmiseks 14 päevaks', '+ брони на ближайшие 14 дней', '+ bookings for the next 14 days')}</span>
      </div>
      {error && <p className="alert alert-error">{error}</p>}
      {!data && !error && <p className="loading">…</p>}
      {data && (
        <>
          <div className="stats-tiles">
            <Tile label={tr('Broneeringuid', 'Броней', 'Bookings')} value={data.totalReservations} />
            <Tile label={tr('Külalisi', 'Гостей', 'Guests')} value={data.totalGuests} />
            <Tile label={tr('Tuli', 'Пришли', 'Arrived')} value={data.arrived} />
            <Tile label={tr('Ei tulnud', 'Не пришли', 'No-shows')} value={`${data.noShow} · ${data.noShowPercent} %`} />
            <Tile label={tr('Tulemas', 'Ещё придут', 'Upcoming')} value={data.upcoming} />
            <Tile label={tr('Tühistatud', 'Отменены', 'Cancelled')} value={data.cancelled} />
          </div>
          <div className="stats-grid">
            <Bars title={tr('Nädalapäevad', 'Дни недели', 'Weekdays')} rows={data.byWeekday.map((r, i) => ({ ...r, label: weekday(i) }))} tr={tr} />
            <Bars title={tr('Kellaajad', 'Время', 'Times')} rows={data.byHour} tr={tr} />
          </div>
          <p className="eyebrow">{tr('Hõivatumad kuupäevad', 'Самые загруженные даты', 'Busiest dates')}</p>
          <ol className="stats-dates">
            {data.busiestDates.slice(0, 5).map((d) => (
              <li key={d.label}><span>{dateLabel(d.label)}</span><span className="muted">{d.reservations} {tr('broneeringut', 'броней', 'bookings')} · {d.guests} {tr('külalist', 'гостей', 'guests')}</span></li>
            ))}
          </ol>
        </>
      )}
    </div>
  )
}

function Tile({ label, value }) {
  return (
    <div className="stats-tile">
      <span className="eyebrow">{label}</span>
      <span className="big">{value}</span>
    </div>
  )
}

/** Horisontaalsed tulbad: väärtus kirjas tulba kõrval; kõige suurem on tumedam. */
function Bars({ title, rows, tr }) {
  const max = Math.max(1, ...rows.map((r) => r.reservations))
  return (
    <div className="stats-bars">
      <p className="eyebrow">{title}</p>
      {rows.map((r) => (
        <div key={r.label} className={r.reservations === max ? 'bar-row top' : 'bar-row'} title={`${r.reservations} ${tr('broneeringut', 'броней', 'bookings')} · ${r.guests} ${tr('külalist', 'гостей', 'guests')}`}>
          <span className="bar-label">{r.label}</span>
          <span className="bar-track"><span className="bar-fill" style={{ width: `${(r.reservations / max) * 100}%` }} /></span>
          <span className="bar-value">{r.reservations}</span>
        </div>
      ))}
    </div>
  )
}
