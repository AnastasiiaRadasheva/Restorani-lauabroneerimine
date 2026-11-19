import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import DetailsSection from '../components/booking/DetailsSection'
import EveningSection from '../components/booking/EveningSection'
import GastronomySection, { useMenu } from '../components/booking/GastronomySection'
import PlaceSection from '../components/booking/PlaceSection'
import StepsBar from '../components/booking/StepsBar'
import { useBooking } from '../components/booking/useBooking'
import Receipt from '../components/Receipt'
import { useI18n } from '../i18n'
import { money } from '../utils'

const EXTRA_KEY = 'argent-booking-extra'

function readExtra() {
  try {
    return JSON.parse(sessionStorage.getItem(EXTRA_KEY)) ?? {}
  } catch {
    return {}
  }
}

export default function Booking() {
  const { user } = useAuth()
  const { t } = useI18n()
  const navigate = useNavigate()
  const b = useBooking()
  const menu = useMenu(b.restaurantId)
  const [saved] = useState(readExtra)

  const [qty, setQty] = useState(saved.qty ?? {})
  const [wish, setWish] = useState(saved.wish ?? '')
  const [card, setCard] = useState({ cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [done, setDone] = useState(null) // kinnitatud broneering → tšekk

  // Menüü vahetub koos restoraniga: jätame alles ainult selle restorani toidud
  const items = menu.filter((m) => qty[m.id] > 0).map((m) => ({ menuItemId: m.id, quantity: qty[m.id] }))
  const preorderTotal = menu.reduce((s, m) => s + (qty[m.id] ?? 0) * m.price, 0)
  const total = preorderTotal

  async function confirm() {
    setError('')
    if (!b.chosenTable) {
      document.getElementById('plan')?.scrollIntoView()
      return b.setError(t('booking.chooseHint'))
    }

    // Sisselogimata kasutaja: jätame valikud meelde ja suuname sisselogimisele
    if (!user) {
      b.saveDraft()
      sessionStorage.setItem(EXTRA_KEY, JSON.stringify({ qty, wish }))
      navigate('/login', { state: { from: '/broneeri' } })
      return
    }

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
          specialRequest: wish.trim() || null,
          items,
        },
      })
      // Võltsmakse: päris raha ei liigu
      if (total > 0) {
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

  function closeReceipt() {
    setDone(null)
    navigate('/minu')
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
      <EveningSection b={b} />
      <PlaceSection
        b={b}
        action={{ label: t('booking.continue'), onClick: () => document.getElementById('menu')?.scrollIntoView() }}
      />
      <GastronomySection menu={menu} qty={qty} setQty={setQty} />
      <DetailsSection
        user={user}
        wish={wish}
        setWish={setWish}
        card={card}
        setCard={setCard}
        lines={[[t('gastro.preorder'), money(preorderTotal)]]}
        total={total}
        onConfirm={confirm}
        busy={busy}
        error={error || b.error}
      />
      {done && <Receipt r={done} total={done.preOrderTotal} onClose={closeReceipt} />}
    </div>
  )
}
