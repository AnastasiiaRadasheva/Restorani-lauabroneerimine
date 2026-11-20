import { createContext, useContext, useEffect, useState } from 'react'
import et from './locales/et'
import ru from './locales/ru'
import en from './locales/en'

/**
 * Kõik lehe tekstid on ühes kohas (src/locales). Komponendis: const { t } = useI18n(); t('home.title')
 * Muutujad: t('booking.guests', { n: 4 }) → "{n}" asendatakse.
 * Mitmus: kui tõlge on objekt { one, other } (vene keeles ka few, many), valitakse vorm arvu n järgi.
 */
export const LANGUAGES = [
  { code: 'et', short: 'ET', name: 'Eesti', locale: 'et-EE', dict: et },
  { code: 'ru', short: 'RU', name: 'Русский', locale: 'ru-RU', dict: ru },
  { code: 'en', short: 'EN', name: 'English', locale: 'en-GB', dict: en },
]

const STORAGE_KEY = 'argent-lang'
const I18nContext = createContext(null)

function savedLang() {
  try {
    const s = localStorage.getItem(STORAGE_KEY)
    if (LANGUAGES.some((l) => l.code === s)) return s
  } catch {
    /* privaatne aken – kasutame vaikimisi keelt */
  }
  return LANGUAGES[0].code
}

export function I18nProvider({ children }) {
  const [lang, setLangState] = useState(savedLang)
  const current = LANGUAGES.find((l) => l.code === lang) ?? LANGUAGES[0]

  useEffect(() => {
    document.documentElement.lang = current.code
  }, [current.code])

  function setLang(code) {
    setLangState(code)
    try {
      localStorage.setItem(STORAGE_KEY, code)
    } catch {
      /* ignoreerime */
    }
  }

  function t(key, vars = {}) {
    let value = current.dict[key] ?? et[key] ?? key
    if (typeof value === 'object') {
      const form = new Intl.PluralRules(current.locale).select(vars.n ?? 0)
      value = value[form] ?? value.other
    }
    return value.replace(/\{(\w+)\}/g, (m, name) => (vars[name] ?? m))
  }

  const value = { t, lang: current.code, locale: current.locale, setLang, languages: LANGUAGES }
  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

export function useI18n() {
  return useContext(I18nContext)
}
