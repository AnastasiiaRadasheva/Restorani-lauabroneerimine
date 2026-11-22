import { useEffect, useRef, useState } from 'react'
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { CloseIcon, MenuIcon, UserIcon } from './Icons'
import Notifications from './Notifications'
import { useTr } from '../staff'

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
  const tr = useTr()
  const here = useLocation()
  // Pärast sisselogimist tagasi samale lehele (nt pooleli broneering)
  const backHere = ['/login', '/register'].includes(here.pathname) ? here.state : { from: here.pathname + here.search }
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
      {!isStaff && <NavLink to="/" end>{t('nav.home')}</NavLink>}
      {!isStaff && <NavLink to="/broneeri">{t('nav.booking')}</NavLink>}
      {!isStaff && <NavLink to="/menuu">{t('nav.menu')}</NavLink>}
      {!isStaff && <NavLink to="/tugi">{t('nav.support')}</NavLink>}
      {isAdmin && <NavLink to="/admin">{tr('Töölaud', 'Рабочий стол', 'Dashboard')}</NavLink>}
      {isStaff && <NavLink to="/personal/plaan">{tr('Plaan', 'План', 'Plan')}</NavLink>}
      {isStaff && <NavLink to="/personal/kalender">{tr('Kalender', 'Календарь', 'Calendar')}</NavLink>}
      {isStaff && <NavLink to="/personal/broneeringud">{tr('Broneeringud', 'Брони', 'Bookings')}</NavLink>}
      {isStaff && <NavLink to="/personal/uus">+ {tr('Uus broneering', 'Новая бронь', 'New booking')}</NavLink>}
      {isStaff && <NavLink to="/personal/tugi">{tr('Küsimused', 'Вопросы', 'Questions')}</NavLink>}
    </>
  )

  return (
    <header ref={ref} className={open ? 'header open' : 'header'}>
      <div className="header-inner">
        <button className="menu-toggle" onClick={() => setOpen(!open)} aria-label={t('nav.menu')} aria-expanded={open}>
          {open ? <CloseIcon /> : <MenuIcon />}
        </button>

        <nav className="nav" aria-label={t('nav.menu')}>{links}</nav>

        <Link to={isAdmin ? '/admin' : isStaff ? '/personal/plaan' : '/'} className="logo">ARGENT</Link>

        <div className="nav-right">
          <LangSwitch />
          {user ? (
            <>
              {!isStaff && <NavLink to="/minu">{t('nav.my')}</NavLink>}
              <Notifications />
              {!isStaff && (
                <Link to="/profiil" className="avatar-link" title={user.name} aria-label={t('profile.title')}>
                  {user.avatarUrl ? <img src={user.avatarUrl} alt="" /> : <UserIcon />}
                </Link>
              )}
              <button className="link underline" onClick={onLogout} title={user.name}>{t('nav.logout')}</button>
            </>
          ) : (
            <NavLink to="/login" state={backHere} className="link underline">{t('nav.login')}</NavLink>
          )}
        </div>

        {/* Telefonis: kasutaja ikoon paremal */}
        <Link to={!user ? '/login' : isStaff ? '/personal/plaan' : '/profiil'} state={!user ? backHere : undefined} className="user-icon" aria-label={user ? t('nav.my') : t('nav.login')}>
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
              <NavLink to="/login" state={backHere}>{t('nav.login')}</NavLink>
            )}
          </nav>
          {user && <p className="eyebrow">{user.name}</p>}
        </div>
      )}
    </header>
  )
}
