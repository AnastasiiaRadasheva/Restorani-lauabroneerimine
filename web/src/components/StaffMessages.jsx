import { useState } from 'react'
import { fmtDate, fmtTime } from '../staff'
import { CloseIcon } from './Icons'
import Messages from './Messages'

/** Personali nupp "Sõnumid" broneeringute tabelis: avab külgpaneeli, kus saab kliendile vastata. */
export default function StaffMessages({ r }) {
  const [open, setOpen] = useState(false)
  const unanswered = r.messageCount > 0 && !r.lastMessageFromStaff

  return (
    <>
      <button className={unanswered ? 'btn btn-small btn-ox' : 'btn btn-small'} onClick={() => setOpen(true)}>
        Sõnumid{r.messageCount ? ` (${r.messageCount})` : ''}
      </button>
      {open && (
        <aside className="staff-msg-panel" role="dialog" aria-label="Sõnumid">
          <div className="row-between">
            <p className="card-title">{r.customerName}</p>
            <button className="btn btn-small" onClick={() => setOpen(false)} aria-label="Sulge"><CloseIcon /></button>
          </div>
          <p className="muted small">
            {fmtDate(r.startTime)} {fmtTime(r.startTime)} · {r.restaurantName} · laud {r.tableNumber} · {r.guests} in.
          </p>
          {r.extras.length > 0 && <p className="small">Lisateenused: {r.extras.map((x) => x.name).join(', ')}</p>}
          {r.specialRequest && <p className="small italic">Soovid: {r.specialRequest}</p>}
          <Messages reservationId={r.id} staff />
        </aside>
      )}
    </>
  )
}
