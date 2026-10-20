import { notificationsApi } from '../api/services'
import { EmptyState, ErrorBox, Loader, PageHeader } from '../components/ui'
import { useToast } from '../components/ToastContext'
import { formatDateTime, getErrorMessage } from '../utils/format'
import useLoad from '../utils/useLoad'

export default function Notifications() {              // ההתראות של המשתמש (כל תפקיד)
  const toast = useToast()
  const { data, loading, error, reload, setData } = useLoad(() => notificationsApi.list(), [])

  const markRead = async (n) => {                       // מסמן התראה כנקראה
    try {
      await notificationsApi.markRead(n.id)             // PUT /api/notifications/{id}/read
      setData(data.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)))   // מעדכן מקומית בלי טעינה מחדש
    } catch (err) {
      toast.error(getErrorMessage(err))
    }
  }

  return (
    <>
      <PageHeader title="התראות" subtitle="עדכונים על האירועים שלכם" />
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="🔔" title="אין התראות" text="כשיהיה משהו חדש, זה יופיע כאן." />
      ) : (
        <div className="stack">
          {(data || []).map((n) => (
            <div key={n.id} className={`notif ${n.isRead ? 'read' : 'unread'}`}>
              <span className="notif-dot" />
              <div style={{ flex: 1 }}>
                <div>{n.message}</div>
                <div className="muted small">{formatDateTime(n.createdAtUtc)}</div>
              </div>
              {!n.isRead && <button className="btn btn-ghost btn-sm" onClick={() => markRead(n)}>סימון כנקראה</button>}
            </div>
          ))}
        </div>
      )}
    </>
  )
}
