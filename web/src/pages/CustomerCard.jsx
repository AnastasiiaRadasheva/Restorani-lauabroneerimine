import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { api, qs } from '../api'
import StaffMessages from '../components/StaffMessages'
import { fmtDate, fmtTime, useTr } from '../staff'
import { money } from '../utils'

/** Personal: kliendi kaart – kontaktid ja kõik broneeringud. */
export default function CustomerCard() {
  const tr = useTr()
  const [params] = useSearchParams()
  const q = params.get('q') ?? ''
  const [customer, setCustomer] = useState(null)
  const [list, setList] = useState([])
  const [total, setTotal] = useState(0)
  const [sent, setSent] = useState('')

  useEffect(() => {
    api('/api/customers' + qs({ search: q })).then((l) => setCustomer(l[0] ?? null)).catch(() => {})
    api('/api/reservations' + qs({ search: q, pageSize: 100, sort: 'date', desc: true })).then((p) => { setList(p.items); setTotal(p.totalCount) }).catch(() => {})
  }, [q])

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>{customer?.name ?? q}</h1>
        </div>
        <Link className="btn" to="/personal/uus">{tr('Uus broneering sellele kliendile', 'Новая бронь для гостя', 'New booking for this guest')}</Link>
      </div>
      {customer && (
        <div className="paper white">
          <dl className="receipt-dl">
            <dt>E-mail</dt><dd>{customer.email}</dd>
            <dt>{tr('Telefon', 'Телефон', 'Phone')}</dt><dd>{customer.phone ?? '—'}</dd>
            <dt>SMS</dt><dd>{customer.smsOptIn ? tr('jah', 'да', 'yes') : tr('ei', 'нет', 'no')}</dd>
            <dt>{tr('Broneeringuid', 'Броней', 'Bookings')}</dt><dd>{total}</dd>
            <dt>{tr('Konto', 'Аккаунт', 'Account')}</dt>
            <dd>
              {customer.passwordSet
                ? tr('klient on parooli loonud', 'гость создал пароль', 'the guest has a password')
                : <span className="ox">{tr('konto lõi personal, parool on veel loomata', 'аккаунт создан персоналом, пароль ещё не задан', 'created by staff, no password yet')}</span>}
            </dd>
          </dl>
          {!customer.passwordSet && (
            <div className="actions">
              <button className="btn btn-small" onClick={async () => {
                try {
                  await api(`/api/customers/${customer.id}/password-link`, { method: 'POST' })
                  setSent(tr('Link saadeti kliendile (e-kiri ja SMS).', 'Ссылка отправлена гостю (письмо и SMS).', 'The link was sent to the guest (e-mail and SMS).'))
                } catch (e) {
                  setSent(e.message)
                }
              }}>{tr('Saada parooli loomise link uuesti', 'Отправить ссылку для пароля ещё раз', 'Send the password link again')}</button>
              {sent && <span className="small">{sent}</span>}
            </div>
          )}
        </div>
      )}
      <div className="day-list wide">
        {list.map((r) => (
          <div key={r.id} className={r.occasion !== 'None' ? 'person important' : 'person'}>
            <div className="row-between">
              <strong>{fmtDate(r.startTime)} {fmtTime(r.startTime)} · {r.restaurantName}</strong>
              <span className={`badge badge-${r.status}`}>{tr.t(`status.${r.status}`)}</span>
            </div>
            <span className="small">{tr('laud', 'стол', 'table')} {r.tableNumber} · {r.guests} · {money(r.total)}{r.isPaid ? ' ✓' : ''}</span>
            {r.allergies?.length > 0 && <span className="small ox">{r.allergies.map((a) => tr.t(`allergy.${a}`)).join(', ')}</span>}
            {r.specialRequest && <span className="small italic">{r.specialRequest}</span>}
            <div className="actions">
              <Link className="btn btn-small" to={`/broneering/${r.id}`}>{tr('Ava / muuda', 'Открыть / изменить', 'Open / edit')}</Link>
              <StaffMessages r={r} />
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
