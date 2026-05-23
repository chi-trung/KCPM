# Feature-Specific Cards

Specialized card components built on top of the base `Card` component for displaying domain-specific information throughout the application.

## Components

### 1. ReportCard

Displays waste reports with status, location, and reward information.

**Props:**

- `id: string` - Unique report identifier
- `title: string` - Report title
- `description: string` - Waste description
- `location: string` - Report location
- `wasteType: string` - Type of waste
- `status: 'pending' | 'assigned' | 'completed' | 'cancelled'` - Current status
- `image?: string` - Optional report image URL
- `createdAt: string` - Creation date (ISO format)
- `points?: number` - Reward points for completion
- `onActionClick?: () => void` - Action button callback
- `actionButtonLabel?: string` - Custom button label (default: "View Details")

**Example:**

```tsx
<ReportCard
  id="1"
  title="Mixed Plastic Waste"
  description="Found plastic bottles near the park"
  location="Central Park"
  wasteType="Plastic"
  status="pending"
  createdAt="2024-03-13"
  points={50}
/>
```

---

### 2. TaskCard

Displays collection tasks with priority, assignment, and completion tracking.

**Props:**

- `id: string` - Task identifier
- `title: string` - Task title
- `description: string` - Task description
- `location: string` - Collection location
- `status: 'pending' | 'in_progress' | 'completed' | 'cancelled'` - Task status
- `assignedTo?: string` - Collector name
- `priority?: 'low' | 'medium' | 'high'` - Priority level
- `dueDate: string` - Due date (ISO format)
- `reward?: number` - Completion reward points
- `acceptedAt?: string` - When task was accepted
- `onActionClick?: () => void` - Action button callback
- `actionButtonLabel?: string` - Custom button label (default: "View Task")

**Features:**

- Automatic overtime detection with warning indicator
- Priority badge with color coding
- Status badges with appropriate colors

**Example:**

```tsx
<TaskCard
  id="1"
  title="Collect Plastic Bottles"
  description="Downtown plastic collection"
  location="Downtown District"
  status="in_progress"
  priority="high"
  dueDate="2024-03-15"
  reward={100}
  assignedTo="John Collector"
/>
```

---

### 3. RewardCard

Displays available rewards with point requirements and redemption tracking.

**Props:**

- `id: string` - Reward identifier
- `name: string` - Reward name
- `description: string` - Reward description
- `points: number` - Points required to redeem
- `currentPoints?: number` - User's current points (default: 0)
- `image?: string` - Optional reward image
- `category?: string` - Reward category
- `available?: boolean` - Stock availability (default: true)
- `stock?: number` - Remaining stock quantity
- `onRedeemClick?: () => void` - Redeem button callback

**Features:**

- Visual progress bar showing point progress
- Smart button state (enabled/disabled based on points)
- Stock availability indicator

**Example:**

```tsx
<RewardCard
  id="1"
  name="Eco Bag"
  description="Reusable shopping bag"
  points={200}
  currentPoints={150}
  category="Accessories"
  available={true}
  stock={45}
/>
```

---

### 4. CollectorCard

Displays collector profiles with ratings and performance metrics.

**Props:**

- `id: string` - Collector identifier
- `name: string` - Collector name
- `avatar?: string` - Profile image URL
- `rating: number` - Star rating (0-5)
- `completedTasks: number` - Total tasks completed
- `reviews: number` - Total reviews count
- `location: string` - Service area
- `status: 'available' | 'busy' | 'offline'` - Current status
- `responseTime?: string` - Average response time
- `onContactClick?: () => void` - Contact button callback

**Features:**

- Star rating display
- Status badge with real-time availability
- Performance statistics
- Response time tracking

**Example:**

```tsx
<CollectorCard
  id="1"
  name="Alex Johnson"
  rating={4.8}
  completedTasks={156}
  reviews={89}
  location="Downtown Area"
  status="available"
  responseTime="5 mins"
/>
```

---

### 5. EnterpriseCard

Displays enterprise/organization information and task management metrics.

**Props:**

- `id: string` - Enterprise identifier
- `name: string` - Company name
- `logo?: string` - Company logo URL
- `description: string` - Company description
- `serviceArea: string` - Geographic service area
- `status: 'active' | 'inactive' | 'pending'` - Enterprise status
- `tasksPosted: number` - Total tasks posted
- `rating?: number` - Enterprise rating
- `contactEmail: string` - Email contact
- `contactPhone: string` - Phone number
- `onContactClick?: () => void` - Contact button callback

**Features:**

- Logo display with hover effect
- Service area and contact information
- Task posting metrics
- Enterprise rating display

**Example:**

```tsx
<EnterpriseCard
  id="1"
  name="Green Logistics Inc"
  description="Professional waste management"
  serviceArea="City Center"
  status="active"
  tasksPosted={45}
  rating={4.5}
  contactEmail="contact@greenlogistics.com"
  contactPhone="+1-555-0123"
/>
```

---

### 6. StatCard

Compact statistics card for dashboard displays.

**Props:**

- `icon?: React.ReactNode` - Icon or emoji
- `label: string` - Statistic label
- `value: string | number` - Main value
- `unit?: string` - Unit of measurement
- `trend?: 'up' | 'down' | 'neutral'` - Trend direction
- `trendValue?: string` - Trend text description
- `color?: 'amber' | 'green' | 'blue' | 'red'` - Card color theme (default: 'amber')
- `className?: string` - Custom CSS classes

**Features:**

- Colored backgrounds matching design system
- Trend indicators with directional arrows
- Flexible layout for various metrics

**Example:**

```tsx
<StatCard
  icon="♻️"
  label="Total Reports"
  value={1234}
  color="amber"
  trend="up"
  trendValue="15% increase"
/>
```

---

### 7. UserProfileCard

Displays user profile information with achievements and statistics.

**Props:**

- `name: string` - User's full name
- `avatar?: string` - Profile photo URL
- `role: 'citizen' | 'collector' | 'enterprise' | 'admin'` - User role
- `email: string` - Email address
- `phone?: string` - Phone number
- `joinedDate: string` - Join date (ISO format)
- `stats: Array<{label: string, value: number | string}>` - Statistics array
- `badges?: string[]` - Achievement badges
- `verified?: boolean` - Verification status

**Features:**

- Role-based color coding
- Verification indicator
- Three-column statistics grid
- Achievement badges display

**Example:**

```tsx
<UserProfileCard
  name="Sarah Williams"
  avatar="https://..."
  role="citizen"
  email="sarah@example.com"
  phone="+1-555-0456"
  joinedDate="2023-06-15"
  stats={[
    { label: "Reports", value: 12 },
    { label: "Points", value: 850 },
    { label: "Rewards", value: 3 },
  ]}
  badges={["Eco Warrior", "Top Reporter"]}
  verified={true}
/>
```

---

## Color Scheme

All cards use the unified Amber + White color scheme:

- **Primary Color**: Amber (#d97706)
- **Secondary Color**: Green (#16a34a)
- **Success**: Green
- **Warning**: Orange/Yellow
- **Danger**: Red
- **Info**: Blue

---

## Best Practices

1. **Responsive Design**: All cards are responsive and work on mobile, tablet, and desktop
2. **Hover Effects**: Interactive cards include hover effects for better UX
3. **Loading States**: Use `Spinner` component for async operations
4. **Status Badges**: Use appropriate badge variants for status display
5. **Images**: Always provide fallback for missing images
6. **Accessibility**: All interactive elements are keyboard accessible

---

## Examples in Showcase

View all cards in action at the `/components` page.
