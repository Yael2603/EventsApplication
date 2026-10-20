import { useEffect } from 'react'                       // לסגירת חלון בלחיצה על Escape

export function Loader({ text = 'טוען...' }) {          // אינדיקציית טעינה
  return (
    <div className="loader-wrap">
      <div className="loader" />{/* עיגול מסתובב */}
      <span>{text}</span>
    </div>
  )
}

export function ErrorBox({ message, onRetry }) {        // תיבת שגיאה עם כפתור ניסיון חוזר
  if (!message) return null                             // אין שגיאה: לא מציג כלום
  return (
    <div className="alert alert-error">
      <span>{message}</span>
      {onRetry && <button className="btn btn-ghost btn-sm" onClick={onRetry}>נסה שוב</button>}
    </div>
  )
}

export function EmptyState({ icon = '✨', title, text, children }) {   // מצב ריק (אין נתונים)
  return (
    <div className="empty">
      <div className="empty-icon">{icon}</div>
      <h3>{title}</h3>
      {text && <p>{text}</p>}
      {children}
    </div>
  )
}

export function PageHeader({ title, subtitle, children }) {    // כותרת עמוד, עם אזור לכפתורים
  return (
    <div className="page-header">
      <div>
        <h1>{title}</h1>
        {subtitle && <p className="muted">{subtitle}</p>}
      </div>
      <div className="page-header-actions">{children}</div>
    </div>
  )
}

export function Badge({ tone = 'neutral', children }) { // תגית צבעונית קטנה
  return <span className={`badge badge-${tone}`}>{children}</span>
}

export function Modal({ title, onClose, children, wide }) {     // חלון קופץ
  useEffect(() => {                                     // סגירה ב-Escape
    const onKey = (e) => { if (e.key === 'Escape') onClose() }  // מקש Escape
    window.addEventListener('keydown', onKey)           // מתחיל להאזין
    return () => window.removeEventListener('keydown', onKey)   // מפסיק בסגירה
  }, [onClose])

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>{/* לחיצה על הרקע סוגרת */}
      <div className={`modal ${wide ? 'modal-wide' : ''}`} role="dialog" aria-modal="true"
           onMouseDown={(e) => e.stopPropagation()}>{/* לחיצה בתוך החלון לא סוגרת */}
        <div className="modal-head">
          <h2>{title}</h2>
          <button className="icon-btn" onClick={onClose} aria-label="סגירה">✕</button>
        </div>
        <div className="modal-body">{children}</div>
      </div>
    </div>
  )
}

export function ConfirmModal({ title, text, confirmText = 'אישור', danger, busy, onConfirm, onClose }) {   // אישור פעולה
  return (
    <Modal title={title} onClose={onClose}>
      <p>{text}</p>
      <div className="modal-actions">
        <button className="btn btn-ghost" onClick={onClose} disabled={busy}>ביטול</button>
        <button className={`btn ${danger ? 'btn-danger' : 'btn-primary'}`} onClick={onConfirm} disabled={busy}>
          {busy ? 'מבצע...' : confirmText}
        </button>
      </div>
    </Modal>
  )
}
