import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { Table } from '../Table'

type Item = { id: number; name: string; status: string }

const columns = [
  { key: 'id' as keyof Item, label: 'ID' },
  { key: 'name' as keyof Item, label: 'Name' },
  { key: 'status' as keyof Item, label: 'Status' },
]

const data: Item[] = [
  { id: 1, name: 'Alice', status: 'active' },
  { id: 2, name: 'Bob', status: 'inactive' },
  { id: 3, name: 'Charlie', status: 'active' },
]

describe('Table', () => {
  it('renders column headers', () => {
    render(<Table columns={columns} data={data} />)
    expect(screen.getByText('ID')).toBeInTheDocument()
    expect(screen.getByText('Name')).toBeInTheDocument()
    expect(screen.getByText('Status')).toBeInTheDocument()
  })

  it('renders data rows', () => {
    render(<Table columns={columns} data={data} />)
    expect(screen.getByText('Alice')).toBeInTheDocument()
    expect(screen.getByText('Bob')).toBeInTheDocument()
    expect(screen.getByText('Charlie')).toBeInTheDocument()
  })

  it('renders "No data available" when data is empty', () => {
    render(<Table columns={columns} data={[]} />)
    expect(screen.getByText('No data available')).toBeInTheDocument()
  })

  it('uses custom render function for cells', () => {
    const columnsWithRender = [
      ...columns,
      {
        key: 'status' as keyof Item,
        label: 'Status Badge',
        render: (value: string) => <span data-testid="status-badge">{value.toUpperCase()}</span>,
      },
    ]
    render(<Table columns={columnsWithRender} data={data} />)
    const badges = screen.getAllByTestId('status-badge')
    expect(badges[0]).toHaveTextContent('ACTIVE')
    expect(badges[1]).toHaveTextContent('INACTIVE')
  })

  it('calls onRowClick when a row is clicked', () => {
    const onRowClick = vi.fn()
    render(<Table columns={columns} data={data} onRowClick={onRowClick} />)
    // Click the row containing Alice
    fireEvent.click(screen.getByText('Alice').closest('tr')!)
    expect(onRowClick).toHaveBeenCalledWith(data[0])
  })

  it('does not call onRowClick when not provided', () => {
    render(<Table columns={columns} data={data} />)
    // Should render without error
    expect(screen.getByText('Alice')).toBeInTheDocument()
  })

  it('applies striped styles to even rows by default', () => {
    const { container } = render(<Table columns={columns} data={data} />)
    const rows = container.querySelectorAll('tbody tr')
    expect(rows[0].className).toContain('bg-gray-50') // even (index 0)
    expect(rows[1].className).toContain('bg-white')    // odd (index 1)
  })

  it('does not apply striped styles when striped is false', () => {
    const { container } = render(<Table columns={columns} data={data} striped={false} />)
    const rows = container.querySelectorAll('tbody tr')
    expect(rows[0].className).not.toContain('bg-gray-50')
    expect(rows[0].className).toContain('bg-white')
  })

  it('applies hoverable styles by default', () => {
    const { container } = render(<Table columns={columns} data={data} />)
    const rows = container.querySelectorAll('tbody tr')
    expect(rows[0].className).toContain('hover:bg-blue-50')
  })

  it('does not apply hover styles when hoverable is false', () => {
    const { container } = render(<Table columns={columns} data={data} hoverable={false} />)
    const rows = container.querySelectorAll('tbody tr')
    expect(rows[0].className).not.toContain('hover:bg-blue-50')
  })

  it('applies cursor-pointer style when onRowClick is provided', () => {
    const { container } = render(<Table columns={columns} data={data} onRowClick={vi.fn()} />)
    const rows = container.querySelectorAll('tbody tr')
    expect(rows[0].className).toContain('cursor-pointer')
  })

  it('applies custom className to table element', () => {
    const { container } = render(<Table columns={columns} data={data} className="my-table" />)
    const table = container.querySelector('table')
    expect(table?.className).toContain('my-table')
  })

  it('applies column width when specified', () => {
    const columnsWithWidth = [
      { key: 'id' as keyof Item, label: 'ID', width: '100px' },
    ]
    const { container } = render(<Table columns={columnsWithWidth} data={data} />)
    const th = container.querySelector('th')
    expect(th?.style.width).toBe('100px')
  })

  it('shows dash for null/undefined values', () => {
    const dataWithNull = [{ id: 1, name: null as any, status: undefined as any }]
    const columnsForNull = [
      { key: 'name' as any, label: 'Name' },
    ]
    render(<Table columns={columnsForNull} data={dataWithNull} />)
    expect(screen.getByText('-')).toBeInTheDocument()
  })
})
