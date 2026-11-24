import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { formatDate, formatTime, money, ZONE_KEY } from '../utils'
import EventJoinDialog from './EventJoinDialog'
import { CloseIcon } from './Icons'
import DishTags from './DishTags'
import { drinkTags } from '../dishInfo'
import { useEventText } from '../eventText'

// Pildi kujud: sirge, kaar ja vapp (nagu kujunduses)
const SHAPES = [
  'polygon(0 0, 100% 0, 100% 100%, 0 100%)',
  'polygon(50% 0, 100% 9%, 100% 100%, 0 100%, 0 9%)',
  'polygon(50% 0, 100% 9%, 100% 91%, 50% 100%, 0 91%, 0 9%)',
]

/**
 * Avalehe plokk "Gastronoomilised õhtud": karussell.
 * Sirvida saab hiirega lohistades, puuteekraanil libistades või noolte abil; all on edenemise riba ja loendur.
 * Kaardile klõpsates avaneb õhtu info (programm, riietus, reeglid) ja sealt saab registreeruda.
 */
export default function EventsSection() {
  const { t } = useI18n()
  const [events, setEvents] = useState([])
  const [error, setError] = useState('')
  const { user } = useAuth()
  const track = useRef(null)
  const drag = useRef(null)
  const [pos, setPos] = useState({ start: true, end: false, progress: 0, first: 1, perView: 3 })
  const [dragging, setDragging] = useState(false)

  useEffect(() => {
    api('/api/events').then(setEvents).catch((e) => setError(e.message))
  }, [user])

  const step = () => {
    const card = track.current?.querySelector('.event')
    return card ? card.getBoundingClientRect().width + 32 : 1
  }

  function update() {
    const el = track.current
    if (!el) return
    const max = el.scrollWidth - el.clientWidth
    const s = step()
    setPos({
      start: el.scrollLeft <= 4,
      end: el.scrollLeft >= max - 4,
      progress: max > 0 ? el.scrollLeft / max : 1,
      first: Math.round(el.scrollLeft / s) + 1,
      perView: Math.max(1, Math.round(el.clientWidth / s)),
    })
  }
  useEffect(() => {
    update()
    window.addEventListener('resize', update)
    return () => window.removeEventListener('resize', update)
  }, [events.length]) // eslint-disable-line react-hooks/exhaustive-deps

  function scroll(dir) {
    track.current?.scrollBy({ left: dir * step(), behavior: 'smooth' })
  }

  // Hiirega lohistamine (puuteekraan kerib ise)
  function onPointerDown(e) {
    if (e.pointerType !== 'mouse' || e.button !== 0) return
    // Avatud aken (portal) on Reacti puus kaardi sees, kuid lohistamine kehtib ainult kaartide real
    if (!track.current?.contains(e.target)) return
    drag.current = { x: e.clientX, left: track.current.scrollLeft, moved: false }
  }
  function onPointerMove(e) {
    const d = drag.current
    if (!d) return
    const dx = e.clientX - d.x
    if (!d.moved && Math.abs(dx) < 6) return
    if (!d.moved) {
      d.moved = true
      setDragging(true)
      track.current.setPointerCapture?.(e.pointerId)
    }
    track.current.scrollLeft = d.left - dx
  }
  function onPointerUp() {
    const d = drag.current
    drag.current = null
    if (!d?.moved) return
    setDragging(false)
    // Lähima kaardi juurde
    const s = step()
    track.current.scrollTo({ left: Math.round(track.current.scrollLeft / s) * s, behavior: 'smooth' })
    // Lohistamise lõpus ei tohi kaart avaneda
    const stop = (x) => { x.stopPropagation(); x.preventDefault() }
    window.addEventListener('click', stop, { capture: true, once: true })
    setTimeout(() => window.removeEventListener('click', stop, { capture: true }), 50)
  }

  if (!events.length && !error) return null
  const reload = () => api('/api/events').then(setEvents).catch(() => {})
  const two = (n) => String(n).padStart(2, '0')
  const last = Math.min(events.length, pos.first + pos.perView - 1)
  const share = Math.max(Math.min(pos.perView / events.length, 1), 0.08)

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
        <div className={dragging ? 'event-carousel dragging' : 'event-carousel'}>
          <div
            className="event-list"
            ref={track}
            onScroll={update}
            onPointerDown={onPointerDown}
            onPointerMove={onPointerMove}
            onPointerUp={onPointerUp}
            onPointerCancel={onPointerUp}
            onDragStart={(e) => e.preventDefault()}
          >
            {events.map((e, i) => (
              <EventCard key={e.id} e={e} shape={SHAPES[i % SHAPES.length]} onChange={reload} />
            ))}
          </div>
          {!(pos.start && pos.end) && (
            <div className="carousel-nav">
              <span className="carousel-count">
                <strong>{two(pos.first)}{last > pos.first ? `–${two(last)}` : ''}</strong> / {two(events.length)}
              </span>
              <div className="carousel-bar" aria-hidden="true">
                <span style={{ width: `${share * 100}%`, left: `${pos.progress * (1 - share) * 100}%` }} />
              </div>
              <span className="carousel-hint">{t('events.drag')}</span>
              <button type="button" className="carousel-btn" onClick={() => scroll(-1)} disabled={pos.start} aria-label={t('events.prev')}>←</button>
              <button type="button" className="carousel-btn" onClick={() => scroll(1)} disabled={pos.end} aria-label={t('events.next')}>→</button>
            </div>
          )}
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
  const [info, setInfo] = useState(false)
  const joined = e.myRegistrationId != null
  const d = new Date(e.startTime)
  const { field: tr, dress } = useEventText(e)

  function join() {
    setInfo(false)
    if (!user) return navigate('/login', { state: { from: '/#events' } })
    // Personal ei registreeru ise – registreerib kliendi töölaual
    if (isStaff) return navigate('/admin?tab=events')
    setOpen(true)
  }

  return (
    <article className="event">
      <button type="button" className="event-pic" onClick={() => setInfo(true)} aria-label={`${tr('title', e.title)} – ${t('events.more')}`}>
        <div className="frame" style={{ clipPath: shape }}>
          <div style={{ clipPath: shape }}>
            <img src={e.imageUrl} alt="" style={{ clipPath: shape }} />
          </div>
        </div>
        <span className="event-more" aria-hidden="true">{t('events.more')}</span>
        <div className="date-badge">
          <span className="day">{d.getDate()}</span>
          <span className="rest">
            <span>{d.toLocaleDateString(locale, { month: 'long' })}</span>
            <span className="sub">{d.toLocaleDateString(locale, { weekday: 'short' })} · {formatTime(d, locale)}</span>
          </span>
        </div>
      </button>

      <div className="event-text">
        <span className="eyebrow">{tr('kind', e.kind)}</span>
        <h3><button type="button" className="event-title" onClick={() => setInfo(true)}>{tr('title', e.title)}</button></h3>
        <p>{tr('desc', e.description)}</p>
        {e.program && <span className="event-program"><span aria-hidden="true">♪</span> {tr('program', e.program)}</span>}
        <span className={e.rule ? 'dress strict' : 'dress'}>
          <i aria-hidden="true" />
          {e.dressCode ? t('events.dress', { code: dress() }) : t('events.noDress')}
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
      {/* Aknad renderdatakse otse body sisse: kaart on karussellis (kerimine, hover-transform) ja muidu jääks aken kaardi sisse kinni */}
      {info && createPortal(<EventInfo e={e} joined={joined} onClose={() => setInfo(false)} onJoin={join} />, document.body)}
      {open && createPortal(
        <EventJoinDialog
          e={e}
          onClose={() => setOpen(false)}
          onJoined={() => {
            setOpen(false)
            onChange()
            navigate('/minu', { state: { eventJoined: true } })
          }}
        />,
        document.body,
      )}
    </article>
  )
}

/** Õhtu info eraldi aknas: kirjeldus, muusika ja programm, riietus ja reeglid. Sealt saab registreeruda. */
function EventInfo({ e, joined, onClose, onJoin }) {
  const { t, locale } = useI18n()
  const { field: tr, dress, rule } = useEventText(e)
  const itemName = (d) => (d.code ? t(`drink.${d.code}`, {}, d.name) : d.name)
  const canJoin = !joined && e.seatsLeft > 0 && !e.registrationClosed

  useEffect(() => {
    const onKey = (x) => x.key === 'Escape' && onClose()
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="dialog-backdrop" role="dialog" aria-modal="true" aria-label={tr('title', e.title)} onClick={(x) => x.target === x.currentTarget && onClose()}>
      <div className="dialog wide event-info">
        <button type="button" className="btn btn-small dish-close" onClick={onClose} aria-label={t('details.close')}><CloseIcon /></button>
        <div className="ei-grid">
          {e.imageUrl && <img className="ei-pic" src={e.imageUrl} alt="" />}
          <div className="ei-text">
            <p className="eyebrow">{tr('kind', e.kind)}</p>
            <p className="dialog-title">{tr('title', e.title)}</p>
            <p className="ei-when">
              {formatDate(e.startTime, locale, { weekday: 'long', day: 'numeric', month: 'long' })} · {formatTime(e.startTime, locale)}
              <br />
              {e.restaurantName}{e.zone ? ` · ${t(ZONE_KEY[e.zone])}` : ''}
            </p>
            <p>{tr('desc', e.description)}</p>
            {e.program && (
              <div className="event-program big">
                <span className="eyebrow">♪ {t('events.program')}</span>
                <span>{tr('program', e.program)}</span>
              </div>
            )}
            <ul className="ei-rules">
              <li className={e.rule ? 'ox' : ''}>{e.dressCode ? t('events.dress', { code: dress() }) : t('events.noDress')}</li>
              {e.rule && <li className="ox">{rule()}</li>}
              {e.noChildren && <li className="ox">{t('event.noChildren')}</li>}
            </ul>
            {e.items.length > 0 && (
              <div className="ei-drinks">
                <p className="eyebrow">{t('ej.drinks')}</p>
                {e.items.map((d) => (
                  <p key={d.id} className="small">
                    <span>{itemName(d)}</span> <DishTags tags={drinkTags(d.code)} /> <span className="muted">· {money(d.price)}</span>
                  </p>
                ))}
              </div>
            )}
            <p className="ei-small">{t('events.smallPrint')}</p>
            <div className="ei-foot">
              <span className="italic">{money(e.pricePerGuest)} / {t('events.perGuest')}</span>
              {e.seatsLeft > 0 && !e.registrationClosed && <span className="muted small">{t('events.seatsLeft', { n: e.seatsLeft })}</span>}
            </div>
            <div className="dialog-actions">
              {canJoin && <button type="button" className="btn btn-ox" onClick={onJoin}>{t('events.join')}</button>}
              <button type="button" className="btn" onClick={onClose}>{t('details.close')}</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
