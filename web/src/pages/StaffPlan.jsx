import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, qs } from '../api'
import FloorPlan, { FloorLegend, spotText } from '../components/FloorPlan'
import { useTableSocket } from '../realtime'
import { fmtTime, nowSlot, timeSlots, useTr, ZONE_LIST } from '../staff'
import { pickZone, stageOf, toIsoDate, zonesOf } from '../utils'

/** Päeva laudade plaan teenindajale. */
export default function StaffPlan() {
  const tr = useTr()
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
      setToast(`${tr('Uus broneering', 'Новая бронь', 'New booking')}: ${tr('laud', 'стол', 'table')} ${msg.tableNumber}, ${fmtTime(msg.startTime)}, ${msg.customerName} (${msg.guests})`)
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
  const active = (r) => r.status === 'Active' || r.status === 'Arrived' || r.status === 'PendingPayment'
  const tables = (plan?.tables ?? []).map(({ table, reservations }) => {
    // Broneeritud laud on hõivatud kogu päeva (ka enne saabumise aega)
    const now = reservations.find((r) => active(r) && new Date(r.endTime) > at)
    const next = now ?? reservations.find((r) => active(r) && new Date(r.endTime) > at)
    return {
      ...table,
      reservations,
      current: now,
      next,
      state: table.id === selected ? 'selected' : !now ? (selecting[table.id] ? 'hold' : 'free') : now.status === 'Arrived' ? 'arrived' : 'busy',
      clickable: true,
    }
  })
  const selectedTable = tables.find((t) => t.id === selected)
  // Restorani vahetamisel: kui valitud saali seal pole, näidatakse esimest olemasolevat
  const shownZone = pickZone(tables, zone)

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{tr('Laudade plaan', 'План столов', 'Table plan')} <span className="live">{tr('reaalajas', 'в реальном времени', 'live')}</span></h1>
        </div>
        <p>{tr('Klõpsake laual, et näha selle päeva broneeringuid ja märkida külaline saabunuks. Nimel klõpsates avaneb kliendi kaart.', 'Нажмите на стол, чтобы увидеть брони дня и отметить приход гостя. Клик по имени открывает карточку гостя.', 'Click a table to see the day’s bookings and mark arrivals. Click a name to open the guest card.')}</p>
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
              {zonesOf(tables).map((z) => (
                <button key={z} type="button" role="tab" aria-selected={shownZone === z} className={shownZone === z ? 'on' : ''} onClick={() => setZone(z)}>
                  {tr.zone(z)}
                </button>
              ))}
            </div>
            <FloorPlan
              tables={tables}
              zone={shownZone}
              bar={{ x: plan.restaurant.barX, y: plan.restaurant.barY, width: plan.restaurant.barWidth, height: plan.restaurant.barHeight }}
              stage={stageOf(plan.restaurant)}
              stageLabel={tr('Lava', 'Сцена', 'Stage')}
              edgeLabel={tr.edge(shownZone)}
              entranceLabel={shownZone === 'Upstairs' ? tr('Trepp', 'Лестница', 'Stairs') : tr('Sissepääs', 'Вход', 'Entrance')}
              barLabel={tr('Baar', 'Бар', 'Bar')}
              seatsLabel={(n) => `${n} ${tr('kohta', 'мест', 'seats')}`}
              onSelect={(t) => setSelected(t.id)}
              renderInfo={(t) => t.next && <em className={t.current ? 'guest' : 'guest later'}>{t.next.customerName.split(' ')[0]}{t.current ? '' : ` ${fmtTime(t.next.startTime)}`}</em>}
            />
          </div>

          <aside className="plan-side">
            <FloorLegend items={[['free', tr('vaba', 'свободен', 'free')], ['hold', tr('keegi valib praegu', 'кто-то выбирает', 'someone choosing')], ['busy', tr('broneeritud', 'забронирован', 'booked')], ['arrived', tr('külaline kohal', 'гость на месте', 'guest arrived')], ['selected', tr('valitud', 'выбран', 'selected')]]} />
            <div className="paper white side-card">
              {!selectedTable && <p className="italic muted">{tr('Valige plaanilt laud.', 'Выберите стол на плане.', 'Choose a table on the plan.')}</p>}
              {selectedTable && (
                <>
                  <p className="card-title">{tr('Laud', 'Стол', 'Table')} {selectedTable.number}</p>
                  <p className="muted small">{selectedTable.seats} {tr('kohta', 'мест', 'seats')}{selectedTable.spot ? ` · ${spotText(selectedTable.spot, tr.lang)}` : ''}</p>
                  {selectedTable.reservations.length === 0 && <p className="italic">{tr('Sel päeval broneeringuid pole.', 'В этот день броней нет.', 'No bookings on this day.')}</p>}
                  {selectedTable.reservations.map((r) => (
                    <div key={r.id} className="side-res">
                      <div className="row-between">
                        <strong>{fmtTime(r.startTime)}–{fmtTime(r.endTime)}</strong>
                        <span className={`badge badge-${r.status}`}>{tr.status(r.status)}</span>
                      </div>
                      <div><Link to={`/personal/klient?q=${encodeURIComponent(r.customerEmail ?? r.customerName)}`}>{r.customerName}</Link> · {r.guests} {tr('in.', 'гост.', 'gst.')}{r.hasPreOrder && tr(' · eeltellimus', ' · предзаказ', ' · pre-order')}</div>
                      <Link className="btn btn-small" to={`/broneering/${r.id}`}>{tr('Ava / muuda', 'Открыть / изменить', 'Open / edit')}</Link>
                      {['Active', 'Arrived', 'NoShow'].includes(r.status) && new Date(r.startTime) <= new Date(new Date().setHours(23, 59, 59, 999)) && (
                        <div className="actions">
                          {r.status !== 'Arrived' && <button className="btn btn-small btn-dark" onClick={() => setStatus(r.id, 'Arrived')}>{tr('Saabus', 'Пришли', 'Arrived')}</button>}
                          {r.status !== 'NoShow' && <button className="btn btn-small" onClick={() => setStatus(r.id, 'NoShow')}>{tr('Ei tulnud', 'Не пришли', 'No-show')}</button>}
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
  const tr = useTr()
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
      <p className="card-title">{tr('Päeva ülevaade', 'Обзор дня', 'Day overview')}</p>
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
                title={`${fmtTime(r.startTime)}–${fmtTime(r.endTime)} ${r.customerName}, ${r.guests}`}
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
