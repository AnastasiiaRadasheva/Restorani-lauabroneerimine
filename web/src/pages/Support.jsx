import { Link } from 'react-router-dom'
import { useEffect, useRef, useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { useAuth } from '../auth'
import { formatDate, formatTime } from '../utils'

/** Klienditugi: vestlus administraatoriga ja kuum liin. */
export default function Support() {
  const { t, locale } = useI18n()
  const [list, setList] = useState(null)
  const [draft, setDraft] = useState('')
  const { user } = useAuth()
  const [name, setName] = useState('')
  const [error, setError] = useState('')
  const box = useRef(null)

  useEffect(() => {
    if (!user) return undefined // külaline näeb KKK-d; vestlus pärast sisselogimist
    const load = () => api('/api/support/my').then((l) => {
      setList(l)
      // Eelmine kord kirjutatud nimi (või konto nimi) täidetakse ette
      const mine = l.filter((m) => !m.fromStaff)
      setName((n) => n || mine[mine.length - 1]?.authorName || user?.name?.split(' ')[0] || '')
    }).catch((e) => setError(e.message))
    load()
    const timer = setInterval(load, 10000)
    return () => clearInterval(timer)
  }, [])

  useEffect(() => {
    if (box.current) box.current.scrollTop = box.current.scrollHeight
  }, [list])

  async function send(e) {
    e.preventDefault()
    if (!draft.trim()) return
    if (name.trim().length < 2) return setError(t('support.nameNeeded'))
    try {
      const m = await api('/api/support/my', { method: 'POST', body: { text: draft, name } })
      setList((l) => [...(l ?? []), m])
      setDraft('')
    } catch (err) {
      setError(err.message)
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
        {!user && (
          <div className="paper white messages">
            <p className="italic">{t('support.loginToWrite')}</p>
            <Link className="btn btn-dark" to="/login" state={{ from: '/tugi' }}>{t('nav.login')}</Link>
          </div>
        )}
        {user && (
        <div className="paper white messages">
          <div className="msg-list tall" ref={box}>
            {list?.length === 0 && <p className="italic muted">{t('support.empty')}</p>}
            {list?.map((m) => (
              <div key={m.id} className={m.fromStaff ? 'msg' : 'msg mine'}>
                <span className="who">{m.fromStaff ? 'Argent' : `${m.authorName} (${t('messages.you')})`} · {formatDate(m.createdAt, locale)} {formatTime(m.createdAt, locale)}</span>
                <span>{m.text}</span>
              </div>
            ))}
          </div>
          <form className="msg-form" onSubmit={send}>
            <label className="field">
              {t('support.name')}
              <input value={name} maxLength={80} onChange={(e) => setName(e.target.value)} placeholder={t('support.namePh')} />
            </label>
            <label className="field">
              {t('messages.yours')}
              <textarea rows={3} maxLength={1000} value={draft} onChange={(e) => setDraft(e.target.value)} />
            </label>
            <button className="btn btn-dark" disabled={!draft.trim()}>{t('messages.send')}</button>
          </form>
          {error && <p className="field-error">{error}</p>}
        </div>
        )}

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
