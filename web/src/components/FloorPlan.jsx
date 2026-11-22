import { useRef, useState } from 'react'
import { useI18n } from '../i18n'

/** Laudade kirjeldused on andmebaasis eesti keeles – siin nende tõlked. */
const SPOTS = {
  'akna all': ['у окна', 'by the window'],
  diivan: ['диван', 'sofa'],
  'saali keskel': ['в центре зала', 'centre of the room'],
  'pikk laud': ['длинный стол', 'long table'],
  'vaikne nurk': ['тихий угол', 'quiet corner'],
  'baari juures': ['у бара', 'by the bar'],
  'sissepääsu juures': ['у входа', 'by the entrance'],
  'kamina juures': ['у камина', 'by the fireplace'],
  'privaatne ruum': ['отдельный зал', 'private room'],
  'pidulik laud': ['праздничный стол', 'banquet table'],
  'vaade merele': ['вид на море', 'sea view'],
  'mere ääres': ['у моря', 'by the sea'],
  'päikese käes': ['на солнце', 'in the sun'],
  'varikatuse all': ['под навесом', 'under the canopy'],
  'vaade pargile': ['вид на парк', 'park view'],
  'purskkaevu juures': ['у фонтана', 'by the fountain'],
}
export function spotText(spot, lang) {
  const tr = SPOTS[spot?.toLowerCase()]
  return !tr || lang === 'et' ? spot : lang === 'ru' ? tr[0] : tr[1]
}

/**
 * Saali või terrassi plaan. Lauad paigutatakse protsentides, seega plaan skaleerub igale ekraanile.
 * Laua ümber on toolid (ümmargusel laual rombid, kandilisel laual pulgad pikkadel külgedel).
 *
 * tables: [{ id, number, seats, location, x, y, width, height, spot, isRound, state }]
 * state: 'free' | 'busy' | 'hold' | 'small' | 'selected' | (personalile) 'arrived'
 */
export default function FloorPlan({ tables, zone, bar, stage, stageLabel, edgeLabel, entranceLabel, barLabel, seatsLabel, onSelect, onMove, renderInfo }) {
  const { lang } = useI18n()
  const list = tables.filter((t) => t.location === zone)
  const roomRef = useRef(null)
  // Admin: laua lohistamine hiirega (onMove). Väike liigutus = klõps (laua valimine).
  const [drag, setDrag] = useState(null)

  function down(e, t) {
    if (!onMove) return
    e.preventDefault()
    const r = roomRef.current.getBoundingClientRect()
    setDrag({ id: t.id, px: e.clientX, py: e.clientY, ox: t.x, oy: t.y, x: t.x, y: t.y, w: r.width, h: r.height, moved: false })
  }
  function move(e) {
    if (!drag) return
    const t = list.find((x) => x.id === drag.id)
    const clamp = (v, max) => Math.max(0, Math.min(max, Math.round(v)))
    const x = clamp(drag.ox + ((e.clientX - drag.px) / drag.w) * 100, 100 - t.width)
    const y = clamp(drag.oy + ((e.clientY - drag.py) / drag.h) * 100, 100 - t.height)
    const moved = drag.moved || Math.abs(e.clientX - drag.px) + Math.abs(e.clientY - drag.py) > 4
    setDrag({ ...drag, x, y, moved })
  }
  function up() {
    if (!drag) return
    const t = list.find((x) => x.id === drag.id)
    if (drag.moved && (drag.x !== t.x || drag.y !== t.y)) onMove(t, drag.x, drag.y)
    else if (onSelect) onSelect(t)
    setDrag(null)
  }
  // Kas lohistatav laud kattuks teisega (näidatakse punasena)
  const hits = (a, b) => b && b.width > 0 && a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y
  const overlaps = (a) =>
    list.some((b) => b.id !== a.id && hits(a, b)) || (zone === 'Hall' && hits(a, bar)) || (stage?.zone === zone && hits(a, stage))

  return (
    <div className="plan">
      <div className="plan-edge">
        <span>{edgeLabel}</span>
      </div>
      <div className={onMove ? 'plan-room editable' : 'plan-room'} ref={roomRef} onPointerMove={move} onPointerUp={up} onPointerLeave={up}>
        {zone === 'Hall' && bar && bar.width > 0 && (
          <div className="plan-bar" style={{ left: `${bar.x}%`, top: `${bar.y}%`, width: `${bar.width}%`, height: `${bar.height}%` }}>
            <span>{barLabel}</span>
          </div>
        )}

        {stage && stage.zone === zone && stage.width > 0 && (
          <div className="plan-stage" style={{ left: `${stage.x}%`, top: `${stage.y}%`, width: `${stage.width}%`, height: `${stage.height}%` }}>
            <span>♪ {stageLabel}</span>
          </div>
        )}

        <div className="plan-entrance">
          <svg aria-hidden="true" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
            <path d="M12 20V5M6 11l6-6 6 6" />
          </svg>
          {entranceLabel}
        </div>

        {list.map((t0) => {
          const t = drag?.id === t0.id ? { ...t0, x: drag.x, y: drag.y } : t0
          const dragging = drag?.id === t0.id && drag.moved
          const clickable = onSelect && (t.state === 'free' || t.state === 'selected' || t.clickable)
          return (
            <div
              key={t.id}
              className={`plan-table state-${t.state}${t.isRound ? ' round' : ''}${onMove ? ' draggable' : ''}${dragging ? ' dragging' : ''}${dragging && overlaps(t) ? ' bad' : ''}`}
              style={{ left: `${t.x}%`, top: `${t.y}%`, width: `${t.width}%`, height: `${t.height}%` }}
              onPointerDown={(e) => down(e, t0)}
            >
              <Chairs table={t} />
              <button
                type="button"
                onClick={() => !onMove && clickable && onSelect(t)}
                disabled={!clickable && !onMove}
                aria-pressed={t.state === 'selected'}
                aria-label={`${t.number}, ${seatsLabel(t.seats)}${t.spot ? `, ${spotText(t.spot, lang)}` : ''}`}
              >
                <strong>{t.number}</strong>
                <small>{seatsLabel(t.seats)}</small>
                {renderInfo && renderInfo(t)}
              </button>
              {/* Kirjeldus laua all, et see ei jääks laua sisse peitu */}
              {t.spot && <em className="plan-spot">{spotText(t.spot, lang)}</em>}
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
