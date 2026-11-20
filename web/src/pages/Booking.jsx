import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import DetailsSection from '../components/booking/DetailsSection'
import EveningDetails, { useExtrasCatalog } from '../components/booking/EveningDetails'
import EveningSection from '../components/booking/EveningSection'
import GastronomySection, { useMenu } from '../components/booking/GastronomySection'
import PlaceSection from '../components/booking/PlaceSection'
import StepsBar from '../components/booking/StepsBar'
import { useBooking } from '../components/booking/useBooking'
import Receipt, { eveningRows, EveningReceiptLines } from '../components/Receipt'
import { useI18n } from '../i18n'
import { money } from '../utils'

const EXTRA_KEY = 'argent-booking-extra'

const emptyEvening = { occasion: 'None', gift: 'Cake', hostType: 'Me', hostName: '', addressForm: 'Name', serviceLanguage: 'et', extras: [] }

function readExtra() {
  try {
    return JSON.parse(sessionStorage.getItem(EXTRA_KEY)) ?? {}
  } catch {
    return {}
  }
}

/** Tipptunnid 18:00–20:00: laua ise valimine maksab 30 €, muul ajal 10 € (sama reegel on API-s). */
const isPeak = (time) => time >= '18:00' && time <= '20:00'
const seatFee = (manual, time) => (!manual ? 0 : isPeak(time) ? 30 : 10)

export default function Booking() {
  const { user } = useAuth()
  const { t, lang } = useI18n()
  const navigate = useNavigate()
  const b = useBooking()
  const menu = useMenu(b.restaurantId)
  const catalog = useExtrasCatalog()
  const [saved] = useState(readExtra)

  const [qty, setQty] = useState(saved.qty ?? {})
  const [wish, setWish] = useState(saved.wish ?? '')
  const [ev, setEv] = useState(saved.ev ?? { ...emptyEvening, serviceLanguage: lang })
  const [card, setCard] = useState({ cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [done, setDone] = useState(null) // kinnitatud broneering → tšekk

  const items = menu.filter((m) => qty[m.id] > 0).map((m) => ({ menuItemId: m.id, quantity: qty[m.id] }))
  const preorderTotal = menu.reduce((s, m) => s + (qty[m.id] ?? 0) * m.price, 0)
  const extrasTotal = catalog.filter((x) => ev.extras.includes(x.code)).reduce((s, x) => s + x.price, 0)
  const fee = seatFee(b.mode === 'manual', b.time)
  const total = preorderTotal + extrasTotal + fee
  const hasOccasion = ev.occasion !== 'None'

  // Laua valimise tasu info kuvatakse I ja II sammus
  const bx = {
    ...b,
    isPeak,
    peakNote: <span className="peak-note">{t('fee.peakNote')}</span>,
    manualPrice: isPeak(b.time) ? '+30 €' : '+10 €',
    manualSub: isPeak(b.time) ? t('fee.subPeak') : t('fee.subNormal'),
    seatFeeText: fee ? `+${fee} €` : '',
  }

  async function confirm() {
    setError('')
    if (!b.chosenTable) {
      document.getElementById('plan')?.scrollIntoView()
      return b.setError(t('booking.chooseHint'))
    }
    if (!user) {
      b.saveDraft()
      sessionStorage.setItem(EXTRA_KEY, JSON.stringify({ qty, wish, ev }))
      navigate('/login', { state: { from: '/broneeri' } })
      return
    }
    if (ev.hostType === 'Other' && !ev.hostName.trim()) return setError(t('evening.err.hostName'))
    if (total > 0) {
      if (card.cardNumber.replace(/\D/g, '').length < 12) return setError(t('details4.err.card'))
      if (!/^(0[1-9]|1[0-2])\/\d{2}$/.test(card.expiry)) return setError(t('details4.err.expiry'))
      if (!/^\d{3}$/.test(card.cvc)) return setError(t('details4.err.cvc'))
    }

    setBusy(true)
    try {
      let r = await api('/api/reservations', {
        method: 'POST',
        body: {
          restaurantId: b.restaurantId,
          date: b.date,
          time: b.time,
          guests: b.guests,
          tableId: b.chosenTable.id,
          chosenByGuest: b.mode === 'manual',
          specialRequest: wish.trim() || null,
          items,
          occasion: ev.occasion,
          gift: hasOccasion ? ev.gift : 'None',
          hostType: ev.hostType,
          hostName: ev.hostType === 'Other' ? ev.hostName.trim() : null,
          addressForm: ev.addressForm,
          serviceLanguage: ev.serviceLanguage,
          extras: ev.extras,
        },
      })
      // Võltsmakse: päris raha ei liigu
      if (r.total > 0) {
        r = await api(`/api/reservations/${r.id}/pay`, { method: 'POST', body: { cardHolder: user.name, ...card } })
      }
      b.clearDraft()
      sessionStorage.removeItem(EXTRA_KEY)
      setDone(r)
    } catch (e) {
      setError(e.message)
      if (e.status === 409) {
        b.setTableId(null)
        b.reload()
      }
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="booking">
      <StepsBar
        steps={[
          { id: 'book', label: t('booking.eveningTitle') },
          { id: 'plan', label: t('booking.placeTitle') },
          { id: 'menu', label: t('gastro.title') },
          { id: 'details', label: t('details4.short') },
        ]}
      />
      <EveningSection b={bx} />
      <PlaceSection b={bx} action={{ label: t('booking.continue'), onClick: () => document.getElementById('menu')?.scrollIntoView() }} />
      <GastronomySection menu={menu} qty={qty} setQty={setQty} />
      <DetailsSection
        user={user}
        wish={wish}
        setWish={setWish}
        card={card}
        setCard={setCard}
        lines={[
          [t('gastro.preorder'), money(preorderTotal)],
          [t('fee.line'), fee ? `${b.mode === 'manual' && isPeak(b.time) ? '18:00–20:00 · ' : ''}${money(fee)}` : `${t('fee.maitre')} · 0 €`],
          [t('evening.extras'), money(extrasTotal)],
          [t('evening.gift'), hasOccasion ? `${t(`gift.${ev.gift}`).toLowerCase()} · 0 €` : '—', 'ox'],
        ]}
        total={total}
        onConfirm={confirm}
        busy={busy}
        error={error || b.error}
      >
        <EveningDetails ev={ev} setEv={setEv} catalog={catalog} userName={user?.name} />
      </DetailsSection>
      {done && (
        <Receipt r={done} total={done.total} rows={eveningRows(done, t)} onClose={() => { setDone(null); navigate('/minu') }}>
          <EveningReceiptLines r={done} />
        </Receipt>
      )}
    </div>
  )
}
