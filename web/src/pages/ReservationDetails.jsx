import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { api, qs } from '../api'
import CancelDialog from '../components/CancelDialog'
import { useI18n } from '../i18n'
import { useAuth } from '../auth'
import StaffMessages from '../components/StaffMessages'
import Messages from '../components/Messages'
import { useTr } from '../staff'
import { addDays, formatDate, formatTime, money, reservationCode, toIsoDate, ZONE_KEY } from '../utils'

/** Ühe broneeringu vaade: andmed, muutmine (rohkem kui 24 h enne) ja tühistamine. */
export default function ReservationDetails() {
  const { id } = useParams()
  const location = useLocation()
  const { t, locale } = useI18n()
  const { isStaff } = useAuth()
  const tr = useTr()
  const [r, setR] = useState(null)
  const [error, setError] = useState('')
  const [message, setMessage] = useState(location.state?.justCreated ? t('details.created') : '')
  // "Muuda kuupäeva ja lauda" (Minu broneeringud) avab kohe muutmise vormi; personal näeb ainult vaadet
  const [editing, setEditing] = useState(() => !isStaff && new URLSearchParams(location.search).has('muuda'))
  useEffect(() => {
    if (editing && r) document.querySelector('.edit-card')?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }, [editing, r])

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

  const [cancelOpen, setCancelOpen] = useState(false)

  if (!r) return <div className="container loading">{error || t('common.loading')}</div>

  // Personal näeb broneeringut ainult lugemiseks (muudab ja tühistab klient ise või personal oma vaadetes)
  const active = !isStaff && (r.status === 'Active' || r.status === 'PendingPayment')
  const canModify = r.canModify
  const zone = t(ZONE_KEY[r.tableLocation])

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
          {isStaff && (
            <>
              <dt>{t('receipt.who')}</dt>
              <dd><Link to={`/personal/klient?q=${encodeURIComponent(r.customerEmail)}`}>{r.customerName}</Link><br /><span className="muted small">{r.customerPhone ?? ''} {r.customerEmail}</span></dd>
            </>
          )}
          <dt>{t('details.status')}</dt>
          <dd className={r.status === 'PendingPayment' ? 'ox' : ''}>{t(`status.${r.status}`)}</dd>
          {isStaff && r.status === 'PendingPayment' && r.paymentToken && (
            <>
              <dt>{t('pay.title')}</dt>
              <dd className="small"><a href={`/maksa/${r.paymentToken}`}>{`${window.location.origin}/maksa/${r.paymentToken}`}</a></dd>
            </>
          )}
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

        {active && !canModify && <p className="lock-note">{t('details.locked')}</p>}

        {active && !canModify && (
          <div className="actions">
            <button className="btn btn-ox-line" onClick={() => setCancelOpen(true)}>{t('details.cancel')}</button>
          </div>
        )}
        {active && canModify && !editing && (
          <div className="actions">
            <button className="btn btn-dark" onClick={() => setEditing(true)}>{t('details.change')}</button>
            <button className="btn btn-ox-line" onClick={() => setCancelOpen(true)}>{t('details.cancel')}</button>
          </div>
        )}
      </div>

      {isStaff && <GuestWishes r={r} />}
      {isStaff && <div className="actions"><StaffMessages r={r} /></div>}
      {!isStaff && <div className="paper white details-card"><Messages reservationId={r.id} /></div>}
      {editing && <EditReservation r={r} onSaved={(x) => updated(x, t('details.saved'))} onError={setError} onClose={() => setEditing(false)} />}

      {cancelOpen && (
        <CancelDialog
          r={r}
          onClose={() => setCancelOpen(false)}
          onCancelled={(x) => {
            setCancelOpen(false)
            updated(x, x.refundAmount > 0 ? t('cancel.refundNote', { sum: `${x.refundAmount} €` }) : t('details.cancelled'))
          }}
        />
      )}

      <p className="back-link">
        {isStaff
          ? <Link to="/personal/broneeringud">← {tr('Broneeringud', 'Брони', 'Bookings')}</Link>
          : <Link to="/minu">← {t('nav.my')}</Link>}
      </p>
    </div>
  )
}

/** Personalile: kõik, mida külaline broneerides soovis (ainult lugemiseks). */
function GuestWishes({ r }) {
  const { t } = useI18n()
  const tr = useTr()
  const none = <span className="muted">—</span>
  const host = r.hostType === 'Other' ? r.hostName : t(`host.${r.hostType}`)
  return (
    <div className="paper white details-card wishes">
      <p className="eyebrow">{tr('Mida külaline soovis', 'Что хотел гость', 'What the guest asked for')}</p>
      <dl className="receipt-dl">
        <dt>{tr('Eeltellimus', 'Предзаказ', 'Pre-order')}</dt>
        <dd>
          {r.preOrder.length === 0 ? none : (
            <ul className="wish-list">
              {r.preOrder.map((p) => <li key={p.menuItemId}><span>{p.quantity} × {p.name}</span><span>{money(p.total)}</span></li>)}
            </ul>
          )}
        </dd>
        <dt>{tr('Lisateenused', 'Доп. услуги', 'Extras')}</dt>
        <dd>
          {r.extras.length === 0 ? none : (
            <ul className="wish-list">
              {r.extras.map((x) => (
                <li key={x.code}>
                  <span>{t(`extra.${x.code}`, {}, x.name)} <small className="muted">{x.locked ? tr('(broneerimisel, makstud)', '(при брони, оплачено)', '(at booking, paid)') : tr('(lisatud hiljem, kohapeal)', '(добавлено позже, на месте)', '(added later, on site)')}</small></span>
                  <span>{x.price ? money(x.price) : tr('tasuta', 'бесплатно', 'free')}</span>
                </li>
              ))}
            </ul>
          )}
        </dd>
        <dt>{tr('Tähtpäev', 'Повод', 'Occasion')}</dt>
        <dd>{r.occasion === 'None' ? none : <span className="ox">◆ {t(`occasion.${r.occasion}`)}</span>}</dd>
        {r.gift !== 'None' && (<><dt>{tr('Peakoka kingitus', 'Подарок от шефа', "Chef's gift")}</dt><dd>{t(`gift.${r.gift}`)}</dd></>)}
        <dt>{tr('Kes kutsub', 'Кто приглашает', 'Host')}</dt>
        <dd>{host || none}</dd>
        <dt>{tr('Pöördumine', 'Обращение', 'Address as')}</dt>
        <dd>{t(`address.${r.addressForm}`)}</dd>
        <dt>{tr('Teeninduse keel', 'Язык обслуживания', 'Service language')}</dt>
        <dd>{(r.serviceLanguage ?? 'et').toUpperCase()}</dd>
        <dt>{tr('Allergiad', 'Аллергии', 'Allergies')}</dt>
        <dd>
          {r.allergies.length === 0 && !r.allergyNote ? none : (
            <span className="ox">{r.allergies.map((a) => t(`allergy.${a}`)).join(', ')}{r.allergyNote ? <><br /><span className="italic">{r.allergyNote}</span></> : null}</span>
          )}
        </dd>
        <dt>{tr('Soovid', 'Пожелания', 'Requests')}</dt>
        <dd className="italic">{r.specialRequest || none}</dd>
        <dt>{tr('Kokku', 'Итого', 'Total')}</dt>
        <dd>
          <strong>{money(r.total)}</strong>
          {r.seatFee > 0 && <span className="small muted d-block">{tr('laua valimine', 'выбор стола', 'table choice')} {money(r.seatFee)}</span>}
          {r.paidAmount > 0 && <span className="small d-block">{tr('makstud kaardiga', 'оплачено картой', 'paid by card')} {money(r.paidAmount)}</span>}
          {r.onSiteAmount > 0 && <span className="small d-block ox">{tr('kohapeal maksta', 'оплатить на месте', 'to pay on site')} {money(r.onSiteAmount)}</span>}
        </dd>
      </dl>
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
