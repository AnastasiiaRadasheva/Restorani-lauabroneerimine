import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useMenu } from '../components/booking/GastronomySection'
import { Arrow } from '../components/Icons'
import { useI18n } from '../i18n'
import { money } from '../utils'

/** Menüü leht: restorani valik ja toidud kategooriate kaupa. */
export default function Menu() {
  const { t } = useI18n()
  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantId] = useState(null)
  const menu = useMenu(restaurantId)
  const categories = [...new Set(menu.map((m) => m.category))]

  useEffect(() => {
    api('/api/restaurants').then((l) => {
      setRestaurants(l)
      setRestaurantId(l[0]?.id ?? null)
    })
  }, [])

  return (
    <div className="menu-page">
      <section className="section first">
        <div className="container">
          <div className="sec-head">
            <div className="title">
              <span className="num">◆</span>
              <h1>{t('nav.menu')}</h1>
            </div>
            <p>{t('menu.lead')}</p>
          </div>

          <div className="chips">
            {restaurants.map((r) => (
              <button key={r.id} type="button" className={r.id === restaurantId ? 'chip on' : 'chip'} onClick={() => setRestaurantId(r.id)}>
                {r.name}
              </button>
            ))}
          </div>

          <div className="menu-cols">
            {categories.map((c) => (
              <div key={c} className="menu-cat">
                <h2>{c}</h2>
                {menu.filter((m) => m.category === c).map((m) => (
                  <div key={m.id} className="menu-row">
                    <div>
                      <span className="name">{m.name}</span>
                      <span className="desc">{m.description}</span>
                    </div>
                    <span className="dots" />
                    <span className="price">{money(m.price)}</span>
                  </div>
                ))}
              </div>
            ))}
          </div>

          <Link to={`/broneeri?restaurant=${restaurantId ?? ''}`} className="btn-chamfer menu-book">
            {t('menu.book')}
            <Arrow />
          </Link>
        </div>
      </section>
    </div>
  )
}
