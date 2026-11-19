import { useState } from 'react'
import { useI18n } from '../../i18n'
import { addDays, formatDate, formatDistance, parseIsoDate, toIsoDate, weekdayShort } from '../../utils'
import { CalendarIcon } from '../Icons'
import RestaurantMap from '../RestaurantMap'

/**
 * I. Teie õhtu: kuupäev (9 päeva riba + 30 päeva kalender), kellaaeg, külaliste arv
 * ja restoran kaardil (lähim restoran kasutaja asukoha järgi).
 */
export default function EveningSection({ b }) {
  const { t, locale } = useI18n()
  const [monthOpen, setMonthOpen] = useState(false)

  const today = new Date()
  const strip = Array.from({ length: 9 }, (_, i) => addDays(today, i))
  const month = Array.from({ length: 30 }, (_, i) => addDays(today, i))
  // Kalendri esimene rida algab esmaspäevast
  const offset = (today.getDay() + 6) % 7

  const daySlots = (iso) => b.slots.find((d) => d.date === iso)?.slots
  const isFull = (iso) => {
    const s = daySlots(iso)
    return s ? s.every((x) => x.freeTables === 0) : false
  }
  const times = daySlots(b.date) ?? []
  const weekdays = Array.from({ length: 7 }, (_, i) => weekdayShort(addDays(new Date(2026, 10, 16), i), locale))

  return (
    <section id="book" className="section first">
      <div className="container">
        <div className="sec-head">
          <div className="title">
            <span className="num">I.</span>
            <h1>{t('booking.eveningTitle')}</h1>
          </div>
          <p>{t('booking.eveningLead')}</p>
        </div>

        <div className="evening">
          <div className="evening-left">
            {/* ----- Kuupäev ----- */}
            <div className="block">
              <p className="eyebrow">{t('booking.date')} — {formatDate(today, locale, { month: 'long', year: 'numeric' })}</p>
              <div className="day-strip" role="group" aria-label={t('booking.date')}>
                {strip.map((d) => {
                  const iso = toIsoDate(d)
                  return (
                    <button
                      key={iso}
                      type="button"
                      className={iso === b.date ? 'day on' : 'day'}
                      onClick={() => b.setDate(iso)}
                      disabled={isFull(iso)}
                      aria-pressed={iso === b.date}
                    >
                      <span className="dow">{weekdayShort(d, locale)}</span>
                      <span className="num">{d.getDate()}</span>
                    </button>
                  )
                })}
              </div>
              <div className="month-row">
                <button type="button" className={monthOpen ? 'btn btn-dark' : 'btn'} onClick={() => setMonthOpen(!monthOpen)} aria-expanded={monthOpen}>
                  <CalendarIcon />
                  {monthOpen ? t('booking.monthClose') : t('booking.monthOpen')}
                </button>
                <span className="date-label">
                  {formatDate(parseIsoDate(b.date), locale)}, {b.time}
                </span>
              </div>

              {monthOpen && (
                <div className="month">
                  <div className="month-head">
                    <span className="big">
                      {formatDate(month[0], locale, { month: 'long' })} — {formatDate(month[29], locale, { month: 'long', year: 'numeric' })}
                    </span>
                    <span className="muted small">{t('booking.next30')}</span>
                  </div>
                  <div className="month-grid">
                    {weekdays.map((w) => <span key={w} className="wd">{w}</span>)}
                    {Array.from({ length: offset }, (_, i) => <span key={`e${i}`} />)}
                    {month.map((d) => {
                      const iso = toIsoDate(d)
                      const full = isFull(iso)
                      return (
                        <button
                          key={iso}
                          type="button"
                          className={iso === b.date ? 'cal on' : 'cal'}
                          disabled={full}
                          onClick={() => b.setDate(iso)}
                          aria-pressed={iso === b.date}
                          aria-label={formatDate(d, locale) + (full ? `, ${t('booking.full')}` : '')}
                        >
                          {d.getDate()}
                        </button>
                      )
                    })}
                  </div>
                  <p className="muted small">{t('booking.fullDays')}</p>
                </div>
              )}
            </div>

            {/* ----- Kellaaeg ----- */}
            <div className="block">
              <p className="eyebrow">{t('booking.time')}</p>
              <div className="time-grid" role="group" aria-label={t('booking.time')}>
                {times.map((s) => (
                  <button
                    key={s.time}
                    type="button"
                    className={`time${s.time === b.time ? ' on' : ''}${b.isPeak?.(s.time) ? ' peak' : ''}`}
                    disabled={s.freeTables === 0}
                    onClick={() => b.setTime(s.time)}
                    aria-pressed={s.time === b.time}
                  >
                    {s.time}
                  </button>
                ))}
              </div>
              <p className="hint">
                <span>{t('booking.fullTimes')}</span>
                {b.peakNote}
              </p>
            </div>

            {/* ----- Külalised ----- */}
            <div className="guests-row">
              <p className="eyebrow">{t('booking.guests')}</p>
              <div className="stepper">
                <button type="button" onClick={() => b.setGuests(b.guests - 1)} disabled={b.guests <= 1} aria-label={t('booking.fewer')}>−</button>
                <span className="value" aria-live="polite">{b.guests}</span>
                <button type="button" onClick={() => b.setGuests(b.guests + 1)} disabled={b.guests >= 12} aria-label={t('booking.more')}>+</button>
              </div>
            </div>
          </div>

          {/* ----- Restoran ----- */}
          <div className="evening-right">
            <div className="addr-head">
              <p className="eyebrow">{t('booking.address')}</p>
              <label className="check">
                <input type="checkbox" checked={b.geo} onChange={(e) => b.setGeo(e.target.checked)} />
                {t('booking.nearest')}
              </label>
            </div>
            {b.geoError && <p className="field-error">{b.geoError}</p>}
            <RestaurantMap
              restaurants={b.restaurants}
              userPos={b.userPos}
              selectedId={b.restaurantId}
              onSelect={(r) => b.setRestaurantId(r.id)}
              labels={{ me: t('booking.mapMe'), distance: t('booking.mapDistance') }}
            />
            <div className="places">
              {b.restaurants.map((r) => (
                <button
                  key={r.id}
                  type="button"
                  className={r.id === b.restaurantId ? 'place on' : 'place'}
                  onClick={() => b.setRestaurantId(r.id)}
                  aria-pressed={r.id === b.restaurantId}
                >
                  <span className={r.id === b.restaurantId ? 'diamond' : 'diamond hollow'} />
                  <span className="text">
                    <span className="name">{r.name}</span>
                    <span className="addr">{r.address}</span>
                  </span>
                  {r.distanceMeters != null && <span className="dist">{formatDistance(r.distanceMeters)}</span>}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
