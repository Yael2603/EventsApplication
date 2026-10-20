import { useState } from 'react'
import { providersApi } from '../../api/services'
import { Badge, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { formatMoney, getErrorMessage, providerStatusLabels } from '../../utils/format'
import useLoad from '../../utils/useLoad'

const tones = { Pending: 'gold', Approved: 'green', Rejected: 'red' }
const tabs = [{ key: 'Pending', label: 'ממתינים' }, { key: 'Approved', label: 'מאושרים' }, { key: 'Rejected', label: 'נדחו' }]

export default function AdminProviders() {             // ניהול ספקים: אישור ודחייה
  const toast = useToast()
  const [status, setStatus] = useState('Pending')       // הלשונית הפעילה
  const { data, loading, error, reload } = useLoad(() => providersApi.list(status), [status])   // GET /api/providers?status=
  const [busyId, setBusyId] = useState(null)            // ספק שמתבצעת עליו פעולה

  const change = async (p, next) => {
    setBusyId(p.id)
    try {
      await providersApi.setStatus(p.id, next)          // PUT /api/providers/{id}/status
      toast.success(next === 'Approved' ? 'הספק אושר' : 'הספק נדחה')
      await reload()
    } catch (err) {
      toast.error(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <>
      <PageHeader title="ספקים" subtitle="אישור ספקים חדשים וניהול סטטוס" />
      <div className="tabs">
        {tabs.map((t) => <button key={t.key} className={`tab ${status === t.key ? 'active' : ''}`} onClick={() => setStatus(t.key)}>{t.label}</button>)}
      </div>
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="🧑‍🎤" title="אין ספקים בסטטוס זה" />
      ) : (
        <div className="grid grid-2">
          {(data || []).map((p) => (
            <div className="card" key={p.id}>
              <div className="row row-between">
                <h3 style={{ margin: 0 }}>{p.businessName}</h3>
                <Badge tone={tones[p.status]}>{providerStatusLabels[p.status]}</Badge>
              </div>
              <div className="meta" style={{ marginTop: 8 }}><span>{p.categoryName}</span><span>{p.city}</span><span>החל מ-<span className="price">{formatMoney(p.startingPrice)}</span></span></div>
              <p className="muted" style={{ marginTop: 10 }}>{p.description || 'ללא תיאור'}</p>
              <div className="small muted">📞 {p.phone} · ✉️ {p.email}</div>
              <div className="divider" />
              <div className="row">
                {p.status !== 'Approved' && <button className="btn btn-success btn-sm" disabled={busyId === p.id} onClick={() => change(p, 'Approved')}>אישור</button>}
                {p.status !== 'Rejected' && <button className="btn btn-danger btn-sm" disabled={busyId === p.id} onClick={() => change(p, 'Rejected')}>דחייה</button>}
              </div>
            </div>
          ))}
        </div>
      )}
    </>
  )
}
