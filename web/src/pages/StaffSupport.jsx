import { useEffect, useState } from 'react'
import { api } from '../api'
import { fmtDate, fmtTime, useTr } from '../staff'
import QuickReplies from '../components/QuickReplies'

/** Personal: klientide küsimused ja vastamine. */
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
    const timer = setInterval(loadThreads, 10000)
    return () => clearInterval(timer)
  }, [])

  useEffect(() => {
    if (!current) return
    api(`/api/support/threads/${current.userId}`).then(setMessages).then(loadThreads).catch((e) => setError(e.message))
  }, [current?.userId]) // eslint-disable-line react-hooks/exhaustive-deps

  async function reply(e) {
    e.preventDefault()
    if (!draft.trim() || !current) return
    try {
      const m = await api(`/api/support/threads/${current.userId}`, { method: 'POST', body: { text: draft } })
      setMessages((l) => [...l, m])
      setDraft('')
      loadThreads()
    } catch (err) {
      setError(err.message)
    }
  }

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
            <button key={th.userId} type="button" className={current?.userId === th.userId ? 'thread on' : 'thread'} onClick={() => setCurrent(th)}>
              <span className="row-between">
                <strong>{th.name}</strong>
                {th.unread > 0 && <span className="badge badge-Active">{th.unread} {tr('uut', 'новых', 'new')}</span>}
              </span>
              <span className="small muted">{th.email}{th.phone ? ` · ${th.phone}` : ''}</span>
              <span className="small">{th.lastText}</span>
              <span className="small muted">{fmtDate(th.lastAt)} {fmtTime(th.lastAt)}</span>
            </button>
          ))}
        </div>

        <div className="paper white messages">
          {!current && <p className="italic muted">{tr('Valige vasakult vestlus.', 'Выберите диалог слева.', 'Choose a conversation on the left.')}</p>}
          {current && (
            <>
              <p className="card-title"><a href={`/personal/klient?q=${encodeURIComponent(current.email)}`}>{current.name}</a></p>
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
