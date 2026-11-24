import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api'
import { useI18n } from '../i18n'
import { eventTitle } from '../eventText'
import { formatDate, formatTime, money } from '../utils'

/** Gastronoomilise õhtu makseleht lingi kaudu (sisselogimist pole vaja). */
export default function EventPayLink() {
  const { token } = useParams()
  const { t, locale, lang } = useI18n()
  const [r, setR] = useState(null)
  const [card, setCard] = useState({ cardHolder: '', cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api(`/api/events/payments/${token}`).then((x) => {
      setR(x)
      setCard((c) => ({ ...c, cardHolder: x.customerName }))
    }).catch((e) => setError(e.message))
  }, [token])

  async function pay(e) {
    e.preventDefault()
    setBusy(true)
    try {
      setR(await api(`/api/events/payments/${token}`, { method: 'POST', body: card }))
      setError('')
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  if (!r) return <div className="container loading">{error || t('common.loading')}</div>
  const pending = r.status === 'PendingPayment'
  const set = (f) => (e) => setCard({ ...card, [f]: e.target.value })

  return (
    <div className="auth">
      <div className="container narrow">
        <form className="auth-card" onSubmit={pay}>
          <div className="head">
            <span className="diamond" aria-hidden="true" />
            <p className="eyebrow ox">{pending ? t('pay.pending') : r.status === 'Active' ? t('visits.paid') : t(`status.${r.status}`)}</p>
            <h1>{eventTitle(t, lang, r)}</h1>
          </div>
          <dl className="receipt-dl">
            <dt>{t('receipt.who')}</dt><dd>{r.customerName}</dd>
            <dt>{t('details.where')}</dt><dd>{r.restaurantName}</dd>
            <dt>{t('details.when')}</dt><dd>{formatDate(r.startTime, locale)}, {formatTime(r.startTime, locale)}</dd>
            <dt>{t('details.guests')}</dt><dd>{t('booking.guestsN', { n: r.guests })}{r.tableNumber ? ` · ${t('booking.table', { n: r.tableNumber })}` : ''}</dd>
            {r.lines.length > 0 && (<><dt>{t('ej.drinks')}</dt><dd>{r.lines.map((l) => `${l.quantity} × ${l.code ? t(`drink.${l.code}`, {}, l.name) : l.name}`).join(', ')}</dd></>)}
            <dt>{t('details4.total')}</dt><dd>{money(r.total)}</dd>
          </dl>
          {error && <p className="alert alert-error">{error}</p>}
          {pending && (
            <>
              <p className="muted small">{t('ev.payNote')}</p>
              <label className="field">{t('details4.cardNumber')}<input value={card.cardNumber} onChange={set('cardNumber')} /></label>
              <div className="field-row">
                <label className="field">{t('details4.expiry')}<input value={card.expiry} onChange={set('expiry')} /></label>
                <label className="field">CVC<input value={card.cvc} onChange={set('cvc')} maxLength={3} /></label>
              </div>
              <button className="btn btn-dark btn-pill btn-big btn-block" disabled={busy}>{t('pay.payBtn', { sum: money(r.total) })}</button>
            </>
          )}
          {r.status === 'Active' && (
            <>
              <p className="alert alert-ok">{t('ev.paidOk')}</p>
              <Link className="btn btn-block" to="/minu">{t('nav.my')}</Link>
            </>
          )}
        </form>
      </div>
    </div>
  )
}
