import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import React from 'react'
import { ConfirmationModal, useConfirmation } from '../ConfirmationModal'

// Mock Portal to render children directly in the document
vi.mock('../Portal', () => ({
  Portal: ({ children }: { children: React.ReactNode }) => <>{children}</>
}))

// Mock lucide-react icons
vi.mock('lucide-react', () => ({
  AlertTriangle: () => <svg data-testid="alert-triangle" />,
  HelpCircle: () => <svg data-testid="help-circle" />,
}))

const confirmConfig = {
  title: 'Delete Item',
  message: 'Are you sure you want to delete this item?',
  type: 'confirm' as const,
  confirmText: 'Delete',
  cancelText: 'Cancel',
}

const promptConfig = {
  title: 'Enter Reason',
  message: 'Please provide a reason for rejection.',
  type: 'prompt' as const,
  placeholder: 'Enter reason here...',
  confirmText: 'Submit',
  cancelText: 'Cancel',
}

describe('ConfirmationModal', () => {
  it('renders nothing when isOpen is false', () => {
    render(
      <ConfirmationModal
        isOpen={false}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.queryByText('Delete Item')).not.toBeInTheDocument()
  })

  it('renders nothing when config is null', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={null}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('renders title and message for confirm type', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByText('Delete Item')).toBeInTheDocument()
    expect(screen.getByText('Are you sure you want to delete this item?')).toBeInTheDocument()
  })

  it('renders AlertTriangle icon for confirm type', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByTestId('alert-triangle')).toBeInTheDocument()
  })

  it('renders HelpCircle icon for prompt type', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={promptConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByTestId('help-circle')).toBeInTheDocument()
  })

  it('renders confirm and cancel buttons with custom labels', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByRole('button', { name: 'Delete' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
  })

  it('renders default button labels when not provided', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={{ title: 'Test', message: 'Msg', type: 'confirm' }}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByRole('button', { name: 'Xác nhận' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Hủy' })).toBeInTheDocument()
  })

  it('calls onCancel when cancel button clicked', () => {
    const onCancel = vi.fn()
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={onCancel}
      />
    )
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))
    expect(onCancel).toHaveBeenCalledTimes(1)
  })

  it('calls onConfirm with true for confirm type', () => {
    const onConfirm = vi.fn()
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={onConfirm}
        onCancel={vi.fn()}
      />
    )
    fireEvent.click(screen.getByRole('button', { name: 'Delete' }))
    expect(onConfirm).toHaveBeenCalledWith(true)
  })

  it('renders input for prompt type', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={promptConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByPlaceholderText('Enter reason here...')).toBeInTheDocument()
  })

  it('confirm button disabled in prompt mode when input is empty', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={promptConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    expect(screen.getByRole('button', { name: 'Submit' })).toBeDisabled()
  })

  it('confirm button enabled in prompt mode after typing', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={promptConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />
    )
    const input = screen.getByPlaceholderText('Enter reason here...')
    fireEvent.change(input, { target: { value: 'Some reason' } })
    expect(screen.getByRole('button', { name: 'Submit' })).not.toBeDisabled()
  })

  it('calls onConfirm with input value for prompt type', () => {
    const onConfirm = vi.fn()
    render(
      <ConfirmationModal
        isOpen={true}
        config={promptConfig}
        onConfirm={onConfirm}
        onCancel={vi.fn()}
      />
    )
    const input = screen.getByPlaceholderText('Enter reason here...')
    fireEvent.change(input, { target: { value: 'My reason' } })
    fireEvent.click(screen.getByRole('button', { name: 'Submit' }))
    expect(onConfirm).toHaveBeenCalledWith('My reason')
  })

  it('shows loading text when isLoading is true', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        isLoading={true}
      />
    )
    expect(screen.getByRole('button', { name: 'Đang xử lý...' })).toBeInTheDocument()
  })

  it('disables buttons when isLoading is true', () => {
    render(
      <ConfirmationModal
        isOpen={true}
        config={confirmConfig}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        isLoading={true}
      />
    )
    const buttons = screen.getAllByRole('button')
    buttons.forEach(btn => expect(btn).toBeDisabled())
  })
})

describe('useConfirmation hook', () => {
  const TestComponent = () => {
    const { isOpen, config, confirm, prompt, onConfirm, onCancel } = useConfirmation()
    return (
      <div>
        <button data-testid="open-confirm" onClick={() => confirm({ title: 'T', message: 'M', type: 'confirm' })}>
          Open Confirm
        </button>
        <button data-testid="open-prompt" onClick={() => prompt({ title: 'P', message: 'PM' })}>
          Open Prompt
        </button>
        <ConfirmationModal isOpen={isOpen} config={config} onConfirm={onConfirm} onCancel={onCancel} />
      </div>
    )
  }

  it('opens confirm modal when confirm() is called', () => {
    render(<TestComponent />)
    fireEvent.click(screen.getByTestId('open-confirm'))
    expect(screen.getByText('T')).toBeInTheDocument()
    expect(screen.getByText('M')).toBeInTheDocument()
  })

  it('opens prompt modal when prompt() is called', () => {
    render(<TestComponent />)
    fireEvent.click(screen.getByTestId('open-prompt'))
    expect(screen.getByText('P')).toBeInTheDocument()
    expect(screen.getByText('PM')).toBeInTheDocument()
  })

  it('closes modal when onCancel is called', () => {
    render(<TestComponent />)
    fireEvent.click(screen.getByTestId('open-confirm'))
    expect(screen.getByText('T')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Hủy' }))
    expect(screen.queryByText('T')).not.toBeInTheDocument()
  })

  it('closes modal when onConfirm is called', () => {
    render(<TestComponent />)
    fireEvent.click(screen.getByTestId('open-confirm'))
    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận' }))
    expect(screen.queryByText('T')).not.toBeInTheDocument()
  })
})
