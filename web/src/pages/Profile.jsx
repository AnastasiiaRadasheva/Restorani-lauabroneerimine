import { useState } from 'react'
import { api } from '../api'
import { useAuth } from '../auth'
import { UserIcon } from '../components/Icons'
import { useI18n } from '../i18n'

/** Vähendab pildi 256×256 JPEG-iks (et andmebaas ei paisuks). */
function resizeImage(file) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.onload = () => {
      const size = 256
      const canvas = document.createElement('canvas')
      canvas.width = size
      canvas.height = size
      const s = Math.min(img.width, img.height)
      canvas.getContext('2d').drawImage(img, (img.width - s) / 2, (img.height - s) / 2, s, s, 0, 0, size, size)
      resolve(canvas.toDataURL('image/jpeg', 0.82))
    }
    img.onerror = reject
    img.src = URL.createObjectURL(file)
  })
}

/** Minu profiil: pilt, nimi, telefon, e-post ja SMS-ide nõusolek. */
export default function Profile() {
  const { user, updateUser } = useAuth()
  const { t } = useI18n()
  const [form, setForm] = useState({ name: user.name, phone: user.phone ?? '', smsOptIn: user.smsOptIn ?? true })
  const [avatar, setAvatar] = useState(user.avatarUrl)
  const [changedAvatar, setChangedAvatar] = useState(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  async function pick(e) {
    const file = e.target.files?.[0]
    if (!file) return
    try {
      const data = await resizeImage(file)
      setAvatar(data)
      setChangedAvatar(data)
    } catch {
      setError(t('profile.badImage'))
    }
  }

  async function save(e) {
    e.preventDefault()
    setError('')
    setMessage('')
    try {
      const u = await api('/api/auth/me', {
        method: 'PUT',
        body: { name: form.name, phone: form.phone || null, smsOptIn: form.smsOptIn, avatarUrl: changedAvatar ?? '' },
      })
      updateUser(u)
      setChangedAvatar(null)
      setMessage(t('profile.saved'))
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="auth">
      <div className="container narrow">
        <form className="auth-card" onSubmit={save}>
          <div className="head">
            <label className="avatar-pick" title={t('profile.photo')}>
              {avatar ? <img src={avatar} alt="" /> : <UserIcon />}
              <input type="file" accept="image/*" onChange={pick} />
            </label>
            <p className="eyebrow">{t('profile.photo')}</p>
            <h1>{t('profile.title')}</h1>
          </div>
          {message && <p className="alert alert-ok">{message}</p>}
          {error && <p className="alert alert-error">{error}</p>}
          <label className="field">
            {t('auth.name')}
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </label>
          <label className="field">
            {t('auth.email')}
            <input value={user.email} disabled />
          </label>
          <label className="field">
            {t('details4.phone')}
            <input type="tel" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="+372 5555 5555" />
          </label>
          <label className="check">
            <input type="checkbox" checked={form.smsOptIn} onChange={(e) => setForm({ ...form, smsOptIn: e.target.checked })} />
            <span>{t('profile.sms')}</span>
          </label>
          <button className="btn btn-dark btn-pill btn-big btn-block">{t('details.save')}</button>
        </form>
      </div>
    </div>
  )
}
