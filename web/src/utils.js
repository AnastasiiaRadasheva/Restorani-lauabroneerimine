/** Kuupäev kujul yyyy-MM-dd kohaliku aja järgi (mitte UTC). */
export function toIsoDate(d) {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

/** "2026-11-20" → Date (kohalik aeg). */
export function parseIsoDate(s) {
  const [y, m, d] = s.split('-').map(Number)
  return new Date(y, m - 1, d)
}

export function addDays(d, days) {
  const copy = new Date(d)
  copy.setDate(copy.getDate() + days)
  return copy
}

export const formatDate = (value, locale, opts = { day: 'numeric', month: 'long' }) =>
  new Date(value).toLocaleDateString(locale, opts)

export const formatTime = (value, locale) =>
  new Date(value).toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit' })

/** Lühike nädalapäev: "E", "T" … (eesti), "Пн" … (vene). */
export const weekdayShort = (d, locale) =>
  d.toLocaleDateString(locale, { weekday: 'short' }).replace('.', '').slice(0, 2)

export function formatDistance(meters) {
  if (meters == null) return ''
  return meters < 1000 ? `${Math.round(meters / 10) * 10} m` : `${(meters / 1000).toFixed(1).replace('.', ',')} km`
}

/** Raha: täisarvud ilma kümnendkohtadeta (45 €), muidu 2 kohta (6,50 €). */
export function money(v) {
  const n = Number(v)
  return `${Number.isInteger(n) ? n : n.toFixed(2).replace('.', ',')} €`
}

/** Broneeringu number tšekil, nt AR-2011-1900-42. */
export function reservationCode(r) {
  const d = new Date(r.startTime)
  const dd = String(d.getDate()).padStart(2, '0')
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const hhmm = `${String(d.getHours()).padStart(2, '0')}${String(d.getMinutes()).padStart(2, '0')}`
  return `AR-${dd}${mm}-${hhmm}-${r.id}`
}
