import { useEffect } from 'react'
import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import Header from './components/Header'
import Footer from './components/Footer'
import { useAuth } from './auth'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import NotFound from './pages/NotFound'
import Booking from './pages/Booking'
import ReservationDetails from './pages/ReservationDetails'
import MyReservations from './pages/MyReservations'
import Menu from './pages/Menu'
import StaffPlan from './pages/StaffPlan'
import StaffReservations from './pages/StaffReservations'
import Admin from './pages/Admin'
import VisitExtras from './components/VisitExtras'
import StaffMessages from './components/StaffMessages'
import Support from './pages/Support'
import StaffSupport from './pages/StaffSupport'
import StaffCalendar from './pages/StaffCalendar'
import Profile from './pages/Profile'
import PhoneBooking from './pages/PhoneBooking'
import PayLink from './pages/PayLink'
import EventPayLink from './pages/EventPayLink'
import SetPassword from './pages/SetPassword'
import CustomerCard from './pages/CustomerCard'

/** Leht ainult sisse loginud kasutajale (soovi korral ainult teatud rollidele). */
export function RequireAuth({ roles, children }) {
  const { user } = useAuth()
  const location = useLocation()
  if (!user) return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />
  return children
}

/** Uue lehe avamisel keritakse üles (kui aadressis pole #ankrut). */
function ScrollToTop() {
  const { pathname, hash } = useLocation()
  useEffect(() => {
    if (!hash) window.scrollTo(0, 0)
  }, [pathname, hash])
  return null
}

/** Personal ei broneeri enda nimele: avaleht ja broneerimine suunavad töölauale. */
function GuestOnly({ children }) {
  const { user } = useAuth()
  if (user?.role === 'Admin') return <Navigate to="/admin" replace />
  if (user?.role === 'Waiter') return <Navigate to="/personal/plaan" replace />
  return children
}

/** "Küsi meilt": KKK kõigile, vestlus sisseloginud kliendile; personal läheb klientide küsimuste lehele. */
function SupportRoute() {
  const { user } = useAuth()
  if (user?.role === 'Admin' || user?.role === 'Waiter') return <Navigate to="/personal/tugi" replace />
  return <Support />
}

export default function App() {
  return (
    <div className="app">
      <ScrollToTop />
      <Header />
      <main className="main">
        <Routes>
          <Route path="/" element={<GuestOnly><Home /></GuestOnly>} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/broneeri" element={<GuestOnly><Booking /></GuestOnly>} />
          <Route path="/broneering/:id" element={<RequireAuth><ReservationDetails /></RequireAuth>} />
          <Route path="/minu" element={<RequireAuth><MyReservations renderExtra={(r, reload) => <VisitExtras r={r} onChanged={reload} />} /></RequireAuth>} />
          <Route path="/menuu" element={<Menu />} />
          <Route path="/personal/plaan" element={<RequireAuth roles={['Admin', 'Waiter']}><StaffPlan /></RequireAuth>} />
          <Route path="/personal/broneeringud" element={<RequireAuth roles={['Admin', 'Waiter']}><StaffReservations renderRowExtra={(r) => <StaffMessages r={r} />} /></RequireAuth>} />
          <Route path="/admin" element={<RequireAuth roles={['Admin']}><Admin /></RequireAuth>} />
          <Route path="/tugi" element={<SupportRoute />} />
          <Route path="/profiil" element={<RequireAuth roles={['Customer']}><Profile /></RequireAuth>} />
          <Route path="/maksa/:token" element={<PayLink />} />
          <Route path="/maksa/ohtu/:token" element={<EventPayLink />} />
          <Route path="/parool/:token" element={<SetPassword />} />
          <Route path="/personal/uus" element={<RequireAuth roles={['Admin', 'Waiter']}><PhoneBooking /></RequireAuth>} />
          <Route path="/personal/klient" element={<RequireAuth roles={['Admin', 'Waiter']}><CustomerCard /></RequireAuth>} />
          <Route path="/personal/kalender" element={<RequireAuth roles={['Admin', 'Waiter']}><StaffCalendar /></RequireAuth>} />
          <Route path="/personal/tugi" element={<RequireAuth roles={['Admin', 'Waiter']}><StaffSupport /></RequireAuth>} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}
