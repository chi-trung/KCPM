import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { UserProfileMenu } from '../UserProfileMenu'

// Mock next/link
vi.mock('next/link', () => ({
  default: ({ children, href, onClick }: any) => (
    <a
      href={href}
      onClick={(event) => {
        event.preventDefault()
        onClick?.(event)
      }}
    >
      {children}
    </a>
  )
}))

// Mock next/navigation
const mockPush = vi.fn()
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

// Mock lucide-react icons
vi.mock('lucide-react', () => ({
  User: () => <svg data-testid="user-icon" />,
  Settings: () => <svg data-testid="settings-icon" />,
  LogOut: () => <svg data-testid="logout-icon" />,
  ChevronDown: () => <svg data-testid="chevron-icon" />,
}))

// Mock AuthContext
const mockLogout = vi.fn()
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}))

import { useAuth } from '@/contexts/AuthContext'

const mockUser = {
  fullName: 'Nguyễn Văn A',
  email: 'user@example.com',
  role: 'citizen',
}

describe('UserProfileMenu', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders nothing when user is not authenticated', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: null,
      isAuthenticated: false,
      logout: mockLogout,
    } as any)
    const { container } = render(<UserProfileMenu />)
    expect(container.firstChild).toBeNull()
  })

  it('renders nothing when user is null even if isAuthenticated', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: null,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    const { container } = render(<UserProfileMenu />)
    expect(container.firstChild).toBeNull()
  })

  it('renders user name when authenticated', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    expect(screen.getByText('Nguyễn Văn A')).toBeInTheDocument()
  })

  it('opens dropdown when toggle button is clicked', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    // Dropdown should not be visible initially
    expect(screen.queryByText('Đăng xuất')).not.toBeInTheDocument()
    // Click to open
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('Đăng xuất')).toBeInTheDocument()
  })

  it('closes dropdown when button clicked again', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    const toggleBtn = screen.getByRole('button')
    fireEvent.click(toggleBtn) // open
    expect(screen.getByText('Đăng xuất')).toBeInTheDocument()
    fireEvent.click(toggleBtn) // close
    expect(screen.queryByText('Đăng xuất')).not.toBeInTheDocument()
  })

  it('shows dropdown menu items when open', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('Thông tin tài khoản')).toBeInTheDocument()
    expect(screen.getByText('Cài đặt')).toBeInTheDocument()
    expect(screen.getByText('Đăng xuất')).toBeInTheDocument()
  })

  it('shows user email in dropdown', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('user@example.com')).toBeInTheDocument()
  })

  it('shows "Công dân" for citizen role', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { ...mockUser, role: 'citizen' },
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('Công dân')).toBeInTheDocument()
  })

  it('shows "Tài xế" for collector role', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { ...mockUser, role: 'collector' },
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('Tài xế')).toBeInTheDocument()
  })

  it('shows "Doanh nghiệp" for enterprise role', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { ...mockUser, role: 'enterprise' },
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('Doanh nghiệp')).toBeInTheDocument()
  })

  it('shows "Quản trị viên" for admin role', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { ...mockUser, role: 'admin' },
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button'))
    expect(screen.getByText('Quản trị viên')).toBeInTheDocument()
  })

  it('calls logout when Đăng xuất button is clicked', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button')) // open dropdown
    fireEvent.click(screen.getByText('Đăng xuất'))
    expect(mockLogout).toHaveBeenCalledTimes(1)
  })

  it('closes dropdown when a link is clicked', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      isAuthenticated: true,
      logout: mockLogout,
    } as any)
    render(<UserProfileMenu />)
    fireEvent.click(screen.getByRole('button')) // open
    expect(screen.getByText('Cài đặt')).toBeInTheDocument()
    fireEvent.click(screen.getByText('Cài đặt'))
    expect(screen.queryByText('Đăng xuất')).not.toBeInTheDocument()
  })
})
