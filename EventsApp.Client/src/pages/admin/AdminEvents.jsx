import { Link } from 'react-router-dom'
import { eventsApi } from '../../api/services'
import { Badge, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { eventTypeLabels, formatShortDate, isFutureDate } from '../../utils/format'
import useLoad from '../../utils/useLoad'

export default function AdminEvents() {                 // כל האירועים במערכת
  const { data, loading, error, reload } = useLoad(() => eventsApi.all(), [])   // GET /api/events (מנהל)

  return (
    <>
      <PageHeader title="כל האירועים" subtitle="כניסה לאירוע לשיבוץ ספקים" />
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="📅" title="אין אירועים במערכת" />
      ) : (
        <div className="table-wrap">
          <table>
            <thead><tr><th>תאריך</th><th>סוג</th><th>מקום</th><th>לקוח</th><th>שיבוץ</th><th /></tr></thead>
            <tbody>
              {(data || []).map((ev) => {
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
    </>
  )
}
