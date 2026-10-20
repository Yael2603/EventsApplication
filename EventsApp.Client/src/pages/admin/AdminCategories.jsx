import { useState } from 'react'
import { categoriesApi } from '../../api/services'
import { ConfirmModal, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { getErrorMessage } from '../../utils/format'
import useLoad from '../../utils/useLoad'

export default function AdminCategories() {            // ניהול קטגוריות: הוספה, שינוי שם ומחיקה
  const toast = useToast()
  const { data, loading, error, reload } = useLoad(() => categoriesApi.list(), [])
  const [newName, setNewName] = useState('')            // שם לקטגוריה חדשה
  const [editId, setEditId] = useState(null)            // קטגוריה במצב עריכה
  const [editName, setEditName] = useState('')          // השם בעריכה
  const [target, setTarget] = useState(null)            // קטגוריה למחיקה
  const [busy, setBusy] = useState(false)

  const run = async (action, text) => {                 // פעולה אחידה עם הודעות
    setBusy(true)
    try { await action(); toast.success(text); await reload(); return true }
    catch (err) { toast.error(getErrorMessage(err)); return false }   // 409: שם קיים או קטגוריה בשימוש
    finally { setBusy(false) }
  }

  const add = async (e) => {
    e.preventDefault()
    if (await run(() => categoriesApi.create(newName.trim()), 'הקטגוריה נוספה')) setNewName('')
  }

  const save = async (id) => {
    if (await run(() => categoriesApi.update(id, editName.trim()), 'השם עודכן')) setEditId(null)
  }

  const remove = async () => {
    await run(() => categoriesApi.remove(target.id), 'הקטגוריה נמחקה')
    setTarget(null)
  }

  return (
    <>
      <PageHeader title="קטגוריות" subtitle="הקטגוריות שמופיעות בלקוחות ובספקים" />
      <form className="card row" onSubmit={add} style={{ marginBottom: 20 }}>
        <input style={{ flex: 1, minWidth: 200 }} required maxLength={100} placeholder="שם קטגוריה חדשה" value={newName} onChange={(e) => setNewName(e.target.value)} />
        <button className="btn btn-primary" disabled={busy}>+ הוספה</button>
      </form>
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? <EmptyState icon="🏷️" title="אין קטגוריות" /> : (
        <div className="stack">
          {(data || []).map((c) => (
            <div className="item-row" key={c.id}>
              {editId === c.id ? (
                <input style={{ flex: 1, minWidth: 200 }} maxLength={100} value={editName} onChange={(e) => setEditName(e.target.value)} autoFocus />
              ) : <strong>{c.name}</strong>}
              <div className="row">
                {editId === c.id ? (
                  <>
                    <button className="btn btn-primary btn-sm" disabled={busy || !editName.trim()} onClick={() => save(c.id)}>שמירה</button>
                    <button className="btn btn-ghost btn-sm" onClick={() => setEditId(null)}>ביטול</button>
                  </>
                ) : (
                  <>
                    <button className="btn btn-ghost btn-sm" onClick={() => { setEditId(c.id); setEditName(c.name) }}>שינוי שם</button>
                    <button className="btn btn-danger btn-sm" onClick={() => setTarget(c)}>מחיקה</button>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
      {target && <ConfirmModal title="מחיקת קטגוריה" danger busy={busy} confirmText="מחיקה" onClose={() => setTarget(null)}
        text={`למחוק את "${target.name}"? קטגוריה שמשויכים אליה ספקים או סעיפים לא תימחק.`} onConfirm={remove} />}
    </>
  )
}
