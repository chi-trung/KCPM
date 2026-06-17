import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Avatar } from '../Avatar'

describe('Avatar', () => {
  it('renders initials when no src provided', () => {
    render(<Avatar initials="JD" />)
    expect(screen.getByText('JD')).toBeInTheDocument()
  })

  it('renders default initials "?" when nothing provided', () => {
    render(<Avatar />)
    expect(screen.getByText('?')).toBeInTheDocument()
  })

  it('renders img element when src is provided', () => {
    render(<Avatar src="https://example.com/avatar.jpg" alt="John" />)
    const img = screen.getByRole('img')
    expect(img).toBeInTheDocument()
    expect(img).toHaveAttribute('src', 'https://example.com/avatar.jpg')
    expect(img).toHaveAttribute('alt', 'John')
  })

  it('does not render initials when src is provided', () => {
    render(<Avatar src="https://example.com/avatar.jpg" initials="JD" />)
    expect(screen.queryByText('JD')).not.toBeInTheDocument()
  })

  it('applies sm size classes', () => {
    const { container } = render(<Avatar size="sm" initials="A" />)
    expect(container.firstElementChild?.className).toContain('w-8')
    expect(container.firstElementChild?.className).toContain('h-8')
  })

  it('applies md size by default', () => {
    const { container } = render(<Avatar initials="A" />)
    expect(container.firstElementChild?.className).toContain('w-10')
    expect(container.firstElementChild?.className).toContain('h-10')
  })

  it('applies lg size', () => {
    const { container } = render(<Avatar size="lg" initials="A" />)
    expect(container.firstElementChild?.className).toContain('w-12')
    expect(container.firstElementChild?.className).toContain('h-12')
  })

  it('applies xl size', () => {
    const { container } = render(<Avatar size="xl" initials="A" />)
    expect(container.firstElementChild?.className).toContain('w-16')
    expect(container.firstElementChild?.className).toContain('h-16')
  })

  it('applies custom className', () => {
    const { container } = render(<Avatar initials="A" className="border-2" />)
    expect(container.firstElementChild?.className).toContain('border-2')
  })

  it('shows amber background for initials avatar', () => {
    const { container } = render(<Avatar initials="AB" />)
    expect(container.firstElementChild?.className).toContain('bg-amber-100')
  })

  it('uses default alt text when not provided', () => {
    render(<Avatar src="https://example.com/a.jpg" />)
    expect(screen.getByRole('img')).toHaveAttribute('alt', 'User avatar')
  })
})
