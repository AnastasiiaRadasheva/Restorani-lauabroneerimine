import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { api } from '../api'
import Receipt from '../components/Receipt'
import { useI18n } from '../i18n'
import { formatDate, formatTime, money } from '../utils'

/** Makseleht lingi kaudu (sisselogimist pole vaja). Pärast makset on broneering kinnitatud. */
export default function PayLink() {
  const { token } = useParams()
  const { t, locale } = useI18n()
  const [r, setR] = useState(null)
  const [card, setCard] = useState({ cardHolder: '', cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [error, setError] = useState('')
  const [done, setDone] = useState(null)

  useEffect(() => {
    api(`/api/payments/${token}`).then((x) => {
      setR(x)
      setCard((c) => ({ ...c, cardHolder: x.customerName }))
    }).catch((e) => setError(e.message))
  }, [token])

  async function pay(e) {
    e.preventDefault()
    try {
      setDone(await api(`/api/payments/${token}`, { method: 'POST', body: card }))
    } catch (err) {
      setError(err.message)
    }
  }

  if (done) return <Receipt r={done} total={done.total} onClose={() => (window.location.href = '/')} />
  if (!r) return <div className="container loading">{error || t('common.loading')}</div>

  const pending = r.status === 'PendingPayment'
  const set = (f) => (e) => setCard({ ...card, [f]: e.target.value })

  return (
    <div className="auth">
      <div className="container narrow">
        <form className="auth-card" onSubmit={pay}>
          <div className="head">
            <span className="diamond" aria-hidden="true" />
            <p className="eyebrow ox">{pending ? t('pay.pending') : t(`status.${r.status}`)}</p>
            <h1>{t('pay.title')}</h1>
          </div>
          <dl className="receipt-dl">
            <dt>{t('receipt.who')}</dt><dd>{r.customerName}</dd>
            <dt>{t('details.where')}</dt><dd>{r.restaurantName}</dd>
            <dt>{t('details.when')}</dt><dd>{formatDate(r.startTime, locale)}, {formatTime(r.startTime, locale)}</dd>
            <dt>{t('details.guests')}</dt><dd>{t('booking.guestsN', { n: r.guests })}</dd>
            <dt>{t('details4.total')}</dt><dd>{money(r.total)}</dd>
          </dl>
          {error && <p className="alert alert-error">{error}</p>}
          {pending && (
            <>
              <p className="muted small">{t('pay.note')}</p>
              {r.total > 0 && (
                <>
                  <label className="field">{t('details4.cardNumber')}<input value={card.cardNumber} onChange={set('cardNumber')} /></label>
                  <div className="field-row">
                    <label className="field">{t('details4.expiry')}<input value={card.expiry} onChange={set('expiry')} /></label>
                    <label className="field">CVC<input value={card.cvc} onChange={set('cvc')} maxLength={3} /></label>
                  </div>
                </>
              )}
              <button className="btn btn-dark btn-pill btn-big btn-block">{r.total > 0 ? t('pay.payBtn', { sum: money(r.total) }) : t('pay.confirmBtn')}</button>
            </>
          )}
        </form>
      </div>
    </div>
  )
}
