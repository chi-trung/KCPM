import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Select } from '../Select'

describe('Select', () => {
  const defaultOptions = [
    { value: 'opt1', label: 'Option 1' },
    { value: 'opt2', label: 'Option 2' },
    { value: 'opt3', label: 'Option 3' },
  ]

  it('renders with label and proper htmlFor association', () => {
    render(
      <Select label="Test Label" options={defaultOptions} value="opt1" onChange={vi.fn()} />
    )
    
    const label = screen.getByText('Test Label')
    expect(label).toBeInTheDocument()
    expect(label.tagName).toBe('LABEL')
    expect(label).toHaveAttribute('for', 'test-label')
    
    const select = screen.getByRole('combobox')
    expect(select).toHaveAttribute('id', 'test-label')
  })

  it('renders with custom id', () => {
    render(
      <Select id="custom-id" label="My Label" options={defaultOptions} value="opt1" onChange={vi.fn()} />
    )
    
    const select = screen.getByRole('combobox')
    expect(select).toHaveAttribute('id', 'custom-id')
  })

  it('renders all options', () => {
    render(
      <Select options={defaultOptions} value="opt1" onChange={vi.fn()} />
    )
    
    const options = screen.getAllByRole('option')
    expect(options).toHaveLength(3)
  })

  it('renders placeholder when provided', () => {
    render(
      <Select options={defaultOptions} value="" onChange={vi.fn()} placeholder="Select..." />
    )
    
    expect(screen.getByText('Select...')).toBeInTheDocument()
  })

  it('calls onChange when value changes', () => {
    const onChange = vi.fn()
    render(
      <Select options={defaultOptions} value="opt1" onChange={onChange} />
    )
    
    fireEvent.change(screen.getByRole('combobox'), { target: { value: 'opt2' } })
    expect(onChange).toHaveBeenCalled()
  })

  it('shows error styling and message', () => {
    render(
      <Select options={defaultOptions} value="opt1" onChange={vi.fn()} error="Required field" />
    )
    
    expect(screen.getByText('Required field')).toBeInTheDocument()
  })
})
