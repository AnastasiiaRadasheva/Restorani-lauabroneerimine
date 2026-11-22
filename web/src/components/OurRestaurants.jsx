import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useI18n } from '../i18n'
import RestaurantMap from './RestaurantMap'
import { useReveal } from './SignatureDishes'

/** Iga restorani foto (avalehe kaardil). */
const PHOTO = { 1: '/img/menu/set-argent.jpg', 2: '/img/menu/krabi.jpg', 3: '/img/menu/set-teras.jpg' }
const hhmm = (s) => (s ?? '').slice(0, 5)

/** Avalehe plokk "Meie restoranid": kaart + kolm kaarti aadressi, kellaaja ja broneerimisega. */
export default function OurRestaurants() {
  const { t } = useI18n()
  const [restaurants, setRestaurants] = useState([])
  const [selected, setSelected] = useState(null)
  const ref = useReveal()

  useEffect(() => {
    api('/api/restaurants').then((l) => setRestaurants([...l].sort((a, b) => a.id - b.id))).catch(() => {})
  }, [])

  if (restaurants.length === 0) return null
  return (
    <section className="section home-places" ref={ref} id="restoranid">
      <div className="container">
        <div className="sec-head reveal">
          <div className="title">
            <span className="num">◆</span>
            <h2>{t('home.locTitle')}</h2>
          </div>
          <p>{t('home.locLead')}</p>
        </div>
        <div className="places-grid">
          <div className="places-map reveal">
            <RestaurantMap
              restaurants={restaurants}
              selectedId={selected}
              onSelect={(r) => setSelected(r.id)}
              labels={{ me: t('booking.mapMe'), distance: t('booking.mapDistance') }}
            />
          </div>
          <div className="places-list">
            {restaurants.map((r, i) => (
              <article
                key={r.id}
                className={`place-card reveal d${i}${selected === r.id ? ' on' : ''}`}
                onMouseEnter={() => setSelected(r.id)}
                onFocus={() => setSelected(r.id)}
                tabIndex={0}
              >
                <img src={PHOTO[r.id] ?? '/img/helmet.jpg'} alt="" loading="lazy" />
                <div className="place-text">
                  <p className="eyebrow">{t('home.locHours', { from: hhmm(r.opensAt), to: hhmm(r.closesAt) })}</p>
                  <h3>{r.name}</h3>
                  <p className="place-addr">{r.address}</p>
                  {r.description && <p className="place-desc">{r.description}</p>}
                  <div className="place-actions">
                    <Link to={`/broneeri?restaurant=${r.id}`} className="btn btn-dark btn-small">{t('home.book')}</Link>
                    <a className="link-under" href={`https://www.openstreetmap.org/directions?to=${r.latitude}%2C${r.longitude}`} target="_blank" rel="noreferrer">{t('home.locRoute')}</a>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
