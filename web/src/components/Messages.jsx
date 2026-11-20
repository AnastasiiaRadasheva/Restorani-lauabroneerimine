import { useEffect, useRef, useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { formatTime } from '../utils'

/**
 * Sõnumid broneeringu kohta. Klient kirjutab restoranile, personal vastab.
 * Uued sõnumid laetakse iga 20 sekundi järel.
 */
export default function Messages({ reservationId, staff = false, onSent }) {
  const { t, locale } = useI18n()
  const [list, setList] = useState(null)
  const [draft, setDraft] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const box = useRef(null)

  useEffect(() => {
    const load = () => api(`/api/reservations/${reservationId}/messages`).then(setList).catch((e) => setError(e.message))
    load()
    const timer = setInterval(load, 20000)
    return () => clearInterval(timer)
  }, [reservationId])

  useEffect(() => {
    if (box.current) box.current.scrollTop = box.current.scrollHeight
  }, [list])

  async function send(e) {
    e.preventDefault()
    if (!draft.trim()) return
    setBusy(true)
    try {
      const m = await api(`/api/reservations/${reservationId}/messages`, { method: 'POST', body: { text: draft } })
      setList((l) => [...(l ?? []), m])
      setDraft('')
      setError('')
      onSent?.()
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  // Kliendi vaates on "mina" klient, personali vaates personal
  const mine = (m) => (staff ? m.fromStaff : !m.fromStaff)

  return (
    <div className="messages">
      <p className="eyebrow">{t('messages.title')}</p>
      <div className="msg-list" ref={box}>
        {list?.length === 0 && <p className="italic muted small">{t('messages.empty')}</p>}
        {list?.map((m) => (
          <div key={m.id} className={mine(m) ? 'msg mine' : 'msg'}>
            <span className="who">{mine(m) ? t('messages.you') : m.fromStaff ? 'Argent' : m.authorName} · {formatTime(m.createdAt, locale)}</span>
            <span>{m.text}</span>
          </div>
        ))}
      </div>
      <form className="msg-form" onSubmit={send}>
        <label className="field">
          {t('messages.yours')}
          <textarea rows={2} maxLength={1000} value={draft} onChange={(e) => setDraft(e.target.value)} />
        </label>
        <button className="btn btn-dark" disabled={busy || !draft.trim()}>{t('messages.send')}</button>
      </form>
      {error && <p className="field-error">{error}</p>}
    </div>
  )
}
