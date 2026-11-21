import { useI18n } from '../../i18n'
import { EDGE_KEY, formatDate, parseIsoDate, ZONE_KEY, zonesOf } from '../../utils'
import FloorPlan, { FloorLegend } from '../FloorPlan'

/**
 * II. Teie koht: kas metrdotel valib laua või klient valib ise plaanilt.
 * Saal ja terrass on eraldi vahelehtedel.
 */
export default function PlaceSection({ b }) {
  const { t, locale } = useI18n()
  const restaurant = b.restaurant
  const chosen = b.chosenTable
  const zoneName = (z) => t(ZONE_KEY[z]).toLowerCase()
  const zones = zonesOf(b.availability?.tables ?? [])
  const seatsLabel = (n) => t('booking.seats', { n })

  const planTables = (b.availability?.tables ?? []).map((tb) => ({
    ...tb,
    state: tb.id === chosen?.id ? 'selected' : tb.state,
  }))

  return (
    <section id="plan" className="section alt">
      <div className="container">
        <div className="sec-head">
          <div className="title">
            <span className="num">II.</span>
            <h2>{t('booking.placeTitle')}</h2>
          </div>
          {restaurant && (
            <p className="place-line">
              {restaurant.name} · {formatDate(parseIsoDate(b.date), locale)} · {b.time} · {t('booking.guestsN', { n: b.guests })}
            </p>
          )}
        </div>

        <p className="seat-note">
          <span className="diamond ox" aria-hidden="true" />
          <span>
            {t('seat.note')} <b>{b.isPeak?.(b.time) ? t('seat.feePeak') : t('seat.fee')}</b>
          </span>
        </p>

        {b.error && <p className="alert alert-error">{b.error}</p>}

        <div className="plan-wrap">
          <div className="plan-col">
            <div className="zone-tabs" role="tablist">
              {zones.map((z) => (
                <button key={z} type="button" role="tab" aria-selected={b.zone === z} className={b.zone === z ? 'on' : ''} onClick={() => b.setZone(z)}>
                  {t(ZONE_KEY[z])}
                </button>
              ))}
            </div>
            {restaurant && b.availability ? (
              <FloorPlan
                tables={planTables}
                zone={b.zone}
                bar={{ x: restaurant.barX, y: restaurant.barY, width: restaurant.barWidth, height: restaurant.barHeight }}
                edgeLabel={t(EDGE_KEY[b.zone])}
                entranceLabel={b.zone === 'Upstairs' ? t('booking.stairs') : t('booking.entrance')}
                barLabel={t('booking.bar')}
                seatsLabel={seatsLabel}
                onSelect={(tb) => b.setTableId(tb.id)}
              />
            ) : (
              <div className="plan loading">{t('common.loading')}</div>
            )}
          </div>

          <aside className="plan-side">
            <FloorLegend items={b.legend} />
            <div className="dark-card">
              <p className="eyebrow">{b.mode === 'auto' ? t('booking.pickAuto') : t('booking.pickManual')}</p>
              <p className="big">
                {chosen
                  ? `${t('booking.table', { n: chosen.number })}, ${zoneName(chosen.location)}`
                  : b.mode === 'auto' ? t('booking.noTable') : t('booking.chooseTable')}
              </p>
              <p className="text">
                {chosen
                  ? [
                      [chosen.spot, seatsLabel(chosen.seats), b.seatFeeText].filter(Boolean).join(' · '),
                      `${formatDate(parseIsoDate(b.date), locale)}, ${b.time}`,
                      `${t('booking.guestsN', { n: b.guests })} · ${t('booking.holdNote')}`,
                    ].join('\n')
                  : b.mode === 'auto' ? t('booking.noTableHint') : t('booking.chooseHint')}
              </p>
              {b.mode === 'manual' && (
                <button type="button" className="link-light" onClick={() => b.setTableId(null)}>
                  {t('seat.backToSuggestion')}
                </button>
              )}
            </div>
          </aside>
        </div>
      </div>
    </section>
  )
}
