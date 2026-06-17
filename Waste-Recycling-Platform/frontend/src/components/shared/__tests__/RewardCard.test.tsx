import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import React from 'react'
import { RewardCard } from '../RewardCard'

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  )
}))

const defaultProps = {
  id: 'reward-1',
  name: 'Coffee Voucher',
  description: 'Get a free coffee at any partner café.',
  points: 100,
}

describe('RewardCard', () => {
  it('renders name and description', () => {
    render(<RewardCard {...defaultProps} />)
    expect(screen.getByText('Coffee Voucher')).toBeInTheDocument()
    expect(screen.getByText('Get a free coffee at any partner café.')).toBeInTheDocument()
  })

  it('renders points required', () => {
    render(<RewardCard {...defaultProps} points={100} />)
    expect(screen.getByText('100')).toBeInTheDocument()
  })

  it('renders category when provided', () => {
    render(<RewardCard {...defaultProps} category="Food & Drink" />)
    expect(screen.getByText('Food & Drink')).toBeInTheDocument()
  })

  it('does not render category when not provided', () => {
    render(<RewardCard {...defaultProps} />)
    expect(screen.queryByText('Food & Drink')).not.toBeInTheDocument()
  })

  it('shows "Redeem Now" when user has enough points', () => {
    render(<RewardCard {...defaultProps} points={100} currentPoints={150} available />)
    expect(screen.getByRole('button', { name: 'Redeem Now' })).toBeInTheDocument()
    expect(screen.getByRole('button')).not.toBeDisabled()
  })

  it('shows "Not Enough Points" when user lacks points', () => {
    render(<RewardCard {...defaultProps} points={100} currentPoints={50} />)
    expect(screen.getByRole('button', { name: 'Not Enough Points' })).toBeInTheDocument()
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('shows "Not Enough Points" when not available even with enough points', () => {
    render(<RewardCard {...defaultProps} points={50} currentPoints={200} available={false} />)
    expect(screen.getByRole('button', { name: 'Not Enough Points' })).toBeInTheDocument()
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('shows Out of Stock badge when not available', () => {
    render(<RewardCard {...defaultProps} available={false} />)
    expect(screen.getByText('Out of Stock')).toBeInTheDocument()
  })

  it('does not show Out of Stock badge when available', () => {
    render(<RewardCard {...defaultProps} available />)
    expect(screen.queryByText('Out of Stock')).not.toBeInTheDocument()
  })

  it('renders stock info when provided and > 0', () => {
    render(<RewardCard {...defaultProps} stock={5} />)
    expect(screen.getByText('5 items available')).toBeInTheDocument()
  })

  it('renders out of stock text when stock is 0', () => {
    render(<RewardCard {...defaultProps} stock={0} />)
    expect(screen.getByText('Out of stock')).toBeInTheDocument()
  })

  it('does not render stock info when not provided', () => {
    render(<RewardCard {...defaultProps} />)
    expect(screen.queryByText(/items available/)).not.toBeInTheDocument()
  })

  it('renders image when provided', () => {
    render(<RewardCard {...defaultProps} image="https://example.com/reward.jpg" />)
    const img = screen.getByRole('img')
    expect(img).toHaveAttribute('src', 'https://example.com/reward.jpg')
    expect(img).toHaveAttribute('alt', 'Coffee Voucher')
  })

  it('calls onRedeemClick when redeem button clicked and canRedeem', () => {
    const onRedeemClick = vi.fn()
    render(<RewardCard {...defaultProps} currentPoints={200} available onRedeemClick={onRedeemClick} />)
    fireEvent.click(screen.getByRole('button'))
    expect(onRedeemClick).toHaveBeenCalledTimes(1)
  })

  it('renders current points info', () => {
    render(<RewardCard {...defaultProps} currentPoints={75} />)
    expect(screen.getByText('75')).toBeInTheDocument()
  })
})
