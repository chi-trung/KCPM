# UI Components Library

Một bộ UI components đầy đủ dùng chung cho toàn bộ ứng dụng.

## 🎨 Các Components Có Sẵn

### 1. **Button**

Nút bấm với nhiều variant và size.

```tsx
import { Button } from '@/components/ui';

<Button variant="primary" size="md">Click me</Button>
<Button variant="secondary">Secondary</Button>
<Button variant="danger">Delete</Button>
<Button variant="success">Save</Button>
<Button variant="outline">Outline</Button>
<Button isLoading>Loading...</Button>
```

**Props:**

- `variant`: 'primary' | 'secondary' | 'danger' | 'success' | 'outline'
- `size`: 'sm' | 'md' | 'lg'
- `isLoading`: boolean
- `disabled`: boolean

---

### 2. **Input**

Trường input với label, error message, và helper text.

```tsx
import { Input } from '@/components/ui';

<Input
  label="Email"
  type="email"
  placeholder="user@example.com"
  error="Invalid email"
/>
<Input
  label="Password"
  type="password"
  helperText="Must be at least 8 characters"
/>
```

**Props:**

- `label`: string
- `error`: string
- `helperText`: string
- Tất cả standard input attributes

---

### 3. **Card**

Container với header, body, và footer.

```tsx
import { Card } from "@/components/ui";

<Card hoverable>
  <Card.Header>
    <h2>Title</h2>
  </Card.Header>
  <Card.Body>Content here</Card.Body>
  <Card.Footer>
    <Button>Action</Button>
  </Card.Footer>
</Card>;
```

---

### 4. **Badge**

Nhãn nhỏ để hiển thị status hoặc tags.

```tsx
import { Badge } from '@/components/ui';

<Badge variant="success">Active</Badge>
<Badge variant="warning" size="sm">Pending</Badge>
<Badge variant="danger">Error</Badge>
```

**Variants:** 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'default'

---

### 5. **Alert**

Thông báo alert với icon và dismiss button.

```tsx
import { Alert } from "@/components/ui";
import { useState } from "react";

const [show, setShow] = useState(true);

{
  show && (
    <Alert
      variant="success"
      title="Success!"
      message="Your changes have been saved."
      onClose={() => setShow(false)}
      dismissible
    />
  );
}
```

---

### 6. **Table**

Bảng generic với custom render.

```tsx
import { Table } from "@/components/ui";

interface User {
  id: number;
  name: string;
  email: string;
  status: "active" | "inactive";
}

const columns = [
  { key: "name" as const, label: "Name" },
  { key: "email" as const, label: "Email" },
  {
    key: "status" as const,
    label: "Status",
    render: (value) => (
      <Badge variant={value === "active" ? "success" : "default"}>
        {value}
      </Badge>
    ),
  },
];

<Table columns={columns} data={users} />;
```

---

### 7. **Modal**

Dialog với header, body, footer.

```tsx
import { Modal } from "@/components/ui";
import { useState } from "react";

const [isOpen, setIsOpen] = useState(false);

<Modal
  isOpen={isOpen}
  title="Confirm Action"
  onClose={() => setIsOpen(false)}
  onConfirm={() => {
    // Handle confirm
    setIsOpen(false);
  }}
  confirmText="Yes, proceed"
  cancelText="Cancel"
>
  Are you sure you want to delete?
</Modal>;
```

---

### 8. **Select**

Dropdown select với label và error.

```tsx
import { Select } from "@/components/ui";

<Select
  label="Choose Option"
  options={[
    { value: "1", label: "Option 1" },
    { value: "2", label: "Option 2" },
  ]}
  placeholder="Select..."
  error={errors.role}
/>;
```

---

### 9. **Spinner**

Loading spinner.

```tsx
import { Spinner } from '@/components/ui';

<Spinner size="md" color="blue" />
<Spinner size="lg" color="green" />
```

---

### 10. **Pagination**

Phân trang với điều hướng.

```tsx
import { Pagination } from "@/components/ui";
import { useState } from "react";

const [page, setPage] = useState(1);

<Pagination currentPage={page} totalPages={10} onPageChange={setPage} />;
```

---

## 🎨 Theme & Colors

File `src/lib/theme.ts` chứa tất cả màu sắc và design tokens.

```tsx
import { theme, statusColors } from "@/lib/theme";

// Usage
const primaryColor = theme.colors.primary[600];
const spacingMd = theme.spacing.md;
const badgeColor = statusColors.active; // 'success'
```

---

## 🪝 Custom Hooks

### useForm

Hook để quản lý form state, validation, errors...

```tsx
import { useForm } from "@/hooks/useForm";

const { values, errors, handleChange, handleSubmit } = useForm({
  initialValues: { email: "", password: "" },
  validate: (values) => {
    const errors = {};
    if (!values.email) errors.email = "Email is required";
    return errors;
  },
  onSubmit: async (values) => {
    await api.login(values);
  },
});

<form onSubmit={handleSubmit}>
  <Input
    name="email"
    value={values.email}
    onChange={handleChange}
    error={errors.email}
  />
</form>;
```

---

## 📂 Folder Structure

```
src/components/
├── ui/                    # Base UI components
│   ├── Button.tsx
│   ├── Input.tsx
│   ├── Card.tsx
│   ├── Badge.tsx
│   ├── Alert.tsx
│   ├── Table.tsx
│   ├── Modal.tsx
│   ├── Select.tsx
│   ├── Spinner.tsx
│   ├── Pagination.tsx
│   └── index.ts
├── shared/               # Shared utilities & hooks
├── layout/               # Layout: Navbar, Sidebar, Footer
├── report/               # Report-related components
├── reward/               # Reward-related components
├── task/                 # Task-related components
└── map/                  # Map-related components

src/lib/
└── theme.ts              # Theme configuration

src/hooks/
└── useForm.ts            # Form management hook
```

---

## 💡 Best Practices

1. **Import từ barrel exports**

   ```tsx
   // ✅ Good
   import { Button, Input, Modal } from "@/components/ui";

   // ❌ Avoid
   import { Button } from "@/components/ui/Button";
   ```

2. **Sử dụng theme colors**

   ```tsx
   import { theme } from '@/lib/theme';

   className={`text-[${theme.colors.primary[600]}]`}  // Dynamic
   className="text-blue-600"  // Tailwind (preferred)
   ```

3. **Form validation**

   ```tsx
   const validateEmail = (email) => {
     return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
   };
   ```

4. **Loading states**
   ```tsx
   <Button isLoading={isLoading} disabled={isSubmitting}>
     {isLoading ? "Loading..." : "Submit"}
   </Button>
   ```

---

## 🚀 Extending Components

Để custom hoặc extend components:

```tsx
import { Button } from "@/components/ui";

export const CustomButton = ({ ...props }) => (
  <Button {...props} className={`rounded-full ${props.className}`} />
);
```
