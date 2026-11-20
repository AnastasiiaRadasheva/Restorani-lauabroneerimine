import { useEffect, useRef } from 'react'
import { API_URL } from './api'
import { useAuth } from './auth'

/**
 * Püsiühendus API-ga: ws://localhost:5000/ws/tables?access_token=...
 * onMessage kutsutakse iga serveri sõnumiga. Tagastab funktsiooni send(),
 * millega saab serverile sõnumi saata. Katkestuse korral ühendutakse uuesti.
 */
export function useTableSocket(onMessage) {
  const { token } = useAuth()
  const handler = useRef(onMessage)
  const socket = useRef(null)

  useEffect(() => {
    handler.current = onMessage
  })

  useEffect(() => {
    if (!token) return undefined
    let closed = false
    let retry

    function connect() {
      const url = `${API_URL.replace(/^http/, 'ws')}/ws/tables?access_token=${encodeURIComponent(token)}`
      const ws = new WebSocket(url)
      socket.current = ws
      ws.onmessage = (e) => {
        try {
          handler.current(JSON.parse(e.data))
        } catch {
          /* vigane sõnum – ignoreerime */
        }
      }
      ws.onclose = () => {
        if (!closed) retry = setTimeout(connect, 3000)
      }
    }

    connect()
    return () => {
      closed = true
      clearTimeout(retry)
      socket.current?.close()
    }
  }, [token])

  return function send(message) {
    if (socket.current?.readyState === WebSocket.OPEN) socket.current.send(JSON.stringify(message))
  }
}
