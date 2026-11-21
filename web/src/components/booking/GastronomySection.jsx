import { useEffect, useState } from 'react'
import { api, qs } from '../../api'
import { useI18n } from '../../i18n'
import { money } from '../../utils'

const ROMAN = ['i', 'ii', 'iii', 'iv', 'v', 'vi', 'vii', 'viii']

/** Laeb restorani menüü (ainult saadaval olevad toidud). */
export function useMenu(restaurantId) {
  const [menu, setMenu] = useState([])
  useEffect(() => {
    if (!restaurantId) return
    api('/api/menu' + qs({ restaurantId, onlyAvailable: true, pageSize: 100 }))
      .then((p) => setMenu(p.items))
      .catch(() => setMenu([]))
  }, [restaurantId])
  return menu
}

/** III. Gastronoomia: toitude eeltellimus kategooriate kaupa. Paremal must kast kogusummaga. */
export default function GastronomySection({ menu, qty, setQty, extra }) {
  const { t } = useI18n()
  const categories = [...new Set(menu.map((m) => m.category))]
  const [cat, setCat] = useState(null)
  const current = cat && categories.includes(cat) ? cat : categories[0]

  const change = (id, d) => setQty((q) => ({ ...q, [id]: Math.max(0, Math.min(50, (q[id] ?? 0) + d)) }))
  const total = menu.reduce((sum, m) => sum + (qty[m.id] ?? 0) * m.price, 0)
  const count = Object.values(qty).reduce((a, b) => a + b, 0)
  // Kõige pikema kategooria jagu ruumi, et leht ei hüppaks kategooriat vahetades
  const maxRows = Math.max(1, ...categories.map((c) => menu.filter((m) => m.category === c).length))

  return (
    <section id="menu" className="section">
      <div className="container">
        <div className="sec-head">
          <div className="title">
            <span className="num">III.</span>
            <h2>{t('gastro.title')}</h2>
          </div>
          <p>{t('gastro.lead')}</p>
        </div>

        <div className="gastro">
          <div className="gastro-main">
            <div className="cat-tabs" role="tablist">
              {categories.map((c) => (
                <button key={c} type="button" role="tab" aria-selected={c === current} className={c === current ? 'chip caps on' : 'chip caps'} onClick={() => setCat(c)}>
                  {c}
                </button>
              ))}
            </div>

            <div className="dish-list" style={{ minHeight: `${maxRows * 150}px` }}>
            {menu.filter((m) => m.category === current).map((m, i) => (
              <div key={m.id} className="dish">
                <span className="n">{ROMAN[i] ?? i + 1}</span>
                {m.imageUrl && <img className="dish-photo" src={m.imageUrl} alt={m.name} loading="lazy" />}
                <div className="text">
                  <span className="eyebrow">{m.category}</span>
                  <span className="name">{m.name}</span>
                  <span className="desc">{m.description}</span>
                </div>
                <span className="price">{money(m.price)}</span>
                <div className="stepper round">
                  <button type="button" onClick={() => change(m.id, -1)} aria-label={t('gastro.remove', { name: m.name })}>−</button>
                  <span className="value">{qty[m.id] ?? 0}</span>
                  <button type="button" onClick={() => change(m.id, 1)} aria-label={t('gastro.add', { name: m.name })}>+</button>
                </div>
              </div>
            ))}
            </div>
            {extra}
          </div>

          <aside className="dark-card sticky-card">
            <p className="eyebrow">{t('gastro.preorder')}</p>
            <span className="big">{money(total)}</span>
            <p className="text">{count ? t('gastro.served', { n: count }) : t('gastro.empty')}</p>
          </aside>
        </div>
      </div>
    </section>
  )
}
