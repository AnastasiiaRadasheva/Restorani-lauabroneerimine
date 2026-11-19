import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api'
import { ChevronDown } from '../components/Icons'
import { useI18n } from '../i18n'
import { formatDate, formatTime, money } from '../utils'

/** Teie külastused: tulevased (avatavad kaardid) ja varasemad. */
export default function MyReservations({ renderExtra }) {
  const { t, locale } = useI18n()
  const [list, setList] = useState(null)
  const [error, setError] = useState('')
  const [open, setOpen] = useState(null)

  function load() {
    return api('/api/reservations/my')
      .then((l) => {
        setList(l)
        return l
      })
      .catch((e) => setError(e.message))
  }

  useEffect(() => {
    load().then((l) => {
      const first = l?.filter(isUpcoming).sort(byStart)[0]
      if (first) setOpen(first.id)
    })
  }, [])

  if (!list) return <div className="container loading">{error || t('common.loading')}</div>

  const upcoming = list.filter(isUpcoming).sort(byStart)
  const past = list.filter((r) => !isUpcoming(r)).sort((a, b) => byStart(b, a))

  return (
    <div className="visits">
      <div className="container">
        <div className="visits-head">
          <span className="diamond" aria-hidden="true" />
          <h1>{t('visits.title')}</h1>
          <p>{t('visits.lead')}</p>
        </div>

        {error && <p className="alert alert-error">{error}</p>}

        <p className="eyebrow">{t('visits.upcoming')}</p>
        {upcoming.length === 0 && (
          <div className="paper empty">
            <p className="italic">{t('visits.none')}</p>
            <Link to="/broneeri" className="btn btn-dark btn-pill">{t('home.book')}</Link>
          </div>
        )}
        <div className="visit-list">
          {upcoming.map((r) => (
            <VisitCard
              key={r.id}
              r={r}
              open={open === r.id}
              onToggle={() => setOpen(open === r.id ? null : r.id)}
              onChanged={load}
              setError={setError}
              renderExtra={renderExtra}
            />
          ))}
        </div>

        {past.length > 0 && (
          <>
            <p className="eyebrow past-title">{t('visits.past')}</p>
            <div className="past-list">
              {past.map((r) => (
                <div key={r.id} className="past">
                  <div>
                    <span className="when">{formatDate(r.startTime, locale)}, {formatTime(r.startTime, locale)}</span>
                    <span className="muted small">
                      {r.restaurantName} · {t('booking.table', { n: r.tableNumber })} · {t('booking.guestsN', { n: r.guests })} · {t(`status.${r.status}`)}
                    </span>
                  </div>
                  <Link to={`/broneeri?restaurant=${r.restaurantId}&guests=${r.guests}`} className="link-caps">
                    {t('visits.repeat')}
                  </Link>
                </div>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  )
}

const isUpcoming = (r) => r.status === 'Active' && new Date(r.endTime) >= new Date()
const byStart = (a, b) => new Date(a.startTime) - new Date(b.startTime)

function VisitCard({ r, open, onToggle, onChanged, setError, renderExtra }) {
  const { t, locale } = useI18n()
  const navigate = useNavigate()
  const hours = (new Date(r.startTime) - new Date()) / 3600000
  const badge = hours < 24 ? t('visits.today') : t('visits.inDays', { n: Math.round(hours / 24) })
  const zone = r.tableLocation === 'Hall' ? t('booking.hall') : t('booking.terrace')

  async function cancel() {
    if (!window.confirm(t('details.cancelConfirm'))) return
    try {
      await api(`/api/reservations/${r.id}/cancel`, { method: 'PATCH' })
      onChanged()
    } catch (e) {
      setError(e.message)
    }
  }

  return (
    <article className={open ? 'visit open' : 'visit'}>
      <button type="button" className="visit-head" onClick={onToggle} aria-expanded={open}>
        <span className="when">
          <span className="eyebrow">
            {badge} · {t(`status.${r.status}`)}
            {r.lastMessageFromStaff && <span className="new-reply"> · {t('visits.newReply')}</span>}
          </span>
          <span className="big">{formatDate(r.startTime, locale)}, {formatTime(r.startTime, locale)}</span>
        </span>
        <span className="where">
          <span>{r.restaurantName}</span>
          <span className="sub">{t('booking.table', { n: r.tableNumber })}, {zone.toLowerCase()} · {t('booking.guestsN', { n: r.guests })}</span>
        </span>
        <span className="toggle">
          {open ? t('visits.collapse') : t('visits.open')}
          <ChevronDown />
        </span>
      </button>

      {open && (
        <div className="visit-body">
          <div className="visit-info">
            <p className="card-title">{t('visits.yours')}</p>
            <dl className="receipt-dl">
              <dt>{t('visits.onName')}</dt>
              <dd>{r.customerName}</dd>
              <dt>{t('details.where')}</dt>
              <dd>{r.restaurantAddress}</dd>
              <dt>{t('gastro.preorder')}</dt>
              <dd>
                {r.preOrder.length ? r.preOrder.map((p) => <div key={p.menuItemId}>{p.quantity} × {p.name}</div>) : '—'}
              </dd>
              {r.specialRequest && (
                <>
                  <dt>{t('details.wish')}</dt>
                  <dd className="italic">{r.specialRequest}</dd>
                </>
              )}
              {r.occasion !== 'None' && (
                <>
                  <dt>{t('evening.occasion')}</dt>
                  <dd>{t(`occasion.${r.occasion}`)}</dd>
                </>
              )}
              {r.extras.length > 0 && (
                <>
                  <dt>{t('evening.extras')}</dt>
                  <dd>{r.extras.map((x) => <div key={x.code}>{t(`extra.${x.code}`)}</div>)}</dd>
                </>
              )}
              <dt>{t('details4.total')}</dt>
              <dd className="sum">{money(r.total)}{r.isPaid ? ` · ${t('visits.paid')}` : ''}</dd>
            </dl>
            {!r.canModify && <p className="lock-note">{t('visits.locked')}</p>}
            <div className="actions">
              <button className="btn btn-dark" disabled={!r.canModify} onClick={() => navigate(`/broneering/${r.id}`)}>
                {t('details.change')}
              </button>
              <button className="btn btn-ox-line" disabled={!r.canModify} onClick={cancel}>{t('details.cancel')}</button>
            </div>
          </div>
          {renderExtra && renderExtra(r, onChanged)}
        </div>
      )}
    </article>
  )
}
