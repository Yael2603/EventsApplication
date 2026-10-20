import { Navigate, Route, Routes } from 'react-router-dom'  // הגדרת הנתיבים
import Layout from './components/Layout'                // מעטפת: תפריט ותחתית
import ProtectedRoute from './auth/ProtectedRoute'      // הגנה לפי תפקיד
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import RegisterProvider from './pages/RegisterProvider'
import NotFound from './pages/NotFound'
import Notifications from './pages/Notifications'
import EventDetails from './pages/EventDetails'
import NewEvent from './pages/client/NewEvent'
import MyEvents from './pages/client/MyEvents'
import OpenItems from './pages/provider/OpenItems'
import MyCandidacies from './pages/provider/MyCandidacies'
import Bookings from './pages/provider/Bookings'
import ProviderProfile from './pages/provider/ProviderProfile'
import Dashboard from './pages/admin/Dashboard'
import AdminEvents from './pages/admin/AdminEvents'
import AdminProviders from './pages/admin/AdminProviders'
import AdminCategories from './pages/admin/AdminCategories'
import AdminDeletionRequests from './pages/admin/AdminDeletionRequests'

const client = ['Client']                               // תפקידים מותרים לדפי לקוח
const provider = ['Provider']                           // תפקידים מותרים לדפי ספק
const admin = ['Admin']                                 // תפקידים מותרים לדפי מנהל
const anyRole = ['Client', 'Provider', 'Admin']         // כל משתמש מחובר

export default function App() {                         // טבלת הנתיבים של האתר
  return (
    <Routes>
      <Route element={<Layout />}>{/* כל הדפים בתוך המעטפת */}
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/register-provider" element={<RegisterProvider />} />

        <Route path="/events" element={<ProtectedRoute roles={client}><MyEvents /></ProtectedRoute>} />
        <Route path="/events/new" element={<ProtectedRoute roles={client}><NewEvent /></ProtectedRoute>} />
        <Route path="/events/:id" element={<ProtectedRoute roles={['Client', 'Admin']}><EventDetails /></ProtectedRoute>} />

        <Route path="/provider" element={<Navigate to="/provider/open" replace />} />
        <Route path="/provider/open" element={<ProtectedRoute roles={provider}><OpenItems /></ProtectedRoute>} />
        <Route path="/provider/candidacies" element={<ProtectedRoute roles={provider}><MyCandidacies /></ProtectedRoute>} />
        <Route path="/provider/bookings" element={<ProtectedRoute roles={provider}><Bookings /></ProtectedRoute>} />
        <Route path="/provider/profile" element={<ProtectedRoute roles={provider}><ProviderProfile /></ProtectedRoute>} />

        <Route path="/admin" element={<ProtectedRoute roles={admin}><Dashboard /></ProtectedRoute>} />
        <Route path="/admin/events" element={<ProtectedRoute roles={admin}><AdminEvents /></ProtectedRoute>} />
        <Route path="/admin/providers" element={<ProtectedRoute roles={admin}><AdminProviders /></ProtectedRoute>} />
        <Route path="/admin/categories" element={<ProtectedRoute roles={admin}><AdminCategories /></ProtectedRoute>} />
        <Route path="/admin/deletion-requests" element={<ProtectedRoute roles={admin}><AdminDeletionRequests /></ProtectedRoute>} />

        <Route path="/notifications" element={<ProtectedRoute roles={anyRole}><Notifications /></ProtectedRoute>} />
        <Route path="*" element={<NotFound />} />{/* כל כתובת אחרת */}
      </Route>
    </Routes>
  )
}
