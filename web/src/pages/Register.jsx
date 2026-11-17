import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'

export default function Register() {
  const { register } = useAuth()
  const { t } = useI18n()
  const navigate = useNavigate()
  const location = useLocation()
  const [form, setForm] = useState({ name: '', email: '', phone: '', password: '', password2: '' })
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.value })

  // Kliendipoolne valideerimine (samad reeglid on ka API-s)
  function validate() {
    const e = {}
    if (form.name.trim().length < 2) e.name = t('auth.err.name')
    if (!/^\S+@\S+\.\S+$/.test(form.email)) e.email = t('auth.err.email')
    if (form.phone && !/^[+\d\s-]{5,30}$/.test(form.phone)) e.phone = t('auth.err.phone')
    if (form.password.length < 6) e.password = t('auth.err.passwordShort')
    if (form.password !== form.password2) e.password2 = t('auth.err.passwordMatch')
    return e
  }

  async function onSubmit(ev) {
    ev.preventDefault()
    setError('')
    const e = validate()
    setErrors(e)
    if (Object.keys(e).length) return

    setBusy(true)
    try {
      await register({ name: form.name, email: form.email, phone: form.phone || null, password: form.password })
      navigate(location.state?.from ?? '/broneeri')
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  const field = (name, label, type = 'text', autoComplete) => (
    <label className="field">
      {label}
      <input type={type} value={form[name]} onChange={set(name)} autoComplete={autoComplete} />
      {errors[name] && <span className="field-error">{errors[name]}</span>}
    </label>
  )

  return (
    <div className="auth">
      <div className="container narrow">
        <form className="auth-card" onSubmit={onSubmit} noValidate>
          <div className="head">
            <span className="diamond" aria-hidden="true" />
            <p className="eyebrow">Argent</p>
            <h1>{t('auth.registerTitle')}</h1>
          </div>
          {error && <p className="alert alert-error">{error}</p>}
          {field('name', t('auth.name'), 'text', 'name')}
          {field('email', t('auth.email'), 'email', 'email')}
          {field('phone', t('auth.phone'), 'tel', 'tel')}
          <div className="field-row">
            {field('password', t('auth.password'), 'password', 'new-password')}
            {field('password2', t('auth.password2'), 'password', 'new-password')}
          </div>
          <button className="btn btn-dark btn-pill btn-big btn-block" disabled={busy}>
            {busy ? t('auth.wait') : t('auth.registerBtn')}
          </button>
          <p className="foot">
            {t('auth.hasAccount')} <Link to="/login" state={location.state}>{t('auth.loginBtn')}</Link>
          </p>
        </form>
      </div>
    </div>
  )
}
