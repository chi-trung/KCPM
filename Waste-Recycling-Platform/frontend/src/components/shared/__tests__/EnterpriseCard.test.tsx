import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { EnterpriseCard } from '../EnterpriseCard'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  )
}))

const defaultProps = {
  id: 'ent-1',
  name: 'Green Solutions Co.',
  description: 'A leading waste management enterprise.',
  serviceArea: 'District 1-5, HCMC',
  status: 'active' as const,
  tasksPosted: 120,
  contactEmail: 'contact@greensolutions.vn',
  contactPhone: '0901234567',
}

describe('EnterpriseCard', () => {
  it('renders name and description', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.getByText('Green Solutions Co.')).toBeInTheDocument()
    expect(screen.getByText('A leading waste management enterprise.')).toBeInTheDocument()
  })

  it('renders service area', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.getByText('District 1-5, HCMC')).toBeInTheDocument()
  })

  it('renders tasks posted count', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.getByText('120')).toBeInTheDocument()
  })

  it('renders contact email', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.getByText('contact@greensolutions.vn')).toBeInTheDocument()
  })

  it('renders "Active" badge for active status', () => {
    render(<EnterpriseCard {...defaultProps} status="active" />)
    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('renders "Inactive" badge for inactive status', () => {
    render(<EnterpriseCard {...defaultProps} status="inactive" />)
    expect(screen.getByText('Inactive')).toBeInTheDocument()
  })

  it('renders "Pending" badge for pending status', () => {
    render(<EnterpriseCard {...defaultProps} status="pending" />)
    expect(screen.getByText('Pending')).toBeInTheDocument()
  })

  it('renders rating when provided', () => {
    render(<EnterpriseCard {...defaultProps} rating={4.8} />)
    expect(screen.getByText(/4.8/)).toBeInTheDocument()
  })

  it('does not render rating when not provided', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.queryByText(/⭐/)).not.toBeInTheDocument()
  })

  it('renders logo image when provided', () => {
    render(<EnterpriseCard {...defaultProps} logo="https://example.com/logo.png" />)
    const img = screen.getByRole('img')
    expect(img).toHaveAttribute('src', 'https://example.com/logo.png')
    expect(img).toHaveAttribute('alt', 'Green Solutions Co.')
  })

  it('does not render image when logo not provided', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.queryByRole('img')).not.toBeInTheDocument()
  })

  it('renders Contact Enterprise button', () => {
    render(<EnterpriseCard {...defaultProps} />)
    expect(screen.getByRole('button', { name: 'Contact Enterprise' })).toBeInTheDocument()
  })

  it('calls onContactClick when contact button is clicked', () => {
    const onContactClick = vi.fn()
    render(<EnterpriseCard {...defaultProps} onContactClick={onContactClick} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onContactClick).toHaveBeenCalledTimes(1)
  })

  it('links to correct enterprise detail page', () => {
    render(<EnterpriseCard {...defaultProps} id="ent-42" />)
    expect(screen.getByRole('link')).toHaveAttribute('href', '/enterprises/ent-42')
  })
})
