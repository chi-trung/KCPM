import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { EmptyState } from '../EmptyState'

describe('EmptyState', () => {
  it('renders title', () => {
    render(<EmptyState title="No data found" />)
    expect(screen.getByText('No data found')).toBeInTheDocument()
  })

  it('renders description when provided', () => {
    render(<EmptyState title="Empty" description="Try adding some items first" />)
    expect(screen.getByText('Try adding some items first')).toBeInTheDocument()
  })

  it('does not render description when not provided', () => {
    render(<EmptyState title="Empty" />)
    expect(screen.queryByRole('paragraph')).not.toBeInTheDocument()
  })

  it('renders icon when provided', () => {
    render(<EmptyState title="Empty" icon={<span data-testid="icon">📭</span>} />)
    expect(screen.getByTestId('icon')).toBeInTheDocument()
  })

  it('does not render icon container when not provided', () => {
    const { container } = render(<EmptyState title="Empty" />)
    // icon wrapper div only rendered when icon prop is given
    const iconWrapper = container.querySelector('.mb-4.text-6xl')
    expect(iconWrapper).not.toBeInTheDocument()
  })

  it('renders action when provided', () => {
    render(
      <EmptyState
        title="Empty"
        action={<button>Add Item</button>}
      />
    )
    expect(screen.getByRole('button', { name: 'Add Item' })).toBeInTheDocument()
  })

  it('does not render action when not provided', () => {
    render(<EmptyState title="Empty" />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<EmptyState title="Empty" className="my-class" />)
    expect(container.firstElementChild?.className).toContain('my-class')
  })

  it('renders title as h3', () => {
    render(<EmptyState title="My Title" />)
    const heading = screen.getByRole('heading', { level: 3 })
    expect(heading).toHaveTextContent('My Title')
  })
})
