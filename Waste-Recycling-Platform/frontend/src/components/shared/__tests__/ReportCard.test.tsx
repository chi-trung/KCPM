import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { ReportCard } from '../ReportCard'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  )
}))

const defaultProps = {
  id: 'report-1',
  title: 'Waste at Park',
  description: 'There is a pile of garbage at the north entrance.',
  location: 'Central Park, District 1',
  wasteType: 'Mixed Waste',
  status: 'pending' as const,
  createdAt: '2026-01-15T10:00:00Z',
}

describe('ReportCard', () => {
  it('renders title and description', () => {
    render(<ReportCard {...defaultProps} />)
    expect(screen.getByText('Waste at Park')).toBeInTheDocument()
    expect(screen.getByText('There is a pile of garbage at the north entrance.')).toBeInTheDocument()
  })

  it('renders location and waste type', () => {
    render(<ReportCard {...defaultProps} />)
    expect(screen.getByText('Central Park, District 1')).toBeInTheDocument()
    expect(screen.getByText('Mixed Waste')).toBeInTheDocument()
  })

  it('renders "Pending" badge for pending status', () => {
    render(<ReportCard {...defaultProps} status="pending" />)
    expect(screen.getByText('Pending')).toBeInTheDocument()
  })

  it('renders "Assigned" badge for assigned status', () => {
    render(<ReportCard {...defaultProps} status="assigned" />)
    expect(screen.getByText('Assigned')).toBeInTheDocument()
  })

  it('renders "Completed" badge for completed status', () => {
    render(<ReportCard {...defaultProps} status="completed" />)
    expect(screen.getByText('Completed')).toBeInTheDocument()
  })

  it('renders "Cancelled" badge for cancelled status', () => {
    render(<ReportCard {...defaultProps} status="cancelled" />)
    expect(screen.getByText('Cancelled')).toBeInTheDocument()
  })

  it('renders points when provided', () => {
    render(<ReportCard {...defaultProps} points={50} />)
    expect(screen.getByText('+50')).toBeInTheDocument()
  })

  it('does not render points section when not provided', () => {
    render(<ReportCard {...defaultProps} />)
    expect(screen.queryByText(/\+\d+/)).not.toBeInTheDocument()
  })

  it('renders image when provided', () => {
    render(<ReportCard {...defaultProps} image="https://example.com/photo.jpg" />)
    const img = screen.getByRole('img')
    expect(img).toHaveAttribute('src', 'https://example.com/photo.jpg')
    expect(img).toHaveAttribute('alt', 'Waste at Park')
  })

  it('does not render image when not provided', () => {
    render(<ReportCard {...defaultProps} />)
    expect(screen.queryByRole('img')).not.toBeInTheDocument()
  })

  it('renders default action button label', () => {
    render(<ReportCard {...defaultProps} />)
    expect(screen.getByRole('button', { name: 'View Details' })).toBeInTheDocument()
  })

  it('renders custom action button label', () => {
    render(<ReportCard {...defaultProps} actionButtonLabel="Assign Now" />)
    expect(screen.getByRole('button', { name: 'Assign Now' })).toBeInTheDocument()
  })

  it('calls onActionClick when button clicked', () => {
    const onActionClick = vi.fn()
    render(<ReportCard {...defaultProps} onActionClick={onActionClick} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onActionClick).toHaveBeenCalledTimes(1)
  })

  it('renders formatted date', () => {
    render(<ReportCard {...defaultProps} createdAt="2026-01-15T10:00:00Z" />)
    // The date is formatted with toLocaleDateString, just check it exists
    const dateEl = screen.getByText(/2026|Jan|1\/15/)
    expect(dateEl).toBeInTheDocument()
  })
})
