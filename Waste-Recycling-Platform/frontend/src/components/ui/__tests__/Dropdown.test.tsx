import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Dropdown } from '../Dropdown'

describe('Dropdown', () => {
  const mockItems = [
    { label: 'Edit', onClick: vi.fn() },
    { label: 'Delete', onClick: vi.fn(), danger: true },
  ]

  it('renders trigger as a button element', () => {
    render(
      <Dropdown trigger={<span>Menu</span>} items={mockItems} />
    )
    
    const trigger = screen.getByRole('button')
    expect(trigger).toBeInTheDocument()
    expect(trigger).toHaveAttribute('type', 'button')
  })

  it('opens menu when trigger is clicked', () => {
    render(
      <Dropdown trigger={<span>Menu</span>} items={mockItems} />
    )
    
    const trigger = screen.getByRole('button')
    fireEvent.click(trigger)
    
    expect(screen.getByText('Edit')).toBeInTheDocument()
    expect(screen.getByText('Delete')).toBeInTheDocument()
  })

  it('closes menu and calls item callback when item is clicked', () => {
    const editFn = vi.fn()
    const items = [
      { label: 'Edit', onClick: editFn },
    ]
    
    render(
      <Dropdown trigger={<span>Menu</span>} items={items} />
    )
    
    fireEvent.click(screen.getByRole('button'))
    fireEvent.click(screen.getByText('Edit'))
    
    expect(editFn).toHaveBeenCalledTimes(1)
  })

  it('renders divider items', () => {
    const items = [
      { label: 'Edit', onClick: vi.fn() },
      { label: '', onClick: vi.fn(), divider: true },
      { label: 'Delete', onClick: vi.fn() },
    ]
    
    render(
      <Dropdown trigger={<span>Menu</span>} items={items} />
    )
    
    fireEvent.click(screen.getByRole('button'))
    // Menu should be open with items
    expect(screen.getByText('Edit')).toBeInTheDocument()
    expect(screen.getByText('Delete')).toBeInTheDocument()
  })
})
