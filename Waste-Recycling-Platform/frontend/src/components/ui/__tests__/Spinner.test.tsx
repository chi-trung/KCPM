import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import { Spinner } from '../Spinner'

// Helper: get class string from SVG element (jsdom uses SVGAnimatedString)
const getSvgClass = (el: Element | null): string =>
  el?.getAttribute('class') ?? ''

describe('Spinner', () => {
  it('renders an svg element', () => {
    const { container } = render(<Spinner />)
    const svg = container.querySelector('svg')
    expect(svg).toBeInTheDocument()
  })

  it('has animate-spin class by default', () => {
    const { container } = render(<Spinner />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('animate-spin')
  })

  it('applies md size by default', () => {
    const { container } = render(<Spinner />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('w-8')
    expect(getSvgClass(svg)).toContain('h-8')
  })

  it('applies sm size', () => {
    const { container } = render(<Spinner size="sm" />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('w-4')
    expect(getSvgClass(svg)).toContain('h-4')
  })

  it('applies lg size', () => {
    const { container } = render(<Spinner size="lg" />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('w-12')
    expect(getSvgClass(svg)).toContain('h-12')
  })

  it('applies blue color by default', () => {
    const { container } = render(<Spinner />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('text-blue-600')
  })

  it('applies green color', () => {
    const { container } = render(<Spinner color="green" />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('text-green-600')
  })

  it('applies red color', () => {
    const { container } = render(<Spinner color="red" />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('text-red-600')
  })

  it('applies gray color', () => {
    const { container } = render(<Spinner color="gray" />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('text-gray-600')
  })

  it('applies custom className', () => {
    const { container } = render(<Spinner className="my-spinner" />)
    const svg = container.querySelector('svg')
    expect(getSvgClass(svg)).toContain('my-spinner')
  })
})

