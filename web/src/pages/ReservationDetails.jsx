import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { api, qs } from '../api'
import { useI18n } from '../i18n'
import { addDays, formatDate, formatTime, reservationCode, toIsoDate } from '../utils'

/** Ühe broneeringu vaade: andmed, muutmine (rohkem kui 24 h enne) ja tühistamine. */
export default function ReservationDetails() {
  const { id } = useParams()
  const location = useLocation()
  const { t, locale } = useI18n()
  const [r, setR] = useState(null)
  const [error, setError] = useState('')
  const [message, setMessage] = useState(location.state?.justCreated ? t('details.created') : '')
  const [editing, setEditing] = useState(false)

  useEffect(() => {
    api(`/api/reservations/${id}`).then(setR).catch((e) => setError(e.message))
  }, [id])

  function updated(newR, text) {
    setR(newR)
    setMessage(text)
    setError('')
    setEditing(false)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function cancel() {
    if (!window.confirm(t('details.cancelConfirm'))) return
    try {
      updated(await api(`/api/reservations/${id}/cancel`, { method: 'PATCH' }), t('details.cancelled'))
    } catch (e) {
      setError(e.message)
    }
  }

  if (!r) return <div className="container loading">{error || t('common.loading')}</div>

  const active = r.status === 'Active'
  const zone = r.tableLocation === 'Hall' ? t('booking.hall') : t('booking.terrace')

  return (
    <div className="details-page container narrow">
      <div className="details-head">
        <span className="diamond" aria-hidden="true" />
        <p className="eyebrow">{t('details.number', { code: reservationCode(r) })}</p>
        <h1>
          {t('details.title')} <em>{formatDate(r.startTime, locale)}</em>
        </h1>
      </div>

      {message && <p className="alert alert-ok">{message}</p>}
      {error && <p className="alert alert-error">{error}</p>}

      <div className="paper white details-card">
        <dl className="receipt-dl">
          <dt>{t('details.status')}</dt>
          <dd>{t(`status.${r.status}`)}</dd>
          <dt>{t('details.where')}</dt>
          <dd>{r.restaurantName}<br /><span className="muted">{r.restaurantAddress}</span></dd>
          <dt>{t('details.when')}</dt>
          <dd>{formatDate(r.startTime, locale, { weekday: 'long', day: 'numeric', month: 'long' })}, {formatTime(r.startTime, locale)}–{formatTime(r.endTime, locale)}</dd>
          <dt>{t('details.table')}</dt>
          <dd>{t('booking.table', { n: r.tableNumber })} · {zone.toLowerCase()}</dd>
          <dt>{t('details.guests')}</dt>
          <dd>{t('booking.guestsN', { n: r.guests })}</dd>
          {r.specialRequest && (
            <>
              <dt>{t('details.wish')}</dt>
              <dd className="italic">{r.specialRequest}</dd>
            </>
          )}
        </dl>

        {active && !r.canModify && <p className="lock-note">{t('details.locked')}</p>}

        {active && r.canModify && !editing && (
          <div className="actions">
            <button className="btn btn-dark" onClick={() => setEditing(true)}>{t('details.change')}</button>
            <button className="btn btn-ox-line" onClick={cancel}>{t('details.cancel')}</button>
          </div>
        )}
      </div>

      {editing && <EditReservation r={r} onSaved={(x) => updated(x, t('details.saved'))} onError={setError} onClose={() => setEditing(false)} />}

      <p className="back-link"><Link to="/minu">← {t('nav.my')}</Link></p>
    </div>
  )
}

/** Broneeringu muutmine: kuupäev, kellaaeg ja külaliste arv. */
function EditReservation({ r, onSaved, onError, onClose }) {
  const { t } = useI18n()
  const start = new Date(r.startTime)
  const [date, setDate] = useState(toIsoDate(start))
  const [time, setTime] = useState(formatTime(r.startTime, 'et-EE'))
  const [guests, setGuests] = useState(r.guests)
  const [slots, setSlots] = useState([])
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api('/api/tables/slots' + qs({ restaurantId: r.restaurantId, from: date, days: 1, guests }))
      .then((d) => setSlots(d[0]?.slots ?? []))
      .catch(() => setSlots([]))
  }, [r.restaurantId, date, guests])

  async function save(e) {
    e.preventDefault()
    setBusy(true)
    try {
      onSaved(await api(`/api/reservations/${r.id}`, { method: 'PUT', body: { date, time, guests, specialRequest: r.specialRequest } }))
    } catch (err) {
      onError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="paper edit-card" onSubmit={save}>
      <h2>{t('details.changeTitle')}</h2>
      <div className="field-row">
        <label className="field">
          {t('booking.date')}
          <input type="date" value={date} min={toIsoDate(addDays(new Date(), 2))} onChange={(e) => setDate(e.target.value)} />
        </label>
        <label className="field">
          {t('booking.guests')}
          <input type="number" min={1} max={12} value={guests} onChange={(e) => setGuests(Number(e.target.value))} />
        </label>
      </div>
      <p className="eyebrow">{t('booking.time')}</p>
      <div className="chips">
        {slots.map((s) => (
          <button
            key={s.time}
            type="button"
            className={s.time === time ? 'chip on' : 'chip'}
            disabled={s.freeTables === 0 && s.time !== time}
            onClick={() => setTime(s.time)}
          >
            {s.time}
          </button>
        ))}
      </div>
      <p className="muted small">{t('details.changeNote')}</p>
      <div className="actions">
        <button className="btn btn-dark" disabled={busy}>{t('details.save')}</button>
        <button type="button" className="btn" onClick={onClose}>{t('details.close')}</button>
      </div>
    </form>
  )
}
