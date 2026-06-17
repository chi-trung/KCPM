import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { CollectorCard, CollectorCardProps } from '../CollectorCard'

// Mock next/link to just render the anchor tag
vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  )
}))

describe('CollectorCard', () => {
  const defaultProps: CollectorCardProps = {
    id: '123',
    name: 'Nguyễn Văn A',
    rating: 4.2,
    completedTasks: 45,
    reviews: 12,
    location: 'Quận 1, TP.HCM',
    status: 'available',
    responseTime: '15 mins',
    onContactClick: vi.fn()
  }

  it('renders collector details correctly', () => {
    render(<CollectorCard {...defaultProps} />)
    
    expect(screen.getByText('Nguyễn Văn A')).toBeInTheDocument()
    expect(screen.getByText('Quận 1, TP.HCM')).toBeInTheDocument()
    expect(screen.getByText('Available')).toBeInTheDocument()
    expect(screen.getByText('45')).toBeInTheDocument()
    expect(screen.getByText('15 mins')).toBeInTheDocument()
    expect(screen.getByText('4.2 (12 reviews)')).toBeInTheDocument()
  })

  it('renders rating stars correctly', () => {
    render(<CollectorCard {...defaultProps} rating={3.2} />)
    
    // rating 3.2 rounds to 3 filled stars, 2 empty stars
    // 3.2 rounds to 3 (Math.round(3.2) = 3)
    const filledStars = screen.getAllByText('⭐')
    const emptyStars = screen.getAllByText('☆')
    expect(filledStars).toHaveLength(3)
    expect(emptyStars).toHaveLength(2)
  })

  it('calls onContactClick when contact button is clicked', () => {
    const onContactClick = vi.fn()
    render(<CollectorCard {...defaultProps} onContactClick={onContactClick} />)
    
    const contactBtn = screen.getByText('Contact Collector')
    fireEvent.click(contactBtn)
    
    expect(onContactClick).toHaveBeenCalledTimes(1)
  })

  it('renders without response time if not provided', () => {
    const props = { ...defaultProps, responseTime: undefined }
    render(<CollectorCard {...props} />)
    
    expect(screen.queryByText('15 mins')).not.toBeInTheDocument()
  })
})
