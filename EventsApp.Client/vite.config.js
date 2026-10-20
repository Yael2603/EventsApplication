import { defineConfig } from 'vite'            // פונקציית עזר להגדרות Vite
import react from '@vitejs/plugin-react'       // תוסף שמאפשר JSX ו-Fast Refresh

export default defineConfig({                  // ההגדרות של שרת הפיתוח והבנייה
  plugins: [react()],                          // הפעלת תוסף React
  server: {
    port: 5173,                                // הפורט שמוגדר ב-CORS בשרת (Cors:AllowedOrigins)
    strictPort: true                           // אם הפורט תפוס: שגיאה, ולא מעבר שקט לפורט אחר (שהשרת היה חוסם)
  }
})
