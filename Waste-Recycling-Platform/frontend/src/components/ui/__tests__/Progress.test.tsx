import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Progress } from '../Progress'

describe('Progress', () => {
  it('renders without crashing', () => {
    const { container } = render(<Progress value={50} />)
    expect(container.firstChild).toBeInTheDocument()
  })

  it('sets width style to 50% for value=50 max=100', () => {
    const { container } = render(<Progress value={50} max={100} />)
    const bar = container.querySelector('[style*="width"]') as HTMLElement
    expect(bar?.style.width).toBe('50%')
  })

  it('sets width to 100% for value=max', () => {
    const { container } = render(<Progress value={200} max={200} />)
    const bar = container.querySelector('[style*="width"]') as HTMLElement
    expect(bar?.style.width).toBe('100%')
  })

  it('sets width to 0% for value=0', () => {
    const { container } = render(<Progress value={0} max={100} />)
    const bar = container.querySelector('[style*="width"]') as HTMLElement
    expect(bar?.style.width).toBe('0%')
  })

  it('shows label when showLabel is true', () => {
    render(<Progress value={75} showLabel />)
    expect(screen.getByText('75%')).toBeInTheDocument()
  })

  it('does not show label by default', () => {
    render(<Progress value={75} />)
    expect(screen.queryByText('75%')).not.toBeInTheDocument()
  })

  it('applies amber color by default', () => {
    const { container } = render(<Progress value={50} />)
    const bar = container.querySelector('.bg-amber-600')
    expect(bar).toBeInTheDocument()
  })

  it('applies green color', () => {
    const { container } = render(<Progress value={50} color="green" />)
    const bar = container.querySelector('.bg-green-600')
    expect(bar).toBeInTheDocument()
  })

  it('applies blue color', () => {
    const { container } = render(<Progress value={50} color="blue" />)
    const bar = container.querySelector('.bg-blue-600')
    expect(bar).toBeInTheDocument()
  })

  it('applies red color', () => {
    const { container } = render(<Progress value={50} color="red" />)
    const bar = container.querySelector('.bg-red-600')
    expect(bar).toBeInTheDocument()
  })

  it('applies sm size', () => {
    const { container } = render(<Progress value={50} size="sm" />)
    const track = container.querySelector('.h-2')
    expect(track).toBeInTheDocument()
  })

  it('applies lg size', () => {
    const { container } = render(<Progress value={50} size="lg" />)
    const track = container.querySelector('.h-4')
    expect(track).toBeInTheDocument()
  })

  it('calculates percentage correctly with custom max', () => {
    render(<Progress value={1} max={4} showLabel />)
    expect(screen.getByText('25%')).toBeInTheDocument()
  })
})
