import { useEffect, useRef } from 'react'
import { Link } from 'react-router-dom'
import { useI18n } from '../i18n'
import { money } from '../utils'
import { useMenu } from './booking/GastronomySection'
import DishLink from './DishInfo'
import { Arrow } from './Icons'

/** Maja firmaroad – valitud fotode järgi (sama roog on kõigis restoranides). */
const SIGNATURE = ['austrid', 'krabi', 'tartar', 'sokolaad']
const keyOf = (m) => m.imageUrl?.split('/').pop()?.replace(/\.[a-z]+$/i, '')

/** Lisab elemendile klassi "in", kui see kerimisel nähtavale tuleb (sujuv ilmumine). */
export function useReveal() {
  const ref = useRef(null)
  useEffect(() => {
    const el = ref.current
    if (!el) return undefined
    if (!('IntersectionObserver' in window)) { el.classList.add('in'); return undefined }
    const io = new IntersectionObserver((entries) => {
      entries.forEach((e) => { if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target) } })
    }, { threshold: 0.15 })
    el.querySelectorAll('.reveal').forEach((x) => io.observe(x))
    return () => io.disconnect()
  })
  return ref
}

/** Avalehe plokk "Meie firmaroad" + jooksev ribbon roogade nimedega. */
export default function SignatureDishes() {
  const { t } = useI18n()
  const menu = useMenu(1)
  const ref = useReveal()
  const dishes = SIGNATURE.map((k) => menu.find((m) => keyOf(m) === k)).filter(Boolean)
  const ticker = menu.filter((m) => m.category !== menu.find((x) => /vein|wine|вин/i.test(x.category))?.category).map((m) => m.name)

  if (dishes.length === 0) return null
  return (
    <>
      <section className="section signature" ref={ref}>
        <div className="container">
          <div className="sec-head reveal">
            <div className="title">
              <span className="num">◆</span>
              <h2>{t('home.sigTitle')}</h2>
            </div>
            <p>{t('home.sigLead')}</p>
          </div>
          <div className="sig-grid">
            {dishes.map((m, i) => (
              <DishLink key={m.id} item={m} className={`sig-card reveal d${i}`}>
                <span className="sig-photo"><img src={m.imageUrl} alt="" loading="lazy" /></span>
                <span className="sig-no">{String(i + 1).padStart(2, '0')}</span>
                <span className="sig-text">
                  <span className="sig-name">{m.name}</span>
                  <span className="sig-desc">{m.description}</span>
                  <span className="sig-price">{money(m.price)}<em> · {t('dish.more')}</em></span>
                </span>
              </DishLink>
            ))}
          </div>
          <Link to="/menuu" className="link-under sig-all reveal">{t('home.sigAll')} <Arrow /></Link>
        </div>
      </section>

      {ticker.length > 0 && (
        <div className="ticker" aria-hidden="true">
          <div className="ticker-track">
            {[...ticker, ...ticker].map((n, i) => <span key={i}>{n}<i>◆</i></span>)}
          </div>
        </div>
      )}
    </>
  )
}
