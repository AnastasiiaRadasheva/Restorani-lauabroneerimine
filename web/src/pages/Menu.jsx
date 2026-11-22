import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useMenu } from '../components/booking/GastronomySection'
import DishLink from '../components/DishInfo'
import DishTags, { TagLegend } from '../components/DishTags'
import { Arrow } from '../components/Icons'
import { useI18n } from '../i18n'
import { money } from '../utils'

/** Menüü leht: menüü on kõigis restoranides sama. Klassikaline nimekiri koos väikeste fotodega. */
export default function Menu() {
  const { t } = useI18n()
  const [restaurantId, setRestaurantId] = useState(null)
  const menu = useMenu(restaurantId)
  const categories = [...new Set(menu.map((m) => m.category))]

  useEffect(() => {
    api('/api/restaurants').then((l) => setRestaurantId(l[0]?.id ?? null))
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

          <TagLegend />

          <div className="menu-cols">
            {categories.map((c) => (
              <div key={c} className="menu-cat">
                <h2>{c}</h2>
                {menu.filter((m) => m.category === c).map((m) => (
                  <div key={m.id} className="menu-row">
                    {m.imageUrl && <DishLink item={m} className="dish-pic"><img className="menu-thumb" src={m.imageUrl} alt="" loading="lazy" /></DishLink>}
                    <div className="menu-text">
                      <DishLink item={m} className="dish-name"><span className="name">{m.name}</span></DishLink>
                      <span className="desc">{m.description}</span>
                      <DishTags tags={m.tags} />
                    </div>
                    <span className="dots" />
                    <span className="price">{money(m.price)}</span>
                  </div>
                ))}
              </div>
            ))}
          </div>

          <Link to="/broneeri" className="btn-chamfer menu-book">
            {t('menu.book')}
            <Arrow />
          </Link>
        </div>
      </section>
    </div>
  )
}
