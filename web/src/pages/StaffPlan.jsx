import { useCallback, useEffect, useState } from 'react'
import { api, qs } from '../api'
import FloorPlan, { FloorLegend } from '../components/FloorPlan'
import { useTableSocket } from '../realtime'
import { EDGE_TEXT, fmtTime, LOCATION_TEXT, nowSlot, STATUS_TEXT, timeSlots, ZONE_LIST } from '../staff'
import { toIsoDate } from '../utils'

/** Päeva laudade plaan teenindajale. */
export default function StaffPlan() {
  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantId] = useState(1)
  const [date, setDate] = useState(toIsoDate(new Date()))
  const [time, setTime] = useState(nowSlot())
  const [zone, setZone] = useState('Hall')
  const [plan, setPlan] = useState(null)
  const [selected, setSelected] = useState(null)
  const [error, setError] = useState('')
  const [toast, setToast] = useState(null)
  const [selecting, setSelecting] = useState({}) // tableId -> aeg, kui keegi valib lauda

  useEffect(() => {
    api('/api/restaurants').then(setRestaurants).catch((e) => setError(e.message))
  }, [])

  const load = useCallback(() => {
    return api('/api/tables/plan' + qs({ restaurantId, date }))
      .then((p) => {
        setPlan(p)
        setError('')
      })
      .catch((e) => setError(e.message))
  }, [restaurantId, date])

  useEffect(() => {
    load()
  }, [load])

  // Reaalajas: uus broneering või laua oleku muutus -> plaan uueneb kohe
  useTableSocket((msg) => {
    if (msg.restaurantId !== Number(restaurantId)) return
    if (msg.type === 'TableSelecting') {
      setSelecting((s) => ({ ...s, [msg.tableId]: msg.selecting ? Date.now() : undefined }))
      return
    }
    if (!msg.startTime?.startsWith(date)) return
    load()
    if (msg.type === 'ReservationCreated') {
      setToast(`Uus broneering: laud ${msg.tableNumber}, ${fmtTime(msg.startTime)}, ${msg.customerName} (${msg.guests} in.)`)
      setTimeout(() => setToast(null), 6000)
    }
  })

  // "Keegi valib praegu" kaob 30 sekundi pärast
  useEffect(() => {
    const t = setInterval(() => {
      setSelecting((s) => Object.fromEntries(Object.entries(s).filter(([, v]) => v && Date.now() - v < 30000)))
    }, 5000)
    return () => clearInterval(t)
  }, [])

  async function setStatus(reservationId, status) {
    try {
      await api(`/api/reservations/${reservationId}/status`, { method: 'PATCH', body: { status } })
      load()
    } catch (e) {
      setError(e.message)
    }
  }

  // Laua olek valitud kellaajal
  const at = new Date(`${date}T${time}:00`)
  const active = (r) => r.status === 'Active' || r.status === 'Arrived'
  const tables = (plan?.tables ?? []).map(({ table, reservations }) => {
    const now = reservations.find((r) => active(r) && new Date(r.startTime) <= at && new Date(r.endTime) > at)
    return {
      ...table,
      reservations,
      current: now,
      state: table.id === selected ? 'selected' : !now ? (selecting[table.id] ? 'hold' : 'free') : now.status === 'Arrived' ? 'arrived' : 'busy',
      clickable: true,
    }
  })
  const selectedTable = tables.find((t) => t.id === selected)

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>Laudade plaan <span className="live">reaalajas</span></h1>
        </div>
        <p>Klõpsake laual, et näha selle päeva broneeringuid ja märkida külaline saabunuks.</p>
      </div>
      {error && <p className="alert alert-error">{error}</p>}
      {toast && <div className="toast" role="status">{toast}</div>}

      <div className="filters">
        <select value={restaurantId} onChange={(e) => setRestaurantId(Number(e.target.value))}>
          {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        <select value={time} onChange={(e) => setTime(e.target.value)}>
          {timeSlots().map((s) => <option key={s}>{s}</option>)}
        </select>
      </div>

      {plan && (
        <div className="plan-wrap">
          <div className="plan-col">
            <div className="zone-tabs" role="tablist">
              {ZONE_LIST.filter((z) => tables.some((tb) => tb.location === z)).map((z) => (
                <button key={z} type="button" role="tab" aria-selected={zone === z} className={zone === z ? 'on' : ''} onClick={() => setZone(z)}>
                  {LOCATION_TEXT[z]}
                </button>
              ))}
            </div>
            <FloorPlan
              tables={tables}
              zone={zone}
              bar={{ x: plan.restaurant.barX, y: plan.restaurant.barY, width: plan.restaurant.barWidth, height: plan.restaurant.barHeight }}
              edgeLabel={EDGE_TEXT[zone]}
              entranceLabel={zone === 'Upstairs' ? 'Trepp' : 'Sissepääs'}
              barLabel="Baar"
              seatsLabel={(n) => `${n} kohta`}
              onSelect={(t) => setSelected(t.id)}
              renderInfo={(t) => t.current && <em className="guest">{t.current.customerName.split(' ')[0]}</em>}
            />
          </div>

          <aside className="plan-side">
            <FloorLegend items={[['free', 'vaba'], ['hold', 'keegi valib praegu'], ['busy', 'broneeritud'], ['arrived', 'külaline kohal'], ['selected', 'valitud']]} />
            <div className="paper white side-card">
              {!selectedTable && <p className="italic muted">Valige plaanilt laud.</p>}
              {selectedTable && (
                <>
                  <p className="card-title">Laud {selectedTable.number}</p>
                  <p className="muted small">{selectedTable.seats} kohta{selectedTable.spot ? ` · ${selectedTable.spot}` : ''}</p>
                  {selectedTable.reservations.length === 0 && <p className="italic">Sel päeval broneeringuid pole.</p>}
                  {selectedTable.reservations.map((r) => (
                    <div key={r.id} className="side-res">
                      <div className="row-between">
                        <strong>{fmtTime(r.startTime)}–{fmtTime(r.endTime)}</strong>
                        <span className={`badge badge-${r.status}`}>{STATUS_TEXT[r.status]}</span>
                      </div>
                      <div>{r.customerName} · {r.guests} in.{r.hasPreOrder && ' · eeltellimus'}</div>
                      {r.status === 'Active' && (
                        <div className="actions">
                          <button className="btn btn-small btn-dark" onClick={() => setStatus(r.id, 'Arrived')}>Saabus</button>
                          <button className="btn btn-small" onClick={() => setStatus(r.id, 'NoShow')}>Ei tulnud</button>
                        </div>
                      )}
                    </div>
                  ))}
                </>
              )}
            </div>
          </aside>
        </div>
      )}

      {plan && <DayTimeline tables={tables} />}
    </div>
  )
}

/** Päeva ajatelg: iga laud on rida, broneeringud on ribad. */
function DayTimeline({ tables }) {
  const startHour = 16
  const endHour = 24
  const span = (endHour - startHour) * 60
  const pos = (d) => {
    const x = new Date(d)
    return ((x.getHours() * 60 + x.getMinutes() - startHour * 60) / span) * 100
  }
  const hours = Array.from({ length: endHour - startHour + 1 }, (_, i) => startHour + i)

  return (
    <div className="paper timeline">
      <p className="card-title">Päeva ülevaade</p>
      <div className="tl-row tl-head">
        <span />
        <div className="tl-track">
          {hours.map((h) => <span key={h} style={{ left: `${(((h - startHour) * 60) / span) * 100}%` }}>{h}</span>)}
        </div>
      </div>
      {tables.map((t) => (
        <div key={t.id} className="tl-row">
          <span className="tl-label">{t.number} <small>({t.seats})</small></span>
          <div className="tl-track">
            {t.reservations.filter((r) => r.status !== 'Cancelled').map((r) => (
              <div
                key={r.id}
                className={`tl-bar tl-${r.status}`}
                style={{ left: `${pos(r.startTime)}%`, width: `${pos(r.endTime) - pos(r.startTime)}%` }}
                title={`${fmtTime(r.startTime)}–${fmtTime(r.endTime)} ${r.customerName}, ${r.guests} in.`}
              >
                {r.customerName.split(' ')[0]}
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  )
}
