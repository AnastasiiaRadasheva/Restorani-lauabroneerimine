import { useEffect, useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { formatDate, formatTime, money, ZONE_KEY } from '../utils'

/**
 * "Minu broneeringud": gastronoomilised õhtud, kuhu olen registreeritud.
 * Personali tehtud registreerimise saab siin kohe maksta; tühistada saab samuti.
 */
export default function EventVisits({ justJoined }) {
  const { t } = useI18n()
  const [list, setList] = useState(null)
  const load = () => api('/api/events/registrations/my').then(setList).catch(() => setList([]))

  useEffect(() => {
    load()
  }, [])

  if (!list) return null
  const now = new Date()
  const upcoming = list.filter((r) => r.status !== 'Cancelled' && new Date(r.startTime) > now)
  const other = list.filter((r) => !upcoming.includes(r))
  if (list.length === 0) return null

  return (
    <div className="event-visits">
      <p className="eyebrow">{t('ev.title')}</p>
      {justJoined && <p className="alert alert-ok">{t('ev.joined')}</p>}
      {upcoming.map((r) => <EventVisit key={r.id} r={r} onChanged={load} />)}
      {other.length > 0 && (
        <details className="ev-past">
          <summary className="small muted">{t('ev.past', { n: other.length })}</summary>
          {other.map((r) => <EventVisit key={r.id} r={r} onChanged={load} />)}
        </details>
      )}
    </div>
  )
}

function EventVisit({ r, onChanged }) {
  const { t, locale } = useI18n()
  const [paying, setPaying] = useState(false)
  const [cancelling, setCancelling] = useState(false)
  const [agree, setAgree] = useState(false)
  const [card, setCard] = useState({ cardHolder: r.customerName, cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [error, setError] = useState('')
  const pending = r.status === 'PendingPayment'
  const active = r.status === 'Active' || pending
  const title = t(`event.${r.eventCode}.title`, {}, r.eventTitle)
  const lateNoRefund = active && r.isPaid && !r.freeCancel

  async function pay(e) {
    e.preventDefault()
    try {
      await api(`/api/events/registrations/${r.id}/pay`, { method: 'POST', body: card })
      setPaying(false)
      onChanged()
    } catch (err) {
      setError(err.message)
    }
  }

  async function cancel() {
    try {
      await api(`/api/events/registrations/${r.id}/cancel`, { method: 'PATCH', body: { acceptNoRefund: agree } })
      setCancelling(false)
      onChanged()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className={`ev-card paper white${pending ? ' pending' : ''}${r.status === 'Cancelled' ? ' cancelled' : ''}`}>
      {r.imageUrl && <img className="ev-img" src={r.imageUrl} alt="" />}
      <div className="ev-body">
        <span className={`badge badge-${r.status}`}>{r.status === 'Active' ? t('visits.paid') : t(`status.${r.status}`)}</span>
        <p className="card-title">{title}</p>
        <p className="small">
          {formatDate(r.startTime, locale, { weekday: 'long', day: 'numeric', month: 'long' })}, {formatTime(r.startTime, locale)} · {r.restaurantName}
        </p>
        <p className="small">
          {t('booking.guestsN', { n: r.guests })}
          {r.tableNumber ? ` · ${t('booking.table', { n: r.tableNumber })}` : ''}
          {r.tableLocation ? ` · ${t(ZONE_KEY[r.tableLocation]).toLowerCase()}` : ''}
        </p>
        {r.lines.length > 0 && (
          <p className="small muted">{r.lines.map((l) => `${l.quantity} × ${l.code ? t(`drink.${l.code}`, {}, l.name) : l.name}`).join(', ')}</p>
        )}
        {r.extras.length > 0 && <p className="small muted">{r.extras.map((x) => t(`extra.${x}`)).join(', ')}</p>}
        <p className="sum">{money(r.total)}</p>
        {r.status === 'Cancelled' && r.refundAmount > 0 && <p className="small">{t('cancel.refundNote', { sum: money(r.refundAmount) })}</p>}

        {pending && <p className="lock-note">{t('ev.payNote')}</p>}
        {error && <p className="field-error">{error}</p>}

        {active && !paying && !cancelling && (
          <div className="actions">
            {pending && <button className="btn btn-dark" onClick={() => setPaying(true)}>{t('ev.pay', { sum: money(r.total) })}</button>}
            <button className="btn btn-ox-line" onClick={() => setCancelling(true)}>{t('details.cancel')}</button>
          </div>
        )}

        {paying && (
          <form className="ev-pay" onSubmit={pay}>
            <label className="field">{t('details4.cardNumber')}<input value={card.cardNumber} onChange={(x) => setCard({ ...card, cardNumber: x.target.value })} /></label>
            <div className="field-row">
              <label className="field">{t('details4.expiry')}<input value={card.expiry} onChange={(x) => setCard({ ...card, expiry: x.target.value })} /></label>
              <label className="field">CVC<input value={card.cvc} maxLength={3} onChange={(x) => setCard({ ...card, cvc: x.target.value })} /></label>
            </div>
            <div className="actions">
              <button className="btn btn-dark">{t('ev.pay', { sum: money(r.total) })}</button>
              <button type="button" className="btn" onClick={() => setPaying(false)}>{t('po.cancel')}</button>
            </div>
          </form>
        )}

        {cancelling && (
          <div className="ev-cancel">
            <p className="small">{lateNoRefund ? t('ev.cancelLate') : r.isPaid ? t('ev.cancelRefund', { sum: money(r.total) }) : t('ev.cancelFree')}</p>
            {lateNoRefund && (
              <label className="check ox"><input type="checkbox" checked={agree} onChange={(x) => setAgree(x.target.checked)} /> <span>{t('cancel.agree')}</span></label>
            )}
            <div className="actions">
              <button className="btn btn-ox" disabled={lateNoRefund && !agree} onClick={cancel}>{t('ev.cancelConfirm')}</button>
              <button className="btn" onClick={() => setCancelling(false)}>{t('po.cancel')}</button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
