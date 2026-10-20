import { useState } from 'react'
import { deletionApi } from '../../api/services'
import { Badge, ConfirmModal, EmptyState, ErrorBox, Loader, Modal, PageHeader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { deletionStatusLabels, eventTypeLabels, formatDate, formatDateTime, getErrorMessage } from '../../utils/format'
import useLoad from '../../utils/useLoad'

const tabs = [{ key: 'Pending', label: 'ממתינות' }, { key: 'Rejected', label: 'נדחו' }]   // אישור מוחק את הבקשה, ולכן אין לשונית כזו

export default function AdminDeletionRequests() {      // בקשות מחיקת אירוע מלקוחות
  const toast = useToast()
  const [status, setStatus] = useState('Pending')
  const { data, loading, error, reload } = useLoad(() => deletionApi.list(status), [status])   // GET /api/deletion-requests?status=
  const [approve, setApprove] = useState(null)          // בקשה לאישור
  const [reject, setReject] = useState(null)            // בקשה לדחייה
  const [note, setNote] = useState('')                  // הערת דחייה
  const [busy, setBusy] = useState(false)

  const run = async (action, text, done) => {
    setBusy(true)
    try { await action(); toast.success(text); done(); await reload() }
    catch (err) { toast.error(getErrorMessage(err)) }
    finally { setBusy(false) }
  }

  return (
    <>
      <PageHeader title="בקשות מחיקה" subtitle="אישור מוחק את האירוע, משחרר את התאריך ומתריע לספקים ששובצו" />
      <div className="tabs">
        {tabs.map((t) => <button key={t.key} className={`tab ${status === t.key ? 'active' : ''}`} onClick={() => setStatus(t.key)}>{t.label}</button>)}
      </div>
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="🗂️" title="אין בקשות" />
      ) : (
        <div className="grid grid-2">
          {(data || []).map((r) => (
            <div className="card" key={r.id}>
              <div className="row row-between">
                <span className="event-date">{formatDate(r.eventDate)}</span>
                <Badge tone={r.status === 'Pending' ? 'gold' : 'red'}>{deletionStatusLabels[r.status]}</Badge>
              </div>
              <h3 style={{ marginTop: 10 }}>{eventTypeLabels[r.eventType]} · {r.location}</h3>
              <div className="muted small">לקוח: {r.clientName} · הוגשה {formatDateTime(r.createdAtUtc)}</div>
              <p style={{ marginTop: 12 }}>"{r.reason}"</p>
              {r.adminNote && <div className="alert alert-info">הערת מנהל: {r.adminNote}</div>}
              {r.status === 'Pending' && (
                <div className="row">
                  <button className="btn btn-danger btn-sm" onClick={() => setApprove(r)}>אישור ומחיקה</button>
                  <button className="btn btn-ghost btn-sm" onClick={() => { setReject(r); setNote('') }}>דחייה</button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {approve && <ConfirmModal title="אישור מחיקה" danger busy={busy} confirmText="אישור ומחיקה" onClose={() => setApprove(null)}
        text={`האירוע בתאריך ${formatDate(approve.eventDate)} יימחק, והתאריך ישתחרר. פעולה זו אינה הפיכה.`}
        onConfirm={() => run(() => deletionApi.approve(approve.id), 'האירוע נמחק', () => setApprove(null))} />}

      {reject && (
        <Modal title="דחיית בקשה" onClose={() => setReject(null)}>
          <form onSubmit={(e) => { e.preventDefault(); run(() => deletionApi.reject(reject.id, note || null), 'הבקשה נדחתה', () => setReject(null)) }}>
            <div className="field"><label>הסבר ללקוח (אופציונלי)</label>
              <textarea maxLength={1000} value={note} onChange={(e) => setNote(e.target.value)} /></div>
            <div className="modal-actions">
              <button type="button" className="btn btn-ghost" onClick={() => setReject(null)}>ביטול</button>
              <button className="btn btn-primary" disabled={busy}>{busy ? 'שולח...' : 'דחייה'}</button>
            </div>
          </form>
        </Modal>
      )}
    </>
  )
}
