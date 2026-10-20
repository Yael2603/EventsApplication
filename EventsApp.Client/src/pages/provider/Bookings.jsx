import { profileApi } from '../../api/services'
import { Badge, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { eventTypeLabels, formatDate, formatMoney } from '../../utils/format'
import useLoad from '../../utils/useLoad'

export default function Bookings() {                    // אירועים שהספק שובץ אליהם
  const { data, loading, error, reload } = useLoad(() => profileApi.bookings(), [])   // GET /api/providers/me/bookings

  return (
    <>
      <PageHeader title="השיבוצים שלי" subtitle="האירועים שנבחרתם להפיק" />
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="📅" title="עדיין אין שיבוצים" text="כשתיבחרו לסעיף, האירוע יופיע כאן." />
      ) : (
        <div className="grid grid-2">
          {(data || []).map((b) => (
            <div className="card card-hover" key={b.itemId}>
              <div className="row row-between">
                <span className="event-date">{formatDate(b.eventDate)}</span>
                <Badge tone="green">{b.categoryName}</Badge>
              </div>
              <h3 style={{ marginTop: 10 }}>{eventTypeLabels[b.eventType]} · {b.location}</h3>
              <div className="meta"><span>👥 {b.guestCount} אורחים</span><span>תקציב: <span className="price">{formatMoney(b.budget)}</span></span></div>
              {b.details && <p className="muted" style={{ marginTop: 10 }}>{b.details}</p>}
              <div className="divider" />
              <div className="small">לקוח: <strong>{b.clientName}</strong> · <a href={`mailto:${b.clientEmail}`}>{b.clientEmail}</a></div>
            </div>
          ))}
        </div>
      )}
    </>
  )
}
