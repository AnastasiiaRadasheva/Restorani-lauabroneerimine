import { useEffect, useState } from 'react'
import { api } from '../api'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { EDGE_KEY, money, pickZone, stageOf, ZONE_KEY, zonesOf } from '../utils'
import FloorPlan, { FloorLegend } from './FloorPlan'
import DishLink from './DishInfo'
import { CloseIcon } from './Icons'

const EVENT_EXTRAS = [['bottle', 95], ['flowers', 45]]

/**
 * Õhtule registreerimine: laua valik plaanilt (osa laudu on juba võetud, plaanil on näha lava),
 * külaliste arv, "ainult sel õhtul" pakkumised, lubatud lisateenused, reeglitega nõustumine ja makse.
 * Ise registreerudes makstakse kohe – registreerimine ilmub "Minu broneeringutesse".
 */
export default function EventJoinDialog({ e, onClose, onJoined }) {
  const { t } = useI18n()
  const { user } = useAuth()
  const [tables, setTables] = useState(null)
  const [restaurant, setRestaurant] = useState(null)
  const [zone, setZone] = useState(e.zone ?? 'Hall')
  const [tableId, setTableId] = useState(null)
  const [guests, setGuests] = useState(2)
  const [drinks, setDrinks] = useState({})
  const [extras, setExtras] = useState([])
  const [agreed, setAgreed] = useState(false)
  const [card, setCard] = useState({ cardHolder: user?.name ?? '', cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const needsConsent = Boolean(e.rule || e.noChildren)

  useEffect(() => {
    api(`/api/events/${e.id}/tables`).then((list) => {
      setTables(list)
      if (list.length && !list.some((x) => x.location === (e.zone ?? 'Hall'))) setZone(list[0].location)
    }).catch((err) => setError(err.message))
    api(`/api/restaurants/${e.restaurantId}`).then(setRestaurant).catch(() => {})
  }, [e.id, e.restaurantId, e.zone])

  const planTables = (tables ?? []).map((tb) => ({
    ...tb,
    state: tb.id === tableId ? 'selected' : tb.isTaken ? 'busy' : tb.seats < guests ? 'small' : 'free',
  }))
  const chosen = planTables.find((x) => x.id === tableId)
  const shownZone = pickZone(planTables, zone)
  const drinksTotal = e.items.reduce((s, d) => s + (drinks[d.id] ?? 0) * d.price, 0)
  const extrasTotal = EVENT_EXTRAS.filter(([c]) => extras.includes(c)).reduce((s, [, p]) => s + p, 0)
  const total = e.pricePerGuest * guests + drinksTotal + extrasTotal
  const itemName = (d) => (d.code ? t(`drink.${d.code}`, {}, d.name) : d.name)

  const changeDrink = (id, d) => setDrinks((x) => ({ ...x, [id]: Math.max(0, Math.min(20, (x[id] ?? 0) + d)) }))

  async function join() {
    setError('')
    if (!tableId) return setError(t('ej.chooseTable'))
    if (needsConsent && !agreed) return setError(t('ej.needConsent'))
    setBusy(true)
    try {
      const result = await api(`/api/events/${e.id}/join`, {
        method: 'POST',
        body: {
          guests,
          tableId,
          acceptRules: agreed,
          extras,
          drinks: Object.entries(drinks).filter(([, q]) => q > 0).map(([itemId, quantity]) => ({ itemId: Number(itemId), quantity })),
          card: total > 0 ? card : null,
        },
      })
      onJoined(result)
    } catch (err) {
      setError(err.message)
      if (err.status === 409) api(`/api/events/${e.id}/tables`).then(setTables)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="dialog-backdrop" role="dialog" aria-modal="true" aria-label={t('ej.title')}>
      <div className="dialog wide">
        <div className="row-between">
          <p className="eyebrow">{t('ej.title')}</p>
          <button className="btn btn-small" onClick={onClose} aria-label={t('details.close')}><CloseIcon /></button>
        </div>
        <p className="dialog-title">{t(`event.${e.code}.title`, {}, e.title)}</p>
        {e.zone && <p className="muted small">{t('ej.onlyZone', { zone: t(ZONE_KEY[e.zone]) })}</p>}

        <div className="ej-grid">
          <div className="ej-plan">
            <div className="zone-tabs">
              {zonesOf(planTables).map((z) => (
                <button key={z} type="button" className={shownZone === z ? 'on' : ''} onClick={() => setZone(z)}>{t(ZONE_KEY[z])}</button>
              ))}
            </div>
            {tables ? (
              <FloorPlan
                tables={planTables}
                zone={shownZone}
                bar={restaurant ? { x: restaurant.barX, y: restaurant.barY, width: restaurant.barWidth, height: restaurant.barHeight } : null}
                stage={stageOf(restaurant)}
                stageLabel={t('plan.stage')}
                edgeLabel={t(EDGE_KEY[shownZone])}
                entranceLabel={shownZone === 'Upstairs' ? t('booking.stairs') : t('booking.entrance')}
                barLabel={t('booking.bar')}
                seatsLabel={(n) => t('booking.seats', { n })}
                onSelect={(tb) => setTableId(tb.id)}
              />
            ) : (
              <p className="loading">{t('common.loading')}</p>
            )}
            <FloorLegend items={[['free', t('booking.legendFree')], ['busy', t('ej.taken')], ['selected', t('booking.legendMine')]]} />
          </div>

          <div className="ej-side">
            <div className="guests-mini">
              <span className="eyebrow">{t('booking.guests')}</span>
              <div className="stepper round">
                <button type="button" onClick={() => setGuests(Math.max(1, guests - 1))}>−</button>
                <span className="value">{guests}</span>
                <button type="button" onClick={() => setGuests(Math.min(8, guests + 1))}>+</button>
              </div>
            </div>
            <p className="small">{chosen ? `${t('booking.table', { n: chosen.number })} · ${t('booking.seats', { n: chosen.seats })}` : t('ej.chooseTable')}</p>

            {e.items.length > 0 && <p className="eyebrow">{t('ej.drinks')}</p>}
            {e.items.map((d) => (
              <div key={d.id} className="addon">
                {d.imageUrl && <DishLink item={{ ...d, name: itemName(d), tags: null }} className="dish-pic"><img className="addon-photo" src={d.imageUrl} alt="" /></DishLink>}
                <div className="text">
                  <DishLink item={{ ...d, name: itemName(d), tags: null }} className="dish-name"><span className="name">{itemName(d)}</span></DishLink>
                  <span className="desc">{d.description}</span>
                </div>
                <span className="price">{money(d.price)}</span>
                <div className="stepper round">
                  <button type="button" onClick={() => changeDrink(d.id, -1)}>−</button>
                  <span className="value">{drinks[d.id] ?? 0}</span>
                  <button type="button" onClick={() => changeDrink(d.id, 1)}>+</button>
                </div>
              </div>
            ))}

            <p className="eyebrow">{t('ej.extras')}</p>
            {EVENT_EXTRAS.map(([code, price]) => (
              <label key={code} className="check">
                <input type="checkbox" checked={extras.includes(code)} onChange={() => setExtras(extras.includes(code) ? extras.filter((c) => c !== code) : [...extras, code])} />
                <span>{t(`extra.${code}`)} · {money(price)}</span>
              </label>
            ))}
            <p className="muted small">{t('ej.extrasNote')}</p>

            {needsConsent && (
              <div className="consent">
                <p className="eyebrow ox">{t('events.conditions')}</p>
                {e.rule && <p>{t(`event.${e.code}.rule`, {}, e.rule)}</p>}
                {e.noChildren && <p className="warning-text">{t('event.noChildren')}</p>}
                <label className="check ox">
                  <input type="checkbox" checked={agreed} onChange={(x) => setAgreed(x.target.checked)} />
                  <span>{t('events.agree')}</span>
                </label>
              </div>
            )}

            <div className="grand">
              <span className="eyebrow">{t('details4.total')}</span>
              <span className="sum">{money(total)}</span>
            </div>
            {total > 0 && (
              <div className="ej-card">
                <label className="field">{t('details4.cardNumber')}<input value={card.cardNumber} onChange={(x) => setCard({ ...card, cardNumber: x.target.value })} /></label>
                <div className="field-row">
                  <label className="field">{t('details4.expiry')}<input value={card.expiry} onChange={(x) => setCard({ ...card, expiry: x.target.value })} /></label>
                  <label className="field">CVC<input value={card.cvc} maxLength={3} onChange={(x) => setCard({ ...card, cvc: x.target.value })} /></label>
                </div>
                <p className="muted small">{t('ej.payNote')}</p>
              </div>
            )}
            {error && <p className="alert alert-error">{error}</p>}
            <button className="btn btn-ox btn-block" disabled={busy || (needsConsent && !agreed)} onClick={join}>
              {total > 0 ? t('ej.payAndJoin', { sum: money(total) }) : t('events.confirm')}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
