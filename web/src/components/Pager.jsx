/** Lehekülgede nupud PagedResult vastuse jaoks. */
export default function Pager({ page, totalPages, totalCount, onChange }) {
  if (!totalPages || totalPages <= 1) return <p className="muted small">Kokku {totalCount ?? 0}</p>
  return (
    <div className="pager">
      <button className="btn btn-small" disabled={page <= 1} onClick={() => onChange(page - 1)}>‹ Eelmine</button>
      <span>Leht {page} / {totalPages} · kokku {totalCount}</span>
      <button className="btn btn-small" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>Järgmine ›</button>
    </div>
  )
}
