import { useState } from 'react'
import { api } from '../api'
import { useTr } from '../staff'

/** Valmis tekstid kolmes keeles korraga – külalised räägivad eri keeli. */
const TEMPLATES = [
  ['Restoran suletud', 'Ресторан закрыт', 'Restaurant closed',
    'Vabandame! Restoran on sel päeval ootamatult suletud. Teie broneering tühistatakse ja makstud summa tagastatakse täies ulatuses.\nИзвините! В этот день ресторан неожиданно закрыт. Ваша бронь будет отменена, оплаченная сумма вернётся полностью.\nWe apologise! The restaurant is unexpectedly closed on this day. Your booking will be cancelled and the amount paid fully refunded.'],
  ['Hilinenud avamine', 'Откроемся позже', 'Opening late',
    'Vabandame! Teie külastuse päeval avame tehniliste põhjuste tõttu 30 minutit hiljem. Teie laud ootab Teid.\nИзвините! В день Вашего визита мы по техническим причинам откроемся на 30 минут позже. Ваш стол Вас ждёт.\nWe apologise! On the day of your visit we open 30 minutes later for technical reasons. Your table is waiting for you.'],
  ['Terrass suletud', 'Терраса закрыта', 'Terrace closed',
    'Ilmaolude tõttu on terrass Teie külastuse päeval suletud – istutame Teid saali.\nИз-за погоды в день Вашего визита терраса закрыта — мы посадим Вас в зале.\nDue to the weather the terrace is closed on the day of your visit – we will seat you inside.'],
  ['Meeldetuletus', 'Напоминание', 'Reminder',
    'Ootame Teid! Kui hilinete, andke palun teada broneeringu sõnumites.\nЖдём Вас! Если опаздываете, пожалуйста, напишите в сообщениях брони.\nWe look forward to seeing you! If you are running late, please let us know in the booking messages.'],
]

/** Personal: kirjuta korraga kõigile valitud päeva külalistele (nt "restoran on suletud"). */
export default function DayBroadcast({ date, restaurantId, count, onSent }) {
  const tr = useTr()
  const [open, setOpen] = useState(false)
  const [text, setText] = useState('')
  const [smsEmail, setSmsEmail] = useState(true)
  const [cancelAll, setCancelAll] = useState(false)
  const [busy, setBusy] = useState(false)
  const [msg, setMsg] = useState(null)

  async function send() {
    if (text.trim().length < 2) return
    const q = cancelAll
      ? tr(`Saata sõnum ja TÜHISTADA ${count} broneeringut (raha tagastatakse)?`, `Отправить сообщение и ОТМЕНИТЬ брони (${count}) с полным возвратом?`, `Send the message and CANCEL ${count} bookings (full refund)?`)
      : tr(`Saata sõnum ${count} külalisele?`, `Отправить сообщение ${count} гостям?`, `Send the message to ${count} guests?`)
    if (!window.confirm(q)) return
    setBusy(true)
    try {
      const n = await api('/api/reservations/broadcast', { method: 'POST', body: { date, restaurantId: restaurantId ? Number(restaurantId) : null, text, alsoSmsEmail: smsEmail, cancelAll } })
      setMsg({ ok: true, text: tr(`Saadetud ${n} külalisele ✓`, `Отправлено гостям: ${n} ✓`, `Sent to ${n} guests ✓`) })
      setText('')
      setOpen(false)
      onSent?.()
    } catch (e) {
      setMsg({ ok: false, text: e.message })
    } finally {
      setBusy(false)
    }
  }

  // Kui päeval pole enam aktiivseid broneeringuid (nt kõik tühistati), jääb alles ainult tulemuse teade
  if (count === 0 && !msg) return null
  return (
    <div className="broadcast">
      {!open && (
        <button className="btn btn-dark" onClick={() => { setOpen(true); setMsg(null) }}>
          ✉ {tr('Kirjuta kõigile päeva külalistele', 'Написать всем гостям этого дня', 'Message all guests of this day')} ({count})
        </button>
      )}
      {msg && <p className={msg.ok ? 'alert alert-ok' : 'alert alert-error'}>{msg.text}</p>}
      {open && (
        <div className="paper white broadcast-form">
          <p className="eyebrow">{tr('Valmis tekstid (kolmes keeles)', 'Готовые тексты (на трёх языках)', 'Ready texts (in three languages)')}</p>
          <div className="chips">
            {TEMPLATES.map(([et, ru, en, body]) => (
              <button key={et} type="button" className="chip small" onClick={() => { setText(body); setCancelAll(et === 'Restoran suletud') }}>{tr(et, ru, en)}</button>
            ))}
          </div>
          <label className="field">{tr('Sõnum', 'Сообщение', 'Message')}
            <textarea rows={5} maxLength={1000} value={text} onChange={(e) => setText(e.target.value)} />
          </label>
          <label className="check"><input type="checkbox" checked={smsEmail} onChange={(e) => setSmsEmail(e.target.checked)} /> {tr('Saada ka SMS ja e-kiri', 'Также отправить SMS и e-mail', 'Also send SMS and e-mail')}</label>
          <label className="check ox"><input type="checkbox" checked={cancelAll} onChange={(e) => setCancelAll(e.target.checked)} /> {tr('Tühista need broneeringud (makstud summa tagastatakse täies ulatuses)', 'Отменить эти брони (оплаченная сумма вернётся полностью)', 'Cancel these bookings (the amount paid is fully refunded)')}</label>
          <p className="small muted">{tr(
            'Sõnum lisatakse iga broneeringu vestlusesse ja külaline saab teate.',
            'Сообщение попадёт в переписку каждой брони, гость получит уведомление.',
            'The message is added to each booking conversation and the guest is notified.')}</p>
          <div className="actions">
            <button className="btn btn-dark" disabled={busy || text.trim().length < 2} onClick={send}>{tr('Saada', 'Отправить', 'Send')} ({count})</button>
            <button className="btn" onClick={() => setOpen(false)}>{tr('Loobu', 'Отмена', 'Cancel')}</button>
          </div>
        </div>
      )}
    </div>
  )
}
