import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import EveningSection from '../components/booking/EveningSection'
import PlaceSection from '../components/booking/PlaceSection'
import StepsBar from '../components/booking/StepsBar'
import { useBooking } from '../components/booking/useBooking'
import { useI18n } from '../i18n'

export default function Booking() {
  const { user } = useAuth()
  const { t } = useI18n()
  const navigate = useNavigate()
  const b = useBooking()
  const [busy, setBusy] = useState(false)

  async function confirm() {
    b.setError('')
    if (!b.chosenTable) return b.setError(t('booking.chooseHint'))

    // Sisselogimata kasutaja: jätame valikud meelde ja suuname sisselogimisele
    if (!user) {
      b.saveDraft()
      navigate('/login', { state: { from: '/broneeri' } })
      return
    }

    setBusy(true)
    try {
      const created = await api('/api/reservations', {
        method: 'POST',
        body: { restaurantId: b.restaurantId, date: b.date, time: b.time, guests: b.guests, tableId: b.chosenTable.id },
      })
      b.clearDraft()
      navigate(`/broneering/${created.id}`, { state: { justCreated: true } })
    } catch (e) {
      b.setError(e.message)
      if (e.status === 409) {
        // Keegi jõudis ette: värskendame plaani
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
        ]}
      />
      <EveningSection b={b} />
      <PlaceSection
        b={b}
        action={{ label: user ? t('booking.confirm') : t('booking.loginToBook'), onClick: confirm, busy }}
      />
    </div>
  )
}
