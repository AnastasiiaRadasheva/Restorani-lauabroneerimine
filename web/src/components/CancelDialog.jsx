import { useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { money } from '../utils'

/**
 * Broneeringu tühistamine.
 * Rohkem kui 24 h enne: makstud raha tagastatakse lähimate tööpäevade jooksul.
 * Vähem kui 24 h enne: raha EI tagastata – küsime kaks korda ja külaline peab märkima linnukese.
 */
export default function CancelDialog({ r, onClose, onCancelled }) {
  const { t } = useI18n()
  // Hoiatus "raha ei tagastata" ainult siis, kui on midagi makstud ja külastuseni on alla 24 h
  const late = !r.canModify && r.status === 'Active' && r.paidAmount > 0
  const paid = r.paidAmount ?? 0 // kohapeal makstavat osa ei tagastata
  const [step, setStep] = useState(1)
  const [agreed, setAgreed] = useState(false)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function cancel() {
    setBusy(true)
    try {
      const result = await api(`/api/reservations/${r.id}/cancel`, { method: 'PATCH', body: { acceptNoRefund: late && agreed } })
      onCancelled(result)
    } catch (e) {
      setError(e.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="dialog-backdrop" role="dialog" aria-modal="true" aria-label={t('cancel.title')}>
      <div className={late ? 'dialog late' : 'dialog'}>
        <p className="eyebrow">{t('cancel.title')}</p>

        {!late && (
          <>
            <p className="dialog-title">{t('cancel.sure')}</p>
            <p>{paid > 0 ? t('cancel.refund', { sum: money(paid) }) : t('cancel.nothingPaid')}</p>
            <div className="actions">
              <button className="btn btn-ox" disabled={busy} onClick={cancel}>{t('cancel.yes')}</button>
              <button className="btn" onClick={onClose}>{t('cancel.keep')}</button>
            </div>
          </>
        )}

        {late && step === 1 && (
          <>
            <p className="dialog-title">{t('cancel.lateTitle')}</p>
            <p className="warning">{paid > 0 ? t('cancel.noRefund', { sum: money(paid) }) : t('cancel.noRefundGeneric')}</p>
            <label className="check ox">
              <input type="checkbox" checked={agreed} onChange={(e) => setAgreed(e.target.checked)} />
              <span>{t('cancel.agree')}</span>
            </label>
            <div className="actions">
              <button className="btn btn-ox" disabled={!agreed} onClick={() => setStep(2)}>{t('cancel.continue')}</button>
              <button className="btn" onClick={onClose}>{t('cancel.keep')}</button>
            </div>
          </>
        )}

        {late && step === 2 && (
          <>
            <p className="dialog-title">{t('cancel.again')}</p>
            <p className="warning">{paid > 0 ? t('cancel.noRefund', { sum: money(paid) }) : t('cancel.noRefundGeneric')}</p>
            <div className="actions">
              <button className="btn btn-ox" disabled={busy} onClick={cancel}>{t('cancel.yesFinal')}</button>
              <button className="btn" onClick={onClose}>{t('cancel.keep')}</button>
            </div>
          </>
        )}

        {error && <p className="field-error">{error}</p>}
      </div>
    </div>
  )
}
