import { useEffect, useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'

/** Kellukese ikoon teadetega (meeldetuletused saadab taustateenus). */
export default function Notifications() {
  const { t, locale } = useI18n()
  const [items, setItems] = useState([])
  const [open, setOpen] = useState(false)

  useEffect(() => {
    const load = () => api('/api/notifications/my').then(setItems).catch(() => {})
    load()
    const timer = setInterval(load, 60000) // kord minutis
    return () => clearInterval(timer)
  }, [])

  const unread = items.filter((n) => !n.isRead).length

  async function toggle() {
    setOpen(!open)
    if (!open && unread > 0) {
      await Promise.all(items.filter((n) => !n.isRead).map((n) => api(`/api/notifications/${n.id}/read`, { method: 'PATCH' })))
      setTimeout(() => setItems((list) => list.map((n) => ({ ...n, isRead: true }))), 3000)
    }
  }

  return (
    <div className="bell">
      <button className="bell-btn" onClick={toggle} aria-label={t('bell.title')} aria-expanded={open}>
        <svg aria-hidden="true" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.3">
          <path d="M6 16V11a6 6 0 0 1 12 0v5l1.5 2h-15z" />
          <path d="M10 20.5a2 2 0 0 0 4 0" />
        </svg>
        {unread > 0 && <span className="bell-count">{unread}</span>}
      </button>
      {open && (
        <div className="bell-menu">
          <p className="eyebrow">{t('bell.title')}</p>
          {items.length === 0 && <p className="italic muted small">{t('bell.empty')}</p>}
          {items.map((n) => (
            <div key={n.id} className={n.isRead ? 'bell-item' : 'bell-item unread'}>
              <div>{n.message}</div>
              <small className="muted">{new Date(n.createdAt).toLocaleString(locale)}</small>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
