import { Link } from 'react-router-dom'
import EventsSection from '../components/EventsSection'
import NearestRestaurant from '../components/NearestRestaurant'
import OurRestaurants from '../components/OurRestaurants'
import SignatureDishes from '../components/SignatureDishes'
import { Arrow } from '../components/Icons'
import { useI18n } from '../i18n'

export default function Home() {
  const { t } = useI18n()

  return (
    <>
      {/* ===== Suur avaplokk ===== */}
      <section className="hero hero-fit">
        <div className="container">
          <div className="hero-frame">
            <div className="hero-inner">
              <div className="hero-text">
                <p className="kicker">{t('home.kicker')}</p>
                <div>
                  <h1 className="hero-title">ARGENT</h1>
                  <p className="hero-tag">{t('brand.tagline')}</p>
                </div>
                <div className="hero-cta">
                  <p>{t('home.lead')}</p>
                  <Link to="/broneeri" className="btn-chamfer">
                    {t('home.book')}
                    <Arrow />
                  </Link>
                </div>
              </div>

              <figure className="hero-figure">
                <div className="crest crest-anim">
                  <div>
                    <div>
                      <img src="/img/helmet.jpg" alt={t('home.crestAlt')} />
                    </div>
                  </div>
                </div>
                <figcaption>
                  <span>Argent</span>
                  <span>{t('home.crestCaption')}</span>
                </figcaption>
              </figure>
            </div>
          </div>
        </div>
      </section>

      <NearestRestaurant />

      {/* ===== Maja ===== */}
      <section className="house">
        <div className="container">
          <span className="diamond" aria-hidden="true" />
          <h2>{t('home.houseTitle')}</h2>
          <p className="eyebrow">{t('home.houseSub')}</p>
        </div>
      </section>

      {/* ===== Firmaroad + jooksev riba ===== */}
      <SignatureDishes />

      {/* ===== Gastronoomilised õhtud ===== */}
      <EventsSection />

      {/* ===== Meie restoranid kaardil ===== */}
      <OurRestaurants />

      {/* ===== Tsitaat ===== */}
      <section className="quote">
        <div className="container">
          <blockquote>
            <p>{t('home.quote')}</p>
            <footer>
              <span>Anastasiia Radasheva</span>
              <span>{t('home.chefRole')}</span>
            </footer>
          </blockquote>
        </div>
      </section>

      {/* ===== Eraüritused ===== */}
      <section className="private">
        <div className="container">
          <div className="text">
            <p className="eyebrow">{t('home.privateKicker')}</p>
            <p className="big">{t('home.privateTitle')}</p>
            <p>{t('home.privateText')}</p>
          </div>
          <a className="btn btn-big" href="mailto:argent@restoran.ee?subject=Era%C3%BCritus">
            {t('home.privateSend')}
            <Arrow width={28} />
          </a>
        </div>
      </section>
    </>
  )
}
