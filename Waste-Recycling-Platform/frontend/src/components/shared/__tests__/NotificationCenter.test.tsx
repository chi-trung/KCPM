import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react'
import React from 'react'
import { NotificationCenter } from '../NotificationCenter'

// Mocks
const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush
  })
}))

const mockAuth = {
  token: 'fake-token',
  user: { id: 'user-123' }
}
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: () => mockAuth
}))

let signalRCallback: any = null
vi.mock('@/hooks/useSignalR', () => ({
  useSignalR: (config: any) => {
    signalRCallback = config.onNewNotification
  }
}))

vi.mock('@/lib/api/config', () => ({
  API_CONFIG: {
    SERVER_URL: 'http://localhost:5000'
  }
}))

describe('NotificationCenter', () => {
  const mockNotificationsResponse = {
    data: [
      {
        id: 'notif-1',
        title: 'New complaint',
        message: 'A citizen created a report',
        type: 'info',
        createdAt: '2026-06-17T08:00:00.000Z',
        status: 'Unread',
        relatedEntityType: 'complaint',
        relatedEntityId: 'complaint-1'
      }
    ]
  }

  beforeEach(() => {
    vi.resetAllMocks()
    signalRCallback = null
    
    // Mock global fetch
    global.fetch = vi.fn().mockImplementation((url) => {
      if (url.includes('/api/notifications/notif-1/read') || url.includes('/api/notifications/mark-all-read')) {
        return Promise.resolve({
          ok: true,
          json: () => Promise.resolve({ success: true })
        } as Response)
      }
      return Promise.resolve({
        ok: true,
        json: () => Promise.resolve(mockNotificationsResponse)
      } as Response)
    })
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('renders notification badge count and dropdown trigger', async () => {
    render(<NotificationCenter />)
    
    // Initial fetch of notifications should show unread count 1
    await waitFor(() => {
      expect(screen.getByText('1')).toBeInTheDocument()
    })
  })

  it('toggles dropdown when badge is clicked', async () => {
    render(<NotificationCenter />)
    
    const trigger = screen.getByRole('button')
    
    // Initially dropdown content shouldn't be visible
    expect(screen.queryByText('Thông báo')).not.toBeInTheDocument()
    
    // Click trigger to open dropdown
    await act(async () => {
      fireEvent.click(trigger)
    })
    
    await waitFor(() => {
      expect(screen.getByText('Thông báo')).toBeInTheDocument()
      expect(screen.getByText('New complaint')).toBeInTheDocument()
      expect(screen.getByText('A citizen created a report')).toBeInTheDocument()
    })
    
    // Click trigger again to close
    await act(async () => {
      fireEvent.click(trigger)
    })
    await waitFor(() => {
      expect(screen.queryByText('Thông báo')).not.toBeInTheDocument()
    })
  })

  it('marks notification as read and navigates when notification is clicked', async () => {
    render(<NotificationCenter />)
    
    const trigger = screen.getByRole('button')
    await act(async () => {
      fireEvent.click(trigger)
    })
    
    // Wait for the notification button to be rendered
    let notificationBtn: HTMLElement | null = null
    await waitFor(() => {
      notificationBtn = screen.getByText('New complaint').closest('button')
      expect(notificationBtn).not.toBeNull()
    })

    // Click on the notification button
    await act(async () => {
      fireEvent.click(notificationBtn!)
    })
    
    // Wait for the navigation to complete
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/citizen/complaints')
    })
    
    // It should call fetch to mark it as read
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/notifications/notif-1/read'),
      expect.objectContaining({ method: 'PUT' })
    )
  })

  it('allows marking all notifications as read', async () => {
    render(<NotificationCenter />)
    
    const trigger = screen.getByRole('button')
    await act(async () => {
      fireEvent.click(trigger)
    })
    
    await waitFor(() => {
      expect(screen.getByText('Đánh dấu tất cả đã đọc')).toBeInTheDocument()
    })
    
    await act(async () => {
      fireEvent.click(screen.getByText('Đánh dấu tất cả đã đọc'))
    })
    
    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/notifications/mark-all-read'),
        expect.objectContaining({ method: 'PUT' })
      )
    })
  })

  it('receives real-time notifications via SignalR', async () => {
    render(<NotificationCenter />)
    
    // Trigger SignalR callback manually
    expect(signalRCallback).not.toBeNull()
    
    await waitFor(() => {
      expect(screen.getByText('1')).toBeInTheDocument()
    })

    // Simulate incoming new notification from SignalR
    await act(async () => {
      signalRCallback({
        id: 'notif-2',
        title: 'New Task',
        message: 'A new task assigned to you',
        type: 'warning',
        createdAt: '2026-06-17T09:00:00.000Z'
      })
    })

    // Unread count should go up to 2
    await waitFor(() => {
      expect(screen.getByText('2')).toBeInTheDocument()
    })
  })

  it('closes dropdown when clicking outside', async () => {
    render(
      <div>
        <div data-testid="outside-element">Outside</div>
        <NotificationCenter />
      </div>
    )

    const trigger = screen.getByRole('button')
    await act(async () => {
      fireEvent.click(trigger)
    })

    await waitFor(() => {
      expect(screen.getByText('Thông báo')).toBeInTheDocument()
    })

    // Click outside
    await act(async () => {
      fireEvent.mouseDown(screen.getByTestId('outside-element'))
    })

    await waitFor(() => {
      expect(screen.queryByText('Thông báo')).not.toBeInTheDocument()
    })
  })
})
