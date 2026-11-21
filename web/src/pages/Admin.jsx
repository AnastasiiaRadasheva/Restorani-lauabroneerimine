import { useCallback, useEffect, useState } from 'react'
import { api, qs } from '../api'
import BarChart from '../components/BarChart'
import FloorPlan from '../components/FloorPlan'
import Pager from '../components/Pager'
import { EDGE_TEXT, LOCATION_TEXT, ZONE_LIST } from '../staff'
import { money } from '../utils'

const TABS = [
  ['tables', 'Lauad'],
  ['menu', 'Menüü'],
  ['stats', 'Statistika'],
]

export default function Admin() {
  const [tab, setTab] = useState('tables')
  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantId] = useState(1)

  useEffect(() => {
    api('/api/restaurants').then(setRestaurants)
  }, [])

  const restaurant = restaurants.find((r) => r.id === restaurantId)

  return (
    <div className="staff container">
      <div className="sec-head">
        <div className="title">
          <span className="num">◆</span>
          <h1>Admin</h1>
        </div>
        <select value={restaurantId} onChange={(e) => setRestaurantId(Number(e.target.value))}>
          {restaurants.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
      </div>
      <div className="chips">
        {TABS.map(([key, text]) => (
          <button key={key} className={tab === key ? 'chip caps on' : 'chip caps'} onClick={() => setTab(key)}>{text}</button>
        ))}
      </div>
      {tab === 'tables' && restaurant && <TablesAdmin restaurant={restaurant} />}
      {tab === 'menu' && <MenuAdmin restaurantId={restaurantId} />}
      {tab === 'stats' && <Stats restaurantId={restaurantId} />}
    </div>
  )
}

/* ===== Laudade haldus ===== */

const emptyTable = { number: '', seats: 4, location: 'Hall', isActive: true, x: 50, y: 10, width: 16, height: 20, spot: '', isRound: false }

function TablesAdmin({ restaurant }) {
  const [tables, setTables] = useState([])
  const [form, setForm] = useState(null) // null = vorm suletud
  const [editId, setEditId] = useState(null)
  const [zone, setZone] = useState('Hall')
  const [error, setError] = useState('')
  const [ok, setOk] = useState('')

  const load = useCallback(() => {
    api(`/api/restaurants/${restaurant.id}/tables`).then(setTables).catch((e) => setError(e.message))
  }, [restaurant.id])

  useEffect(() => {
    load()
    setForm(null)
  }, [load])

  function edit(t) {
    setEditId(t.id)
    setForm({ ...t, spot: t.spot ?? '' })
    setZone(t.location)
    setError('')
  }

  function create() {
    setEditId(null)
    setForm({ ...emptyTable, location: zone, number: Math.max(0, ...tables.map((t) => t.number)) + 1 })
    setError('')
  }

  async function save(e) {
    e.preventDefault()
    const body = {
      ...form,
      restaurantId: restaurant.id,
      number: Number(form.number),
      seats: Number(form.seats),
      x: Number(form.x),
      y: Number(form.y),
      width: Number(form.width),
      height: Number(form.height),
    }
    if (body.number < 1) return setError('Laua number peab olema positiivne.')
    if (body.seats < 1 || body.seats > 20) return setError('Kohtade arv peab olema 1 kuni 20.')
    try {
      if (editId) await api(`/api/tables/${editId}`, { method: 'PUT', body })
      else await api('/api/tables', { method: 'POST', body })
      setOk(editId ? 'Laud salvestatud.' : 'Laud lisatud.')
      setForm(null)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function remove(t) {
    if (!window.confirm(`Kustutada laud ${t.number}?`)) return
    try {
      await api(`/api/tables/${t.id}`, { method: 'DELETE' })
      setOk('Laud kustutatud.')
      setForm(null)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  const set = (f) => (e) => setForm({ ...form, [f]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })
  const editingId = editId ?? (form ? -1 : null)
  const preview = tables
    .map((t) => (t.id === editId ? { ...form } : t))
    .concat(form && !editId ? [{ ...form, id: -1 }] : [])
    .map((t) => ({
      ...t,
      x: Number(t.x),
      y: Number(t.y),
      width: Number(t.width),
      height: Number(t.height),
      seats: Number(t.seats),
      state: t.id === editingId ? 'selected' : t.isActive ? 'free' : 'small',
    }))

  return (
    <div className="admin-block">
      {error && <p className="alert alert-error">{error}</p>}
      {ok && <p className="alert alert-ok">{ok}</p>}

      <div className="plan-wrap">
        <div className="plan-col">
          <div className="zone-tabs" role="tablist">
            {ZONE_LIST.map((z) => (
              <button key={z} type="button" className={zone === z ? 'on' : ''} onClick={() => setZone(z)}>{LOCATION_TEXT[z]}</button>
            ))}
          </div>
          <FloorPlan
            tables={preview}
            zone={zone}
            bar={{ x: restaurant.barX, y: restaurant.barY, width: restaurant.barWidth, height: restaurant.barHeight }}
            edgeLabel={EDGE_TEXT[zone]}
            entranceLabel={zone === 'Upstairs' ? 'Trepp' : 'Sissepääs'}
            barLabel="Baar"
            seatsLabel={(n) => `${n} kohta`}
            onSelect={(t) => t.id > 0 && edit(tables.find((x) => x.id === t.id))}
          />
          <p className="muted small">Klõpsake laual, et seda muuta. Asukoht on protsentides (0–100).</p>
        </div>

        <aside className="plan-side paper white side-card">
          {!form && <button className="btn btn-dark" onClick={create}>+ Lisa laud</button>}
          {form && (
            <form className="admin-form" onSubmit={save} noValidate>
              <p className="card-title">{editId ? `Laud ${form.number}` : 'Uus laud'}</p>
              <div className="field-row">
                <label className="field">Number<input type="number" min={1} value={form.number} onChange={set('number')} /></label>
                <label className="field">Kohti<input type="number" min={1} max={20} value={form.seats} onChange={set('seats')} /></label>
              </div>
              <label className="field">
                Asukoht
                <select value={form.location} onChange={set('location')}>
                  <option value="Hall">Saal</option>
                  <option value="Terrace">Terrass</option>
                  <option value="Upstairs">Teine korrus</option>
                </select>
              </label>
              <label className="field">Kirjeldus plaanil<input value={form.spot} onChange={set('spot')} placeholder="nt akna all" /></label>
              <div className="field-row">
                <label className="field">X %<input type="number" min={0} max={100} value={form.x} onChange={set('x')} /></label>
                <label className="field">Y %<input type="number" min={0} max={100} value={form.y} onChange={set('y')} /></label>
                <label className="field">Laius<input type="number" min={1} max={100} value={form.width} onChange={set('width')} /></label>
                <label className="field">Kõrgus<input type="number" min={1} max={100} value={form.height} onChange={set('height')} /></label>
              </div>
              <label className="check"><input type="checkbox" checked={form.isRound} onChange={set('isRound')} /> Ümmargune</label>
              <label className="check"><input type="checkbox" checked={form.isActive} onChange={set('isActive')} /> Aktiivne</label>
              <div className="actions">
                <button className="btn btn-dark">Salvesta</button>
                <button type="button" className="btn" onClick={() => setForm(null)}>Loobu</button>
                {editId && <button type="button" className="btn btn-ox-line" onClick={() => remove(form)}>Kustuta</button>}
              </div>
            </form>
          )}
        </aside>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead><tr><th>Nr</th><th>Kohti</th><th>Asukoht</th><th>Kirjeldus</th><th>Aktiivne</th><th /></tr></thead>
          <tbody>
            {tables.map((t) => (
              <tr key={t.id}>
                <td data-label="Nr">{t.number}</td>
                <td data-label="Kohti">{t.seats}</td>
                <td data-label="Asukoht">{LOCATION_TEXT[t.location]}</td>
                <td data-label="Kirjeldus">{t.spot}</td>
                <td data-label="Aktiivne">{t.isActive ? 'jah' : 'ei'}</td>
                <td className="actions"><button className="btn btn-small" onClick={() => edit(t)}>Muuda</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

/* ===== Menüü haldus ===== */

const CATEGORIES = ['Külmad road', 'Mereannid', 'Setid', 'Pearoad', 'Magustoidud', 'Veinid']
const emptyItem = { name: '', description: '', category: 'Pearoad', price: '', isAvailable: true }

function MenuAdmin({ restaurantId }) {
  const [f, setF] = useState({ search: '', category: '', sort: 'category', page: 1, pageSize: 8 })
  const [data, setData] = useState(null)
  const [form, setForm] = useState(null)
  const [editId, setEditId] = useState(null)
  const [error, setError] = useState('')

  const load = useCallback(() => {
    api('/api/menu' + qs({ ...f, restaurantId })).then(setData).catch((e) => setError(e.message))
  }, [f, restaurantId])

  useEffect(() => {
    load()
  }, [load])

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })

  async function save(e) {
    e.preventDefault()
    const body = { ...form, restaurantId, price: Number(String(form.price).replace(',', '.')) }
    if (body.name.trim().length < 2) return setError('Nimi on liiga lühike.')
    if (!body.category.trim()) return setError('Kategooria on kohustuslik.')
    if (!(body.price > 0)) return setError('Hind peab olema suurem kui 0.')
    try {
      if (editId) await api(`/api/menu/${editId}`, { method: 'PUT', body })
      else await api('/api/menu', { method: 'POST', body })
      setForm(null)
      setError('')
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function remove(m) {
    if (!window.confirm(`Kustutada "${m.name}"?`)) return
    try {
      await api(`/api/menu/${m.id}`, { method: 'DELETE' })
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="admin-block">
      {error && <p className="alert alert-error">{error}</p>}
      <div className="filters">
        <input className="grow" placeholder="Otsi…" value={f.search} onChange={(e) => setF({ ...f, search: e.target.value, page: 1 })} />
        <select value={f.category} onChange={(e) => setF({ ...f, category: e.target.value, page: 1 })}>
          <option value="">Kõik kategooriad</option>
          {CATEGORIES.map((c) => <option key={c}>{c}</option>)}
        </select>
        <select value={f.sort} onChange={(e) => setF({ ...f, sort: e.target.value, page: 1 })}>
          <option value="category">Kategooria</option>
          <option value="name">Nimi</option>
          <option value="price">Hind</option>
        </select>
        <button className="btn btn-dark" onClick={() => { setEditId(null); setForm({ ...emptyItem }) }}>+ Lisa toit</button>
      </div>

      {form && (
        <form className="paper white admin-form" onSubmit={save} noValidate>
          <p className="card-title">{editId ? 'Muuda toitu' : 'Uus toit'}</p>
          <label className="field">Nimi<input value={form.name} onChange={set('name')} /></label>
          <div className="field-row">
            <label className="field">Kategooria<input value={form.category} onChange={set('category')} list="cats" /></label>
            <label className="field">Hind €<input value={form.price} onChange={set('price')} inputMode="decimal" /></label>
          </div>
          <datalist id="cats">{CATEGORIES.map((c) => <option key={c} value={c} />)}</datalist>
          <label className="field">Kirjeldus<textarea rows={2} value={form.description} onChange={set('description')} /></label>
          <label className="field">Foto aadress<input value={form.imageUrl ?? ''} onChange={set('imageUrl')} placeholder="/img/menu/austrid.jpg" /></label>
          <label className="check"><input type="checkbox" checked={form.isAvailable} onChange={set('isAvailable')} /> Saadaval</label>
          <div className="actions">
            <button className="btn btn-dark">Salvesta</button>
            <button type="button" className="btn" onClick={() => setForm(null)}>Loobu</button>
          </div>
        </form>
      )}

      <div className="table-wrap">
        <table className="data-table">
          <thead><tr><th>Nimi</th><th>Kategooria</th><th>Hind</th><th>Saadaval</th><th /></tr></thead>
          <tbody>
            {data?.items.map((m) => (
              <tr key={m.id}>
                <td data-label="Nimi">
                  <div className="admin-dish">
                    {m.imageUrl && <img src={m.imageUrl} alt="" />}
                    <span>{m.name}<span className="muted small block">{m.description}</span></span>
                  </div>
                </td>
                <td data-label="Kategooria">{m.category}</td>
                <td data-label="Hind">{money(m.price)}</td>
                <td data-label="Saadaval">{m.isAvailable ? 'jah' : 'ei'}</td>
                <td className="actions">
                  <button className="btn btn-small" onClick={() => { setEditId(m.id); setForm({ ...m }) }}>Muuda</button>
                  <button className="btn btn-small btn-ox-line" onClick={() => remove(m)}>Kustuta</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data && <Pager {...data} onChange={(page) => setF({ ...f, page })} />}
    </div>
  )
}

/* ===== Statistika ===== */

function Stats({ restaurantId }) {
  const [stats, setStats] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api('/api/stats' + qs({ restaurantId })).then(setStats).catch((e) => setError(e.message))
  }, [restaurantId])

  if (error) return <p className="alert alert-error">{error}</p>
  if (!stats) return <p className="loading">Laen…</p>

  return (
    <div className="admin-block">
      <p className="muted">Periood {stats.from} – {stats.to}. Tühistatud broneeringuid ei arvestata.</p>
      <div className="stat-tiles">
        <div className="paper tile"><span className="eyebrow">Broneeringuid</span><strong>{stats.totalReservations}</strong></div>
        <div className="paper tile"><span className="eyebrow">Külalisi</span><strong>{stats.totalGuests}</strong></div>
        <div className="paper tile"><span className="eyebrow">Ei tulnud</span><strong>{stats.noShowPercent}%</strong></div>
      </div>
      <div className="charts">
        <div className="paper white"><BarChart title="Nädalapäev" items={stats.byWeekday} /></div>
        <div className="paper white"><BarChart title="Kellaaeg" items={stats.byHour} /></div>
      </div>
      <div className="paper white">
        <p className="card-title">Kõige hõivatumad päevad</p>
        <ol className="busiest">
          {stats.busiestDates.map((d) => (
            <li key={d.label}>
              {new Date(d.label).toLocaleDateString('et-EE', { weekday: 'long', day: 'numeric', month: 'long' })} – {d.reservations} broneeringut, {d.guests} külalist
            </li>
          ))}
        </ol>
      </div>
    </div>
  )
}
