import { useState } from 'react'
import { candidaciesApi, itemsApi } from '../../api/services'
import { Badge, EmptyState, ErrorBox, Loader, Modal, PageHeader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { eventTypeLabels, formatDate, formatMoney, getErrorMessage } from '../../utils/format'
import useLoad from '../../utils/useLoad'

export default function OpenItems() {                   // סעיפים פתוחים שספק יכול להגיש אליהם מועמדות
  const toast = useToast()
  const { data, loading, error, reload } = useLoad(() => itemsApi.open(), [])   // GET /api/event-items/open
  const [target, setTarget] = useState(null)            // הסעיף שאליו מגישים
  const [form, setForm] = useState({ proposedPrice: '', message: '' })
  const [busy, setBusy] = useState(false)

  const openModal = (item) => { setTarget(item); setForm({ proposedPrice: '', message: '' }) }

  const submit = async (e) => {
    e.preventDefault()
    setBusy(true)
    try {
      await candidaciesApi.apply({                      // POST /api/candidacies
        eventItemId: target.itemId,
        proposedPrice: Number(form.proposedPrice),
        message: form.message || null,
      })
      toast.success('המועמדות הוגשה')
      setTarget(null)
      await reload()
    } catch (err) {
      toast.error(getErrorMessage(err))                 // למשל: ספק שלא אושר, או הגשה כפולה (409)
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <PageHeader title="סעיפים פתוחים" subtitle="סעיפים בקטגוריה שלכם שעדיין ללא ספק" />
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="🔍" title="אין כרגע סעיפים פתוחים" text="סעיפים חדשים בקטגוריה שלכם יופיעו כאן." />
      ) : (
        <div className="grid grid-2">
          {(data || []).map((i) => (
            <div className="card card-hover" key={i.itemId}>
              <div className="row row-between">
                <span className="event-date">{formatDate(i.eventDate)}</span>
                <Badge tone="gold">{i.categoryName}</Badge>
              </div>
              <h3 style={{ marginTop: 10 }}>{eventTypeLabels[i.eventType]} · {i.location}</h3>
              <div className="meta"><span>👥 {i.guestCount} אורחים</span><span>תקציב: <span className="price">{formatMoney(i.budget)}</span></span></div>
              {i.details && <p className="muted" style={{ marginTop: 10 }}>{i.details}</p>}
              <div className="divider" />
              {i.alreadyApplied
                ? <Badge tone="green">הגשתם מועמדות</Badge>
                : <button className="btn btn-primary btn-sm" onClick={() => openModal(i)}>הגשת מועמדות</button>}
            </div>
          ))}
        </div>
      )}

      {target && (
        <Modal title={`מועמדות: ${target.categoryName}`} onClose={() => setTarget(null)}>
          <p className="muted">{formatDate(target.eventDate)} · {target.location} · תקציב הלקוח {formatMoney(target.budget)}</p>
          <form onSubmit={submit}>
            <div className="field"><label>המחיר המוצע שלכם (₪)</label>
              <input type="number" required min="1" step="1" value={form.proposedPrice} onChange={(e) => setForm({ ...form, proposedPrice: e.target.value })} /></div>
            <div className="field"><label>הודעה</label>
              <textarea maxLength={1000} value={form.message} onChange={(e) => setForm({ ...form, message: e.target.value })} placeholder="מה מייחד אתכם?" /></div>
            <div className="modal-actions">
              <button type="button" className="btn btn-ghost" onClick={() => setTarget(null)}>ביטול</button>
              <button className="btn btn-primary" disabled={busy}>{busy ? 'שולח...' : 'הגשה'}</button>
            </div>
          </form>
        </Modal>
      )}
    </>
  )
}
