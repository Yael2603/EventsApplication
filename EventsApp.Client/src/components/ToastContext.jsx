import { createContext, useCallback, useContext, useState } from 'react'   // כלי React

const ToastContext = createContext(null)                // הקשר להודעות קצרות על המסך

export function ToastProvider({ children }) {           // עוטף את האפליקציה
  const [toasts, setToasts] = useState([])              // ההודעות המוצגות כרגע

  const push = useCallback((type, text) => {            // מוסיף הודעה ומסיר אותה אחרי 4.5 שניות
    const id = Math.random().toString(36).slice(2)      // מזהה ייחודי
    setToasts((list) => [...list, { id, type, text }])  // מוסיף לרשימה
    setTimeout(() => setToasts((list) => list.filter((t) => t.id !== id)), 4500)   // מסיר אחרי זמן
  }, [])

  const api = {                                         // הפונקציות שהדפים משתמשים בהן
    success: (text) => push('success', text),           // הודעת הצלחה
    error: (text) => push('error', text),               // הודעת שגיאה
  }

  return (
    <ToastContext.Provider value={api}>
      {children}
      <div className="toast-stack" role="status" aria-live="polite">{/* אזור ההודעות */}
        {toasts.map((t) => (
          <div key={t.id} className={`toast toast-${t.type}`}>{t.text}</div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast() {                            // hook לשימוש בדפים
  return useContext(ToastContext)
}
