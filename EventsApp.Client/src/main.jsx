import React from 'react'                               // React
import ReactDOM from 'react-dom/client'                 // חיבור ל-DOM
import { BrowserRouter } from 'react-router-dom'        // ניווט לפי כתובת הדפדפן
import App from './App'                                 // רכיב השורש
import { AuthProvider } from './auth/AuthContext'       // מצב התחברות לכל האתר
import { ToastProvider } from './components/ToastContext'   // הודעות קצרות
import './styles/index.css'                             // העיצוב הגלובלי

ReactDOM.createRoot(document.getElementById('root')).render(    // מציג את האתר בתוך #root
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <ToastProvider>
          <App />
        </ToastProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>,
)
