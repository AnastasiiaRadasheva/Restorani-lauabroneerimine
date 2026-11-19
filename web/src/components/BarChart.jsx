import { useState } from 'react'

/**
 * Lihtne tulpdiagramm (üks andmerida, seega legendi pole – pealkiri ütleb, mida näidatakse).
 * Hiirega tulba kohal näidatakse täpset väärtust. Suurim tulp on veinipunane.
 */
export default function BarChart({ title, items, valueKey = 'reservations', unit = 'broneeringut' }) {
  const [hover, setHover] = useState(null)
  const max = Math.max(1, ...items.map((i) => i[valueKey]))
  const top = items.reduce((best, i) => (i[valueKey] > (best?.[valueKey] ?? -1) ? i : best), null)

  return (
    <figure className="chart">
      <figcaption>
        <span className="card-title">{title}</span>
        {top && <span className="muted small">kõige rohkem: {top.label} ({top[valueKey]})</span>}
      </figcaption>
      <div className="chart-plot">
        {items.map((i) => (
          <div key={i.label} className="chart-col" onMouseEnter={() => setHover(i)} onMouseLeave={() => setHover(null)}>
            {hover === i && (
              <div className="chart-tip">
                <strong>{i[valueKey]}</strong> {unit}
                <div className="muted">{i.label} · {i.guests} külalist</div>
              </div>
            )}
            <div className={i === top ? 'chart-bar top' : 'chart-bar'} style={{ height: `${(i[valueKey] / max) * 100}%` }} />
            <span className="chart-label">{i.label.slice(0, 5)}</span>
          </div>
        ))}
      </div>
      <details className="chart-table">
        <summary>Näita tabelina</summary>
        <table className="data-table">
          <thead><tr><th>{title}</th><th>Broneeringuid</th><th>Külalisi</th></tr></thead>
          <tbody>
            {items.map((i) => <tr key={i.label}><td>{i.label}</td><td>{i.reservations}</td><td>{i.guests}</td></tr>)}
          </tbody>
        </table>
      </details>
    </figure>
  )
}
