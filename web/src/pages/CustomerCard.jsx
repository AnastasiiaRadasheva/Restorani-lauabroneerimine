import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { api, qs } from '../api'
import StaffMessages from '../components/StaffMessages'
import { fmtDate, fmtTime, useTr } from '../staff'
import { money } from '../utils'

/** Laeb kõik kliendi broneeringud (API annab korraga kuni 100). */
async function loadAll(search) {
  const all = []
  for (let page = 1; page <= 10; page++) {
    const p = await api('/api/reservations' + qs({ search, page, pageSize: 100, sort: 'date', desc: true }))
    all.push(...p.items)
    if (all.length >= p.totalCount || p.items.length === 0) break
  }
  return all
}

const FILTERS = [
  ['all', 'Kõik', 'Все', 'All'],
  ['upcoming', 'Tulevased', 'Предстоящие', 'Upcoming'],
  ['past', 'Möödunud', 'Прошедшие', 'Past'],
  ['Arrived', 'Saabus', 'Пришли', 'Arrived'],
  ['NoShow', 'Ei tulnud', 'Не пришли', 'No-show'],
  ['Cancelled', 'Tühistatud', 'Отменённые', 'Cancelled'],
]

/** Personal: kliendi kaart – vasakul kliendi kaart, paremal tema broneeringud filtritega. */
export default function CustomerCard() {
  const tr = useTr()
  const [params] = useSearchParams()
  const q = params.get('q') ?? ''
  const [customer, setCustomer] = useState(null)
  const [list, setList] = useState(null)
  const [sent, setSent] = useState('')
  const [filter, setFilter] = useState('all')
  const [restaurant, setRestaurant] = useState('')

  useEffect(() => {
    setList(null)
    api('/api/customers' + qs({ search: q })).then((l) => setCustomer(l[0] ?? null)).catch(() => {})
    // Ainult selle kliendi broneeringud (otsing võib leida ka sarnase nimega kliente)
    loadAll(q).then((items) => setList(items.filter((r) => !q.includes('@') || r.customerEmail?.toLowerCase() === q.toLowerCase()))).catch(() => setList([]))
  }, [q])

  const now = new Date()
  const all = list ?? []
  const stats = useMemo(() => {
    const count = (fn) => all.filter(fn).length
    const byRest = {}
    all.filter((r) => r.status !== 'Cancelled').forEach((r) => { byRest[r.restaurantName] = (byRest[r.restaurantName] ?? 0) + 1 })
    const fav = Object.entries(byRest).sort((a, b) => b[1] - a[1])[0]?.[0]
    const allergies = [...new Set(all.flatMap((r) => r.allergies ?? []))]
    return {
      upcoming: count((r) => new Date(r.startTime) >= now && ['Active', 'PendingPayment'].includes(r.status)),
      arrived: count((r) => r.status === 'Arrived'),
      noShow: count((r) => r.status === 'NoShow'),
      cancelled: count((r) => r.status === 'Cancelled'),
      fav,
      allergies,
    }
  }, [list]) // eslint-disable-line react-hooks/exhaustive-deps

  const restaurants = [...new Set(all.map((r) => r.restaurantName))].sort()
  const shown = all
    .filter((r) => !restaurant || r.restaurantName === restaurant)
    .filter((r) => {
      if (filter === 'upcoming') return new Date(r.startTime) >= now && r.status !== 'Cancelled'
      if (filter === 'past') return new Date(r.startTime) < now
      if (filter === 'all') return true
      return r.status === filter
    })
    .sort((a, b) => (filter === 'upcoming' ? new Date(a.startTime) - new Date(b.startTime) : new Date(b.startTime) - new Date(a.startTime)))

  const initials = (customer?.name ?? q).split(/\s+/).map((w) => w[0]).filter(Boolean).slice(0, 2).join('').toUpperCase()
  const countFor = (key) => (key === 'all' ? all.length : key === 'upcoming' ? all.filter((r) => new Date(r.startTime) >= now && r.status !== 'Cancelled').length
    : key === 'past' ? all.filter((r) => new Date(r.startTime) < now).length : all.filter((r) => r.status === key).length)

  return (
    <div className="staff container">
      <div className="cc-grid">
        <aside className="cc-card paper white">
          <div className="cc-avatar" aria-hidden="true">{initials || '?'}</div>
          <h1 className="cc-name">{customer?.name ?? q}</h1>
          {customer && (
            <>
              <p className="cc-contact">{customer.email}<br />{customer.phone ?? '—'}</p>
              <div className="cc-stats">
                <div><strong>{all.length}</strong><span>{tr('broneeringut', 'броней', 'bookings')}</span></div>
                <div><strong>{stats.upcoming}</strong><span>{tr('tulevasi', 'предстоит', 'upcoming')}</span></div>
                <div><strong>{stats.arrived}</strong><span>{tr('saabus', 'пришёл', 'arrived')}</span></div>
                <div><strong>{stats.noShow}</strong><span>{tr('ei tulnud', 'не пришёл', 'no-show')}</span></div>
              </div>
              <dl className="cc-dl">
                <dt>{tr('Lemmik', 'Любимый', 'Favourite')}</dt><dd>{stats.fav ?? '—'}</dd>
                <dt>{tr('Tühistas', 'Отменил', 'Cancelled')}</dt><dd>{stats.cancelled}</dd>
                <dt>SMS</dt><dd>{customer.smsOptIn ? tr('jah', 'да', 'yes') : tr('ei', 'нет', 'no')}</dd>
                <dt>{tr('Konto', 'Аккаунт', 'Account')}</dt>
                <dd>{customer.passwordSet ? tr('parool olemas', 'пароль задан', 'has password') : <span className="ox">{tr('parool loomata', 'пароль не задан', 'no password yet')}</span>}</dd>
                {stats.allergies.length > 0 && (<><dt>{tr('Allergiad', 'Аллергии', 'Allergies')}</dt><dd className="ox">{stats.allergies.map((a) => tr.t(`allergy.${a}`)).join(', ')}</dd></>)}
              </dl>
              {!customer.passwordSet && (
                <button className="btn btn-small" onClick={async () => {
                  try {
                    await api(`/api/customers/${customer.id}/password-link`, { method: 'POST' })
                    setSent(tr('Link saadeti kliendile (e-kiri ja SMS).', 'Ссылка отправлена гостю (письмо и SMS).', 'The link was sent to the guest (e-mail and SMS).'))
                  } catch (e) {
                    setSent(e.message)
                  }
                }}>{tr('Saada parooli link uuesti', 'Отправить ссылку для пароля', 'Send password link again')}</button>
              )}
              {sent && <p className="small">{sent}</p>}
            </>
          )}
          <Link className="btn btn-dark btn-block" to="/personal/uus">+ {tr('Uus broneering', 'Новая бронь', 'New booking')}</Link>
        </aside>

        <section className="cc-main">
          <div className="cc-filters">
            <div className="chips">
              {FILTERS.map(([key, et, ru, en]) => (
                <button key={key} type="button" className={filter === key ? 'chip caps on' : 'chip caps'} onClick={() => setFilter(key)}>
                  {tr(et, ru, en)} <span className="cc-count">{countFor(key)}</span>
                </button>
              ))}
            </div>
            {restaurants.length > 1 && (
              <select value={restaurant} onChange={(e) => setRestaurant(e.target.value)}>
                <option value="">{tr('Kõik restoranid', 'Все рестораны', 'All restaurants')}</option>
                {restaurants.map((r) => <option key={r}>{r}</option>)}
              </select>
            )}
          </div>

          {list === null && <p className="loading">{tr('Laen…', 'Загрузка…', 'Loading…')}</p>}
          {list && shown.length === 0 && <p className="italic muted">{tr('Selliseid broneeringuid pole.', 'Таких броней нет.', 'No such bookings.')}</p>}
          <div className="cc-list">
            {shown.map((r) => (
              <div key={r.id} className={r.occasion !== 'None' ? 'cc-row important' : 'cc-row'}>
                <div className="cc-when">
                  <strong>{fmtDate(r.startTime)}</strong>
                  <span>{fmtTime(r.startTime)}</span>
                </div>
                <div className="cc-what">
                  <span>{r.restaurantName} · {tr('laud', 'стол', 'table')} {r.tableNumber} · {r.guests} {tr('in.', 'гост.', 'gst.')} · {money(r.total)}{r.isPaid ? ' ✓' : ''}</span>
                  {r.occasion !== 'None' && <span className="small ox">◆ {tr.t(`occasion.${r.occasion}`)}</span>}
                  {r.allergies?.length > 0 && <span className="small ox">{r.allergies.map((a) => tr.t(`allergy.${a}`)).join(', ')}</span>}
                  {r.specialRequest && <span className="small italic">{r.specialRequest}</span>}
                </div>
                <span className={`badge badge-${r.status}`}>{tr.t(`status.${r.status}`)}</span>
                <div className="cc-actions">
                  <Link className="btn btn-small" to={`/broneering/${r.id}`}>{tr('Ava', 'Открыть', 'Open')}</Link>
                  <StaffMessages r={r} />
                </div>
              </div>
            ))}
          </div>
        </section>
      </div>
    </div>
  )
}
