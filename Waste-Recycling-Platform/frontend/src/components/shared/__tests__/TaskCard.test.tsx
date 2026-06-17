import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { TaskCard } from '../TaskCard'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  )
}))

const futureDue = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString()
const pastDue = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString()

const defaultProps = {
  id: 'task-1',
  title: 'Collect Waste at Park',
  description: 'Collect and dispose recyclable waste.',
  location: 'Central Park',
  status: 'pending' as const,
  dueDate: futureDue,
}

describe('TaskCard', () => {
  it('renders title and description', () => {
    render(<TaskCard {...defaultProps} />)
    expect(screen.getByText('Collect Waste at Park')).toBeInTheDocument()
    expect(screen.getByText('Collect and dispose recyclable waste.')).toBeInTheDocument()
  })

  it('renders location', () => {
    render(<TaskCard {...defaultProps} />)
    expect(screen.getByText('Central Park')).toBeInTheDocument()
  })

  it('renders "Pending" badge', () => {
    render(<TaskCard {...defaultProps} status="pending" />)
    expect(screen.getByText('Pending')).toBeInTheDocument()
  })

  it('renders "In Progress" badge', () => {
    render(<TaskCard {...defaultProps} status="in_progress" />)
    expect(screen.getByText('In Progress')).toBeInTheDocument()
  })

  it('renders "Completed" badge', () => {
    render(<TaskCard {...defaultProps} status="completed" />)
    expect(screen.getByText('Completed')).toBeInTheDocument()
  })

  it('renders "Cancelled" badge', () => {
    render(<TaskCard {...defaultProps} status="cancelled" />)
    expect(screen.getByText('Cancelled')).toBeInTheDocument()
  })

  it('renders priority badge when provided', () => {
    render(<TaskCard {...defaultProps} priority="high" />)
    expect(screen.getByText('High')).toBeInTheDocument()
  })

  it('renders medium priority badge', () => {
    render(<TaskCard {...defaultProps} priority="medium" />)
    expect(screen.getByText('Medium')).toBeInTheDocument()
  })

  it('renders low priority badge', () => {
    render(<TaskCard {...defaultProps} priority="low" />)
    expect(screen.getByText('Low')).toBeInTheDocument()
  })

  it('does not render priority badge when not provided', () => {
    render(<TaskCard {...defaultProps} />)
    expect(screen.queryByText('High')).not.toBeInTheDocument()
  })

  it('renders assignedTo when provided', () => {
    render(<TaskCard {...defaultProps} assignedTo="Nguyễn Văn A" />)
    expect(screen.getByText('Assigned to: Nguyễn Văn A')).toBeInTheDocument()
  })

  it('does not render assignedTo when not provided', () => {
    render(<TaskCard {...defaultProps} />)
    expect(screen.queryByText(/Assigned to:/)).not.toBeInTheDocument()
  })

  it('renders reward when provided', () => {
    render(<TaskCard {...defaultProps} reward={200} />)
    expect(screen.getByText(/200/)).toBeInTheDocument()
  })

  it('renders acceptedAt date when provided', () => {
    render(<TaskCard {...defaultProps} acceptedAt="2026-01-10T10:00:00Z" />)
    // Should render the Accepted label
    expect(screen.getByText('Accepted')).toBeInTheDocument()
  })

  it('shows overdue indicator for past due date on non-completed task', () => {
    render(<TaskCard {...defaultProps} dueDate={pastDue} status="pending" />)
    expect(screen.getByText(/⚠️/)).toBeInTheDocument()
  })

  it('does not show overdue for completed task even with past due date', () => {
    render(<TaskCard {...defaultProps} dueDate={pastDue} status="completed" />)
    expect(screen.queryByText(/⚠️/)).not.toBeInTheDocument()
  })

  it('renders default action button label', () => {
    render(<TaskCard {...defaultProps} />)
    expect(screen.getByRole('button', { name: 'View Task' })).toBeInTheDocument()
  })

  it('renders custom action button label', () => {
    render(<TaskCard {...defaultProps} actionButtonLabel="Accept Task" />)
    expect(screen.getByRole('button', { name: 'Accept Task' })).toBeInTheDocument()
  })

  it('calls onActionClick when button clicked', () => {
    const onActionClick = vi.fn()
    render(<TaskCard {...defaultProps} onActionClick={onActionClick} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onActionClick).toHaveBeenCalledTimes(1)
  })
})
