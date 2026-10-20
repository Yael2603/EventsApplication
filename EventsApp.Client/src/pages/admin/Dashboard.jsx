import { Link } from 'react-router-dom'
import { statsApi } from '../../api/services'
import { ErrorBox, Loader, PageHeader } from '../../components/ui'
import useLoad from '../../utils/useLoad'

export default function Dashboard() {                   // לוח הבקרה של המנהל
  const { data, loading, error, reload } = useLoad(() => statsApi.get(), [])   // GET /api/admin/stats

  if (loading) return <Loader />
  if (error) return <ErrorBox message={error} onRetry={reload} />
  if (!data) return null

  const cards = [                                       // כרטיסי המספרים, עם קישור למסך הרלוונטי
    { label: 'כל האירועים', value: data.totalEvents, to: '/admin/events' },
    { label: 'אירועים עתידיים', value: data.upcomingEvents, to: '/admin/events' },
    { label: 'ספקים ממתינים לאישור', value: data.pendingProviders, to: '/admin/providers' },
    { label: 'ספקים מאושרים', value: data.approvedProviders, to: '/admin/providers' },
    { label: 'בקשות מחיקה ממתינות', value: data.pendingDeletionRequests, to: '/admin/deletion-requests' },
    { label: 'סעיפים עתידיים ללא ספק', value: data.openItems, to: '/admin/events' },
  ]

  return (
    <>
      <PageHeader title="לוח בקרה" subtitle="תמונת מצב של SHOW הפקות" />
      <div className="grid grid-3">
        {cards.map((c) => (
          <Link key={c.label} to={c.to} className="card card-hover stat" style={{ color: 'inherit', textDecoration: 'none' }}>
            <div className="stat-value">{c.value}</div>
            <div className="stat-label">{c.label}</div>
          </Link>
        ))}
      </div>
    </>
  )
}
