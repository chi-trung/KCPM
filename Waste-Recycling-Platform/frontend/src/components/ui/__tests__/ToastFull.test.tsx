import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, act, waitFor } from '@testing-library/react'
import React from 'react'
import { ToastContainer, ToastProvider, ToastContext, useToast } from '../Toast'

const mockOnClose = vi.fn()

const sampleToast = {
  id: 'toast-1',
  message: 'Operation successful!',
  type: 'success' as const,
  onClose: mockOnClose,
}

describe('ToastContainer', () => {
  it('renders nothing when toasts is empty', () => {
    const { container } = render(<ToastContainer toasts={[]} onClose={mockOnClose} />)
    expect(container.querySelector('[class*="fixed"]')).toBeInTheDocument()
    expect(screen.queryByText('Operation successful!')).not.toBeInTheDocument()
  })

  it('renders a toast message', () => {
    render(<ToastContainer toasts={[sampleToast]} onClose={mockOnClose} />)
    expect(screen.getByText('Operation successful!')).toBeInTheDocument()
  })

  it('renders success toast with checkmark icon', () => {
    render(<ToastContainer toasts={[{ ...sampleToast, type: 'success' }]} onClose={mockOnClose} />)
    expect(screen.getByText('✓')).toBeInTheDocument()
  })

  it('renders error toast with X icon', () => {
    render(<ToastContainer toasts={[{ ...sampleToast, type: 'error' }]} onClose={mockOnClose} />)
    expect(screen.getByText('✕')).toBeInTheDocument()
  })

  it('renders warning toast with ! icon', () => {
    render(<ToastContainer toasts={[{ ...sampleToast, type: 'warning' }]} onClose={mockOnClose} />)
    expect(screen.getByText('!')).toBeInTheDocument()
  })

  it('renders info toast with ℹ icon', () => {
    render(<ToastContainer toasts={[{ ...sampleToast, type: 'info' }]} onClose={mockOnClose} />)
    expect(screen.getByText('ℹ')).toBeInTheDocument()
  })

  it('defaults to info type when no type provided', () => {
    const toastNoType = { id: 'x', message: 'Hello', onClose: mockOnClose }
    render(<ToastContainer toasts={[toastNoType]} onClose={mockOnClose} />)
    expect(screen.getByText('ℹ')).toBeInTheDocument()
  })

  it('calls onClose when close button is clicked', () => {
    const onClose = vi.fn()
    render(<ToastContainer toasts={[sampleToast]} onClose={onClose} />)
    fireEvent.click(screen.getByText('×'))
    expect(onClose).toHaveBeenCalledWith('toast-1')
  })

  it('renders multiple toasts', () => {
    const toasts = [
      { id: '1', message: 'First toast', type: 'success' as const, onClose: mockOnClose },
      { id: '2', message: 'Second toast', type: 'error' as const, onClose: mockOnClose },
    ]
    render(<ToastContainer toasts={toasts} onClose={mockOnClose} />)
    expect(screen.getByText('First toast')).toBeInTheDocument()
    expect(screen.getByText('Second toast')).toBeInTheDocument()
  })

  it('auto-closes after duration', async () => {
    vi.useFakeTimers()
    const onClose = vi.fn()
    render(<ToastContainer toasts={[{ ...sampleToast, duration: 100, onClose }]} onClose={onClose} />)
    act(() => { vi.advanceTimersByTime(200) })
    expect(onClose).toHaveBeenCalledWith('toast-1')
    vi.useRealTimers()
  })
})

describe('ToastProvider', () => {
  const ToastConsumer = () => {
    const { addToast, removeToast, toasts } = useToast()
    return (
      <div>
        <button data-testid="add-success" onClick={() => addToast('Success msg', 'success')}>
          Add Success
        </button>
        <button data-testid="add-error" onClick={() => addToast('Error msg', 'error')}>
          Add Error
        </button>
        <button data-testid="add-default" onClick={() => addToast('Default msg')}>
          Add Default
        </button>
        <button data-testid="remove" onClick={() => removeToast(toasts[0]?.id || '')}>
          Remove
        </button>
        <span data-testid="count">{toasts.length}</span>
      </div>
    )
  }

  it('provides addToast function', () => {
    render(
      <ToastProvider>
        <ToastConsumer />
      </ToastProvider>
    )
    expect(screen.getByTestId('count')).toHaveTextContent('0')
    fireEvent.click(screen.getByTestId('add-success'))
    expect(screen.getByTestId('count')).toHaveTextContent('1')
    expect(screen.getByText('Success msg')).toBeInTheDocument()
  })

  it('adds error toast with correct type', () => {
    render(
      <ToastProvider>
        <ToastConsumer />
      </ToastProvider>
    )
    fireEvent.click(screen.getByTestId('add-error'))
    expect(screen.getByText('Error msg')).toBeInTheDocument()
    expect(screen.getByText('✕')).toBeInTheDocument()
  })

  it('adds default toast with info type', () => {
    render(
      <ToastProvider>
        <ToastConsumer />
      </ToastProvider>
    )
    fireEvent.click(screen.getByTestId('add-default'))
    expect(screen.getByText('Default msg')).toBeInTheDocument()
  })

  it('removes toast when removeToast is called', () => {
    render(
      <ToastProvider>
        <ToastConsumer />
      </ToastProvider>
    )
    fireEvent.click(screen.getByTestId('add-success'))
    expect(screen.getByTestId('count')).toHaveTextContent('1')
    fireEvent.click(screen.getByTestId('remove'))
    expect(screen.getByTestId('count')).toHaveTextContent('0')
  })

  it('renders children', () => {
    render(
      <ToastProvider>
        <div data-testid="child">Child Content</div>
      </ToastProvider>
    )
    expect(screen.getByTestId('child')).toBeInTheDocument()
  })
})

describe('useToast', () => {
  it('throws error when used outside ToastProvider', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})
    const BadComponent = () => {
      useToast()
      return null
    }
    expect(() => render(<BadComponent />)).toThrow('useToast must be used within ToastProvider')
    consoleSpy.mockRestore()
  })
})
