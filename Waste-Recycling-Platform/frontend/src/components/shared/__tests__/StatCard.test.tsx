import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { StatCard } from '../StatCard'

describe('StatCard', () => {
  it('renders label and value', () => {
    render(<StatCard label="Total Tasks" value={42} />)
    expect(screen.getByText('Total Tasks')).toBeInTheDocument()
    expect(screen.getByText('42')).toBeInTheDocument()
  })

  it('renders string value', () => {
    render(<StatCard label="Status" value="Active" />)
    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('renders unit when provided', () => {
    render(<StatCard label="Weight" value={100} unit="kg" />)
    expect(screen.getByText('kg')).toBeInTheDocument()
  })

  it('does not render unit when not provided', () => {
    render(<StatCard label="Count" value={5} />)
    expect(screen.queryByText('kg')).not.toBeInTheDocument()
  })

  it('renders icon when provided', () => {
    render(<StatCard label="Tasks" value={10} icon={<span data-testid="icon">📋</span>} />)
    expect(screen.getByTestId('icon')).toBeInTheDocument()
  })

  it('renders trend up with arrow', () => {
    render(<StatCard label="Score" value={95} trend="up" trendValue="+5%" />)
    expect(screen.getByText(/\+5%/)).toBeInTheDocument()
    expect(screen.getByText(/↑/)).toBeInTheDocument()
  })

  it('renders trend down with arrow', () => {
    render(<StatCard label="Score" value={80} trend="down" trendValue="-3%" />)
    expect(screen.getByText(/↓/)).toBeInTheDocument()
  })

  it('renders trend neutral with arrow', () => {
    render(<StatCard label="Score" value={80} trend="neutral" trendValue="0%" />)
    expect(screen.getByText(/→/)).toBeInTheDocument()
  })

  it('does not render trend when not provided', () => {
    render(<StatCard label="Count" value={5} />)
    expect(screen.queryByText('↑')).not.toBeInTheDocument()
    expect(screen.queryByText('↓')).not.toBeInTheDocument()
  })

  it('applies amber color class by default', () => {
    const { container } = render(<StatCard label="Test" value={1} />)
    expect(container.firstElementChild?.className).toContain('border-amber-200')
  })

  it('applies green color class', () => {
    const { container } = render(<StatCard label="Test" value={1} color="green" />)
    expect(container.firstElementChild?.className).toContain('border-green-200')
  })

  it('applies blue color class', () => {
    const { container } = render(<StatCard label="Test" value={1} color="blue" />)
    expect(container.firstElementChild?.className).toContain('border-blue-200')
  })

  it('applies red color class', () => {
    const { container } = render(<StatCard label="Test" value={1} color="red" />)
    expect(container.firstElementChild?.className).toContain('border-red-200')
  })

  it('applies custom className', () => {
    const { container } = render(<StatCard label="Test" value={1} className="my-class" />)
    expect(container.firstElementChild?.className).toContain('my-class')
  })
})
