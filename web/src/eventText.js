import { useI18n } from './i18n'

/**
 * Gastronoomilise õhtu tekstid valitud keeles.
 * Näidisõhtutel (kood) on valmis tõlked. Admini loodud õhtul on pealkiri ja kirjeldus admini keeles,
 * kuid riietus ja riietuse reegel tulevad valmisvalikust – need tõlgitakse automaatselt.
 */
const DRESS = {
  'smart casual': 'dress.smartCasual',
  'smart elegant': 'dress.smartElegant',
  kokteil: 'dress.cocktail',
  'black tie': 'dress.blackTie',
  'white tie': 'dress.whiteTie',
  'täielikult valge': 'dress.allWhite',
}

/** Riietuse valmisvalikud admini vormis ('' = riietuse reeglit pole). */
export const DRESS_OPTIONS = ['', ...Object.keys(DRESS)]

/** Riietuse reegli tekst, mille admini vorm paneb kohustusliku riietuse korral. */
export const dressRuleEt = (dress) => `Õhtu riietus on ${dress}. Kui riietus ei vasta nõuetele, võib administratsioon osalemisest keelduda; osalustasu ei tagastata.`

// Veebi väljanimi -> admini tõlke väli
const FIELD = { title: 'title', kind: 'kind', desc: 'description', program: 'program' }

/**
 * Õhtu tekst valitud keeles: 1) admini tõlge, 2) näidisõhtu valmis tõlge (kood), 3) eestikeelne tekst.
 * ev: õhtu ({ code, translations }) või registreerimine ({ eventCode, translations }).
 */
export function eventField(t, lang, ev, f, fallback) {
  const code = ev?.code ?? ev?.eventCode
  return ev?.translations?.[lang]?.[FIELD[f] ?? f] || (code ? t(`event.${code}.${f}`, {}, '') : '') || fallback
}

export const eventTitle = (t, lang, ev) => eventField(t, lang, ev, 'title', ev?.title ?? ev?.eventTitle)

export function useEventText(e) {
  const { t, lang } = useI18n()
  const own = (f) => (e.code ? t(`event.${e.code}.${f}`, {}, '') : '')
  const field = (f, fallback) => eventField(t, lang, e, f, fallback)
  const dress = () => {
    if (!e.dressCode) return ''
    return own('dress') || (DRESS[e.dressCode] ? t(DRESS[e.dressCode]) : e.dressCode)
  }
  const rule = () => {
    if (!e.rule) return ''
    return own('rule') || (e.rule === dressRuleEt(e.dressCode) ? t('dress.rule', { dress: dress() }) : e.rule)
  }
  return { field, dress, rule }
}
