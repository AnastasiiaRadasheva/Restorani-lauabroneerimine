// Kõik päringud API-le käivad läbi selle faili.
// Klient ei pöördu kunagi otse andmebaasi poole.

export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

const STORAGE_KEY = 'argent-auth'

export function getSavedAuth() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY)) ?? null
  } catch {
    return null
  }
}

export function saveAuth(auth) {
  if (auth) localStorage.setItem(STORAGE_KEY, JSON.stringify(auth))
  else localStorage.removeItem(STORAGE_KEY)
}

export class ApiError extends Error {
  constructor(message, status, errors) {
    super(message)
    this.status = status
    this.errors = errors ?? {}
  }
}

/** Teeb päringu API-le. Lisab JWT tokeni ja muudab ProblemDetails vea loetavaks teateks. */
export async function api(path, { method = 'GET', body } = {}) {
  const headers = { Accept: 'application/json' }
  const auth = getSavedAuth()
  if (auth?.token) headers.Authorization = `Bearer ${auth.token}`
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  let response
  try {
    response = await fetch(API_URL + path, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError('Serveriga ei saa ühendust. Kas API töötab?', 0)
  }

  if (response.status === 204) return null
  const data = await response.json().catch(() => null)

  if (!response.ok) {
    if (response.status === 401 && auth?.token) {
      // Token aegus: logime välja
      saveAuth(null)
      window.dispatchEvent(new Event('auth-expired'))
    }
    throw new ApiError(problemMessage(data, response.status), response.status, data?.errors)
  }
  return data
}

function problemMessage(data, status) {
  if (data?.errors) {
    return Object.values(data.errors).flat().join(' ')
  }
  if (data?.detail) return data.detail
  if (status === 401) return 'Palun logi sisse.'
  if (status === 403) return 'Sul pole selleks õigust.'
  if (status === 404) return 'Ei leitud.'
  return data?.title ?? 'Midagi läks valesti.'
}

/** Abifunktsioon päringuparameetrite jaoks: tühjad väärtused jäetakse välja. */
export function qs(params) {
  const p = new URLSearchParams()
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') p.append(k, v)
  })
  const s = p.toString()
  return s ? `?${s}` : ''
}
