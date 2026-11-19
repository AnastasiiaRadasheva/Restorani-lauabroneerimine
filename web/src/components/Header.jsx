import { useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'
import { MenuIcon } from './Icons'

export default function Header() {
  const { user, isStaff, isAdmin, logout } = useAuth()
  const { t } = useI18n()
  const [open, setOpen] = useState(false)
  const navigate = useNavigate()
  const close = () => setOpen(false)

  function onLogout() {
    logout()
    close()
    navigate('/')
  }

  return (
    <header className={open ? 'header open' : 'header'}>
      <div className="header-inner">
        <button className="menu-toggle" onClick={() => setOpen(!open)} aria-label="Menüü" aria-expanded={open}>
          <MenuIcon />
        </button>

        <nav className="nav" aria-label="Põhimenüü">
          <NavLink to="/" end onClick={close}>{t('nav.home')}</NavLink>
          <NavLink to="/broneeri" onClick={close}>{t('nav.booking')}</NavLink>
          <NavLink to="/menuu" onClick={close}>{t('nav.menu')}</NavLink>
          {isStaff && <NavLink to="/personal/plaan" onClick={close}>Plaan</NavLink>}
          {isStaff && <NavLink to="/personal/broneeringud" onClick={close}>Broneeringud</NavLink>}
          {isAdmin && <NavLink to="/admin" onClick={close}>Admin</NavLink>}
        </nav>

        <Link to="/" className="logo" onClick={close}>ARGENT</Link>

        <div className="nav-right">
          {user ? (
            <>
              {!isStaff && <NavLink to="/minu" onClick={close}>{t('nav.my')}</NavLink>}
              <span className="user">{user.name}</span>
              <button className="link underline" onClick={onLogout}>{t('nav.logout')}</button>
            </>
          ) : (
            <NavLink to="/login" className="link underline" onClick={close}>{t('nav.login')}</NavLink>
          )}
        </div>
      </div>
    </header>
  )
}
