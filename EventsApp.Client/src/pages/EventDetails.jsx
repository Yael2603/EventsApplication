import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { categoriesApi, deletionApi, eventsApi, itemsApi } from '../api/services'
import { useAuth } from '../auth/AuthContext'
import { Badge, ConfirmModal, ErrorBox, Loader, Modal, PageHeader } from '../components/ui'
import { useToast } from '../components/ToastContext'
import {
  eventTypeLabels, formatDate, formatDateTime, formatMoney, getErrorMessage, isFutureDate,
} from '../utils/format'
import useLoad from '../utils/useLoad'

export default function EventDetails() {                // דף אירוע: לקוח (בעלים) ומנהל
  const { id } = useParams()                            // מזהה האירוע מהכתובת
  const { user } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const { data: ev, loading, error, reload } = useLoad(() => eventsApi.get(id), [id])   // GET /api/events/{id}
  const { data: categories } = useLoad(() => categoriesApi.list(), [])

  const [modal, setModal] = useState(null)              // איזה חלון פתוח: edit / add / remove / delete / assign
  const [target, setTarget] = useState(null)            // הסעיף שעליו פועלים
  const [busy, setBusy] = useState(false)

  if (loading) return <Loader />
  if (error) return <ErrorBox message={error} onRetry={reload} />
  if (!ev) return null

  const isOwner = user.role === 'Client' && ev.clientId === user.userId   // הבעלים של האירוע
  const isAdmin = user.role === 'Admin'
  const future = isFutureDate(ev.date)                  // רק אירוע עתידי ניתן לעריכה
  const canEdit = isOwner && future

  const close = () => { setModal(null); setTarget(null) }   // סוגר חלון
  const run = async (action, successText) => {         // מריץ פעולה עם טיפול אחיד בשגיאות
    setBusy(true)
    try {
      await action()
      if (successText) toast.success(successText)
      close()
      await reload()                                    // טוען מחדש את האירוע
    } catch (err) {
      toast.error(getErrorMessage(err))                 // למשל 409: סעיף עם ספק לא ניתן להסרה
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <PageHeader title={`${eventTypeLabels[ev.type]} · ${ev.location}`} subtitle={formatDate(ev.date)}>
        <Link to={isAdmin ? '/admin/events' : '/events'} className="btn btn-ghost">חזרה לרשימה</Link>
        {canEdit && <button className="btn btn-ghost" onClick={() => setModal('edit')}>עריכת פרטים</button>}
        {canEdit && <button className="btn btn-danger" onClick={() => setModal('delete')}>בקשת מחיקת אירוע</button>}
      </PageHeader>

      <div className="card">
        <div className="meta">
          <span>👥 {ev.guestCount} אורחים</span>
          {isAdmin && <span>👤 {ev.clientName}</span>}
          <span>נוצר: {formatDateTime(ev.createdAtUtc)}</span>
          <Badge tone={future ? 'gold' : 'neutral'}>{future ? 'עתידי' : 'התקיים'}</Badge>
        </div>
        {ev.notes && <><div className="divider" /><p style={{ margin: 0 }}>{ev.notes}</p></>}
      </div>

      <div className="spacer" />
      <div className="row row-between">
        <h2>סעיפי האירוע</h2>
        {canEdit && <button className="btn btn-primary btn-sm" onClick={() => setModal('add')}>+ הוספת סעיף</button>}
      </div>
      <div className="stack">
        {ev.items.map((item) => (
          <div className="item-row" key={item.id}>
            <div>
              <strong>{item.categoryName}</strong>
              <div className="muted small">תקציב: <span className="price">{formatMoney(item.budget)}</span>{item.details ? ` · ${item.details}` : ''}</div>
            </div>
            <div className="row">
              {item.providerName
                ? <Badge tone="green">שובץ: {item.providerName}</Badge>
                : <Badge tone="violet">מחכה לספק</Badge>}
              {isAdmin && future && (
                <button className="btn btn-ghost btn-sm" onClick={() => { setTarget(item); setModal('assign') }}>
                  {item.providerName ? 'החלפת ספק' : 'מועמדויות ושיבוץ'}
                </button>
              )}
              {canEdit && !item.providerId && (
                <button className="btn btn-danger btn-sm" onClick={() => { setTarget(item); setModal('remove') }}>הסרה</button>
              )}
            </div>
          </div>
        ))}
      </div>

      {modal === 'edit' && <EditModal ev={ev} busy={busy} onClose={close}
        onSave={(body) => run(() => eventsApi.update(ev.id, body), 'האירוע עודכן')} />}
      {modal === 'add' && <AddItemModal ev={ev} categories={categories || []} busy={busy} onClose={close}
        onSave={(body) => run(() => eventsApi.addItem(ev.id, body), 'הסעיף נוסף')} />}
      {modal === 'remove' && <ConfirmModal title="הסרת סעיף" danger busy={busy} confirmText="הסרה" onClose={close}
        text={`להסיר את הסעיף "${target.categoryName}"? ביטול הסעיף ימחק גם מועמדויות שהוגשו אליו.`}
        onConfirm={() => run(() => itemsApi.remove(target.id), 'הסעיף הוסר')} />}
      {modal === 'delete' && <DeletionModal busy={busy} onClose={close}
        onSend={(reason) => run(async () => { await deletionApi.create(ev.id, reason); navigate('/events') }, 'הבקשה נשלחה למנהל')} />}
      {modal === 'assign' && <AssignModal item={target} onClose={close}
        onAssign={(providerId) => run(() => itemsApi.assign(target.id, providerId), 'הספק שובץ')} busy={busy} />}
    </>
  )
}

function EditModal({ ev, busy, onClose, onSave }) {     // חלון עריכת פרטי אירוע (בלי תאריך וסעיפים)
  const [form, setForm] = useState({ type: ev.type, location: ev.location, guestCount: ev.guestCount, notes: ev.notes || '' })
  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })
  return (
    <Modal title="עריכת פרטי האירוע" onClose={onClose}>
      <form onSubmit={(e) => { e.preventDefault(); onSave({ ...form, guestCount: Number(form.guestCount), notes: form.notes || null }) }}>
        <div className="field"><label>סוג האירוע</label>
          <select value={form.type} onChange={set('type')}>
            {Object.entries(eventTypeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
          </select></div>
        <div className="field"><label>מקום</label><input required maxLength={200} value={form.location} onChange={set('location')} /></div>
        <div className="field"><label>מספר אורחים</label><input type="number" required min="1" max="5000" value={form.guestCount} onChange={set('guestCount')} /></div>
        <div className="field"><label>הערות</label><textarea maxLength={1000} value={form.notes} onChange={set('notes')} /></div>
        <p className="muted small">שינוי תאריך אינו אפשרי. במקרה כזה יש לבקש מחיקה וליצור אירוע חדש.</p>
        <div className="modal-actions">
          <button type="button" className="btn btn-ghost" onClick={onClose}>ביטול</button>
          <button className="btn btn-primary" disabled={busy}>{busy ? 'שומר...' : 'שמירה'}</button>
        </div>
      </form>
    </Modal>
  )
}

function AddItemModal({ ev, categories, busy, onClose, onSave }) {   // חלון הוספת סעיף
  const used = ev.items.map((i) => i.categoryId)        // קטגוריות שכבר קיימות באירוע
  const [form, setForm] = useState({ categoryId: '', budget: '', details: '' })
  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })
  return (
    <Modal title="הוספת סעיף" onClose={onClose}>
      <form onSubmit={(e) => { e.preventDefault(); onSave({ categoryId: Number(form.categoryId), budget: Number(form.budget), details: form.details || null }) }}>
        <div className="field"><label>קטגוריה</label>
          <select required value={form.categoryId} onChange={set('categoryId')}>
            <option value="">בחירת קטגוריה</option>
            {categories.map((c) => <option key={c.id} value={c.id} disabled={used.includes(c.id)}>{c.name}{used.includes(c.id) ? ' (קיים)' : ''}</option>)}
          </select></div>
        <div className="field"><label>תקציב (₪)</label><input type="number" required min="1" step="1" value={form.budget} onChange={set('budget')} /></div>
        <div className="field"><label>פרטים</label><textarea maxLength={1000} value={form.details} onChange={set('details')} /></div>
        <div className="modal-actions">
          <button type="button" className="btn btn-ghost" onClick={onClose}>ביטול</button>
          <button className="btn btn-primary" disabled={busy}>{busy ? 'מוסיף...' : 'הוספה'}</button>
        </div>
      </form>
    </Modal>
  )
}

function DeletionModal({ busy, onClose, onSend }) {     // חלון בקשת מחיקה (המנהל מאשר או דוחה)
  const [reason, setReason] = useState('')
  return (
    <Modal title="בקשת מחיקת אירוע" onClose={onClose}>
      <form onSubmit={(e) => { e.preventDefault(); onSend(reason) }}>
        <p className="muted">מחיקת אירוע מתבצעת באישור מנהל. עם האישור, התאריך ישתחרר והספקים ששובצו יקבלו התראה.</p>
        <div className="field"><label>סיבת הבקשה</label>
          <textarea required minLength={10} maxLength={1000} value={reason} onChange={(e) => setReason(e.target.value)} placeholder="לפחות 10 תווים" /></div>
        <div className="modal-actions">
          <button type="button" className="btn btn-ghost" onClick={onClose}>ביטול</button>
          <button className="btn btn-danger" disabled={busy}>{busy ? 'שולח...' : 'שליחת בקשה'}</button>
        </div>
      </form>
    </Modal>
  )
}

function AssignModal({ item, busy, onClose, onAssign }) {   // חלון מנהל: מועמדויות לסעיף ושיבוץ
  const [list, setList] = useState(null)
  const [error, setError] = useState('')
  useEffect(() => {
    itemsApi.candidacies(item.id).then(setList).catch((err) => setError(getErrorMessage(err)))   // GET /api/event-items/{id}/candidacies
  }, [item.id])
  return (
    <Modal title={`מועמדויות: ${item.categoryName}`} onClose={onClose} wide>
      {error && <div className="alert alert-error">{error}</div>}
      {!list && !error && <Loader />}
      {list && list.length === 0 && <p className="muted">עדיין לא הוגשו מועמדויות לסעיף זה.</p>}
      <div className="stack">
        {(list || []).map((c) => (
          <div className="item-row" key={c.id}>
            <div>
              <strong>{c.providerName}</strong> <span className="muted small">· {c.city} · {c.phone}</span>
              <div>מחיר מוצע: <span className="price">{formatMoney(c.proposedPrice)}</span></div>
              {c.message && <div className="muted small">{c.message}</div>}
            </div>
            <button className="btn btn-primary btn-sm" disabled={busy || c.providerId === item.providerId} onClick={() => onAssign(c.providerId)}>
              {c.providerId === item.providerId ? 'משובץ' : 'שיבוץ'}
            </button>
          </div>
        ))}
      </div>
    </Modal>
  )
}
