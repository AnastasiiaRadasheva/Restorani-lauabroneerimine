import { useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { money } from '../utils'
import { useExtrasCatalog } from './booking/EveningDetails'
import Messages from './Messages'

/** "Minu külastused" kaardi parem pool: sõnumid restoranile ja broneeringu täiendamine. */
export default function VisitExtras({ r, onChanged }) {
  const { t } = useI18n()
  const catalog = useExtrasCatalog()
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const codes = r.extras.map((x) => x.code)
  const lockedCodes = r.extras.filter((x) => x.locked).map((x) => x.code)

  async function toggle(code) {
    setBusy(true)
    try {
      // Broneerimisel makstud teenust eemaldada ei saa; hiljem lisatut saab
      if (r.extras.find((x) => x.code === code)?.locked) return
      const next = codes.includes(code) ? codes.filter((c) => c !== code) : [...codes, code]
      await api(`/api/reservations/${r.id}/extras`, { method: 'PUT', body: { extras: next } })
      setError('')
      onChanged()
    } catch (e) {
      setError(e.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="visit-extra">
      <Messages reservationId={r.id} />

      <div className="addons">
        <div className="extras-head">
          <p className="eyebrow">{t('visits.addTitle')}</p>
          <span className="italic">{r.onSiteAmount ? t('visits.onSitePart', { sum: money(r.onSiteAmount) }) : ''}</span>
        </div>
        {catalog.map((x) => {
          const on = codes.includes(x.code)
          return (
            <div key={x.code} className="addon">
              <div className="text">
                <span className="name">{t(`extra.${x.code}`)}</span>
                <span className="desc">{t(`extra.${x.code}.desc`)}</span>
              </div>
              <span className="price">{x.price ? money(x.price) : t('evening.free')}</span>
              <button type="button" className={on ? 'btn btn-small btn-dark' : 'btn btn-small'} disabled={busy || lockedCodes.includes(x.code)} onClick={() => toggle(x.code)} aria-pressed={on} title={on && !lockedCodes.includes(x.code) ? t('visits.removeHint') : undefined}>
                {lockedCodes.includes(x.code) ? t('visits.paidLocked') : on ? t('visits.remove') : t('visits.add')}
              </button>
            </div>
          )
        })}
        {lockedCodes.length > 0 && <p className="muted small italic">{t('visits.noRemove')}</p>}
        {r.isPaid && <p className="muted small italic">{t('visits.payOnSite')}</p>}
        {error && <p className="field-error">{error}</p>}
      </div>
    </div>
  )
}
