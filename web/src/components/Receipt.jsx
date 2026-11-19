import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useI18n } from '../i18n'
import { formatDate, formatTime, money, reservationCode } from '../utils'
import { CheckIcon, CloseIcon } from './Icons'

/**
 * Tšekk pärast broneeringu kinnitamist (täisekraanil).
 * children – lisaread (tähtpäev, lisateenused jms), mis tulevad hiljem.
 */
export default function Receipt({ r, total, onClose, rows = [], children }) {
  const { t, locale } = useI18n()

  useEffect(() => {
    const onKey = (e) => e.key === 'Escape' && onClose()
    document.body.style.overflow = 'hidden'
    window.addEventListener('keydown', onKey)
    return () => {
      document.body.style.overflow = ''
      window.removeEventListener('keydown', onKey)
    }
  }, [onClose])

  const code = reservationCode(r)
  const zone = r.tableLocation === 'Hall' ? t('booking.hall') : t('booking.terrace')

  return (
    <div className="receipt-overlay" role="dialog" aria-modal="true" aria-label={t('receipt.confirmed')}>
      <button type="button" className="receipt-close" onClick={onClose} aria-label={t('details.close')}>
        <CloseIcon />
      </button>
      <div className="receipt-wrap">
        <div className="receipt-head">
          <span className="seal" aria-hidden="true"><CheckIcon /></span>
          <p className="eyebrow">{t('receipt.confirmed')}</p>
          <h2>
            {t('details.title')} <em>{formatDate(r.startTime, locale)}</em>
          </h2>
        </div>

        <article className="receipt">
          <div className="receipt-body">
            <div className="brand">
              <span className="word">ARGENT</span>
              <span className="tag">{t('brand.tagline')}</span>
              <span className="eyebrow">{t('details.number', { code })}</span>
            </div>
            <div className="cut" />
            <dl className="receipt-dl">
              <dt>{t('receipt.who')}</dt>
              <dd>{r.customerName}{r.customerPhone && <><br /><span className="muted">{r.customerPhone}</span></>}</dd>
              {rows.map(([k, v]) => (
                <Row key={k} k={k} v={v} />
              ))}
              <dt>{t('details.where')}</dt>
              <dd>{r.restaurantName}<br /><span className="muted">{r.restaurantAddress}</span></dd>
              <dt>{t('details.when')}</dt>
              <dd>{formatDate(r.startTime, locale, { weekday: 'long', day: 'numeric', month: 'long' })}, {formatTime(r.startTime, locale)}</dd>
              <dt>{t('details.table')}</dt>
              <dd>{t('booking.table', { n: r.tableNumber })} · {zone.toLowerCase()}</dd>
              <dt>{t('details.guests')}</dt>
              <dd>{t('booking.guestsN', { n: r.guests })}</dd>
            </dl>
            <div className="cut" />
            <div className="lines">
              <span className="eyebrow">{t('gastro.preorder')}</span>
              {r.preOrder.map((p) => (
                <div key={p.menuItemId} className="line">
                  <span className="q">{p.quantity}×</span>
                  <span className="name">{p.name}</span>
                  <span>{money(p.total)}</span>
                </div>
              ))}
              {r.preOrder.length === 0 && <span className="italic muted">{t('receipt.noPreorder')}</span>}
            </div>
            {children}
            {r.specialRequest && (
              <div className="wish-box">
                <span className="eyebrow ox">{t('details4.wish')}</span>
                <span className="italic">{r.specialRequest}</span>
              </div>
            )}
            <div className="cut solid" />
            <div className="grand">
              <span className="eyebrow">{t('details4.total')}</span>
              <span className="sum">{money(total)}</span>
            </div>
            {r.isPaid && <span className="paid">{t('receipt.paid')}</span>}
            <div className="barcode" aria-hidden="true">
              <div />
              <span>{code}</span>
            </div>
            <p className="note">{t('receipt.note')}</p>
          </div>
          <div className="zigzag" aria-hidden="true" />
        </article>

        <div className="receipt-actions">
          <button type="button" className="btn btn-dark btn-pill" onClick={onClose}>{t('details.close')}</button>
          <Link to="/minu" className="btn btn-pill">{t('nav.my')}</Link>
        </div>
      </div>
    </div>
  )
}

function Row({ k, v }) {
  return (
    <>
      <dt>{k}</dt>
      <dd>{v}</dd>
    </>
  )
}
