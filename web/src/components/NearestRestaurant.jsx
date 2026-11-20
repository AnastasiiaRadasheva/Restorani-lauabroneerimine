import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, qs } from '../api'
import { useI18n } from '../i18n'
import { formatDistance } from '../utils'

/**
 * Telefonis avalehel: lähim restoran. Asukohta küsitakse ainult siis,
 * kui kasutaja on selle brauseris juba lubanud – muidu näidatakse esimest restorani.
 */
export default function NearestRestaurant() {
  const { t } = useI18n()
  const [r, setR] = useState(null)

  useEffect(() => {
    const load = (pos) =>
      api('/api/restaurants' + qs({ lat: pos?.lat, lng: pos?.lng }))
        .then((list) => setR(list[0] ?? null))
        .catch(() => {})

    navigator.permissions
      ?.query({ name: 'geolocation' })
      .then((p) => {
        if (p.state === 'granted') {
          navigator.geolocation.getCurrentPosition(
            (x) => load({ lat: x.coords.latitude, lng: x.coords.longitude }),
            () => load(),
          )
        } else load()
      })
      .catch(() => load()) ?? load()
  }, [])

  if (!r) return null
  return (
    <Link to={`/broneeri?restaurant=${r.id}`} className="nearest">
      <span className="diamond" aria-hidden="true" />
      <span className="text">
        <span className="eyebrow">{t('home.nearest')}</span>
        <span className="name">{r.name}</span>
      </span>
      {r.distanceMeters != null && <span className="dist">{formatDistance(r.distanceMeters)}</span>}
    </Link>
  )
}
