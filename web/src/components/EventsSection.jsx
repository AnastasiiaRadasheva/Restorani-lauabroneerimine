import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { formatTime, money } from '../utils'

// Pildi kujud: sirge, kaar ja vapp (nagu kujunduses)
const SHAPES = [
  'polygon(0 0, 100% 0, 100% 100%, 0 100%)',
  'polygon(50% 0, 100% 9%, 100% 100%, 0 100%, 0 9%)',
  'polygon(50% 0, 100% 9%, 100% 91%, 50% 100%, 0 91%, 0 9%)',
]

/** Avalehe plokk "Gastronoomilised õhtud": registreerimine ja riietuse reegliga nõustumine. */
export default function EventsSection() {
  const { t } = useI18n()
  const [events, setEvents] = useState([])
  const [error, setError] = useState('')
  const { user } = useAuth()

  useEffect(() => {
    api('/api/events').then(setEvents).catch((e) => setError(e.message))
  }, [user])

  if (!events.length && !error) return null

  const replace = (e) => setEvents((list) => list.map((x) => (x.id === e.id ? e : x)))

  return (
    <section id="events" className="events">
      <div className="container">
        <div className="events-head">
          <div className="title">
            <span className="season">{t('events.season')}</span>
            <h2>{t('events.title')}</h2>
          </div>
          <p>{t('events.lead')}</p>
        </div>
        {error && <p className="alert alert-error">{error}</p>}
        <div className="event-list">
          {events.map((e, i) => (
            <EventCard key={e.id} e={e} shape={SHAPES[i % SHAPES.length]} onChange={replace} />
          ))}
        </div>
      </div>
    </section>
  )
}

function EventCard({ e, shape, onChange }) {
  const { t, locale } = useI18n()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const [agreed, setAgreed] = useState(false)
  const [guests, setGuests] = useState(2)
  const [error, setError] = useState('')
  const joined = e.myGuests > 0
  const d = new Date(e.startTime)

  async function join() {
    if (!user) return navigate('/login', { state: { from: '/#events' } })
    if (e.rule && !agreed) return setOpen(true)
    setError('')
    try {
      onChange(await api(`/api/events/${e.id}/join`, { method: 'POST', body: { guests, acceptRules: agreed } }))
      setOpen(false)
    } catch (err) {
      setError(err.message)
    }
  }

  async function leave() {
    try {
      onChange(await api(`/api/events/${e.id}/join`, { method: 'DELETE' }))
      setAgreed(false)
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <article className="event">
      <div className="event-pic">
        <div className="frame" style={{ clipPath: shape }}>
          <div style={{ clipPath: shape }}>
            <img src={e.imageUrl} alt={e.title} style={{ clipPath: shape }} />
          </div>
        </div>
        <div className="date-badge">
          <span className="day">{d.getDate()}</span>
          <span className="rest">
            <span>{d.toLocaleDateString(locale, { month: 'long' })}</span>
            <span className="sub">{d.toLocaleDateString(locale, { weekday: 'short' })} · {formatTime(d, locale)}</span>
          </span>
        </div>
      </div>

      <div className="event-text">
        <span className="eyebrow">{e.kind}</span>
        <h3>{e.title}</h3>
        <p>{e.description}</p>
        <span className={e.rule ? 'dress strict' : 'dress'}>
          <i aria-hidden="true" />
          {t('events.dress', { code: e.dressCode })}
        </span>
      </div>

      <div className="event-foot">
        <span className="italic">{money(e.pricePerGuest)} / {t('events.perGuest')}</span>
        {joined ? (
          <button type="button" className="btn btn-dark" onClick={leave} aria-pressed="true">
            {t('events.joined', { n: e.myGuests })}
          </button>
        ) : (
          <button type="button" className="btn" onClick={join} disabled={e.seatsLeft === 0} aria-expanded={open}>
            {e.seatsLeft === 0 ? t('events.full') : open ? t('events.rulesBelow') : t('events.join')}
          </button>
        )}
      </div>
      {!joined && e.seatsLeft > 0 && <span className="muted small">{t('events.seatsLeft', { n: e.seatsLeft })}</span>}

      {open && !joined && (
        <div className="consent" role="group" aria-label={t('events.conditions')}>
          <p className="eyebrow ox">{t('events.conditions')}</p>
          <p>{e.rule}</p>
          <div className="guests-mini">
            <span>{t('booking.guests')}</span>
            <div className="stepper round">
              <button type="button" onClick={() => setGuests(Math.max(1, guests - 1))}>−</button>
              <span className="value">{guests}</span>
              <button type="button" onClick={() => setGuests(Math.min(8, guests + 1))}>+</button>
            </div>
          </div>
          <label className="check ox">
            <input type="checkbox" checked={agreed} onChange={(ev) => setAgreed(ev.target.checked)} />
            <span>{t('events.agree')}</span>
          </label>
          <div className="actions">
            <button type="button" className={agreed ? 'btn btn-ox' : 'btn'} disabled={!agreed} onClick={join}>{t('events.confirm')}</button>
            <button type="button" className="btn" onClick={() => setOpen(false)}>{t('details.close')}</button>
          </div>
        </div>
      )}
      {joined && e.rule && <p className="italic muted small">{t('events.accepted')}</p>}
      {error && <p className="field-error">{error}</p>}
    </article>
  )
}
