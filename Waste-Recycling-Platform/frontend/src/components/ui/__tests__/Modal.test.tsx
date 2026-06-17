import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Modal } from '../Modal'

describe('Modal', () => {
  const defaultProps = {
    isOpen: true,
    onClose: vi.fn(),
    title: 'Test Modal',
    children: <p>Modal content</p>,
  }

  beforeEach(() => {
    vi.clearAllMocks()
    document.body.style.overflow = ''
  })

  it('renders with correct ARIA attributes when open', () => {
    render(<Modal {...defaultProps} />)
    const dialog = screen.getByRole('dialog')
    expect(dialog).toBeInTheDocument()
    expect(dialog).toHaveAttribute('aria-modal', 'true')
    expect(dialog).toHaveAttribute('aria-labelledby', 'modal-title')
  })

  it('does not render when isOpen is false', () => {
    render(<Modal {...defaultProps} isOpen={false} />)
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('calls onClose when Escape key is pressed on dialog', () => {
    const onClose = vi.fn()
    render(<Modal {...defaultProps} onClose={onClose} />)
    const dialog = screen.getByRole('dialog')
    fireEvent.keyDown(dialog, { key: 'Escape' })
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('calls onClose when Escape key is pressed on backdrop', () => {
    const onClose = vi.fn()
    render(<Modal {...defaultProps} onClose={onClose} />)
    const presentations = screen.getAllByRole('presentation')
    fireEvent.keyDown(presentations[0], { key: 'Escape' })
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('renders title correctly', () => {
    render(<Modal {...defaultProps} />)
    expect(screen.getByText('Test Modal')).toBeInTheDocument()
  })

  it('renders children content', () => {
    render(<Modal {...defaultProps} />)
    expect(screen.getByText('Modal content')).toBeInTheDocument()
  })

  it('renders confirm button when onConfirm is provided', () => {
    const onConfirm = vi.fn()
    render(<Modal {...defaultProps} onConfirm={onConfirm} confirmText="OK" />)
    const confirmBtn = screen.getByText('OK')
    fireEvent.click(confirmBtn)
    expect(onConfirm).toHaveBeenCalledTimes(1)
  })

  it('does not render confirm button when onConfirm is not provided', () => {
    render(<Modal {...defaultProps} confirmText="OK" />)
    expect(screen.queryByText('OK')).not.toBeInTheDocument()
  })

  it('calls onClose when close (X) button is clicked', () => {
    const onClose = vi.fn()
    render(<Modal {...defaultProps} onClose={onClose} />)
    // The X SVG button is next to the title
    const closeBtn = screen.getByRole('button', { name: '' }) 
    fireEvent.click(closeBtn)
    expect(onClose).toHaveBeenCalled()
  })

  it('calls onClose when backdrop (presentation div) is clicked', () => {
    const onClose = vi.fn()
    render(<Modal {...defaultProps} onClose={onClose} />)
    const presentations = screen.getAllByRole('presentation')
    fireEvent.click(presentations[0])
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('renders default cancel button text', () => {
    render(<Modal {...defaultProps} />)
    expect(screen.getByText('Cancel')).toBeInTheDocument()
  })

  it('renders custom cancel button text', () => {
    render(<Modal {...defaultProps} cancelText="Đóng" />)
    expect(screen.getByText('Đóng')).toBeInTheDocument()
  })

  it('applies sm size class', () => {
    render(<Modal {...defaultProps} size="sm" />)
    const dialog = screen.getByRole('dialog')
    expect(dialog.className).toContain('w-96')
  })

  it('applies lg size class', () => {
    render(<Modal {...defaultProps} size="lg" />)
    const dialog = screen.getByRole('dialog')
    expect(dialog.className).toContain('w-[700px]')
  })

  it('applies md size class by default', () => {
    render(<Modal {...defaultProps} />)
    const dialog = screen.getByRole('dialog')
    expect(dialog.className).toContain('w-[500px]')
  })

  it('sets body overflow to hidden when open', () => {
    render(<Modal {...defaultProps} />)
    expect(document.body.style.overflow).toBe('hidden')
  })
})
