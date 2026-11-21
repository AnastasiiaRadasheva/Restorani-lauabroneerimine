import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useI18n } from '../i18n'
import { formatDate, formatTime, money, reservationCode, ZONE_KEY } from '../utils'
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
  const zone = t(ZONE_KEY[r.tableLocation])

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

const LANG_NAMES = { et: 'Eesti', ru: 'Русский', en: 'English' }

/** Tšeki lisaread: peakülaline, teeninduse keel ja õhtu põhjus. */
export function eveningRows(r, t) {
  const host =
    r.hostType === 'Social' ? (
      <>{t('host.Social')}</>
    ) : (
      <>
        {r.hostType === 'Other' ? r.hostName : r.customerName}
        <br />
        <span className="muted italic">{t(`address.${r.addressForm}`).toLowerCase()}</span>
      </>
    )
  return [
    [t('evening.host'), host],
    [t('evening.language'), LANG_NAMES[r.serviceLanguage] ?? r.serviceLanguage],
    [t('evening.occasion'), <span className="italic">{t(`occasion.${r.occasion}`)}</span>],
  ]
}

/** Tšeki lisateenused, laua valimise tasu ja peakoka kingitus. */
export function EveningReceiptLines({ r }) {
  const { t } = useI18n()
  const lines = [...(r.seatFee ? [[t('fee.line'), r.seatFee]] : []), ...r.extras.map((x) => [t(`extra.${x.code}`), x.price])]
  return (
    <>
      {lines.length > 0 && (
        <div className="lines">
          <span className="eyebrow">{t('evening.extras')}</span>
          {lines.map(([name, price]) => (
            <div key={name} className="line">
              <span className="name">{name}</span>
              <span>{money(price)}</span>
            </div>
          ))}
        </div>
      )}
      {r.gift !== 'None' && (
        <div className="gift-line">
          <span className="diamond ox" aria-hidden="true" />
          <span className="name">{t('evening.gift')}: {t(`gift.${r.gift}`).toLowerCase()}</span>
          <span>0 €</span>
        </div>
      )}
    </>
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
