import { useEffect, useState } from 'react'
import { api } from '../../api'
import { useI18n } from '../../i18n'
import { money } from '../../utils'

export const OCCASIONS = ['None', 'Birthday', 'Anniversary', 'Proposal', 'Business']
export const GIFTS = ['Cake', 'Champagne', 'Dessert']
export const HOSTS = ['Me', 'Other', 'Social']
export const ADDRESS_FORMS = ['Name', 'Patronymic', 'Formal']
export const SERVICE_LANGS = [
  ['et', 'ET', 'Eesti'],
  ['ru', 'RU', 'Русский'],
  ['en', 'EN', 'English'],
]

/** Lisateenuste kataloog API-st (hinnad on serveris). */
export function useExtrasCatalog() {
  const [list, setList] = useState([])
  useEffect(() => {
    api('/api/reservations/extras').then(setList).catch(() => setList([]))
  }, [])
  return list
}

/**
 * Õhtu detailid: põhjus, peakülaline, peakoka kingitus ja lisateenused.
 * ev – olek { occasion, gift, hostType, hostName, addressForm, serviceLanguage, extras: [] }
 */
export default function EveningDetails({ ev, setEv, catalog, userName }) {
  const { t } = useI18n()
  const [open, setOpen] = useState(ev.occasion !== 'None' || ev.extras.length > 0)
  const set = (patch) => setEv({ ...ev, ...patch })
  const toggleExtra = (code) =>
    set({ extras: ev.extras.includes(code) ? ev.extras.filter((c) => c !== code) : [...ev.extras, code] })

  const extrasTotal = catalog.filter((x) => ev.extras.includes(x.code)).reduce((s, x) => s + x.price, 0)
  const summary = [ev.occasion !== 'None' && t(`occasion.${ev.occasion}`).toLowerCase(), ev.extras.length && `+${ev.extras.length}`]
    .filter(Boolean)
    .join(' · ')

  const person = ev.hostType === 'Other' ? ev.hostName || t('evening.guest') : userName || t('evening.guest')
  const [first, last] = person.split(' ')
  const example =
    ev.addressForm === 'Name'
      ? t('evening.exName', { name: first })
      : ev.addressForm === 'Patronymic'
        ? t('evening.exPatronymic', { name: first })
        : t('evening.exFormal', { name: last || first })

  return (
    <>
      <button type="button" className={open ? 'details-toggle open' : 'details-toggle'} onClick={() => setOpen(!open)} aria-expanded={open}>
        <span className="plus" aria-hidden="true">{open ? '−' : '+'}</span>
        <span className="text">
          <span className="big">{open ? t('evening.titleOpen') : t('evening.title')}</span>
          <span className="sub">{t('evening.sub')}</span>
        </span>
        <span className="italic">{summary || t('evening.optional')}</span>
      </button>

      {open && (
        <div className="evening-details">
          <div className="block">
            <p className="eyebrow">{t('evening.occasion')}</p>
            <div className="chips" role="radiogroup">
              {OCCASIONS.map((o) => (
                <button key={o} type="button" role="radio" aria-checked={ev.occasion === o} className={ev.occasion === o ? 'chip on' : 'chip'} onClick={() => set({ occasion: o })}>
                  {t(`occasion.${o}`)}
                </button>
              ))}
            </div>
          </div>

          <div className="paper white host-card">
            <p className="card-title">{t('evening.host')}</p>
            <p className="muted small">{t('evening.hostNote')}</p>
            <div className="chips" role="radiogroup">
              {HOSTS.map((h) => (
                <button key={h} type="button" role="radio" aria-checked={ev.hostType === h} className={ev.hostType === h ? 'chip on' : 'chip'} onClick={() => set({ hostType: h })}>
                  {t(`host.${h}`)}
                </button>
              ))}
            </div>
            {ev.hostType === 'Other' && (
              <label className="field">
                {t('evening.hostName')}
                <input value={ev.hostName} maxLength={100} onChange={(e) => set({ hostName: e.target.value })} placeholder="Mihkel Saar" />
              </label>
            )}
            {ev.hostType === 'Social' && <p className="social-note">{t('evening.socialNote')}</p>}
            {ev.hostType !== 'Social' && (
              <div className="host-row">
                <div className="host-col">
                  <span className="eyebrow">{t('evening.addressForm')}</span>
                  <div className="chips">
                    {ADDRESS_FORMS.map((a) => (
                      <button key={a} type="button" className={ev.addressForm === a ? 'chip small on' : 'chip small'} onClick={() => set({ addressForm: a })}>
                        {t(`address.${a}`)}
                      </button>
                    ))}
                  </div>
                  <span className="italic small">{example}</span>
                </div>
                <div className="host-col">
                  <span className="eyebrow">{t('evening.language')}</span>
                  <div className="lang-pick" role="radiogroup">
                    {SERVICE_LANGS.map(([code, short, name]) => (
                      <button key={code} type="button" role="radio" lang={code} aria-checked={ev.serviceLanguage === code} className={ev.serviceLanguage === code ? 'on' : ''} onClick={() => set({ serviceLanguage: code })}>
                        <b>{short}</b>
                        <span>{name}</span>
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            )}
          </div>

          {ev.occasion !== 'None' && (
            <div className="paper gift-card">
              <div className="row-between">
                <p className="card-title">{t('evening.gift')}</p>
                <span className="eyebrow ox">{t('evening.free')} · {t(`occasion.${ev.occasion}`).toLowerCase()}</span>
              </div>
              <p className="muted small">{t('evening.giftNote')}</p>
              <div className="gifts" role="radiogroup">
                {GIFTS.map((g) => (
                  <button key={g} type="button" role="radio" aria-checked={ev.gift === g} className={ev.gift === g ? 'gift on' : 'gift'} onClick={() => set({ gift: g })}>
                    <span className="dot" />
                    <span className="text">
                      <span className="name">{t(`gift.${g}`)}</span>
                      <span className="desc">{t(`gift.${g}.desc`)}</span>
                      <span className="price">0 €</span>
                    </span>
                  </button>
                ))}
              </div>
            </div>
          )}

          <div className="block">
            <div className="extras-head">
              <p className="eyebrow">{t('evening.extras')}</p>
              <span className="italic">{money(extrasTotal)}</span>
            </div>
            {catalog.map((x) => {
              const on = ev.extras.includes(x.code)
              return (
                <div key={x.code} className="extra">
                  <span className={on ? 'diamond ox' : 'diamond hollow'} aria-hidden="true" />
                  <div className="text">
                    <span className="name">{t(`extra.${x.code}`)}</span>
                    <span className="desc">{t(`extra.${x.code}.desc`)}</span>
                  </div>
                  <span className="price">{x.price ? money(x.price) : t('evening.free')}</span>
                  <button type="button" className={on ? 'square on' : 'square'} onClick={() => toggleExtra(x.code)} aria-pressed={on} aria-label={t(`extra.${x.code}`)}>
                    {on ? '✓' : '+'}
                  </button>
                </div>
              )
            })}
          </div>
        </div>
      )}
    </>
  )
}
