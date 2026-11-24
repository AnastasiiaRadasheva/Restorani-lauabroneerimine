// Personali lehtede abifunktsioonid. Tekstid järgivad saidi keelt (ET/RU/EN).
import { useI18n } from './i18n'

/** tr('eesti', 'русский', 'english') – lühike tõlge personali lehtedel; tr.t = tavaline t(). */
export function useTr() {
  const { t, lang } = useI18n()
  const tr = (et, ru, en) => (lang === 'ru' ? ru : lang === 'en' ? en : et)
  tr.t = t
  tr.lang = lang
  tr.status = (s) => t(`status.${s}`)
  tr.zone = (z) => t({ Hall: 'booking.hall', Terrace: 'booking.terrace', Upstairs: 'booking.upstairs' }[z])
  tr.edge = (z) => tr(EDGE_TEXT[z], { Hall: 'улица · окна', Terrace: 'улица · навес', Upstairs: 'лестница · окна' }[z], { Hall: 'street · windows', Terrace: 'street · canopy', Upstairs: 'stairs · windows' }[z])
  return tr
}

export const STATUS_TEXT = {
  Active: 'Aktiivne',
  Arrived: 'Saabunud',
  Cancelled: 'Tühistatud',
  NoShow: 'Ei tulnud',
  PendingPayment: 'Ootab makset',
}

export const LOCATION_TEXT = { Hall: 'Saal', Terrace: 'Terrass', Upstairs: 'Teine korrus' }
export const EDGE_TEXT = { Hall: 'tänav · aknad', Terrace: 'tänav · varikatus', Upstairs: 'trepp · aknad' }
export const ZONE_LIST = ['Hall', 'Terrace', 'Upstairs']

export const fmtDate = (v) => new Date(v).toLocaleDateString(document.documentElement.lang || 'et', { weekday: 'short', day: 'numeric', month: 'short' })
export const fmtTime = (v) => new Date(v).toLocaleTimeString(document.documentElement.lang || 'et', { hour: '2-digit', minute: '2-digit' })

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

/** Vestluse eelvaade nimekirjas: ainult kaks esimest lauset (ja kuni 160 märki), edasi „…“. */
export function preview(text, sentences = 2, max = 160) {
  const flat = (text ?? '').replace(/\s+/g, ' ').trim()
  const parts = flat.match(/[^.!?…]+[.!?…]+["“”»)]*\s*|[^.!?…]+$/g) ?? [flat]
  let out = parts.slice(0, sentences).join('').trim()
  if (out.length > max) out = out.slice(0, max).replace(/\s+\S*$/, '')
  return out.length < flat.length ? out + ' …' : out
}
