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

export default function App() {
  return (
    <div className="app">
      <ScrollToTop />
      <Header />
      <main className="main">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/broneeri" element={<Booking />} />
          <Route path="/broneering/:id" element={<RequireAuth><ReservationDetails /></RequireAuth>} />
          <Route path="/minu" element={<RequireAuth><MyReservations /></RequireAuth>} />
          <Route path="/menuu" element={<Menu />} />
          <Route path="/personal/plaan" element={<RequireAuth roles={['Admin', 'Waiter']}><StaffPlan /></RequireAuth>} />
          <Route path="/personal/broneeringud" element={<RequireAuth roles={['Admin', 'Waiter']}><StaffReservations /></RequireAuth>} />
          <Route path="/admin" element={<RequireAuth roles={['Admin']}><Admin /></RequireAuth>} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}
