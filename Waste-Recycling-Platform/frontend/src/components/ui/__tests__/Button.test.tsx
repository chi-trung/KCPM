import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from '../Button'

describe('Button', () => {
  it('renders with children text', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByText('Click me')).toBeInTheDocument()
  })

  it('calls onClick handler when clicked', () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Click</Button>)
    
    fireEvent.click(screen.getByText('Click'))
    expect(onClick).toHaveBeenCalledTimes(1)
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled</Button>)
    expect(screen.getByText('Disabled').closest('button')).toBeDisabled()
  })

  it('is disabled when isLoading is true', () => {
    render(<Button isLoading>Loading</Button>)
    expect(screen.getByText('Loading').closest('button')).toBeDisabled()
  })

  it('renders loading spinner when isLoading', () => {
    render(<Button isLoading>Save</Button>)
    const button = screen.getByText('Save').closest('button')!
    const spinner = button.querySelector('svg.animate-spin')
    expect(spinner).toBeInTheDocument()
  })

  it('applies variant styles', () => {
    const { rerender } = render(<Button variant="primary">Primary</Button>)
    let button = screen.getByText('Primary').closest('button')!
    expect(button.className).toContain('bg-amber-600')

    rerender(<Button variant="danger">Danger</Button>)
    button = screen.getByText('Danger').closest('button')!
    expect(button.className).toContain('bg-red-600')
  })

  it('applies size styles', () => {
    render(<Button size="sm">Small</Button>)
    const button = screen.getByText('Small').closest('button')!
    expect(button.className).toContain('text-sm')
  })
})
