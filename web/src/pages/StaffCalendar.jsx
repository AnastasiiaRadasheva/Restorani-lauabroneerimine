import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { eventTitle } from '../eventText'
import StaffMessages from '../components/StaffMessages'
import DayBroadcast from '../components/DayBroadcast'
import { Link, useSearchParams } from 'react-router-dom'
import { fmtTime, useTr } from '../staff'
import { addDays, money, toIsoDate } from '../utils'


const NOTE_COLORS = ['ox', 'gold', 'green', 'blue', 'grey']
/** Erimärked: päev käsitsi oluliseks või tavaliseks (ei ole värviline märge). */
const SPECIAL = ['important', 'normal']
const NOTE_PRESETS = [
  ['Olulised külalised', 'Важные гости', 'Important guests'],
  ['Palju külalisi', 'Много гостей', 'Busy day'],
  ['Eraüritus', 'Частное мероприятие', 'Private event'],
  ['Terrass suletud', 'Терраса закрыта', 'Terrace closed'],
  ['Vähe personali', 'Мало персонала', 'Short-staffed'],
]

/** Päeva märked: personal kirjutab ise sildi (tekst + värv), see on kalendris kõigile näha. */
function DayNotes({ date, restaurantId, notes, onChanged }) {
  const tr = useTr()
  const [text, setText] = useState('')
  const [color, setColor] = useState('gold')
  const [err, setErr] = useState('')
  const [busy, setBusy] = useState(false)

  async function add() {
    if (text.trim().length < 2) return
    setBusy(true)
    setErr('')
    try {
      await api('/api/day-notes', { method: 'POST', body: { date, restaurantId: restaurantId ? Number(restaurantId) : null, text: text.trim(), color } })
      setText('')
      onChanged()
    } catch (e) {
      setErr(e.message)
    } finally {
      setBusy(false)
    }
  }
  async function remove(id) {
    try {
      await api(`/api/day-notes/${id}`, { method: 'DELETE' })
      onChanged()
    } catch (e) {
      setErr(e.message)
    }
  }

  // Käsitsi "oluline päev" / "tavaline päev" (eemaldab automaatse punase raami)
  const mark = notes.find((n) => SPECIAL.includes(n.color))
  async function setMark(kind) {
    setErr('')
    try {
      if (mark) await api(`/api/day-notes/${mark.id}`, { method: 'DELETE' })
      if (mark?.color !== kind) {
        const text = kind === 'important' ? tr('Oluline päev', 'Важный день', 'Important day') : tr('Tavaline päev', 'Обычный день', 'Normal day')
        await api('/api/day-notes', { method: 'POST', body: { date, restaurantId: restaurantId ? Number(restaurantId) : null, text, color: kind } })
      }
      onChanged()
    } catch (e) {
      setErr(e.message)
    }
  }
  const regular = notes.filter((n) => !SPECIAL.includes(n.color))

  const dayText = new Date(date).toLocaleDateString(tr.lang, { day: 'numeric', month: 'long' })
  return (
    <div className="day-notes">
      <div className="dn-marks">
        <span className="dn-label">{dayText}</span>
        <button type="button" className={mark?.color === 'important' ? 'chip small on ox' : 'chip small'} onClick={() => setMark('important')}>
          ◆ {tr('Oluline päev', 'Важный день', 'Important day')}
        </button>
        <button type="button" className={mark?.color === 'normal' ? 'chip small on' : 'chip small'} onClick={() => setMark('normal')}>
          {tr('Tavaline päev', 'Обычный день', 'Normal day')}
        </button>
      </div>
      <div className="dn-form">
        <span className="dn-label">{tr('Märge', 'Заметка', 'Note')} · {dayText}</span>
        <input list="dn-presets" value={text} maxLength={80} placeholder={tr('nt VIP õhtu', 'напр. VIP-вечер', 'e.g. VIP evening')} onChange={(e) => setText(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && add()} />
        <datalist id="dn-presets">
          {NOTE_PRESETS.map(([et, ru, en]) => <option key={et} value={tr(et, ru, en)} />)}
        </datalist>
        <span className="dn-colors" role="radiogroup" aria-label={tr('Värv', 'Цвет', 'Colour')}>
          {NOTE_COLORS.map((c) => (
            <button key={c} type="button" role="radio" aria-checked={color === c} className={`sw lc-${c}${color === c ? ' on' : ''}`} onClick={() => setColor(c)} aria-label={c} />
          ))}
        </span>
        <button type="button" className="btn btn-dark btn-small" disabled={busy || text.trim().length < 2} onClick={add}>+</button>
      </div>
      {regular.length > 0 && (
        <div className="dn-list">
          {regular.map((n) => (
            <span key={n.id} className="dn-chip" title={n.authorName}>
              <i className={`sw lc-${n.color}`} />{n.text}
              <button type="button" onClick={() => remove(n.id)} aria-label={tr('Kustuta', 'Удалить', 'Delete')}>×</button>
            </span>
          ))}
        </div>
      )}
      {err && <p className="small field-error">{err}</p>}
    </div>
  )
}

/** Personal: kuu kalender. Päeval klõpsates on näha, kes broneeris, ja neile saab kirjutada. */
export default function StaffCalendar() {
  const tr = useTr()
  const [params] = useSearchParams()
  // ?date=… tuleb broneeringute nimekirjast ("Vaata kalendris")
  const start = /^\d{4}-\d{2}-\d{2}$/.test(params.get('date') ?? '') ? params.get('date') : toIsoDate(new Date())
  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantId] = useState('')
  const [month, setMonth] = useState(() => {
    const d = new Date(start)
    return new Date(d.getFullYear(), d.getMonth(), 1)
  })
  const [days, setDays] = useState([])
  const [selected, setSelected] = useState(start)
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

  // Personali märked päevadele ja gastronoomilised õhtud (automaatsed sildid)
  const [notes, setNotes] = useState([])
  const [events, setEvents] = useState([])
  const loadNotes = () =>
    api('/api/day-notes' + qs({ from: toIsoDate(first), days: 42, restaurantId }))
      .then(setNotes)
      .catch(() => setNotes([]))
  useEffect(() => { loadNotes() }, [restaurantId, month]) // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => {
    api('/api/events/all').then(setEvents).catch(() => setEvents([]))
  }, [])

  const manyGuests = restaurantId ? 30 : 60
  const labelsFor = (d) => {
    const list = []
    for (const e of events) {
      if (e.startTime?.slice(0, 10) === d.date && (!restaurantId || e.restaurantId === Number(restaurantId))) {
        list.push({ key: 'ev' + e.id, color: 'blue', text: eventTitle(tr.t, tr.lang, e) })
      }
    }
    // Üks tähtpäevaga broneering on näha punase raami ja ◆ märgiga; silt alates kahest
    if (d.important >= 2) list.push({ key: 'imp', color: 'ox', text: tr('Olulised külalised', 'Важные гости', 'Important guests') })
    if (d.guests >= manyGuests) list.push({ key: 'many', color: 'green', text: tr('Palju külalisi', 'Много гостей', 'Busy day') })
    for (const n of notes.filter((x) => x.date === d.date && !SPECIAL.includes(x.color))) list.push({ key: 'n' + n.id, color: n.color, text: n.text, note: true })
    return list
  }

  // Legendi jaoks: selle kuu märked (sama tekst + värv ühe reana, kuupäevad kõrval)
  const monthNotes = Object.values(
    notes
      .filter((n) => new Date(n.date).getMonth() === month.getMonth() && !SPECIAL.includes(n.color))
      .reduce((acc, n) => {
        const key = n.color + '|' + n.text
        const day = new Date(n.date).getDate() + '.'
        acc[key] = acc[key] ? { ...acc[key], dates: acc[key].dates + ', ' + day } : { key, color: n.color, text: n.text, dates: day }
        return acc
      }, {}),
  )

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
              const labels = labelsFor(d)
              // Personali märkega päev värvitakse tervenisti märke värvi
              const fill = labels.find((l) => l.note)?.color
              // Käsitsi märge on tähtsam kui automaatne (tähtpäevaga broneeringud)
              const mark = notes.find((n) => n.date === d.date && SPECIAL.includes(n.color))?.color
              const important = mark === 'important' || (d.important > 0 && mark !== 'normal')
              const dots = labels.filter((l) => !(l.note && l.color === fill))
              return (
                <button
                  key={d.date}
                  type="button"
                  className={`sc-day${d.date === selected ? ' on' : ''}${other ? ' other' : ''}${important ? ' important' : ''}${mark === 'important' ? ' marked' : ''}${fill ? ` filled lc-${fill}` : ''}`}
                  onClick={() => setSelected(d.date)}
                  title={labels.map((l) => l.text).join(' · ')}
                >
                  <span className="n">{date.getDate()}</span>
                  {d.reservations > 0 && <span className="c">{d.reservations} {tr('br.', 'бр.', 'bk.')} · {d.guests} {tr('in.', 'гост.', 'gst.')}</span>}
                  {d.important > 0 && <span className="i">◆ {d.important}</span>}
                  {dots.length > 0 && (
                    <span className="sc-dots" aria-label={labels.map((l) => l.text).join(', ')}>
                      {dots.map((l) => <i key={l.key} className={`dot lc-${l.color}`} />)}
                    </span>
                  )}
                </button>
              )
            })}
          </div>
          <div className="sc-legend">
            <ul>
              <li className="lg-head">{tr('Täpid', 'Точки', 'Dots')}</li>
              <li><i className="frame" />{tr('oluline päev (tähtpäev või märgitud käsitsi)', 'важный день (повод у гостей или отмечен вручную)', 'important day (occasion or marked by hand)')}</li>
              <li><i className="dot lc-ox" />{tr('olulised külalised', 'важные гости', 'important guests')}</li>
              <li><i className="dot lc-green" />{tr(`palju külalisi (${manyGuests}+)`, `много гостей (${manyGuests}+)`, `busy day (${manyGuests}+)`)}</li>
              <li><i className="dot lc-blue" />{tr('gastronoomiline õhtu', 'гастрономический вечер', 'gastronomic evening')}</li>
            </ul>
            <ul>
              <li className="lg-head">{tr('Märked', 'Заметки', 'Notes')}</li>
              {monthNotes.length === 0 && <li className="muted">{tr('Sel kuul märkeid pole', 'В этом месяце заметок нет', 'No notes this month')}</li>}
              {monthNotes.map((n) => (
                <li key={n.key}><i className={`sw lc-${n.color}`} />{n.text} <small className="muted">{n.dates}</small></li>
              ))}
            </ul>
          </div>
          <DayNotes date={selected} restaurantId={restaurantId} notes={notes.filter((n) => n.date === selected)} onChanged={loadNotes} />
        </div>

        <div className="day-list">
          <div className="row-between day-head">
            <p className="card-title">{new Date(selected).toLocaleDateString(tr.lang, { weekday: 'long', day: 'numeric', month: 'long' })}</p>
            <Link className="chip caps chip-link" to={`/personal/broneeringud?date=${selected}`}>
              {tr('Päeva broneeringud nimekirjas', 'Брони дня списком', 'Day bookings as list')} →
            </Link>
          </div>
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
