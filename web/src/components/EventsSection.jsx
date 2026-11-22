import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { formatTime, money } from '../utils'
import EventJoinDialog from './EventJoinDialog'

// Pildi kujud: sirge, kaar ja vapp (nagu kujunduses)
const SHAPES = [
  'polygon(0 0, 100% 0, 100% 100%, 0 100%)',
  'polygon(50% 0, 100% 9%, 100% 100%, 0 100%, 0 9%)',
  'polygon(50% 0, 100% 9%, 100% 91%, 50% 100%, 0 91%, 0 9%)',
]

/** Avalehe plokk "Gastronoomilised õhtud". Registreerimisel avaneb aken laua, jookide ja reeglitega. */
export default function EventsSection() {
  const { t } = useI18n()
  const [events, setEvents] = useState([])
  const [error, setError] = useState('')
  const { user } = useAuth()

  useEffect(() => {
    api('/api/events').then(setEvents).catch((e) => setError(e.message))
  }, [user])

  if (!events.length && !error) return null
  const reload = () => api('/api/events').then(setEvents).catch(() => {})

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
            <EventCard key={e.id} e={e} shape={SHAPES[i % SHAPES.length]} onChange={reload} />
          ))}
        </div>
      </div>
    </section>
  )
}

function EventCard({ e, shape, onChange }) {
  const { t, locale } = useI18n()
  const { user, isStaff } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const joined = e.myRegistrationId != null
  const d = new Date(e.startTime)
  const tr = (field, fallback) => t(`event.${e.code}.${field}`, {}, fallback)

  function join() {
    if (!user) return navigate('/login', { state: { from: '/#events' } })
    // Personal ei registreeru ise – registreerib kliendi töölaual
    if (isStaff) return navigate('/admin?tab=events')
    setOpen(true)
  }

  return (
    <article className="event">
      <div className="event-pic">
        <div className="frame" style={{ clipPath: shape }}>
          <div style={{ clipPath: shape }}>
            <img src={e.imageUrl} alt={tr('title', e.title)} style={{ clipPath: shape }} />
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
        <span className="eyebrow">{tr('kind', e.kind)}</span>
        <h3>{tr('title', e.title)}</h3>
        <p>{tr('desc', e.description)}</p>
        <span className={e.rule ? 'dress strict' : 'dress'}>
          <i aria-hidden="true" />
          {t('events.dress', { code: tr('dress', e.dressCode) })}
        </span>
        {e.noChildren && <span className="dress strict"><i aria-hidden="true" />{t('event.noChildrenShort')}</span>}
      </div>

      <div className="event-foot">
        <span className="italic">{money(e.pricePerGuest)} / {t('events.perGuest')}</span>
        {joined ? (
          <button type="button" className="btn btn-dark" onClick={() => navigate('/minu')} aria-pressed="true">
            {t('events.joined', { n: e.myGuests })}
          </button>
        ) : (
          <button type="button" className="btn" onClick={join} disabled={e.seatsLeft === 0 || e.registrationClosed}>
            {e.registrationClosed ? t('events.closed') : e.seatsLeft === 0 ? t('events.full') : t('events.join')}
          </button>
        )}
      </div>
      {joined && (
        <span className="muted small">
          {e.myTableNumber ? `${t('booking.table', { n: e.myTableNumber })} · ` : ''}{t('details4.total')} {money(e.myTotal)}
          {' · '}{e.myIsPaid ? t('visits.paid') : t('status.PendingPayment')}
        </span>
      )}
      {!joined && e.seatsLeft > 0 && <span className="muted small">{t('events.seatsLeft', { n: e.seatsLeft })}</span>}
      {open && (
        <EventJoinDialog
          e={e}
          onClose={() => setOpen(false)}
          onJoined={() => {
            setOpen(false)
            onChange()
            navigate('/minu', { state: { eventJoined: true } })
          }}
        />
      )}
    </article>
  )
}
