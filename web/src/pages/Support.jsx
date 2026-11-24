import { Link } from 'react-router-dom'
import { useEffect, useRef, useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { useAuth } from '../auth'
import { useGuestSocket, useTableSocket } from '../realtime'
import { useLeaveWarning } from '../leaveGuard'
import { formatDate, formatTime } from '../utils'

// Sisse logitud kliendi mustand hoitakse brauseris, et kirjutatu ei kaoks
const DRAFT_KEY = 'argent-support-draft'
function readDraft() {
  try {
    return JSON.parse(localStorage.getItem(DRAFT_KEY)) ?? {}
  } catch {
    return {}
  }
}
function writeDraft(d) {
  try {
    if (d.text?.trim() || d.name?.trim()) localStorage.setItem(DRAFT_KEY, JSON.stringify(d))
    else localStorage.removeItem(DRAFT_KEY)
  } catch {
    /* privaatne aken – mustand jääb ainult lehele */
  }
}

// Külalise vestluse võti: uus iga kord, kui leht avatakse (konto puudub, seega lehelt lahkudes vestlus kaob)
function newKey() {
  if (crypto.randomUUID) return crypto.randomUUID()
  const hex = [...crypto.getRandomValues(new Uint8Array(16))].map((b) => b.toString(16).padStart(2, '0')).join('')
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-4${hex.slice(13, 16)}-8${hex.slice(17, 20)}-${hex.slice(20, 32)}`
}

/**
 * Klienditugi: vestlusruum restoraniga ja kuum liin.
 * Sõnumid liiguvad reaalajas WebSocketi kaudu (sõnum SupportMessage).
 * Sisse logitud kliendi vestlus salvestub kontole. Külaline saab kirjutada ilma kontota,
 * kuid tema vestlus on seotud ainult selle lehega – lahkudes see kaob (hoiatame enne).
 */
export default function Support() {
  const { t, locale } = useI18n()
  const { user } = useAuth()
  const saved = useRef(user ? readDraft() : {})
  const [guestKey] = useState(newKey)
  const [list, setList] = useState(user ? null : [])
  const [draft, setDraft] = useState(saved.current.text ?? '')
  const [name, setName] = useState(saved.current.name ?? '')
  const [error, setError] = useState('')
  const [sending, setSending] = useState(false)
  const box = useRef(null)

  const guestHasChat = !user && (list?.length > 0 || Boolean(draft.trim()))
  useLeaveWarning(user ? Boolean(draft.trim()) : guestHasChat, user ? t('support.leaveWarn') : t('support.guestLeaveWarn'))

  useEffect(() => {
    if (user) writeDraft({ name, text: draft })
  }, [user, name, draft])

  // Uus sõnum tuleb kohe (ka administraatori vastus): kliendile /ws/tables, külalisele /ws/support
  const onSocket = (msg) => {
    if (msg.type !== 'SupportMessage') return
    setList((l) => (l?.some((m) => m.id === msg.message.id) ? l : [...(l ?? []), msg.message]))
  }
  useTableSocket(onSocket)
  useGuestSocket(user ? null : guestKey, onSocket)

  useEffect(() => {
    if (!user) {
      setList([])
      return undefined
    }
    const load = () => api('/api/support/my').then((l) => {
      setList(l)
      // Eelmine kord kirjutatud nimi (või konto nimi) täidetakse ette
      const mine = l.filter((m) => !m.fromStaff)
      setName((n) => n || mine[mine.length - 1]?.authorName || user?.name?.split(' ')[0] || '')
    }).catch((e) => setError(e.message))
    load()
    // Varuvariant, kui WebSocket on katkenud
    const timer = setInterval(load, 30000)
    return () => clearInterval(timer)
  }, [user?.email]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (box.current) box.current.scrollTop = box.current.scrollHeight
  }, [list])

  async function send(e) {
    e.preventDefault()
    if (!draft.trim()) return
    if (name.trim().length < 2) return setError(t('support.nameNeeded'))
    setSending(true)
    setError('')
    try {
      const url = user ? '/api/support/my' : `/api/support/guest/${guestKey}`
      const m = await api(url, { method: 'POST', body: { text: draft, name } })
      setList((l) => (l?.some((x) => x.id === m.id) ? l : [...(l ?? []), m]))
      setDraft('')
    } catch (err) {
      setError(err.message)
    } finally {
      setSending(false)
    }
  }

  return (
    <div className="support container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{t('support.title')}</h1>
        </div>
      </div>

      <section className="faq" aria-label={t('faq.title')}>
        <h2>{t('faq.title')}</h2>
        {[1, 2, 3, 4, 5, 6, 7].map((n) => (
          <details key={n}>
            <summary>{t(`faq.q${n}`)}</summary>
            <p>{t(`faq.a${n}`)}</p>
          </details>
        ))}
      </section>

      <div className="faq-cta">
        <h2>{t('faq.notFound')} {t('faq.write')}</h2>
        <p className="muted">{t('faq.notFoundLead')}</p>
      </div>

      <div className="support-grid">
        <div className="paper white messages">
          {!user && (
            <p className="italic muted">
              {t('support.guestLead')}{' '}
              <Link to="/login" state={{ from: '/tugi' }}>{t('support.guestLogin')}</Link>
            </p>
          )}
          {(user || list?.length > 0) && (
            <div className="msg-list tall" ref={box} aria-live="polite">
              {list?.length === 0 && <p className="italic muted">{t('support.empty')}</p>}
              {list?.map((m) => (
                <div key={m.id} className={m.fromStaff ? 'msg' : 'msg mine'}>
                  <span className="who">{m.fromStaff ? 'Argent' : `${m.authorName} (${t('messages.you')})`} · {formatDate(m.createdAt, locale)} {formatTime(m.createdAt, locale)}</span>
                  <span>{m.text}</span>
                </div>
              ))}
            </div>
          )}
          <form className="msg-form" onSubmit={send}>
            <label className="field">
              {t('support.name')}
              <input value={name} maxLength={80} onChange={(e) => setName(e.target.value)} placeholder={t('support.namePh')} />
            </label>
            <label className="field">
              {t('messages.yours')}
              <textarea rows={3} maxLength={1000} value={draft} onChange={(e) => setDraft(e.target.value)} />
            </label>
            {user && draft.trim() && <p className="small muted">{t('support.draftKept')}</p>}
            <button className="btn btn-dark" disabled={!draft.trim() || sending}>{t('messages.send')}</button>
          </form>
          {error && <p className="field-error">{error}</p>}
        </div>

        <aside className="dark-card hotline">
          <p className="eyebrow">{t('support.hotline')}</p>
          <span className="big">+372 600 0000</span>
          <p className="text">{t('support.hours')}</p>
          <a className="btn-chamfer silver" href="tel:+3726000000">{t('support.call')}</a>
          <a className="mail" href="mailto:argent@restoran.ee">argent@restoran.ee</a>
        </aside>
      </div>
    </div>
  )
}
