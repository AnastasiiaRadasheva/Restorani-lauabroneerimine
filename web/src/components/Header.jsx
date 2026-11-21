import { useEffect, useRef, useState } from 'react'
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { CloseIcon, MenuIcon, UserIcon } from './Icons'
import Notifications from './Notifications'

/** Keelevalik ET / RU / EN. */
export function LangSwitch() {
  const { lang, setLang, languages, t } = useI18n()
  return (
    <div className="lang-switch" role="group" aria-label={t('nav.language')}>
      {languages.map((l) => (
        <button key={l.code} type="button" lang={l.code} aria-label={l.name} aria-pressed={lang === l.code} className={lang === l.code ? 'on' : ''} onClick={() => setLang(l.code)}>
          {l.short}
        </button>
      ))}
    </div>
  )
}

export default function Header() {
  const { user, isStaff, isAdmin, logout } = useAuth()
  const { t } = useI18n()
  const [open, setOpen] = useState(false)
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const ref = useRef(null)

  // Päise kõrgus CSS-i muutujasse, et sammude riba jääks alati päise alla
  useEffect(() => {
    const el = ref.current
    if (!el) return undefined
    const update = () => document.documentElement.style.setProperty('--header-h', `${el.offsetHeight}px`)
    update()
    const observer = new ResizeObserver(update)
    observer.observe(el)
    return () => observer.disconnect()
  }, [])

  // Menüü sulgub lehe vahetusel
  useEffect(() => {
    setOpen(false)
  }, [pathname])

  function onLogout() {
    logout()
    setOpen(false)
    navigate('/')
  }

  const links = (
    <>
      <NavLink to="/" end>{t('nav.home')}</NavLink>
      <NavLink to="/broneeri">{t('nav.booking')}</NavLink>
      <NavLink to="/menuu">{t('nav.menu')}</NavLink>
      {isStaff && <NavLink to="/personal/plaan">Plaan</NavLink>}
      {isStaff && <NavLink to="/personal/broneeringud">Broneeringud</NavLink>}
      {isAdmin && <NavLink to="/admin">Admin</NavLink>}
    </>
  )

  return (
    <header ref={ref} className={open ? 'header open' : 'header'}>
      <div className="header-inner">
        <button className="menu-toggle" onClick={() => setOpen(!open)} aria-label={t('nav.menu')} aria-expanded={open}>
          {open ? <CloseIcon /> : <MenuIcon />}
        </button>

        <nav className="nav" aria-label={t('nav.menu')}>{links}</nav>

        <Link to="/" className="logo">ARGENT</Link>

        <div className="nav-right">
          <LangSwitch />
          {user ? (
            <>
              {!isStaff && <NavLink to="/minu">{t('nav.my')}</NavLink>}
              <Notifications />
              <button className="link underline" onClick={onLogout} title={user.name}>{t('nav.logout')}</button>
            </>
          ) : (
            <NavLink to="/login" className="link underline">{t('nav.login')}</NavLink>
          )}
        </div>

        {/* Telefonis: kasutaja ikoon paremal */}
        <Link to={user ? (isStaff ? '/personal/plaan' : '/minu') : '/login'} className="user-icon" aria-label={user ? t('nav.my') : t('nav.login')}>
          <UserIcon />
        </Link>
      </div>

      {/* Telefoni menüü */}
      {open && (
        <div className="mobile-menu">
          <nav aria-label={t('nav.menu')}>
            {links}
            {user && !isStaff && <NavLink to="/minu">{t('nav.my')}</NavLink>}
            {user ? (
              <button type="button" onClick={onLogout}>{t('nav.logout')}</button>
            ) : (
              <NavLink to="/login">{t('nav.login')}</NavLink>
            )}
          </nav>
          {user && <p className="eyebrow">{user.name}</p>}
        </div>
      )}
    </header>
  )
}
