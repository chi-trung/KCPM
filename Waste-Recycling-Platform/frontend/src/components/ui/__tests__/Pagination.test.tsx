import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Pagination } from '../Pagination'

describe('Pagination', () => {
  it('renders Previous and Next buttons', () => {
    render(<Pagination currentPage={1} totalPages={5} onPageChange={vi.fn()} />)
    expect(screen.getByText(/Previous/)).toBeInTheDocument()
    expect(screen.getByText(/Next/)).toBeInTheDocument()
  })

  it('disables Previous button on first page', () => {
    render(<Pagination currentPage={1} totalPages={5} onPageChange={vi.fn()} />)
    const prevBtn = screen.getByText(/Previous/).closest('button')
    expect(prevBtn).toBeDisabled()
  })

  it('disables Next button on last page', () => {
    render(<Pagination currentPage={5} totalPages={5} onPageChange={vi.fn()} />)
    const nextBtn = screen.getByText(/Next/).closest('button')
    expect(nextBtn).toBeDisabled()
  })

  it('enables both buttons on middle page', () => {
    render(<Pagination currentPage={3} totalPages={5} onPageChange={vi.fn()} />)
    const prevBtn = screen.getByText(/Previous/).closest('button')
    const nextBtn = screen.getByText(/Next/).closest('button')
    expect(prevBtn).not.toBeDisabled()
    expect(nextBtn).not.toBeDisabled()
  })

  it('calls onPageChange with currentPage - 1 when Previous clicked', () => {
    const onPageChange = vi.fn()
    render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />)
    fireEvent.click(screen.getByText(/Previous/))
    expect(onPageChange).toHaveBeenCalledWith(2)
  })

  it('calls onPageChange with currentPage + 1 when Next clicked', () => {
    const onPageChange = vi.fn()
    render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />)
    fireEvent.click(screen.getByText(/Next/))
    expect(onPageChange).toHaveBeenCalledWith(4)
  })

  it('calls onPageChange with correct page number when page button clicked', () => {
    const onPageChange = vi.fn()
    render(<Pagination currentPage={1} totalPages={5} onPageChange={onPageChange} />)
    fireEvent.click(screen.getByText('3'))
    expect(onPageChange).toHaveBeenCalledWith(3)
  })

  it('renders page numbers for small total pages', () => {
    render(<Pagination currentPage={1} totalPages={3} onPageChange={vi.fn()} />)
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('renders ellipsis for large page counts', () => {
    render(<Pagination currentPage={1} totalPages={20} onPageChange={vi.fn()} />)
    // Should show "..." somewhere when total pages is large
    const ellipses = screen.getAllByText('...')
    expect(ellipses.length).toBeGreaterThan(0)
  })

  it('applies custom className', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={3} onPageChange={vi.fn()} className="my-class" />
    )
    expect(container.firstElementChild?.className).toContain('my-class')
  })
})
