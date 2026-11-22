import { createContext, useContext, useEffect, useState } from 'react'
import { api, getSavedAuth, saveAuth } from './api'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(getSavedAuth)

  useEffect(() => {
    const onExpired = () => setAuth(null)
    window.addEventListener('auth-expired', onExpired)
    return () => window.removeEventListener('auth-expired', onExpired)
  }, [])

  // Saidi avamisel kontrollime salvestatud sisselogimist: kui kontot enam pole (nt andmebaas loodi uuesti)
  // või see kuulub nüüd teisele kasutajale, logitakse välja – muidu jääks sait "poolikult sisse logituks"
  useEffect(() => {
    const saved = getSavedAuth()
    if (!saved?.token) return
    api('/api/auth/me')
      .then((me) => {
        if (me.email !== saved.user?.email) {
          saveAuth(null)
          setAuth(null)
        }
      })
      .catch((e) => {
        if (e.status === 401 || e.status === 404) {
          saveAuth(null)
          setAuth(null)
        }
      })
  }, [])

  async function login(email, password) {
    const result = await api('/api/auth/login', { method: 'POST', body: { email, password } })
    saveAuth(result)
    setAuth(result)
    return result.user
  }

  async function register(data) {
    const result = await api('/api/auth/register', { method: 'POST', body: data })
    saveAuth(result)
    setAuth(result)
    return result.user
  }

  /** Profiili muutmisel uuendame salvestatud kasutaja. */
  function updateUser(user) {
    const next = { ...auth, user }
    saveAuth(next)
    setAuth(next)
  }

  /** Sisselogimine valmis vastusega (nt pärast parooli loomist lingi kaudu). */
  function signIn(result) {
    saveAuth(result)
    setAuth(result)
  }

  function logout() {
    saveAuth(null)
    setAuth(null)
  }

  const user = auth?.user ?? null
  const value = {
    user,
    token: auth?.token ?? null,
    isAdmin: user?.role === 'Admin',
    isStaff: user?.role === 'Admin' || user?.role === 'Waiter',
    login,
    register,
    signIn,
    logout,
    updateUser,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  return useContext(AuthContext)
}
