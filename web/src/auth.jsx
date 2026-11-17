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
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  return useContext(AuthContext)
}
