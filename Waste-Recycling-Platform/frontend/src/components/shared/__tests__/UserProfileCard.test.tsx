import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import React from 'react'
import { UserProfileCard } from '../UserProfileCard'

const defaultStats = [
  { label: 'Reports', value: 25 },
  { label: 'Points', value: 500 },
  { label: 'Tasks', value: 10 },
]

const defaultProps = {
  name: 'Nguyễn Văn An',
  email: 'an@example.com',
  role: 'citizen' as const,
  joinedDate: '2025-01-15T00:00:00Z',
  stats: defaultStats,
}

describe('UserProfileCard', () => {
  it('renders user name', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.getAllByText('Nguyễn Văn An').length).toBeGreaterThan(0)
  })

  it('renders user email', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.getByText('an@example.com')).toBeInTheDocument()
  })

  it('renders "Citizen" badge for citizen role', () => {
    render(<UserProfileCard {...defaultProps} role="citizen" />)
    expect(screen.getByText('Citizen')).toBeInTheDocument()
  })

  it('renders "Collector" badge for collector role', () => {
    render(<UserProfileCard {...defaultProps} role="collector" />)
    expect(screen.getByText('Collector')).toBeInTheDocument()
  })

  it('renders "Enterprise" badge for enterprise role', () => {
    render(<UserProfileCard {...defaultProps} role="enterprise" />)
    expect(screen.getByText('Enterprise')).toBeInTheDocument()
  })

  it('renders "Admin" badge for admin role', () => {
    render(<UserProfileCard {...defaultProps} role="admin" />)
    expect(screen.getByText('Admin')).toBeInTheDocument()
  })

  it('renders phone when provided', () => {
    render(<UserProfileCard {...defaultProps} phone="0901234567" />)
    expect(screen.getByText('0901234567')).toBeInTheDocument()
  })

  it('does not render phone when not provided', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.queryByText(/Phone:/)).not.toBeInTheDocument()
  })

  it('renders stats', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.getByText('Reports')).toBeInTheDocument()
    expect(screen.getByText('25')).toBeInTheDocument()
    expect(screen.getByText('Points')).toBeInTheDocument()
    expect(screen.getByText('500')).toBeInTheDocument()
  })

  it('renders verified checkmark when verified', () => {
    render(<UserProfileCard {...defaultProps} verified />)
    expect(screen.getByText('✓')).toBeInTheDocument()
  })

  it('does not render verified checkmark when not verified', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.queryByText('✓')).not.toBeInTheDocument()
  })

  it('renders avatar image when provided', () => {
    render(<UserProfileCard {...defaultProps} avatar="https://example.com/avatar.jpg" />)
    const img = screen.getByRole('img')
    expect(img).toHaveAttribute('src', 'https://example.com/avatar.jpg')
    expect(img).toHaveAttribute('alt', 'Nguyễn Văn An')
  })

  it('renders initials when avatar not provided', () => {
    render(<UserProfileCard {...defaultProps} name="Nguyen Van An" />)
    // Initials would be NVA
    expect(screen.getByText('NVA')).toBeInTheDocument()
  })

  it('renders badges/achievements when provided', () => {
    render(<UserProfileCard {...defaultProps} badges={['Green Champion', 'Top Reporter']} />)
    expect(screen.getByText('Achievements')).toBeInTheDocument()
    expect(screen.getByText('Green Champion')).toBeInTheDocument()
    expect(screen.getByText('Top Reporter')).toBeInTheDocument()
  })

  it('does not render achievements section when badges is empty', () => {
    render(<UserProfileCard {...defaultProps} badges={[]} />)
    expect(screen.queryByText('Achievements')).not.toBeInTheDocument()
  })

  it('does not render achievements section when badges not provided', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.queryByText('Achievements')).not.toBeInTheDocument()
  })

  it('renders joined date', () => {
    render(<UserProfileCard {...defaultProps} />)
    expect(screen.getByText(/Joined:/)).toBeInTheDocument()
  })
})
