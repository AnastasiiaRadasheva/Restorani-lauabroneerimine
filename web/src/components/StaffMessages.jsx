import { useState } from 'react'
import { fmtDate, fmtTime, useTr } from '../staff'
import { CloseIcon } from './Icons'
import Messages from './Messages'

/** Personali nupp "Sõnumid" broneeringute tabelis: avab külgpaneeli, kus saab kliendile vastata. */
export default function StaffMessages({ r }) {
  const tr = useTr()
  const [open, setOpen] = useState(false)
  const unanswered = r.messageCount > 0 && !r.lastMessageFromStaff

  return (
    <>
      <button className={unanswered ? 'btn btn-small btn-ox' : 'btn btn-small'} onClick={() => setOpen(true)}>
        {tr('Sõnumid', 'Сообщения', 'Messages')}{r.messageCount ? ` (${r.messageCount})` : ''}
      </button>
      {open && (
        <aside className="staff-msg-panel" role="dialog" aria-label="messages">
          <div className="row-between">
            <p className="card-title">{r.customerName}</p>
            <button className="btn btn-small" onClick={() => setOpen(false)} aria-label="close"><CloseIcon /></button>
          </div>
          <p className="muted small">
            {fmtDate(r.startTime)} {fmtTime(r.startTime)} · {r.restaurantName} · {tr('laud', 'стол', 'table')} {r.tableNumber} · {r.guests} {tr('in.', 'гост.', 'gst.')}
          </p>
          {r.extras.length > 0 && <p className="small">{tr('Lisateenused', 'Доп. услуги', 'Extras')}: {r.extras.map((x) => x.name).join(', ')}</p>}
          {r.specialRequest && <p className="small italic">{tr('Soovid', 'Пожелания', 'Requests')}: {r.specialRequest}</p>}
          <Messages reservationId={r.id} staff guestName={r.customerName.split(' ')[0]} />
        </aside>
      )}
    </>
  )
}
