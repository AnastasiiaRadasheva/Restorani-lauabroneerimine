import { useI18n } from '../../i18n'

export const ALLERGIES = ['glutenfree', 'lactosefree', 'nutfree', 'noseafood', 'vegetarian', 'vegan']

/** Allergeenid ja toitumine – kõige viimane plokk enne makset. */
export default function AllergyBlock({ allergies, setAllergies, note, setNote }) {
  const { t } = useI18n()
  const toggle = (code) => setAllergies(allergies.includes(code) ? allergies.filter((c) => c !== code) : [...allergies, code])

  return (
    <div className="paper white allergy-card">
      <p className="card-title">{t('allergy.title')}</p>
      <p className="muted small">{t('allergy.lead')}</p>
      <div className="allergy-grid">
        {ALLERGIES.map((code) => (
          <label key={code} className={allergies.includes(code) ? 'allergy on' : 'allergy'}>
            <input type="checkbox" checked={allergies.includes(code)} onChange={() => toggle(code)} />
            {t(`allergy.${code}`)}
          </label>
        ))}
      </div>
      <label className="field">
        {t('allergy.note')}
        <textarea className="box" rows={3} maxLength={500} value={note} onChange={(e) => setNote(e.target.value)} placeholder={t('allergy.placeholder')} />
      </label>
      <p className="allergy-info">{t('allergy.info')}</p>
    </div>
  )
}
