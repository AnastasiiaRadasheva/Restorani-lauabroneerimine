import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useI18n } from '../i18n'
import { Arrow } from './Icons'

export default function Footer() {
  const { t } = useI18n()
  const [restaurants, setRestaurants] = useState([])
  const [email, setEmail] = useState('')
  const [subscribed, setSubscribed] = useState(false)

  useEffect(() => {
    api('/api/restaurants').then(setRestaurants).catch(() => {})
  }, [])

  // Näidisvorm: e-posti ei salvestata (kutsete saatmine pole selle projekti osa)
  function subscribe(e) {
    e.preventDefault()
    if (email.includes('@')) setSubscribed(true)
  }

  return (
    <footer className="footer">
      <div className="footer-inner">
        <div className="footer-cols">
          <div className="footer-brand">
            <span className="word">ARGENT</span>
            <span className="tag">{t('brand.tagline')}</span>
            <form className="newsletter" onSubmit={subscribe}>
              <label htmlFor="news">{t('footer.invites')}</label>
              {subscribed ? (
                <p className="thanks">{t('footer.thanks')}</p>
              ) : (
                <div className="row">
                  <input id="news" type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder={t('footer.email')} />
                  <button aria-label={t('footer.subscribe')}><Arrow width={28} /></button>
                </div>
              )}
            </form>
          </div>

          <nav className="footer-col" aria-label={t('footer.sections')}>
            <span className="head">{t('footer.sections')}</span>
            <Link to="/">{t('nav.home')}</Link>
            <Link to="/broneeri">{t('nav.booking')}</Link>
          </nav>

          <div className="footer-col">
            <span className="head">{t('footer.addresses')}</span>
            {restaurants.map((r) => (
              <span key={r.id}>
                {r.name}
                <br />
                <span className="sub">{r.address}</span>
              </span>
            ))}
          </div>

          <div className="footer-col">
            <span className="head">{t('footer.contact')}</span>
            <span>+372 600 0000</span>
            <a href="mailto:argent@restoran.ee">argent@restoran.ee</a>
            <span className="sub">{t('footer.hours')}</span>
          </div>
        </div>

        <div className="footer-bottom">
          <span>© 2026 Argent</span>
          <span>{t('footer.school')}</span>
        </div>
      </div>
    </footer>
  )
}
