import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Card } from '../Card'

describe('Card', () => {
  it('renders children', () => {
    render(<Card>Card content</Card>)
    expect(screen.getByText('Card content')).toBeInTheDocument()
  })

  it('applies base shadow and background classes', () => {
    const { container } = render(<Card>Content</Card>)
    expect(container.firstElementChild?.className).toContain('bg-white')
    expect(container.firstElementChild?.className).toContain('rounded-lg')
    expect(container.firstElementChild?.className).toContain('shadow-md')
  })

  it('applies hoverable class when hoverable=true', () => {
    const { container } = render(<Card hoverable>Content</Card>)
    expect(container.firstElementChild?.className).toContain('hover:shadow-lg')
  })

  it('does not apply hoverable class by default', () => {
    const { container } = render(<Card>Content</Card>)
    expect(container.firstElementChild?.className).not.toContain('hover:shadow-lg')
  })

  it('applies custom className', () => {
    const { container } = render(<Card className="mt-4">Content</Card>)
    expect(container.firstElementChild?.className).toContain('mt-4')
  })

  it('renders Card.Header subcomponent', () => {
    render(
      <Card>
        <Card.Header>Header Text</Card.Header>
      </Card>
    )
    expect(screen.getByText('Header Text')).toBeInTheDocument()
  })

  it('renders Card.Body subcomponent', () => {
    render(
      <Card>
        <Card.Body>Body Content</Card.Body>
      </Card>
    )
    expect(screen.getByText('Body Content')).toBeInTheDocument()
  })

  it('renders Card.Footer subcomponent', () => {
    render(
      <Card>
        <Card.Footer>Footer Info</Card.Footer>
      </Card>
    )
    expect(screen.getByText('Footer Info')).toBeInTheDocument()
  })

  it('renders all subcomponents together', () => {
    render(
      <Card>
        <Card.Header>Title</Card.Header>
        <Card.Body>Body</Card.Body>
        <Card.Footer>Actions</Card.Footer>
      </Card>
    )
    expect(screen.getByText('Title')).toBeInTheDocument()
    expect(screen.getByText('Body')).toBeInTheDocument()
    expect(screen.getByText('Actions')).toBeInTheDocument()
  })
})
