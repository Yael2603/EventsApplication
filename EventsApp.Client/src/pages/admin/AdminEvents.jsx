import { useState } from 'react'
import { Link } from 'react-router-dom'
import { eventsApi } from '../../api/services'
import { Badge, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { eventTypeLabels, formatShortDate, isFutureDate } from '../../utils/format'
import useLoad from '../../utils/useLoad'

const PAGE_SIZE = 10                                   // כמה אירועים בכל עמוד

export default function AdminEvents() {                 // כל האירועים במערכת
  const [page, setPage] = useState(1)                   // העמוד הנוכחי
  const { data, loading, error, reload } = useLoad(() => eventsApi.all(page, PAGE_SIZE), [page])   // GET /api/events?page=&pageSize= (מנהל)
  const events = data?.items || []                      // שורות העמוד הנוכחי
  const totalPages = data?.totalPages || 1              // מספר העמודים הכולל

  return (
    <>
      <PageHeader title="כל האירועים" subtitle="כניסה לאירוע לשיבוץ ספקים" />
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : events.length === 0 && !error ? (
        <EmptyState icon="📅" title="אין אירועים במערכת" />
      ) : (
        <div className="table-wrap">
          <table>
            <thead><tr><th>תאריך</th><th>סוג</th><th>מקום</th><th>לקוח</th><th>שיבוץ</th><th /></tr></thead>
            <tbody>
              {events.map((ev) => {
                const assigned = ev.items.filter((i) => i.providerId).length
                return (
                  <tr key={ev.id}>
                    <td>{formatShortDate(ev.date)} {!isFutureDate(ev.date) && <Badge>עבר</Badge>}</td>
                    <td>{eventTypeLabels[ev.type]}</td>
                    <td>{ev.location}</td>
                    <td>{ev.clientName}</td>
                    <td><Badge tone={assigned === ev.items.length ? 'green' : 'violet'}>{assigned}/{ev.items.length}</Badge></td>
                    <td><Link to={`/events/${ev.id}`} className="btn btn-ghost btn-sm">פתיחה</Link></td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
      {totalPages > 1 && (
        <div className="row" style={{ justifyContent: 'center', marginTop: 16 }}>
          <button className="btn btn-ghost btn-sm" disabled={page <= 1 || loading} onClick={() => setPage(page - 1)}>הקודם</button>
          <span className="muted small">עמוד {page} מתוך {totalPages} · {data?.totalCount} אירועים</span>
          <button className="btn btn-ghost btn-sm" disabled={page >= totalPages || loading} onClick={() => setPage(page + 1)}>הבא</button>
        </div>
      )}
    </>
  )
}
