import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type Page = 'planner' | 'rooms' | 'reservations' | 'guests' | 'vehicles' | 'parking' | 'users' | 'reports'
type RoomType = 'Single' | 'Double' | 'Twin' | 'Suite' | 'Family'
type RoomStatus = 'Available' | 'Occupied' | 'Maintenance' | 'OutOfService'
type ReservationStatus = 'Pending' | 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled'
type UserRole = 'Receptionist' | 'Manager' | 'Administrator'

type Room = {
  id: string
  number: string
  type: RoomType
  capacity: number
  status: RoomStatus
}

type Reservation = {
  id: string
  guestFirstName: string
  guestLastName: string
  email: string
  checkIn: string
  checkOut: string
  roomId: string
  roomNumber: string
  status: ReservationStatus
  vehicleRegistrationNumbers: string[]
}

type ParkingAccess = {
  vehicleId: string
  reservationId: string
  registrationNumber: string
  parkingAccessFrom: string
  parkingAccessTo: string
  isActive: boolean
  hasValidAccessNow: boolean
}

type ParkingVerification = {
  registrationNumber: string
  hasValidAccess: boolean
  verificationStatus: string
}

type Guest = {
  id: string
  firstName: string
  lastName: string
  email: string
  phone: string
  country: string
  documentNumber: string
  address?: {
    country: string
    city: string
    postalCode: string
    street: string
    buildingNumber: string
  }
}

type VehicleRecord = {
  id: string
  country: string
  registrationNumber: string
  guestId?: string
  reservationId?: string
}

type GuestForm = Omit<Guest, 'id'>
type VehicleForm = Omit<VehicleRecord, 'id'>

type StaffUser = {
  id: string
  name: string
  email: string
  password: string
  role: UserRole
}

type LoginForm = {
  email: string
  password: string
}

const devJwt =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZW1vLXVzZXIiLCJuYW1lIjoiUE1TIE9wZXJhdG9yIiwiaXNzIjoiSG90ZWxNYW5hZ2VtZW50U3lzdGVtIiwiYXVkIjoiSG90ZWxNYW5hZ2VtZW50U3lzdGVtLkFwaSIsImV4cCI6MTg5MzQ1NjAwMCwiaWF0IjoxNzg4OTEyMDAwfQ.M9-p-1hJYAfepw8j9fQxxwImxbDRDB3F2M-E8MimuSo'

const pages: Array<{ id: Page; label: string }> = [
  { id: 'planner', label: 'Booking Plan' },
  { id: 'rooms', label: 'Rooms' },
  { id: 'reservations', label: 'Reservations' },
  { id: 'guests', label: 'Guests' },
  { id: 'vehicles', label: 'Vehicles' },
  { id: 'parking', label: 'Parking' },
  { id: 'users', label: 'Users' },
  { id: 'reports', label: 'Reports' },
]

const roomTypes: RoomType[] = ['Single', 'Double', 'Twin', 'Suite', 'Family']
const roomStatuses: RoomStatus[] = ['Available', 'Occupied', 'Maintenance', 'OutOfService']
const reservationStatuses: ReservationStatus[] = [
  'Pending',
  'Confirmed',
  'CheckedIn',
  'CheckedOut',
  'Cancelled',
]
const today = new Date()
const tomorrow = new Date(today)
tomorrow.setDate(today.getDate() + 1)

const dateLabelFormatter = new Intl.DateTimeFormat(undefined, { weekday: 'short' })
const sampleGuests: Guest[] = [
  {
    id: 'guest-1',
    firstName: 'Olena',
    lastName: 'Koval',
    email: 'olena.koval@example.com',
    phone: '+380 67 123 4567',
    country: 'Ukraine',
    documentNumber: 'AB123456',
    address: {
      country: 'Ukraine',
      city: 'Lviv',
      postalCode: '79000',
      street: 'Shevchenka',
      buildingNumber: '12',
    },
  },
  {
    id: 'guest-2',
    firstName: 'Jan',
    lastName: 'Nowak',
    email: 'jan.nowak@example.com',
    phone: '+48 501 234 567',
    country: 'Poland',
    documentNumber: 'PL998877',
  },
]

const emptyGuestForm: GuestForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  country: '',
  documentNumber: '',
}

const demoUsers: StaffUser[] = [
  {
    id: 'user-manager',
    name: 'PMS Manager',
    email: 'manager@hotel.test',
    password: 'manager123',
    role: 'Manager',
  },
  {
    id: 'user-reception',
    name: 'Reception Desk',
    email: 'reception@hotel.test',
    password: 'reception123',
    role: 'Receptionist',
  },
  {
    id: 'user-admin',
    name: 'System Admin',
    email: 'admin@hotel.test',
    password: 'admin123',
    role: 'Administrator',
  },
]

const emptyVehicleForm: VehicleForm = {
  country: '',
  registrationNumber: '',
  guestId: '',
  reservationId: '',
}

function formatDateInput(date: Date) {
  return date.toISOString().slice(0, 10)
}

function normalizeDate(value: string) {
  return value.slice(0, 10)
}

function addDays(date: Date, days: number) {
  const copy = new Date(date)
  copy.setDate(copy.getDate() + days)
  return copy
}

function isDateInReservation(date: Date, reservation: Reservation) {
  const day = formatDateInput(date)
  return day >= normalizeDate(reservation.checkIn) && day < normalizeDate(reservation.checkOut)
}

function getBookingTone(status: ReservationStatus) {
  if (status === 'Pending') return 'pending-booking'
  if (status === 'CheckedIn') return 'checked-in-booking'
  if (status === 'CheckedOut') return 'checked-out-booking'
  return 'confirmed-booking'
}

function makeId(prefix: string) {
  return `${prefix}-${crypto.randomUUID()}`
}

function textIncludes(value: string | undefined, query: string) {
  return (value ?? '').toLowerCase().includes(query)
}

async function requestJson<T>(path: string, token: string, options: RequestInit = {}) {
  const response = await fetch(path, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...options.headers,
    },
  })

  if (!response.ok) {
    const fallback = `${response.status} ${response.statusText}`
    const payload = await response.json().catch(() => null)
    throw new Error(payload?.detail ?? payload?.title ?? fallback)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

function App() {
  const [page, setPage] = useState<Page>('planner')
  const [token, setToken] = useState(() => localStorage.getItem('hotel_pms_jwt') ?? '')
  const [users, setUsers] = useState<StaffUser[]>(demoUsers)
  const [currentUser, setCurrentUser] = useState<StaffUser | null>(() => {
    const savedEmail = localStorage.getItem('hotel_pms_user_email')
    return demoUsers.find((user) => user.email === savedEmail) ?? null
  })
  const [loginForm, setLoginForm] = useState<LoginForm>({ email: 'manager@hotel.test', password: 'manager123' })
  const [userForm, setUserForm] = useState({
    name: '',
    email: '',
    password: '',
    role: 'Receptionist' as UserRole,
  })
  const [rooms, setRooms] = useState<Room[]>([])
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [parkingAccess, setParkingAccess] = useState<ParkingAccess[]>([])
  const [verification, setVerification] = useState<ParkingVerification | null>(null)
  const [guests, setGuests] = useState<Guest[]>(sampleGuests)
  const [vehicles, setVehicles] = useState<VehicleRecord[]>([
    { id: 'vehicle-1', country: 'Ukraine', registrationNumber: 'AA7777BB', guestId: 'guest-1' },
    { id: 'vehicle-2', country: 'Poland', registrationNumber: 'KR12345', guestId: 'guest-2' },
  ])
  const [selectedReservationId, setSelectedReservationId] = useState('')
  const [selectedReservation, setSelectedReservation] = useState<Reservation | null>(null)
  const [selectedGuest, setSelectedGuest] = useState<Guest | null>(null)
  const [selectedVehicle, setSelectedVehicle] = useState<VehicleRecord | null>(null)
  const [historyGuest, setHistoryGuest] = useState<Guest | null>(null)
  const [registrationNumber, setRegistrationNumber] = useState('')
  const [verifyRegistrationNumber, setVerifyRegistrationNumber] = useState('')
  const [reservationSearch, setReservationSearch] = useState('')
  const [plannerSearch, setPlannerSearch] = useState('')
  const [guestSearch, setGuestSearch] = useState('')
  const [vehicleSearch, setVehicleSearch] = useState('')
  const [selectedRoom, setSelectedRoom] = useState<Room | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const [roomForm, setRoomForm] = useState({
    number: '',
    type: 'Double' as RoomType,
    capacity: 2,
    status: 'Available' as RoomStatus,
  })

  const [reservationForm, setReservationForm] = useState({
    guestFirstName: '',
    guestLastName: '',
    email: '',
    phoneNumber: '',
    checkIn: formatDateInput(today),
    checkOut: formatDateInput(tomorrow),
    roomId: '',
    status: 'Confirmed' as ReservationStatus,
  })

  const [guestForm, setGuestForm] = useState<GuestForm>(emptyGuestForm)
  const [vehicleForm, setVehicleForm] = useState<VehicleForm>(emptyVehicleForm)

  const canManageRooms = currentUser?.role === 'Manager' || currentUser?.role === 'Administrator'
  const canManageUsers = currentUser?.role === 'Manager' || currentUser?.role === 'Administrator'

  const activeReservations = useMemo(
    () => reservations.filter((reservation) => reservation.status !== 'Cancelled'),
    [reservations],
  )

  const dashboardStats = useMemo(() => {
    const availableRooms = rooms.filter((room) => room.status === 'Available').length
    const occupiedRooms = rooms.filter((room) => room.status === 'Occupied').length
    const activeParking = reservations.reduce(
      (total, reservation) => total + reservation.vehicleRegistrationNumbers.length,
      0,
    )

    return { availableRooms, occupiedRooms, activeParking }
  }, [rooms, reservations])

  const plannerDates = useMemo(() => Array.from({ length: 14 }, (_, index) => addDays(today, index)), [])

  const syncedVehicles = useMemo(() => {
    const linkedFromReservations = reservations.flatMap((reservation) =>
      reservation.vehicleRegistrationNumbers.map((plate) => ({
        id: `reservation-${reservation.id}-${plate}`,
        country: 'Unknown',
        registrationNumber: plate,
        reservationId: reservation.id,
      })),
    )
    const existingKeys = new Set(vehicles.map((vehicle) => vehicle.registrationNumber.toLowerCase()))
    return [
      ...vehicles,
      ...linkedFromReservations.filter((vehicle) => !existingKeys.has(vehicle.registrationNumber.toLowerCase())),
    ]
  }, [reservations, vehicles])

  const filteredPlannerReservations = useMemo(
    () => filterReservations(activeReservations, plannerSearch, guests, syncedVehicles),
    [activeReservations, plannerSearch, guests, syncedVehicles],
  )

  const filteredReservations = useMemo(
    () => filterReservations(reservations, reservationSearch, guests, syncedVehicles),
    [reservations, reservationSearch, guests, syncedVehicles],
  )

  const filteredGuests = useMemo(() => {
    const query = guestSearch.trim().toLowerCase()
    if (!query) return guests

    return guests.filter(
      (guest) =>
        textIncludes(guest.firstName, query) ||
        textIncludes(guest.lastName, query) ||
        textIncludes(guest.email, query) ||
        textIncludes(guest.phone, query) ||
        textIncludes(guest.documentNumber, query),
    )
  }, [guestSearch, guests])

  const filteredVehicles = useMemo(() => {
    const query = vehicleSearch.trim().toLowerCase()
    if (!query) return syncedVehicles

    return syncedVehicles.filter(
      (vehicle) => textIncludes(vehicle.country, query) || textIncludes(vehicle.registrationNumber, query),
    )
  }, [syncedVehicles, vehicleSearch])

  const historyReservations = historyGuest
    ? reservations.filter((reservation) => getGuestForReservation(reservation)?.id === historyGuest.id)
    : []

  useEffect(() => {
    localStorage.setItem('hotel_pms_jwt', token)
  }, [token])

  useEffect(() => {
    if (currentUser) {
      localStorage.setItem('hotel_pms_user_email', currentUser.email)
    } else {
      localStorage.removeItem('hotel_pms_user_email')
    }
  }, [currentUser])

  function getGuestForReservation(reservation: Reservation) {
    return guests.find(
      (guest) =>
        guest.email.toLowerCase() === reservation.email.toLowerCase() ||
        (guest.firstName.toLowerCase() === reservation.guestFirstName.toLowerCase() &&
          guest.lastName.toLowerCase() === reservation.guestLastName.toLowerCase()),
    )
  }

  function getReservationVehicles(reservation: Reservation) {
    return syncedVehicles.filter(
      (vehicle) =>
        vehicle.reservationId === reservation.id ||
        reservation.vehicleRegistrationNumbers.some(
          (plate) => plate.toLowerCase() === vehicle.registrationNumber.toLowerCase(),
        ),
    )
  }

  function openReservation(reservation: Reservation) {
    setSelectedReservation({ ...reservation, vehicleRegistrationNumbers: [...reservation.vehicleRegistrationNumbers] })
    setHistoryGuest(null)
  }

  function openGuest(guest: Guest) {
    setSelectedGuest({
      ...guest,
      address: guest.address ? { ...guest.address } : undefined,
    })
  }

  function openVehicle(vehicle: VehicleRecord) {
    setSelectedVehicle({ ...vehicle })
    setVehicleForm({
      country: vehicle.country,
      registrationNumber: vehicle.registrationNumber,
      guestId: vehicle.guestId ?? '',
      reservationId: vehicle.reservationId ?? '',
    })
  }

  async function runAction(action: () => Promise<void>) {
    setIsLoading(true)
    setError('')
    setMessage('')

    try {
      await action()
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unexpected error')
    } finally {
      setIsLoading(false)
    }
  }

  async function refreshDashboard() {
    await runAction(async () => {
      const [loadedRooms, loadedReservations] = await Promise.all([
        requestJson<Room[]>('/api/rooms', token),
        requestJson<Reservation[]>('/api/reservations', token),
      ])

      setRooms(loadedRooms)
      setReservations(loadedReservations)

      if (!reservationForm.roomId && loadedRooms[0]) {
        setReservationForm((current) => ({ ...current, roomId: loadedRooms[0].id }))
      }

      if (!selectedReservationId && loadedReservations[0]) {
        setSelectedReservationId(loadedReservations[0].id)
      }
    })
  }

  function useDevToken() {
    setToken(devJwt)
    setMessage('Development token inserted')
    setError('')
  }

  function login(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const user = users.find(
      (item) =>
        item.email.toLowerCase() === loginForm.email.trim().toLowerCase() &&
        item.password === loginForm.password,
    )

    if (!user) {
      setError('Wrong email or password')
      return
    }

    setCurrentUser(user)
    setToken(devJwt)
    setError('')
    setMessage(`Signed in as ${user.role}`)
  }

  function logout() {
    setCurrentUser(null)
    setToken('')
    setPage('planner')
    setMessage('')
    setError('')
  }

  function createStaffUser(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canManageUsers) {
      setError('Only Manager or Administrator can create PMS users')
      return
    }

    if (users.some((user) => user.email.toLowerCase() === userForm.email.trim().toLowerCase())) {
      setError('User with this email already exists')
      return
    }

    setUsers((current) => [...current, { id: makeId('user'), ...userForm }])
    setUserForm({ name: '', email: '', password: '', role: 'Receptionist' })
    setError('')
    setMessage('PMS user created')
  }

  async function createRoom(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canManageRooms) {
      setError('Only Manager or Administrator can create or change fixed rooms')
      return
    }

    await runAction(async () => {
      await requestJson<Room>('/api/rooms', token, {
        method: 'POST',
        body: JSON.stringify(roomForm),
      })
      setRoomForm({ number: '', type: 'Double', capacity: 2, status: 'Available' })
      setMessage('Room created')
      await refreshDashboard()
    })
  }

  async function deleteRoom(id: string) {
    if (!canManageRooms) {
      setError('Only Manager or Administrator can delete rooms')
      return
    }

    await runAction(async () => {
      await requestJson<void>(`/api/rooms/${id}`, token, { method: 'DELETE' })
      setMessage('Room deleted')
      await refreshDashboard()
    })
  }

  async function updateRoom(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedRoom) return

    if (!canManageRooms) {
      setError('Only Manager or Administrator can change room number or type')
      return
    }

    await runAction(async () => {
      const updatedRoom = await requestJson<Room>(`/api/rooms/${selectedRoom.id}`, token, {
        method: 'PUT',
        body: JSON.stringify(selectedRoom),
      })

      setRooms((current) => current.map((room) => (room.id === updatedRoom.id ? updatedRoom : room)))
      setSelectedRoom(null)
      setMessage('Room updated')
    })
  }

  async function createReservation(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await runAction(async () => {
      const createdReservation = await requestJson<Reservation>('/api/reservations', token, {
        method: 'POST',
        body: JSON.stringify({
          guestFirstName: reservationForm.guestFirstName,
          guestLastName: reservationForm.guestLastName,
          email: reservationForm.email,
          checkIn: reservationForm.checkIn,
          checkOut: reservationForm.checkOut,
          roomId: reservationForm.roomId,
          status: reservationForm.status,
        }),
      })

      upsertGuestFromReservation(reservationForm)
      setReservations((current) => upsertReservation(current, createdReservation))
      setReservationForm((current) => ({
        ...current,
        guestFirstName: '',
        guestLastName: '',
        email: '',
        phoneNumber: '',
      }))
      setMessage('Reservation created')
    })
  }

  async function cancelReservation(id: string) {
    await runAction(async () => {
      await requestJson<void>(`/api/reservations/${id}`, token, { method: 'DELETE' })
      setReservations((current) =>
        current.map((reservation) =>
          reservation.id === id ? { ...reservation, status: 'Cancelled' as ReservationStatus } : reservation,
        ),
      )
      setMessage('Reservation cancelled')
    })
  }

  function saveReservationDetails(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedReservation) return

    setReservations((current) => upsertReservation(current, selectedReservation))
    setSelectedReservation(null)
    setMessage('Reservation details updated in the PMS view')
  }

  function upsertGuestFromReservation(source: {
    guestFirstName: string
    guestLastName: string
    email: string
    phoneNumber?: string
  }) {
    if (!source.email.trim()) return

    setGuests((current) => {
      const existingGuest = current.find((guest) => guest.email.toLowerCase() === source.email.toLowerCase())
      if (existingGuest) {
        return current.map((guest) =>
          guest.id === existingGuest.id
            ? {
                ...guest,
                firstName: source.guestFirstName,
                lastName: source.guestLastName,
                phone: source.phoneNumber || guest.phone,
              }
            : guest,
        )
      }

      return [
        ...current,
        {
          id: makeId('guest'),
          firstName: source.guestFirstName,
          lastName: source.guestLastName,
          email: source.email,
          phone: source.phoneNumber ?? '',
          country: '',
          documentNumber: '',
        },
      ]
    })
  }

  function saveGuest(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const guestToSave: Guest = { id: makeId('guest'), ...guestForm }

    setGuests((current) => [...current, guestToSave])
    setGuestForm(emptyGuestForm)
    setMessage('Guest added')
  }

  function saveGuestDetails(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedGuest) return

    setGuests((current) => upsertById(current, selectedGuest))
    setSelectedGuest(null)
    setMessage('Guest details updated')
  }

  function saveVehicle(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const vehicleToSave: VehicleRecord = { id: selectedVehicle?.id ?? makeId('vehicle'), ...vehicleForm }

    setVehicles((current) => upsertById(current, vehicleToSave))
    setVehicleForm(emptyVehicleForm)
    setSelectedVehicle(null)
    setMessage(selectedVehicle ? 'Vehicle updated' : 'Vehicle added')
  }

  async function assignVehicle(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await runAction(async () => {
      const access = await requestJson<ParkingAccess>(
        `/api/parking/reservations/${selectedReservationId}/vehicles`,
        token,
        {
          method: 'POST',
          body: JSON.stringify({ registrationNumber }),
        },
      )

      setRegistrationNumber('')
      setMessage(`Parking access assigned to ${access.registrationNumber}`)
      await loadParkingAccess(selectedReservationId)
      await refreshDashboard()
    })
  }

  async function loadParkingAccess(reservationId: string) {
    const access = await requestJson<ParkingAccess[]>(
      `/api/parking/reservations/${reservationId}`,
      token,
    )
    setParkingAccess(access)
  }

  async function verifyParkingAccess(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await runAction(async () => {
      const result = await requestJson<ParkingVerification>(
        `/api/parking/access/${encodeURIComponent(verifyRegistrationNumber)}`,
        token,
      )
      setVerification(result)
    })
  }

  const visiblePages = canManageUsers ? pages : pages.filter((item) => item.id !== 'users')

  if (!currentUser) {
    return (
      <LoginPage
        loginForm={loginForm}
        setLoginForm={setLoginForm}
        users={users}
        onLogin={login}
        error={error}
      />
    )
  }

  return (
    <main className="pms-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">H</span>
          <div>
            <strong>Hotel PMS</strong>
            <span>Front desk</span>
          </div>
        </div>

        <nav className="nav-list" aria-label="PMS sections">
          {visiblePages.map((item) => (
            <button
              key={item.id}
              type="button"
              className={page === item.id ? 'active' : ''}
              onClick={() => setPage(item.id)}
            >
              {item.label}
            </button>
          ))}
        </nav>

        <div className="sidebar-card">
          <span>Backend</span>
          <strong>localhost:5114</strong>
        </div>
      </aside>

      <section className="main-area">
        <header className="topbar">
          <div>
            <p className="eyebrow">Small hotel operations</p>
            <h1>{pages.find((item) => item.id === page)?.label}</h1>
          </div>
          <div className="session-panel">
            <div>
              <span>{currentUser.name}</span>
              <strong>{currentUser.role}</strong>
            </div>
            <button type="button" onClick={useDevToken}>
              Demo token
            </button>
            <button type="button" onClick={() => void refreshDashboard()} disabled={!token || isLoading}>
              Refresh
            </button>
            <button type="button" className="ghost" onClick={logout}>
              Logout
            </button>
          </div>
        </header>

        {(message || error) && (
          <section className={`notice ${error ? 'error' : 'success'}`}>{error || message}</section>
        )}

        {page === 'planner' && (
          <section className="page-stack">
            <div className="kpi-grid">
              <article className="kpi-card">
                <span>All rooms</span>
                <strong>{rooms.length}</strong>
              </article>
              <article className="kpi-card">
                <span>Vacant</span>
                <strong>{dashboardStats.availableRooms}</strong>
              </article>
              <article className="kpi-card">
                <span>Occupied</span>
                <strong>{dashboardStats.occupiedRooms}</strong>
              </article>
              <article className="kpi-card">
                <span>Parking</span>
                <strong>{dashboardStats.activeParking}</strong>
              </article>
            </div>

            <section className="planner-panel">
              <div className="planner-toolbar">
                <div className="date-chip">
                  <strong>{formatDateInput(today)}</strong>
                  <span>14 day plan</span>
                </div>
                <label className="toolbar-search">
                  Search reservation
                  <input
                    value={plannerSearch}
                    onChange={(event) => setPlannerSearch(event.target.value)}
                    placeholder="Name, email, phone or plate"
                  />
                </label>
                <button type="button" onClick={() => setPage('reservations')}>
                  New booking
                </button>
              </div>

              {plannerSearch && (
                <SearchResults
                  reservations={filteredPlannerReservations}
                  onOpen={openReservation}
                  getGuestForReservation={getGuestForReservation}
                />
              )}

              <div className="booking-grid">
                <div className="room-heading">Room</div>
                {plannerDates.map((date) => (
                  <div className="day-heading" key={date.toISOString()}>
                    <span>{dateLabelFormatter.format(date)}</span>
                    <strong>{date.getDate()}</strong>
                  </div>
                ))}

                {rooms.map((room) => {
                  const roomReservations = activeReservations.filter(
                    (reservation) => reservation.roomId === room.id,
                  )

                  return (
                    <div className="planner-row" key={room.id}>
                      <div className="room-cell">
                        <strong>{room.number}</strong>
                        <span>
                          {room.type} / {room.capacity}
                        </span>
                        <em className={room.status.toLowerCase()}>{room.status}</em>
                      </div>
                      {plannerDates.map((date) => {
                        const reservation = roomReservations.find((item) => isDateInReservation(date, item))
                        const isStartDay =
                          reservation && normalizeDate(reservation.checkIn) === formatDateInput(date)

                        return (
                          <button
                            type="button"
                            className={`day-cell ${reservation ? 'reserved-cell' : ''}`}
                            key={`${room.id}-${date.toISOString()}`}
                            onClick={() => {
                              if (reservation) openReservation(reservation)
                            }}
                            title={reservation ? `${reservation.guestFirstName} ${reservation.guestLastName}` : 'Free'}
                          >
                            {reservation && (
                              <span className={`booking-bar ${getBookingTone(reservation.status)}`}>
                                {isStartDay ? (
                                  <>
                                    <span>
                                      {reservation.guestFirstName} {reservation.guestLastName}
                                    </span>
                                    {reservation.vehicleRegistrationNumbers.length > 0 && <small>Parking</small>}
                                  </>
                                ) : (
                                  <span aria-hidden="true">•</span>
                                )}
                              </span>
                            )}
                          </button>
                        )
                      })}
                    </div>
                  )
                })}

                {rooms.length === 0 && (
                  <div className="planner-empty">
                    Add rooms first, then reservations will appear on the booking plan.
                  </div>
                )}
              </div>
            </section>
          </section>
        )}

        {page === 'rooms' && (
          <section className="workspace two-column">
            <form className="panel" onSubmit={(event) => void createRoom(event)}>
              <h2>Add room</h2>
              {!canManageRooms && (
                <p className="permission-note">
                  Rooms are fixed. Only Manager or Administrator can create rooms or change room number and type.
                </p>
              )}
              <label>
                Number
                <input
                  value={roomForm.number}
                  onChange={(event) => setRoomForm({ ...roomForm, number: event.target.value })}
                  disabled={!canManageRooms}
                  required
                />
              </label>
              <label>
                Type
                <select
                  value={roomForm.type}
                  onChange={(event) => setRoomForm({ ...roomForm, type: event.target.value as RoomType })}
                  disabled={!canManageRooms}
                >
                  {roomTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Capacity
                <input
                  type="number"
                  min="1"
                  max="20"
                  value={roomForm.capacity}
                  onChange={(event) => setRoomForm({ ...roomForm, capacity: Number(event.target.value) })}
                  disabled={!canManageRooms}
                  required
                />
              </label>
              <label>
                Status
                <select
                  value={roomForm.status}
                  onChange={(event) => setRoomForm({ ...roomForm, status: event.target.value as RoomStatus })}
                  disabled={!canManageRooms}
                >
                  {roomStatuses.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </label>
              <button type="submit" disabled={!token || isLoading || !canManageRooms}>
                Create room
              </button>
            </form>

            <section className="panel wide">
              <div className="panel-heading">
                <h2>Room list</h2>
                <span>{rooms.length} rooms</span>
              </div>
              <RoomTable
                rooms={rooms}
                canManageRooms={canManageRooms}
                onDelete={(id) => void deleteRoom(id)}
                onEdit={(room) => setSelectedRoom({ ...room })}
              />
            </section>
          </section>
        )}

        {page === 'reservations' && (
          <section className="workspace two-column">
            <ReservationCreateForm
              reservationForm={reservationForm}
              setReservationForm={setReservationForm}
              rooms={rooms}
              onSubmit={(event) => void createReservation(event)}
              disabled={!token || isLoading || rooms.length === 0}
            />

            <section className="panel wide">
              <div className="panel-heading stacked-heading">
                <div>
                  <h2>Reservations</h2>
                  <span>Past, current, future and cancelled stays</span>
                </div>
                <label className="list-search">
                  Search
                  <input
                    value={reservationSearch}
                    onChange={(event) => setReservationSearch(event.target.value)}
                    placeholder="Name, email, phone or plate"
                  />
                </label>
              </div>
              <ReservationBuckets
                reservations={filteredReservations}
                onOpen={openReservation}
                onCancel={(id) => void cancelReservation(id)}
              />
            </section>
          </section>
        )}

        {page === 'guests' && (
          <section className="workspace two-column">
            <form className="panel" onSubmit={saveGuest}>
              <h2>Add guest</h2>
              <GuestFields guest={guestForm} onChange={setGuestForm} showAddressToggle />
              <button type="submit">Add guest</button>
            </form>

            <section className="panel wide">
              <div className="panel-heading stacked-heading">
                <div>
                  <h2>Guests</h2>
                  <span>{guests.length} profiles</span>
                </div>
                <label className="list-search">
                  Search guests
                  <input
                    value={guestSearch}
                    onChange={(event) => setGuestSearch(event.target.value)}
                    placeholder="Name, email, phone or document"
                  />
                </label>
              </div>
              <div className="card-list">
                {filteredGuests.map((guest) => (
                  <button className="record-card" type="button" key={guest.id} onClick={() => openGuest(guest)}>
                    <strong>
                      {guest.firstName} {guest.lastName}
                    </strong>
                    <span>{guest.email}</span>
                    <small>
                      {guest.phone || 'No phone'} · {guest.country || 'No country'}
                    </small>
                  </button>
                ))}
                {filteredGuests.length === 0 && <p className="empty">No guests found</p>}
              </div>
            </section>
          </section>
        )}

        {page === 'vehicles' && (
          <section className="workspace two-column">
            <VehicleFormPanel
              vehicleForm={vehicleForm}
              setVehicleForm={setVehicleForm}
              selectedVehicle={selectedVehicle}
              setSelectedVehicle={setSelectedVehicle}
              guests={guests}
              reservations={reservations}
              onSubmit={saveVehicle}
            />

            <section className="panel wide">
              <div className="panel-heading stacked-heading">
                <div>
                  <h2>Vehicles</h2>
                  <span>{syncedVehicles.length} records</span>
                </div>
                <label className="list-search">
                  Search vehicles
                  <input
                    value={vehicleSearch}
                    onChange={(event) => setVehicleSearch(event.target.value)}
                    placeholder="Country or registration"
                  />
                </label>
              </div>
              <VehiclesTable
                vehicles={filteredVehicles}
                guests={guests}
                reservations={reservations}
                onEdit={openVehicle}
                onOpenReservation={openReservation}
              />
            </section>
          </section>
        )}

        {page === 'parking' && (
          <ParkingPage
            activeReservations={activeReservations}
            selectedReservationId={selectedReservationId}
            setSelectedReservationId={setSelectedReservationId}
            loadParkingAccess={loadParkingAccess}
            registrationNumber={registrationNumber}
            setRegistrationNumber={setRegistrationNumber}
            assignVehicle={assignVehicle}
            token={token}
            isLoading={isLoading}
            verifyRegistrationNumber={verifyRegistrationNumber}
            setVerifyRegistrationNumber={setVerifyRegistrationNumber}
            verifyParkingAccess={verifyParkingAccess}
            verification={verification}
            parkingAccess={parkingAccess}
          />
        )}

        {page === 'users' && canManageUsers && (
          <section className="workspace two-column">
            <form className="panel" onSubmit={createStaffUser}>
              <h2>Create PMS user</h2>
              <label>
                Name
                <input
                  value={userForm.name}
                  onChange={(event) => setUserForm({ ...userForm, name: event.target.value })}
                  required
                />
              </label>
              <label>
                Email
                <input
                  type="email"
                  value={userForm.email}
                  onChange={(event) => setUserForm({ ...userForm, email: event.target.value })}
                  required
                />
              </label>
              <label>
                Password
                <input
                  type="password"
                  value={userForm.password}
                  onChange={(event) => setUserForm({ ...userForm, password: event.target.value })}
                  required
                  minLength={6}
                />
              </label>
              <label>
                Access role
                <select
                  value={userForm.role}
                  onChange={(event) => setUserForm({ ...userForm, role: event.target.value as UserRole })}
                >
                  <option value="Receptionist">Receptionist</option>
                  <option value="Manager">Manager</option>
                  <option value="Administrator">Administrator</option>
                </select>
              </label>
              <button type="submit">Create user</button>
            </form>

            <section className="panel wide">
              <div className="panel-heading">
                <h2>PMS users</h2>
                <span>{users.length} accounts</span>
              </div>
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Email</th>
                      <th>Role</th>
                      <th>Room access</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((user) => (
                      <tr key={user.id}>
                        <td>{user.name}</td>
                        <td>{user.email}</td>
                        <td>
                          <span className={`pill ${user.role.toLowerCase()}`}>{user.role}</span>
                        </td>
                        <td>
                          {user.role === 'Receptionist'
                            ? 'View rooms only'
                            : 'Can change room number and type'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          </section>
        )}

        {page === 'reports' && (
          <section className="page-stack">
            <section className="panel wide">
              <div className="panel-heading">
                <h2>Operational overview</h2>
                <button type="button" onClick={() => void refreshDashboard()} disabled={!token || isLoading}>
                  Refresh data
                </button>
              </div>
              <div className="report-grid">
                <div>
                  <span>Total rooms</span>
                  <strong>{rooms.length}</strong>
                </div>
                <div>
                  <span>Reservations</span>
                  <strong>{reservations.length}</strong>
                </div>
                <div>
                  <span>Guests</span>
                  <strong>{guests.length}</strong>
                </div>
                <div>
                  <span>Vehicles</span>
                  <strong>{syncedVehicles.length}</strong>
                </div>
                <div>
                  <span>Parking registrations</span>
                  <strong>{dashboardStats.activeParking}</strong>
                </div>
              </div>
            </section>
          </section>
        )}
      </section>

      {selectedReservation && (
        <ReservationModal
          reservation={selectedReservation}
          rooms={rooms}
          vehicles={getReservationVehicles(selectedReservation)}
          setReservation={setSelectedReservation}
          onClose={() => setSelectedReservation(null)}
          onSave={saveReservationDetails}
        />
      )}

      {selectedGuest && (
        <GuestModal
          guest={selectedGuest}
          setGuest={setSelectedGuest}
          onClose={() => setSelectedGuest(null)}
          onSave={saveGuestDetails}
          onHistory={() => {
            setHistoryGuest(selectedGuest)
            setSelectedGuest(null)
          }}
        />
      )}

      {historyGuest && (
        <HistoryModal
          guest={historyGuest}
          reservations={historyReservations}
          onClose={() => setHistoryGuest(null)}
          onOpenReservation={openReservation}
        />
      )}

      {selectedRoom && canManageRooms && (
        <RoomModal
          room={selectedRoom}
          setRoom={setSelectedRoom}
          onClose={() => setSelectedRoom(null)}
          onSave={(event) => void updateRoom(event)}
        />
      )}
    </main>
  )
}

function filterReservations(
  reservations: Reservation[],
  search: string,
  guests: Guest[],
  vehicles: VehicleRecord[],
) {
  const query = search.trim().toLowerCase()
  if (!query) return reservations

  return reservations.filter((reservation) => {
    const guest = guests.find(
      (item) =>
        item.email.toLowerCase() === reservation.email.toLowerCase() ||
        (item.firstName.toLowerCase() === reservation.guestFirstName.toLowerCase() &&
          item.lastName.toLowerCase() === reservation.guestLastName.toLowerCase()),
    )
    const reservationVehicles = vehicles.filter(
      (vehicle) =>
        vehicle.reservationId === reservation.id ||
        reservation.vehicleRegistrationNumbers.some(
          (plate) => plate.toLowerCase() === vehicle.registrationNumber.toLowerCase(),
        ) ||
        vehicle.guestId === guest?.id,
    )

    return (
      textIncludes(reservation.guestFirstName, query) ||
      textIncludes(reservation.guestLastName, query) ||
      textIncludes(reservation.email, query) ||
      textIncludes(guest?.phone, query) ||
      reservationVehicles.some((vehicle) => textIncludes(vehicle.registrationNumber, query))
    )
  })
}

function upsertReservation(items: Reservation[], reservation: Reservation) {
  return items.some((item) => item.id === reservation.id)
    ? items.map((item) => (item.id === reservation.id ? reservation : item))
    : [...items, reservation]
}

function upsertById<T extends { id: string }>(items: T[], item: T) {
  return items.some((current) => current.id === item.id)
    ? items.map((current) => (current.id === item.id ? item : current))
    : [...items, item]
}

function getReservationTimeBucket(reservation: Reservation) {
  if (reservation.status === 'Cancelled') return 'Cancelled'

  const checkIn = normalizeDate(reservation.checkIn)
  const checkOut = normalizeDate(reservation.checkOut)
  const currentDate = formatDateInput(today)

  if (checkOut < currentDate) return 'Past'
  if (checkIn > currentDate) return 'Future'
  return 'Current'
}

function LoginPage({
  loginForm,
  setLoginForm,
  users,
  onLogin,
  error,
}: {
  loginForm: LoginForm
  setLoginForm: (form: LoginForm) => void
  users: StaffUser[]
  onLogin: (event: FormEvent<HTMLFormElement>) => void
  error: string
}) {
  return (
    <main className="login-shell">
      <section className="login-card">
        <div className="brand login-brand">
          <span className="brand-mark">H</span>
          <div>
            <strong>Hotel PMS</strong>
            <span>Secure staff access</span>
          </div>
        </div>
        <form className="login-form" onSubmit={onLogin}>
          <div>
            <p className="eyebrow">Sign in</p>
            <h1>Welcome back</h1>
          </div>
          {error && <section className="notice error">{error}</section>}
          <label>
            Email address
            <input
              type="email"
              value={loginForm.email}
              onChange={(event) => setLoginForm({ ...loginForm, email: event.target.value })}
              required
            />
          </label>
          <label>
            Password
            <input
              type="password"
              value={loginForm.password}
              onChange={(event) => setLoginForm({ ...loginForm, password: event.target.value })}
              required
            />
          </label>
          <button type="submit">Login</button>
        </form>
        <div className="demo-users">
          <strong>Demo access</strong>
          {users.map((user) => (
            <button
              key={user.id}
              type="button"
              onClick={() => setLoginForm({ email: user.email, password: user.password })}
            >
              <span>{user.role}</span>
              <small>
                {user.email} / {user.password}
              </small>
            </button>
          ))}
        </div>
      </section>
    </main>
  )
}

function RoomModal({
  room,
  setRoom,
  onClose,
  onSave,
}: {
  room: Room
  setRoom: (room: Room) => void
  onClose: () => void
  onSave: (event: FormEvent<HTMLFormElement>) => void
}) {
  return (
    <section className="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="room-title">
      <form className="modal-card" onSubmit={onSave}>
        <div className="modal-heading">
          <div>
            <p className="eyebrow">Manager room settings</p>
            <h2 id="room-title">Room {room.number}</h2>
          </div>
          <button type="button" className="ghost" onClick={onClose}>
            Close
          </button>
        </div>
        <div className="split">
          <label>
            Room number
            <input value={room.number} onChange={(event) => setRoom({ ...room, number: event.target.value })} />
          </label>
          <label>
            Room type
            <select value={room.type} onChange={(event) => setRoom({ ...room, type: event.target.value as RoomType })}>
              {roomTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </label>
        </div>
        <div className="split">
          <label>
            Capacity
            <input
              type="number"
              min="1"
              max="20"
              value={room.capacity}
              onChange={(event) => setRoom({ ...room, capacity: Number(event.target.value) })}
            />
          </label>
          <label>
            Status
            <select
              value={room.status}
              onChange={(event) => setRoom({ ...room, status: event.target.value as RoomStatus })}
            >
              {roomStatuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>
        </div>
        <button type="submit">Save room</button>
      </form>
    </section>
  )
}

function RoomTable({
  rooms,
  canManageRooms,
  onDelete,
  onEdit,
}: {
  rooms: Room[]
  canManageRooms: boolean
  onDelete: (id: string) => void
  onEdit: (room: Room) => void
}) {
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Number</th>
            <th>Type</th>
            <th>Capacity</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {rooms.map((room) => (
            <tr key={room.id}>
              <td>{room.number}</td>
              <td>{room.type}</td>
              <td>{room.capacity}</td>
              <td>
                <span className={`pill ${room.status.toLowerCase()}`}>{room.status}</span>
              </td>
              <td>
                {canManageRooms ? (
                  <div className="inline-actions">
                    <button type="button" className="ghost" onClick={() => onEdit(room)}>
                      Edit
                    </button>
                    <button type="button" className="ghost" onClick={() => onDelete(room.id)}>
                      Delete
                    </button>
                  </div>
                ) : (
                  <span className="subtext">Read only</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {rooms.length === 0 && <p className="empty">No rooms loaded</p>}
    </div>
  )
}

function ReservationCreateForm({
  reservationForm,
  setReservationForm,
  rooms,
  onSubmit,
  disabled,
}: {
  reservationForm: {
    guestFirstName: string
    guestLastName: string
    email: string
    phoneNumber: string
    checkIn: string
    checkOut: string
    roomId: string
    status: ReservationStatus
  }
  setReservationForm: (form: {
    guestFirstName: string
    guestLastName: string
    email: string
    phoneNumber: string
    checkIn: string
    checkOut: string
    roomId: string
    status: ReservationStatus
  }) => void
  rooms: Room[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  disabled: boolean
}) {
  return (
    <form className="panel" onSubmit={onSubmit}>
      <h2>New reservation</h2>
      <label>
        First name
        <input
          value={reservationForm.guestFirstName}
          onChange={(event) => setReservationForm({ ...reservationForm, guestFirstName: event.target.value })}
          required
        />
      </label>
      <label>
        Last name
        <input
          value={reservationForm.guestLastName}
          onChange={(event) => setReservationForm({ ...reservationForm, guestLastName: event.target.value })}
          required
        />
      </label>
      <label>
        Email
        <input
          type="email"
          value={reservationForm.email}
          onChange={(event) => setReservationForm({ ...reservationForm, email: event.target.value })}
          required
        />
      </label>
      <label>
        Phone number
        <input
          value={reservationForm.phoneNumber}
          onChange={(event) => setReservationForm({ ...reservationForm, phoneNumber: event.target.value })}
          placeholder="Used by PMS search"
        />
      </label>
      <label>
        Room
        <select
          value={reservationForm.roomId}
          onChange={(event) => setReservationForm({ ...reservationForm, roomId: event.target.value })}
          required
        >
          <option value="">Select room</option>
          {rooms.map((room) => (
            <option key={room.id} value={room.id}>
              {room.number} - {room.type}
            </option>
          ))}
        </select>
      </label>
      <div className="split">
        <label>
          Check-in
          <input
            type="date"
            value={reservationForm.checkIn}
            onChange={(event) => setReservationForm({ ...reservationForm, checkIn: event.target.value })}
            required
          />
        </label>
        <label>
          Check-out
          <input
            type="date"
            value={reservationForm.checkOut}
            onChange={(event) => setReservationForm({ ...reservationForm, checkOut: event.target.value })}
            required
          />
        </label>
      </div>
      <label>
        Status
        <select
          value={reservationForm.status}
          onChange={(event) =>
            setReservationForm({ ...reservationForm, status: event.target.value as ReservationStatus })
          }
        >
          {reservationStatuses.map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>
      </label>
      <button type="submit" disabled={disabled}>
        Create reservation
      </button>
    </form>
  )
}

function SearchResults({
  reservations,
  onOpen,
  getGuestForReservation,
}: {
  reservations: Reservation[]
  onOpen: (reservation: Reservation) => void
  getGuestForReservation: (reservation: Reservation) => Guest | undefined
}) {
  return (
    <div className="search-results">
      {reservations.map((reservation) => {
        const guest = getGuestForReservation(reservation)

        return (
          <button key={reservation.id} type="button" onClick={() => onOpen(reservation)}>
            <strong>
              {reservation.guestFirstName} {reservation.guestLastName}
            </strong>
            <span>
              {reservation.email} {guest?.phone ? `· ${guest.phone}` : ''}
            </span>
            <small>
              Room {reservation.roomNumber || reservation.roomId.slice(0, 8)} · {normalizeDate(reservation.checkIn)}
            </small>
          </button>
        )
      })}
      {reservations.length === 0 && <p className="empty">No matching reservations</p>}
    </div>
  )
}

function ReservationBuckets({
  reservations,
  onOpen,
  onCancel,
}: {
  reservations: Reservation[]
  onOpen: (reservation: Reservation) => void
  onCancel: (id: string) => void
}) {
  const buckets = ['Current', 'Future', 'Past', 'Cancelled'] as const

  return (
    <div className="reservation-buckets">
      {buckets.map((bucket) => {
        const bucketReservations = reservations.filter(
          (reservation) => getReservationTimeBucket(reservation) === bucket,
        )

        return (
          <section className="reservation-bucket" key={bucket}>
            <h3>
              {bucket} <span>{bucketReservations.length}</span>
            </h3>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Guest</th>
                    <th>Room</th>
                    <th>Dates</th>
                    <th>Status</th>
                    <th>Vehicles</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {bucketReservations.map((reservation) => (
                    <tr key={reservation.id}>
                      <td>
                        <button className="link-button" type="button" onClick={() => onOpen(reservation)}>
                          {reservation.guestFirstName} {reservation.guestLastName}
                        </button>
                        <span className="subtext">{reservation.email}</span>
                      </td>
                      <td>{reservation.roomNumber || reservation.roomId.slice(0, 8)}</td>
                      <td>
                        {normalizeDate(reservation.checkIn)} - {normalizeDate(reservation.checkOut)}
                      </td>
                      <td>
                        <span className={`pill ${reservation.status.toLowerCase()}`}>{reservation.status}</span>
                      </td>
                      <td>{reservation.vehicleRegistrationNumbers.join(', ') || '-'}</td>
                      <td>
                        <button className="ghost" type="button" onClick={() => onOpen(reservation)}>
                          Open
                        </button>
                        <button
                          type="button"
                          className="ghost"
                          onClick={() => onCancel(reservation.id)}
                          disabled={reservation.status === 'Cancelled'}
                        >
                          Cancel
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {bucketReservations.length === 0 && <p className="empty">No {bucket.toLowerCase()} reservations</p>}
            </div>
          </section>
        )
      })}
    </div>
  )
}

function GuestFields<TGuest extends GuestForm | Guest>({
  guest,
  onChange,
  showAddressToggle = false,
}: {
  guest: TGuest
  onChange: (guest: TGuest) => void
  showAddressToggle?: boolean
}) {
  return (
    <>
      <div className="split">
        <label>
          First name
          <input
            value={guest.firstName}
            onChange={(event) => onChange({ ...guest, firstName: event.target.value })}
            required
          />
        </label>
        <label>
          Last name
          <input
            value={guest.lastName}
            onChange={(event) => onChange({ ...guest, lastName: event.target.value })}
            required
          />
        </label>
      </div>
      <label>
        Email address
        <input
          type="email"
          value={guest.email}
          onChange={(event) => onChange({ ...guest, email: event.target.value })}
          required
        />
      </label>
      <label>
        Phone
        <input
          value={guest.phone}
          onChange={(event) => onChange({ ...guest, phone: event.target.value })}
          placeholder="+380 67 123 4567"
        />
      </label>
      <div className="split">
        <label>
          Country
          <input value={guest.country} onChange={(event) => onChange({ ...guest, country: event.target.value })} />
        </label>
        <label>
          Document number
          <input
            value={guest.documentNumber}
            onChange={(event) => onChange({ ...guest, documentNumber: event.target.value })}
          />
        </label>
      </div>
      {showAddressToggle && (
        <>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={Boolean(guest.address)}
              onChange={(event) =>
                onChange({
                  ...guest,
                  address: event.target.checked
                    ? guest.address ?? {
                        country: guest.country,
                        city: '',
                        postalCode: '',
                        street: '',
                        buildingNumber: '',
                      }
                    : undefined,
                })
              }
            />
            Add address
          </label>
          {guest.address && <GuestAddressFields guest={guest} onChange={onChange} />}
        </>
      )}
    </>
  )
}

function GuestAddressFields<TGuest extends GuestForm | Guest>({
  guest,
  onChange,
}: {
  guest: TGuest
  onChange: (guest: TGuest) => void
}) {
  if (!guest.address) return null

  return (
    <div className="address-panel">
      <div className="split">
        <label>
          Country
          <input
            value={guest.address.country}
            onChange={(event) =>
              onChange({ ...guest, address: { ...guest.address!, country: event.target.value } })
            }
          />
        </label>
        <label>
          City / locality
          <input
            value={guest.address.city}
            onChange={(event) => onChange({ ...guest, address: { ...guest.address!, city: event.target.value } })}
          />
        </label>
      </div>
      <div className="split">
        <label>
          Postal code
          <input
            value={guest.address.postalCode}
            onChange={(event) =>
              onChange({ ...guest, address: { ...guest.address!, postalCode: event.target.value } })
            }
          />
        </label>
        <label>
          House / building number
          <input
            value={guest.address.buildingNumber}
            onChange={(event) =>
              onChange({ ...guest, address: { ...guest.address!, buildingNumber: event.target.value } })
            }
          />
        </label>
      </div>
      <label>
        Address / street
        <input
          value={guest.address.street}
          onChange={(event) => onChange({ ...guest, address: { ...guest.address!, street: event.target.value } })}
        />
      </label>
    </div>
  )
}

function VehicleFormPanel({
  vehicleForm,
  setVehicleForm,
  selectedVehicle,
  setSelectedVehicle,
  guests,
  reservations,
  onSubmit,
}: {
  vehicleForm: VehicleForm
  setVehicleForm: (form: VehicleForm) => void
  selectedVehicle: VehicleRecord | null
  setSelectedVehicle: (vehicle: VehicleRecord | null) => void
  guests: Guest[]
  reservations: Reservation[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}) {
  return (
    <form className="panel" onSubmit={onSubmit}>
      <h2>{selectedVehicle ? 'Edit vehicle' : 'Add vehicle'}</h2>
      <label>
        Country
        <input
          value={vehicleForm.country}
          onChange={(event) => setVehicleForm({ ...vehicleForm, country: event.target.value })}
          required
        />
      </label>
      <label>
        Registration number
        <input
          value={vehicleForm.registrationNumber}
          onChange={(event) => setVehicleForm({ ...vehicleForm, registrationNumber: event.target.value.toUpperCase() })}
          required
        />
      </label>
      <label>
        Guest
        <select
          value={vehicleForm.guestId}
          onChange={(event) => setVehicleForm({ ...vehicleForm, guestId: event.target.value })}
        >
          <option value="">Not linked</option>
          {guests.map((guest) => (
            <option key={guest.id} value={guest.id}>
              {guest.firstName} {guest.lastName}
            </option>
          ))}
        </select>
      </label>
      <label>
        Reservation
        <select
          value={vehicleForm.reservationId}
          onChange={(event) => setVehicleForm({ ...vehicleForm, reservationId: event.target.value })}
        >
          <option value="">Not linked</option>
          {reservations.map((reservation) => (
            <option key={reservation.id} value={reservation.id}>
              {reservation.guestLastName}, room {reservation.roomNumber || reservation.roomId.slice(0, 8)}
            </option>
          ))}
        </select>
      </label>
      <button type="submit">{selectedVehicle ? 'Save vehicle' : 'Add vehicle'}</button>
      {selectedVehicle && (
        <button
          className="ghost"
          type="button"
          onClick={() => {
            setSelectedVehicle(null)
            setVehicleForm(emptyVehicleForm)
          }}
        >
          Cancel edit
        </button>
      )}
    </form>
  )
}

function VehiclesTable({
  vehicles,
  guests,
  reservations,
  onEdit,
  onOpenReservation,
}: {
  vehicles: VehicleRecord[]
  guests: Guest[]
  reservations: Reservation[]
  onEdit: (vehicle: VehicleRecord) => void
  onOpenReservation: (reservation: Reservation) => void
}) {
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Country</th>
            <th>Registration</th>
            <th>Guest</th>
            <th>Reservation</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {vehicles.map((vehicle) => {
            const guest = guests.find((item) => item.id === vehicle.guestId)
            const reservation = reservations.find((item) => item.id === vehicle.reservationId)

            return (
              <tr key={vehicle.id}>
                <td>{vehicle.country}</td>
                <td>{vehicle.registrationNumber}</td>
                <td>{guest ? `${guest.firstName} ${guest.lastName}` : '-'}</td>
                <td>
                  {reservation ? (
                    <button className="link-button" type="button" onClick={() => onOpenReservation(reservation)}>
                      {reservation.guestLastName}, {normalizeDate(reservation.checkIn)}
                    </button>
                  ) : (
                    '-'
                  )}
                </td>
                <td>
                  <button className="ghost" type="button" onClick={() => onEdit(vehicle)}>
                    Edit
                  </button>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
      {vehicles.length === 0 && <p className="empty">No vehicles found</p>}
    </div>
  )
}

function ParkingPage({
  activeReservations,
  selectedReservationId,
  setSelectedReservationId,
  loadParkingAccess,
  registrationNumber,
  setRegistrationNumber,
  assignVehicle,
  token,
  isLoading,
  verifyRegistrationNumber,
  setVerifyRegistrationNumber,
  verifyParkingAccess,
  verification,
  parkingAccess,
}: {
  activeReservations: Reservation[]
  selectedReservationId: string
  setSelectedReservationId: (id: string) => void
  loadParkingAccess: (id: string) => Promise<void>
  registrationNumber: string
  setRegistrationNumber: (value: string) => void
  assignVehicle: (event: FormEvent<HTMLFormElement>) => void
  token: string
  isLoading: boolean
  verifyRegistrationNumber: string
  setVerifyRegistrationNumber: (value: string) => void
  verifyParkingAccess: (event: FormEvent<HTMLFormElement>) => void
  verification: ParkingVerification | null
  parkingAccess: ParkingAccess[]
}) {
  return (
    <section className="workspace parking-grid">
      <form className="panel" onSubmit={assignVehicle}>
        <h2>Assign vehicle</h2>
        <label>
          Reservation
          <select
            value={selectedReservationId}
            onChange={(event) => {
              setSelectedReservationId(event.target.value)
              if (event.target.value) void loadParkingAccess(event.target.value)
            }}
            required
          >
            <option value="">Select reservation</option>
            {activeReservations.map((reservation) => (
              <option key={reservation.id} value={reservation.id}>
                {reservation.guestLastName}, room {reservation.roomNumber || reservation.roomId.slice(0, 8)}
              </option>
            ))}
          </select>
        </label>
        <label>
          Registration number
          <input
            value={registrationNumber}
            onChange={(event) => setRegistrationNumber(event.target.value)}
            placeholder="AA 7777 BB"
            required
          />
        </label>
        <button type="submit" disabled={!token || isLoading || !selectedReservationId}>
          Assign access
        </button>
      </form>

      <form className="panel" onSubmit={verifyParkingAccess}>
        <h2>Gate verification</h2>
        <label>
          Registration number
          <input
            value={verifyRegistrationNumber}
            onChange={(event) => setVerifyRegistrationNumber(event.target.value)}
            placeholder="AA7777BB"
            required
          />
        </label>
        <button type="submit" disabled={!token || isLoading}>
          Check access
        </button>
        {verification && (
          <div className={`result ${verification.hasValidAccess ? 'allowed' : 'denied'}`}>
            <strong>{verification.registrationNumber}</strong>
            <span>{verification.verificationStatus}</span>
          </div>
        )}
      </form>

      <section className="panel wide">
        <div className="panel-heading">
          <h2>Parking access</h2>
          <span>{parkingAccess.length} vehicles</span>
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Registration</th>
                <th>Access from</th>
                <th>Access to</th>
                <th>Active</th>
                <th>Valid now</th>
              </tr>
            </thead>
            <tbody>
              {parkingAccess.map((access) => (
                <tr key={access.vehicleId}>
                  <td>{access.registrationNumber}</td>
                  <td>{new Date(access.parkingAccessFrom).toLocaleString()}</td>
                  <td>{new Date(access.parkingAccessTo).toLocaleString()}</td>
                  <td>{access.isActive ? 'Yes' : 'No'}</td>
                  <td>{access.hasValidAccessNow ? 'Allowed' : 'Denied'}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {parkingAccess.length === 0 && <p className="empty">No parking access selected</p>}
        </div>
      </section>
    </section>
  )
}

function ReservationModal({
  reservation,
  rooms,
  vehicles,
  setReservation,
  onClose,
  onSave,
}: {
  reservation: Reservation
  rooms: Room[]
  vehicles: VehicleRecord[]
  setReservation: (reservation: Reservation) => void
  onClose: () => void
  onSave: (event: FormEvent<HTMLFormElement>) => void
}) {
  return (
    <section className="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="reservation-title">
      <form className="modal-card" onSubmit={onSave}>
        <div className="modal-heading">
          <div>
            <p className="eyebrow">Reservation details</p>
            <h2 id="reservation-title">
              {reservation.guestFirstName} {reservation.guestLastName}
            </h2>
          </div>
          <button type="button" className="ghost" onClick={onClose}>
            Close
          </button>
        </div>

        <div className="split">
          <label>
            First name
            <input
              value={reservation.guestFirstName}
              onChange={(event) => setReservation({ ...reservation, guestFirstName: event.target.value })}
              required
            />
          </label>
          <label>
            Last name
            <input
              value={reservation.guestLastName}
              onChange={(event) => setReservation({ ...reservation, guestLastName: event.target.value })}
              required
            />
          </label>
        </div>
        <label>
          Email
          <input
            type="email"
            value={reservation.email}
            onChange={(event) => setReservation({ ...reservation, email: event.target.value })}
            required
          />
        </label>
        <div className="split">
          <label>
            Check-in
            <input
              type="date"
              value={normalizeDate(reservation.checkIn)}
              onChange={(event) => setReservation({ ...reservation, checkIn: event.target.value })}
              required
            />
          </label>
          <label>
            Check-out
            <input
              type="date"
              value={normalizeDate(reservation.checkOut)}
              onChange={(event) => setReservation({ ...reservation, checkOut: event.target.value })}
              required
            />
          </label>
        </div>
        <div className="split">
          <label>
            Room
            <select
              value={reservation.roomId}
              onChange={(event) => {
                const room = rooms.find((item) => item.id === event.target.value)
                setReservation({
                  ...reservation,
                  roomId: event.target.value,
                  roomNumber: room?.number ?? reservation.roomNumber,
                })
              }}
              required
            >
              {rooms.map((room) => (
                <option key={room.id} value={room.id}>
                  {room.number} - {room.type}
                </option>
              ))}
            </select>
          </label>
          <label>
            Status
            <select
              value={reservation.status}
              onChange={(event) => setReservation({ ...reservation, status: event.target.value as ReservationStatus })}
            >
              {reservationStatuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div className="detail-box">
          <strong>Vehicles</strong>
          <span>{vehicles.map((vehicle) => vehicle.registrationNumber).join(', ') || 'No linked vehicles'}</span>
        </div>

        <button type="submit">Save reservation</button>
      </form>
    </section>
  )
}

function GuestModal({
  guest,
  setGuest,
  onClose,
  onSave,
  onHistory,
}: {
  guest: Guest
  setGuest: (guest: Guest) => void
  onClose: () => void
  onSave: (event: FormEvent<HTMLFormElement>) => void
  onHistory: () => void
}) {
  return (
    <section className="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="guest-title">
      <form className="modal-card guest-modal" onSubmit={onSave}>
        <div className="modal-heading">
          <div>
            <p className="eyebrow">Guest profile</p>
            <h2 id="guest-title">
              {guest.firstName} {guest.lastName}
            </h2>
          </div>
          <button type="button" className="ghost" onClick={onClose}>
            Close
          </button>
        </div>

        <GuestFields guest={guest} onChange={setGuest} showAddressToggle />

        <div className="modal-actions">
          <button type="submit">Save guest</button>
          <button type="button" onClick={onHistory}>
            Reservation history
          </button>
        </div>
      </form>
    </section>
  )
}

function HistoryModal({
  guest,
  reservations,
  onClose,
  onOpenReservation,
}: {
  guest: Guest
  reservations: Reservation[]
  onClose: () => void
  onOpenReservation: (reservation: Reservation) => void
}) {
  return (
    <section className="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="history-title">
      <section className="modal-card">
        <div className="modal-heading">
          <div>
            <p className="eyebrow">Guest reservation history</p>
            <h2 id="history-title">
              {guest.firstName} {guest.lastName}
            </h2>
          </div>
          <button type="button" className="ghost" onClick={onClose}>
            Close
          </button>
        </div>
        <div className="card-list">
          {reservations.map((reservation) => (
            <button
              key={reservation.id}
              className="record-card"
              type="button"
              onClick={() => onOpenReservation(reservation)}
            >
              <strong>
                Room {reservation.roomNumber || reservation.roomId.slice(0, 8)} · {reservation.status}
              </strong>
              <span>
                {normalizeDate(reservation.checkIn)} - {normalizeDate(reservation.checkOut)}
              </span>
              <small>{reservation.vehicleRegistrationNumbers.join(', ') || 'No vehicle'}</small>
            </button>
          ))}
          {reservations.length === 0 && <p className="empty">No reservations for this guest yet</p>}
        </div>
      </section>
    </section>
  )
}

export default App
