import { useEffect, useState } from 'react'
import { api } from '../api'
import { fmtDate, fmtTime, useTr } from '../staff'
import QuickReplies from '../components/QuickReplies'
import { useTableSocket } from '../realtime'
import { useLeaveWarning } from '../leaveGuard'

// Vestluse tunnus: kliendil konto id, külalisel (ilma kontota) vestluse võti
const keyOf = (x) => (x.userId != null ? `u${x.userId}` : `g${x.guestKey}`)
const urlOf = (x) => (x.userId != null ? `/api/support/threads/${x.userId}` : `/api/support/threads/guest/${x.guestKey}`)

/** Personal: klientide küsimused ja vastamine. Uued sõnumid tulevad reaalajas (WebSocket, SupportMessage). */
export default function StaffSupport() {
  const tr = useTr()
  const [threads, setThreads] = useState([])
  const [current, setCurrent] = useState(null)
  const [messages, setMessages] = useState([])
  const [draft, setDraft] = useState('')
  const [error, setError] = useState('')

  const loadThreads = () => api('/api/support/threads').then(setThreads).catch((e) => setError(e.message))

  useEffect(() => {
    loadThreads()
    // Varuvariant, kui WebSocket on katkenud
    const timer = setInterval(loadThreads, 30000)
    return () => clearInterval(timer)
  }, [])

  useLeaveWarning(Boolean(draft.trim()), tr('Vastus on saatmata. Kas lahkuda lehelt?', 'Ответ не отправлен. Уйти со страницы?', 'The reply has not been sent. Leave the page?'))

  // Kliendi uus küsimus või teise töötaja vastus ilmub kohe
  useTableSocket((msg) => {
    if (msg.type !== 'SupportMessage') return
    loadThreads()
    if (!current || keyOf(current) !== keyOf(msg)) return
    if (msg.message.fromStaff) setMessages((l) => (l.some((m) => m.id === msg.message.id) ? l : [...l, msg.message]))
    else api(urlOf(msg)).then(setMessages).catch(() => {}) // avatud vestlus: märgitakse loetuks
  })

  useEffect(() => {
    if (!current) return
    api(urlOf(current)).then(setMessages).then(loadThreads).catch((e) => setError(e.message))
  }, [current && keyOf(current)]) // eslint-disable-line react-hooks/exhaustive-deps

  async function reply(e) {
    e.preventDefault()
    if (!draft.trim() || !current) return
    try {
      const m = await api(urlOf(current), { method: 'POST', body: { text: draft } })
      setMessages((l) => (l.some((x) => x.id === m.id) ? l : [...l, m])) // WebSocket võis sama sõnumi juba tuua
      setDraft('')
      loadThreads()
    } catch (err) {
      setError(err.message)
    }
  }

  const guestLabel = tr('külaline ilma kontota', 'гость без аккаунта', 'guest without an account')

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{tr('Klientide küsimused', 'Вопросы гостей', 'Guest questions')}</h1>
        </div>
        <p>{tr('Kuum liin 10:00–20:00. Vastus jõuab kliendile ka teatena (kelluke).', 'Горячая линия 10:00–20:00. Ответ придёт гостю и уведомлением (колокольчик).', 'Hotline 10:00–20:00. The reply also reaches the guest as a notification (bell).')}</p>
      </div>
      {error && <p className="alert alert-error">{error}</p>}

      <div className="support-grid">
        <div className="thread-list">
          {threads.length === 0 && <p className="italic muted">{tr('Küsimusi pole.', 'Вопросов нет.', 'No questions.')}</p>}
          {threads.map((th) => (
            <button key={keyOf(th)} type="button" className={current && keyOf(current) === keyOf(th) ? 'thread on' : 'thread'} onClick={() => setCurrent(th)}>
              <span className="row-between">
                <strong>{th.name}</strong>
                {th.unread > 0 && <span className="badge badge-Active">{th.unread} {tr('uut', 'новых', 'new')}</span>}
              </span>
              <span className="small muted">{th.userId != null ? `${th.email}${th.phone ? ` · ${th.phone}` : ''}` : guestLabel}</span>
              <span className="small">{th.lastText}</span>
              <span className="small muted">{fmtDate(th.lastAt)} {fmtTime(th.lastAt)}</span>
            </button>
          ))}
        </div>

        <div className="paper white messages">
          {!current && <p className="italic muted">{tr('Valige vasakult vestlus.', 'Выберите диалог слева.', 'Choose a conversation on the left.')}</p>}
          {current && (
            <>
              {current.userId != null
                ? <p className="card-title"><a href={`/personal/klient?q=${encodeURIComponent(current.email)}`}>{current.name}</a></p>
                : (
                  <>
                    <p className="card-title">{current.name} <span className="small muted">· {guestLabel}</span></p>
                    <p className="small muted">{tr('Külaline näeb vastust ainult siis, kui ta on veel vestluse lehel.', 'Гость увидит ответ, только если он ещё на странице чата.', 'The guest sees the reply only while still on the chat page.')}</p>
                  </>
                )}
              <div className="msg-list tall">
                {messages.map((m) => (
                  <div key={m.id} className={m.fromStaff ? 'msg mine' : 'msg'}>
                    <span className="who">{m.fromStaff ? 'Argent' : m.authorName} · {fmtDate(m.createdAt)} {fmtTime(m.createdAt)}</span>
                    <span>{m.text}</span>
                  </div>
                ))}
              </div>
              <form className="msg-form" onSubmit={reply}>
                <QuickReplies name={current.name} onPick={setDraft} />
                <label className="field">
                  {tr('Vastus', 'Ответ', 'Reply')}
                  <textarea rows={3} value={draft} onChange={(e) => setDraft(e.target.value)} />
                </label>
                <button className="btn btn-dark" disabled={!draft.trim()}>{tr('Saada', 'Отправить', 'Send')}</button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
