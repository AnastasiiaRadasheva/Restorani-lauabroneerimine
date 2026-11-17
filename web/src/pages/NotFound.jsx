import { Link } from 'react-router-dom'
import { useI18n } from '../i18n'

export default function NotFound() {
  const { t } = useI18n()
  return (
    <div className="notfound container">
      <span className="diamond" aria-hidden="true" />
      <h1>{t('notFound.title')}</h1>
      <p className="muted">{t('notFound.text')}</p>
      <Link to="/" className="btn btn-dark btn-pill">{t('notFound.back')}</Link>
    </div>
  )
}
