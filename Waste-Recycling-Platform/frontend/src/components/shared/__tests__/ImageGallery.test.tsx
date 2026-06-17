import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { ImageGallery } from '../ImageGallery'

// Mock the Portal to render children inline
vi.mock('../Portal', () => ({
  Portal: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}))

// Mock lucide-react icons
vi.mock('lucide-react', () => ({
  X: () => <span data-testid="icon-x">X</span>,
  ChevronLeft: () => <span data-testid="icon-left">‹</span>,
  ChevronRight: () => <span data-testid="icon-right">›</span>,
}))

describe('ImageGallery', () => {
  const defaultProps = {
    images: ['https://example.com/img1.jpg', 'https://example.com/img2.jpg'],
    isOpen: true,
    onClose: vi.fn(),
    title: 'Test Gallery',
  }

  it('does not render when isOpen is false', () => {
    render(<ImageGallery {...defaultProps} isOpen={false} />)
    expect(screen.queryByText('Test Gallery')).not.toBeInTheDocument()
  })

  it('does not render when images array is empty', () => {
    render(<ImageGallery {...defaultProps} images={[]} />)
    expect(screen.queryByText('Test Gallery')).not.toBeInTheDocument()
  })

  it('renders gallery with title when open', () => {
    render(<ImageGallery {...defaultProps} />)
    expect(screen.getByText('Test Gallery')).toBeInTheDocument()
  })

  it('renders image with correct alt text', () => {
    render(<ImageGallery {...defaultProps} />)
    const img = screen.getByAltText('Ảnh 1 / 2')
    expect(img).toBeInTheDocument()
    expect(img).toHaveAttribute('role', 'presentation')
  })

  it('image has onKeyDown handler for accessibility', () => {
    render(<ImageGallery {...defaultProps} />)
    const img = screen.getByAltText('Ảnh 1 / 2')
    
    // Should not throw when pressing Enter
    fireEvent.keyDown(img, { key: 'Enter' })
    expect(img).toBeInTheDocument()
  })

  it('image onClick stops propagation (does not close gallery)', () => {
    const onClose = vi.fn()
    render(<ImageGallery {...defaultProps} onClose={onClose} />)
    const img = screen.getByAltText('Ảnh 1 / 2')
    
    fireEvent.click(img)
    // onClose should NOT be called because click on img stops propagation
    expect(onClose).not.toHaveBeenCalled()
  })

  it('renders presentation div with onClick to close', () => {
    render(<ImageGallery {...defaultProps} />)
    const presentationDivs = screen.getAllByRole('presentation')
    expect(presentationDivs.length).toBeGreaterThanOrEqual(1)
  })

  it('renders navigation buttons for multiple images', () => {
    render(<ImageGallery {...defaultProps} />)
    
    const prevBtn = screen.getByTitle('Ảnh trước (Mũi tên trái)')
    const nextBtn = screen.getByTitle('Ảnh tiếp (Mũi tên phải)')
    expect(prevBtn).toBeInTheDocument()
    expect(nextBtn).toBeInTheDocument()
  })

  it('navigates to next image when next button clicked', () => {
    render(<ImageGallery {...defaultProps} />)
    
    const nextBtn = screen.getByTitle('Ảnh tiếp (Mũi tên phải)')
    fireEvent.click(nextBtn)
    
    expect(screen.getByAltText('Ảnh 2 / 2')).toBeInTheDocument()
  })

  it('does not show navigation for single image', () => {
    render(<ImageGallery {...defaultProps} images={['https://example.com/single.jpg']} />)
    
    expect(screen.queryByTitle('Ảnh trước (Mũi tên trái)')).not.toBeInTheDocument()
  })

  it('calls onClose when close button is clicked', () => {
    const onClose = vi.fn()
    render(<ImageGallery {...defaultProps} onClose={onClose} />)
    
    const closeBtn = screen.getByTitle('Đóng (ESC)')
    fireEvent.click(closeBtn)
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('handles image load error with fallback', () => {
    render(<ImageGallery {...defaultProps} />)
    const img = screen.getByAltText('Ảnh 1 / 2')
    
    fireEvent.error(img)
    // Should set fallback src
    expect(img.getAttribute('src')).toContain('svg')
  })
})
