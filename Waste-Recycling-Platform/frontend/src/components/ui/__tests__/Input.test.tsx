import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Input } from '../Input'

describe('Input', () => {
  it('renders with label and proper htmlFor association', () => {
    render(<Input label="Email" type="email" />)
    
    const label = screen.getByText('Email')
    expect(label).toBeInTheDocument()
    expect(label.tagName).toBe('LABEL')
    expect(label).toHaveAttribute('for', 'email')
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('id', 'email')
  })

  it('renders with custom id overriding auto-generated', () => {
    render(<Input id="custom-email" label="Email" type="email" />)
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('id', 'custom-email')
  })

  it('renders without label', () => {
    render(<Input placeholder="Enter text..." />)
    expect(screen.queryByRole('label')).not.toBeInTheDocument()
    expect(screen.getByPlaceholderText('Enter text...')).toBeInTheDocument()
  })

  it('displays error message and styling', () => {
    render(<Input label="Name" error="Required" />)
    expect(screen.getByText('Required')).toBeInTheDocument()
  })

  it('displays helper text when no error', () => {
    render(<Input label="Name" helperText="Enter your full name" />)
    expect(screen.getByText('Enter your full name')).toBeInTheDocument()
  })

  it('does not show helper text when error is present', () => {
    render(<Input label="Name" error="Required" helperText="Enter your full name" />)
    expect(screen.getByText('Required')).toBeInTheDocument()
    expect(screen.queryByText('Enter your full name')).not.toBeInTheDocument()
  })

  it('renders start icon', () => {
    render(<Input startIcon={<span data-testid="start-icon">🔍</span>} />)
    expect(screen.getByTestId('start-icon')).toBeInTheDocument()
  })

  it('renders end icon', () => {
    render(<Input endIcon={<span data-testid="end-icon">✕</span>} />)
    expect(screen.getByTestId('end-icon')).toBeInTheDocument()
  })

  it('forwards onChange handler', () => {
    const onChange = vi.fn()
    render(<Input onChange={onChange} />)
    
    fireEvent.change(screen.getByRole('textbox'), { target: { value: 'test' } })
    expect(onChange).toHaveBeenCalled()
  })
})
