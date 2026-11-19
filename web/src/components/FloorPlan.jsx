/**
 * Saali või terrassi plaan. Lauad paigutatakse protsentides, seega plaan skaleerub igale ekraanile.
 * Laua ümber on toolid (ümmargusel laual rombid, kandilisel laual pulgad pikkadel külgedel).
 *
 * tables: [{ id, number, seats, location, x, y, width, height, spot, isRound, state }]
 * state: 'free' | 'busy' | 'hold' | 'small' | 'selected' | (personalile) 'arrived'
 */
export default function FloorPlan({ tables, zone, bar, edgeLabel, entranceLabel, barLabel, seatsLabel, onSelect, renderInfo }) {
  const list = tables.filter((t) => t.location === zone)

  return (
    <div className="plan">
      <div className="plan-edge">
        <span>{edgeLabel}</span>
      </div>
      <div className="plan-room">
        {zone === 'Hall' && bar && bar.width > 0 && (
          <div className="plan-bar" style={{ left: `${bar.x}%`, top: `${bar.y}%`, width: `${bar.width}%`, height: `${bar.height}%` }}>
            <span>{barLabel}</span>
          </div>
        )}

        <div className="plan-entrance">
          <svg aria-hidden="true" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
            <path d="M12 20V5M6 11l6-6 6 6" />
          </svg>
          {entranceLabel}
        </div>

        {list.map((t) => {
          const clickable = onSelect && (t.state === 'free' || t.state === 'selected' || t.clickable)
          return (
            <div
              key={t.id}
              className={`plan-table state-${t.state}${t.isRound ? ' round' : ''}`}
              style={{ left: `${t.x}%`, top: `${t.y}%`, width: `${t.width}%`, height: `${t.height}%` }}
            >
              <Chairs table={t} />
              <button
                type="button"
                onClick={() => clickable && onSelect(t)}
                disabled={!clickable}
                aria-pressed={t.state === 'selected'}
                aria-label={`${t.number}, ${seatsLabel(t.seats)}${t.spot ? `, ${t.spot}` : ''}`}
              >
                <strong>{t.number}</strong>
                <small>{seatsLabel(t.seats)}</small>
                {t.spot && <em>{t.spot}</em>}
                {renderInfo && renderInfo(t)}
              </button>
            </div>
          )
        })}
      </div>
    </div>
  )
}

/** Toolid laua ümber. */
function Chairs({ table }) {
  const chairs = []
  if (table.isRound) {
    for (let k = 0; k < table.seats; k++) {
      const a = (Math.PI * 2 * k) / table.seats - Math.PI / 2
      chairs.push({ className: 'chair dot', style: { left: `${50 + 47 * Math.cos(a)}%`, top: `${50 + 47 * Math.sin(a)}%` } })
    }
  } else {
    // Toolid kahel pikemal küljel
    const tall = table.height > table.width * 1.6
    const side = Math.ceil(table.seats / 2)
    for (let k = 0; k < table.seats; k++) {
      const row = k < side ? 0 : 1
      const idx = row === 0 ? k : k - side
      const n = row === 0 ? side : table.seats - side
      const p = `${((idx + 1) / (n + 1)) * 100}%`
      chairs.push(
        tall
          ? { className: 'chair v', style: { top: p, [row === 0 ? 'left' : 'right']: 2 } }
          : { className: 'chair h', style: { left: p, [row === 0 ? 'top' : 'bottom']: 2 } },
      )
    }
  }
  return chairs.map((c, i) => <span key={i} aria-hidden="true" className={c.className} style={c.style} />)
}

/** Värvide seletus plaani kõrval. */
export function FloorLegend({ items }) {
  return (
    <div className="legend">
      {items.map(([state, text]) => (
        <span key={state}>
          <i className={`swatch state-${state}`} /> {text}
        </span>
      ))}
    </div>
  )
}
