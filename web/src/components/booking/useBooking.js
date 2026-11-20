import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { api, qs } from '../../api'
import { useI18n } from '../../i18n'
import { useTableSocket } from '../../realtime'
import { toIsoDate } from '../../utils'

const DRAFT_KEY = 'argent-booking-draft'

function readDraft() {
  try {
    return JSON.parse(sessionStorage.getItem(DRAFT_KEY)) ?? {}
  } catch {
    return {}
  }
}

/**
 * Broneerimise lehe olek ja andmed ühes kohas:
 * restoranid, kuupäev, kellaaeg, külalised, vabad ajad (kalender) ja vabad lauad (plaan).
 */
export function useBooking() {
  const { t } = useI18n()
  const [params] = useSearchParams()
  const draft = useMemo(readDraft, [])

  const [restaurants, setRestaurants] = useState([])
  const [restaurantId, setRestaurantIdState] = useState(draft.restaurantId ?? (Number(params.get('restaurant')) || null))
  const [geo, setGeoState] = useState(false)
  const [userPos, setUserPos] = useState(null)
  const [geoError, setGeoError] = useState('')

  const [date, setDateState] = useState(draft.date ?? toIsoDate(new Date()))
  const [time, setTime] = useState(draft.time ?? '19:00')
  const [guests, setGuestsState] = useState(draft.guests ?? (Number(params.get('guests')) || 2))
  const [slots, setSlots] = useState([])
  const [availability, setAvailability] = useState(null)

  const [mode, setModeState] = useState(draft.mode ?? 'auto') // auto | manual
  const [zone, setZone] = useState(draft.zone ?? 'Hall')
  const [tableId, setTableId] = useState(draft.tableId ?? null)
  const [error, setError] = useState('')
  const [othersSelecting, setOthersSelecting] = useState({}) // tableId -> aeg

  const restaurant = restaurants.find((r) => r.id === restaurantId)

  // Restoranid (kui asukoht on teada, tuleb lähim esimesena koos kaugusega)
  useEffect(() => {
    api('/api/restaurants' + qs({ lat: userPos?.lat, lng: userPos?.lng }))
      .then((list) => {
        setRestaurants(list)
        if (userPos && list.length) setRestaurantIdState(list[0].id)
        else setRestaurantIdState((id) => id ?? list[0]?.id ?? null)
      })
      .catch((e) => setError(e.message))
  }, [userPos])

  // Vabad ajad 30 päevaks (kalendri ja kellaaegade jaoks)
  useEffect(() => {
    if (!restaurantId) return
    api('/api/tables/slots' + qs({ restaurantId, from: toIsoDate(new Date()), days: 30, guests }))
      .then(setSlots)
      .catch((e) => setError(e.message))
  }, [restaurantId, guests])

  // Kui valitud kellaaeg on täis või sellel päeval puudub, valime esimese vaba (eelistame 19:00)
  const daySlots = slots.find((d) => d.date === date)?.slots
  useEffect(() => {
    if (!daySlots) return
    const current = daySlots.find((s) => s.time === time)
    if (current && current.freeTables > 0) return
    const free = daySlots.filter((s) => s.freeTables > 0)
    const next = free.find((s) => s.time === '19:00') ?? free.find((s) => s.time > time) ?? free[0]
    if (next) setTime(next.time)
  }, [daySlots]) // eslint-disable-line react-hooks/exhaustive-deps

  const timeOk = daySlots?.some((s) => s.time === time && s.freeTables > 0)

  // Laudade seis valitud ajal
  function loadAvailability() {
    if (!restaurantId || !timeOk) return Promise.resolve()
    return api('/api/tables/available' + qs({ restaurantId, date, time, guests }))
      .then((a) => {
        setAvailability(a)
        setError('')
      })
      .catch((e) => setError(e.message))
  }

  useEffect(() => {
    setAvailability(null)
    loadAvailability()
  }, [restaurantId, date, time, guests, timeOk]) // eslint-disable-line react-hooks/exhaustive-deps

  // Automaatse valiku korral näitame plaanil seda tsooni, kus pakutud laud asub
  const suggested = availability?.tables.find((tb) => tb.id === availability.suggestedTableId) ?? null
  useEffect(() => {
    if (mode === 'auto' && suggested) setZone(suggested.location)
  }, [mode, suggested?.id]) // eslint-disable-line react-hooks/exhaustive-deps

  // Reaalajas: kui keegi teine broneerib või valib lauda, uuendame plaani kohe
  const send = useTableSocket((msg) => {
    if (msg.restaurantId !== restaurantId || !availability) return
    if (msg.type === 'TableSelecting') {
      if (msg.startTime?.slice(0, 16) === availability.startTime.slice(0, 16)) {
        setOthersSelecting((s) => ({ ...s, [msg.tableId]: msg.selecting ? Date.now() : undefined }))
      }
    } else if (msg.type === 'TableStatusChanged') {
      const overlaps = new Date(msg.startTime) < new Date(availability.endTime) &&
        (!msg.endTime || new Date(msg.endTime) > new Date(availability.startTime))
      if (overlaps) loadAvailability()
    }
  })

  // Teatame teistele, millist lauda praegu valime (nemad näevad seda viirutatuna)
  useEffect(() => {
    if (!availability || !tableId || mode !== 'manual') return undefined
    const msg = { type: 'Selecting', restaurantId, tableId, startTime: availability.startTime }
    send({ ...msg, selecting: true })
    return () => send({ ...msg, selecting: false })
  }, [tableId, mode, availability?.startTime]) // eslint-disable-line react-hooks/exhaustive-deps

  // "Valib praegu" kaob 30 sekundi pärast
  useEffect(() => {
    const t = setInterval(() => {
      setOthersSelecting((s) => Object.fromEntries(Object.entries(s).filter(([, v]) => v && Date.now() - v < 30000)))
    }, 5000)
    return () => clearInterval(t)
  }, [])

  const tables = (availability?.tables ?? []).map((tb) => ({
    ...tb,
    state: !tb.isFree ? 'busy' : !tb.fitsGuests ? 'small' : othersSelecting[tb.id] ? 'hold' : 'free',
    clickable: tb.isFree && tb.fitsGuests, // viirutatud lauda saab ka valida, aga teine võib ette jõuda
  }))
  const manualTable = tables.find((tb) => tb.id === tableId && tb.clickable) ?? null
  const chosenTable = mode === 'auto' ? suggested : manualTable

  function setGeo(on) {
    setGeoState(on)
    setGeoError('')
    if (!on) return setUserPos(null)
    if (!navigator.geolocation) return setGeoError(t('booking.geoUnsupported'))
    navigator.geolocation.getCurrentPosition(
      (p) => setUserPos({ lat: p.coords.latitude, lng: p.coords.longitude }),
      () => {
        setGeoError(t('booking.geoDenied'))
        setGeoState(false)
      },
    )
  }

  return {
    restaurants,
    restaurant,
    restaurantId,
    setRestaurantId: (id) => {
      setRestaurantIdState(id)
      setTableId(null)
    },
    geo,
    setGeo,
    userPos,
    geoError,
    date,
    setDate: (d) => {
      setDateState(d)
      setTableId(null)
    },
    time,
    setTime: (v) => {
      setTime(v)
      setTableId(null)
    },
    guests,
    setGuests: (n) => setGuestsState(Math.max(1, Math.min(12, n))),
    slots,
    availability: availability ? { ...availability, tables } : null,
    mode,
    setMode: setModeState,
    zone,
    setZone,
    tableId,
    setTableId,
    chosenTable,
    error,
    setError,
    reload: loadAvailability,
    legend: [
      ['free', t('booking.legendFree')],
      ['hold', t('booking.legendHold')],
      ['busy', t('booking.legendBusy')],
      ['selected', t('booking.legendMine')],
      ['chair', t('booking.legendSeat')],
    ],
    saveDraft: () => sessionStorage.setItem(DRAFT_KEY, JSON.stringify({ restaurantId, date, time, guests, mode, zone, tableId })),
    clearDraft: () => sessionStorage.removeItem(DRAFT_KEY),
  }
}
