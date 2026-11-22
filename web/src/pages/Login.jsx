import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'

export default function Login() {
  const { login, logout } = useAuth()
  const { t } = useI18n()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    // Kliendipoolne valideerimine
    if (!email.includes('@')) return setError(t('auth.err.email'))
    if (!password) return setError(t('auth.err.password'))

    setBusy(true)
    try {
      const user = await login(email, password)
      // Teenusekonto (Worker) on ainult taustateenuse jaoks – saidile sellega ei logita
      if (user.role === 'Service') {
        logout()
        return setError(t('auth.serviceAccount'))
      }
      const from = location.state?.from
      if (from) navigate(from)
      else if (user.role === 'Admin') navigate('/admin')
      else if (user.role === 'Waiter') navigate('/personal/plaan')
      else navigate('/')
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth">
      <div className="container narrow">
        <form className="auth-card" onSubmit={onSubmit} noValidate>
          <div className="head">
            <span className="diamond" aria-hidden="true" />
            <p className="eyebrow">{t('auth.welcome')}</p>
            <h1>{t('auth.loginTitle')}</h1>
          </div>
          {error && <p className="alert alert-error">{error}</p>}
          <label className="field">
            {t('auth.email')}
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="email" />
          </label>
          <label className="field">
            {t('auth.password')}
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" />
          </label>
          <button className="btn btn-dark btn-pill btn-big btn-block" disabled={busy}>
            {busy ? t('auth.wait') : t('auth.loginBtn')}
          </button>
          <p className="foot">
            {t('auth.noAccount')} <Link to="/register" state={location.state}>{t('auth.register')}</Link>
          </p>
        </form>
      </div>
    </div>
  )
}
