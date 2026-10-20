import { Link } from 'react-router-dom'
import { eventsApi } from '../../api/services'
import { Badge, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { eventTypeLabels, formatDate, isFutureDate } from '../../utils/format'
import useLoad from '../../utils/useLoad'

export default function MyEvents() {                    // רשימת האירועים של הלקוח
  const { data, loading, error, reload } = useLoad(() => eventsApi.mine(), [])   // GET /api/events/my

  return (
    <>
      <PageHeader title="האירועים שלי" subtitle="כל ההפקות שלכם במקום אחד">
        <Link to="/events/new" className="btn btn-primary">+ אירוע חדש</Link>
      </PageHeader>
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="🎉" title="עדיין אין אירועים" text="הזמינו תאריך והתחילו לבנות את האירוע הראשון.">
          <Link to="/events/new" className="btn btn-primary">להזמנת אירוע</Link>
        </EmptyState>
      ) : (
        <div className="grid grid-2">
          {(data || []).map((ev) => {
            const assigned = ev.items.filter((i) => i.providerId).length   // סעיפים עם ספק
            return (
              <Link key={ev.id} to={`/events/${ev.id}`} className="card card-hover" style={{ color: 'inherit', textDecoration: 'none' }}>
                <div className="row row-between">
                  <span className="event-date">{formatDate(ev.date)}</span>
                  <Badge tone={isFutureDate(ev.date) ? 'gold' : 'neutral'}>{isFutureDate(ev.date) ? 'עתידי' : 'התקיים'}</Badge>
                </div>
                <h3 style={{ marginTop: 10 }}>{eventTypeLabels[ev.type]} · {ev.location}</h3>
                <div className="meta"><span>👥 {ev.guestCount} אורחים</span><span>🧩 {ev.items.length} סעיפים</span></div>
                <div className="divider" />
                <Badge tone={assigned === ev.items.length ? 'green' : 'violet'}>{assigned} מתוך {ev.items.length} סעיפים שובצו</Badge>
              </Link>
            )
          })}
        </div>
      )}
    </>
  )
}
