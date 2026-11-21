// Personali lehtede abifunktsioonid. Personali vaade on alati eesti keeles.

export const STATUS_TEXT = {
  Active: 'Aktiivne',
  Arrived: 'Saabunud',
  Cancelled: 'Tühistatud',
  NoShow: 'Ei tulnud',
}

export const LOCATION_TEXT = { Hall: 'Saal', Terrace: 'Terrass', Upstairs: 'Teine korrus' }
export const EDGE_TEXT = { Hall: 'tänav · aknad', Terrace: 'tänav · varikatus', Upstairs: 'trepp · aknad' }
export const ZONE_LIST = ['Hall', 'Terrace', 'Upstairs']

export const fmtDate = (v) => new Date(v).toLocaleDateString('et-EE', { weekday: 'short', day: 'numeric', month: 'short' })
export const fmtTime = (v) => new Date(v).toLocaleTimeString('et-EE', { hour: '2-digit', minute: '2-digit' })

/** Kellaajad 30-minutilise sammuga. */
export function timeSlots(from = '17:00', to = '23:30') {
  const toMin = (s) => Number(s.slice(0, 2)) * 60 + Number(s.slice(3, 5))
  const result = []
  for (let m = toMin(from); m <= toMin(to); m += 30) {
    result.push(`${String(Math.floor(m / 60)).padStart(2, '0')}:${String(m % 60).padStart(2, '0')}`)
  }
  return result
}

/** Praegune kellaaeg ümardatuna pooltunnini, piiratud lahtiolekuajaga. */
export function nowSlot(from = '17:00', to = '23:00') {
  const d = new Date()
  const m = d.getHours() * 60 + (d.getMinutes() < 30 ? 0 : 30)
  const toMin = (s) => Number(s.slice(0, 2)) * 60 + Number(s.slice(3, 5))
  const clamped = Math.min(Math.max(m, toMin(from)), toMin(to))
  return `${String(Math.floor(clamped / 60)).padStart(2, '0')}:${String(clamped % 60).padStart(2, '0')}`
}
