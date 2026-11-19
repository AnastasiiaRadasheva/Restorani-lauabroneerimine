import { useI18n } from '../../i18n'
import { formatDate, parseIsoDate } from '../../utils'
import FloorPlan, { FloorLegend } from '../FloorPlan'
import { Arrow } from '../Icons'

/**
 * II. Teie koht: kas metrdotel valib laua või klient valib ise plaanilt.
 * Saal ja terrass on eraldi vahelehtedel.
 */
export default function PlaceSection({ b, action }) {
  const { t, locale } = useI18n()
  const restaurant = b.restaurant
  const chosen = b.chosenTable
  const zoneName = (z) => (z === 'Hall' ? t('booking.hall') : t('booking.terrace')).toLowerCase()
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

        <ol className="how">
          <li><span className="n on">1</span><span>{t('booking.how1')}</span></li>
          <li><span className="n">2</span><span>{t('booking.how2')}</span></li>
          <li><span className="n">3</span><span>{t('booking.how3')}</span></li>
        </ol>

        <div className="mode" role="radiogroup" aria-label={t('booking.how1')}>
          <button type="button" role="radio" aria-checked={b.mode === 'auto'} className={b.mode === 'auto' ? 'on' : ''} onClick={() => b.setMode('auto')}>
            <span className="dot" />
            <span className="text">
              <span className="big">{t('booking.auto')}</span>
              <span className="sub">{t('booking.autoSub')}</span>
            </span>
          </button>
          <button type="button" role="radio" aria-checked={b.mode === 'manual'} className={b.mode === 'manual' ? 'on' : ''} onClick={() => b.setMode('manual')}>
            <span className="dot" />
            <span className="text">
              <span className="big">
                {t('booking.manual')} {b.manualPrice && <em>{b.manualPrice}</em>}
              </span>
              <span className="sub">{b.manualSub ?? t('booking.manualSub')}</span>
            </span>
          </button>
        </div>

        {b.error && <p className="alert alert-error">{b.error}</p>}

        <div className="plan-wrap">
          <div className="plan-col">
            <div className="zone-tabs" role="tablist">
              {['Hall', 'Terrace'].map((z) => (
                <button key={z} type="button" role="tab" aria-selected={b.zone === z} className={b.zone === z ? 'on' : ''} onClick={() => b.setZone(z)}>
                  {z === 'Hall' ? t('booking.hall') : t('booking.terrace')}
                </button>
              ))}
            </div>
            {restaurant && b.availability ? (
              <FloorPlan
                tables={planTables}
                zone={b.zone}
                bar={{ x: restaurant.barX, y: restaurant.barY, width: restaurant.barWidth, height: restaurant.barHeight }}
                edgeLabel={b.zone === 'Hall' ? t('booking.edgeHall') : t('booking.edgeTerrace')}
                entranceLabel={t('booking.entrance')}
                barLabel={t('booking.bar')}
                seatsLabel={seatsLabel}
                onSelect={b.mode === 'manual' ? (tb) => b.setTableId(tb.id) : undefined}
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
              <button type="button" className="btn-chamfer silver" onClick={action.onClick} disabled={!chosen || action.busy}>
                {action.label}
                <Arrow width={28} />
              </button>
            </div>
          </aside>
        </div>
      </div>
    </section>
  )
}
