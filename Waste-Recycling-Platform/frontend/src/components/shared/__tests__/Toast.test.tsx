import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import React from 'react'
import { useToast, ToastContainer, ToastMessage } from '../Toast'

// Mock Portal to render inline
vi.mock('../Portal', () => ({
  Portal: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}))

// Mock lucide-react icons
vi.mock('lucide-react', () => ({
  X: () => <span data-testid="icon-x">X</span>,
  CheckCircle: () => <span data-testid="icon-success">S</span>,
  AlertCircle: () => <span data-testid="icon-error">E</span>,
  Info: () => <span data-testid="icon-info">I</span>,
}))

// Mock crypto.randomUUID to avoid TS errors with UUID template literal type
let uuidCount = 0
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mockCrypto: any = {
  randomUUID: () => `test-uuid-${uuidCount++}`,
}
vi.stubGlobal('crypto', mockCrypto)

describe('Toast Component & Hook', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    uuidCount = 0
  })

  it('useToast hook manages toast messages state and auto-removes after 3s', () => {
    let testHookResult: any = {}

    const TestComponent = () => {
      const result = useToast()
      testHookResult = result
      return (
        <div>
          <button onClick={() => result.success('Success message')}>Success</button>
          <button onClick={() => result.error('Error message')}>Error</button>
          <button onClick={() => result.info('Info message')}>Info</button>
          <button onClick={() => result.removeToast(result.toasts[0]?.id)}>Remove</button>
          <div data-testid="count">{result.toasts.length}</div>
        </div>
      )
    }

    render(<TestComponent />)

    // Add success toast
    fireEvent.click(screen.getByText('Success'))
    expect(testHookResult.toasts).toHaveLength(1)
    expect(testHookResult.toasts[0].type).toBe('success')
    expect(testHookResult.toasts[0].message).toBe('Success message')
    expect(screen.getByTestId('count').textContent).toBe('1')

    // Add error toast
    fireEvent.click(screen.getByText('Error'))
    expect(testHookResult.toasts).toHaveLength(2)

    // Fast-forward 3 seconds
    act(() => {
      vi.advanceTimersByTime(3000)
    })

    // Toasts should be auto-removed
    expect(testHookResult.toasts).toHaveLength(0)
  })

  it('useToast manually removes a toast', () => {
    const TestComponent = () => {
      const result = useToast()
      return (
        <div>
          <button onClick={() => result.success('Msg')}>Add</button>
          <button onClick={() => result.removeToast(result.toasts[0]?.id)}>Remove</button>
          <div data-testid="count">{result.toasts.length}</div>
        </div>
      )
    }

    render(<TestComponent />)
    fireEvent.click(screen.getByText('Add'))
    expect(screen.getByTestId('count').textContent).toBe('1')

    fireEvent.click(screen.getByText('Remove'))
    expect(screen.getByTestId('count').textContent).toBe('0')
  })

  it('ToastContainer renders messages with correct icons and classes', () => {
    const messages: ToastMessage[] = [
      { id: '1', type: 'success', message: 'Task completed' },
      { id: '2', type: 'error', message: 'Something went wrong' },
      { id: '3', type: 'info', message: 'System update' }
    ]
    const onRemove = vi.fn()

    render(<ToastContainer toasts={messages} onRemove={onRemove} />)

    expect(screen.getByText('Task completed')).toBeInTheDocument()
    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
    expect(screen.getByText('System update')).toBeInTheDocument()

    expect(screen.getByTestId('icon-success')).toBeInTheDocument()
    expect(screen.getByTestId('icon-error')).toBeInTheDocument()
    expect(screen.getByTestId('icon-info')).toBeInTheDocument()

    // Test close button
    const closeBtns = screen.getAllByTestId('icon-x')
    fireEvent.click(closeBtns[0].closest('button')!)
    expect(onRemove).toHaveBeenCalledWith('1')
  })
})
