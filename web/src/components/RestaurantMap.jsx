import { useEffect, useMemo } from 'react'
import L from 'leaflet'
import { MapContainer, Marker, Polyline, TileLayer, useMap } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import { formatDistance } from '../utils'

const TALLINN = [59.4405, 24.755]

/** Liigutab kaardi nii, et kõik punktid oleksid näha. */
function FitBounds({ points }) {
  const map = useMap()
  const key = JSON.stringify(points)
  useEffect(() => {
    // Ilma animatsioonita: animatsiooni ajal lehe uuendamine andis Leafleti vea "_leaflet_pos"
    if (points.length > 1) map.fitBounds(points, { padding: [60, 60], animate: false })
    else if (points.length === 1) map.setView(points[0], 14, { animate: false })
  }, [map, key]) // eslint-disable-line react-hooks/exhaustive-deps
  return null
}

/** Restorani märk: romb ja kaugus (nagu kujunduses). */
function restaurantIcon(r, selected) {
  const dist = r.distanceMeters != null ? `<em>${formatDistance(r.distanceMeters)}</em>` : ''
  return L.divIcon({
    className: '',
    html: `<span class="map-pin${selected ? ' on' : ''}"><i></i><b>${r.name.replace('Argent · ', '')}</b>${dist}</span>`,
    iconSize: [0, 0],
  })
}

const meIcon = L.divIcon({ className: '', html: '<span class="map-me"><i></i></span>', iconSize: [0, 0] })

/**
 * Restoranide kaart (OpenStreetMap, hallides toonides). Kui kasutaja asukoht on teada,
 * näidatakse punane romb ja punktiirjoon igasse restorani.
 */
export default function RestaurantMap({ restaurants, userPos, selectedId, onSelect, labels }) {
  const points = useMemo(
    () => [
      ...restaurants.map((r) => [r.latitude, r.longitude]),
      ...(userPos ? [[userPos.lat, userPos.lng]] : []),
    ],
    [restaurants, userPos],
  )

  return (
    <div className="map-box">
      <MapContainer center={TALLINN} zoom={13} className="map" scrollWheelZoom={false} attributionControl={false}>
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <FitBounds points={points} />

        {userPos &&
          restaurants.map((r) => (
            <Polyline
              key={`line-${r.id}`}
              positions={[[userPos.lat, userPos.lng], [r.latitude, r.longitude]]}
              pathOptions={{ color: '#0c0d0e', weight: 1, dashArray: '3 5', opacity: r.id === selectedId ? 1 : 0.5 }}
            />
          ))}

        {restaurants.map((r) => (
          <Marker
            key={`${r.id}-${r.id === selectedId}-${r.distanceMeters}`}
            position={[r.latitude, r.longitude]}
            icon={restaurantIcon(r, r.id === selectedId)}
            eventHandlers={{ click: () => onSelect(r) }}
          />
        ))}

        {userPos && <Marker position={[userPos.lat, userPos.lng]} icon={meIcon} />}
      </MapContainer>
      <div className="map-legend">
        <span><i className="me" />{labels.me}</span>
        <span><i className="dash" />{labels.distance}</span>
      </div>
      <span className="map-credit">© OpenStreetMap</span>
    </div>
  )
}
