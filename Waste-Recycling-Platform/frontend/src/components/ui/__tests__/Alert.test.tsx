import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Alert } from '../Alert'

describe('Alert', () => {
  it('renders message text', () => {
    render(<Alert message="Something went wrong" />)
    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
  })

  it('renders title when provided', () => {
    render(<Alert message="Details here" title="Error occurred" />)
    expect(screen.getByText('Error occurred')).toBeInTheDocument()
    expect(screen.getByText('Details here')).toBeInTheDocument()
  })

  it('does not render title when not provided', () => {
    render(<Alert message="Only message" />)
    expect(screen.queryByRole('heading')).not.toBeInTheDocument()
  })

  it('renders success variant with correct icon', () => {
    const { container } = render(<Alert variant="success" message="Done!" />)
    expect(container.querySelector('span.text-xl')).toHaveTextContent('✓')
  })

  it('renders error variant with correct icon', () => {
    const { container } = render(<Alert variant="error" message="Failed!" />)
    expect(container.querySelector('span.text-xl')).toHaveTextContent('✕')
  })

  it('renders warning variant with correct icon', () => {
    const { container } = render(<Alert variant="warning" message="Careful!" />)
    expect(container.querySelector('span.text-xl')).toHaveTextContent('!')
  })

  it('renders info variant by default', () => {
    const { container } = render(<Alert message="Info" />)
    expect(container.querySelector('span.text-xl')).toHaveTextContent('ℹ')
  })

  it('shows dismiss button when dismissible and onClose provided', () => {
    const onClose = vi.fn()
    render(<Alert message="Msg" dismissible onClose={onClose} />)
    const closeBtn = screen.getByRole('button')
    expect(closeBtn).toBeInTheDocument()
  })

  it('calls onClose when dismiss button clicked', () => {
    const onClose = vi.fn()
    render(<Alert message="Msg" dismissible onClose={onClose} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('does not show dismiss button when dismissible is false', () => {
    render(<Alert message="Msg" dismissible={false} onClose={vi.fn()} />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('does not show dismiss button when onClose not provided', () => {
    render(<Alert message="Msg" dismissible />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })
})
