# IBTS Design System

## Overview
Nordic-inspired design system with a clean, modern aesthetic. Uses CSS custom properties for consistency.

## Color Palette

### Primary Colors
- **Primary**: `#5E81AC` - Main accent color (buttons, links, focus states)
- **Primary Hover**: `#4C6C8A` - Darker primary for hover states

### Text Colors
- **Primary Text**: `#3B4252` - Headings, important text
- **Secondary Text**: `#434C5E` - Labels, body text
- **Muted Text**: `#6c757d` - Subtle text, captions
- **Light Text**: `#8b949e` - Placeholders
- **Inverse Text**: `#ffffff` - Text on dark backgrounds

### Background Colors
- **Primary BG**: `#ffffff` - Cards, panels
- **Secondary BG**: `#f8f9fa` - Page background, table headers
- **Tertiary BG**: `#e9ecef` - Disabled inputs
- **Dark BG**: `#3B4252` - Sidebar, dark elements

### Status Colors
- **Success**: `#26b050` / bg: `#d4edda` / text: `#155724`
- **Error**: `#dc3545` / bg: `#f8d7da` / text: `#721c24`
- **Warning**: `#ffc107` / bg: `#fff3cd` / text: `#856404`
- **Info**: `#17a2b8` / bg: `#d1ecf1` / text: `#0c5460`

### Border Colors
- **Default**: `#d1d5db`
- **Light**: `#e9ecef`
- **Focus**: `#5E81AC` (primary)

## Spacing Scale (8px base)

| Token | Value | Pixels |
|-------|-------|--------|
| `--space-1` | 0.25rem | 4px |
| `--space-2` | 0.5rem | 8px |
| `--space-3` | 0.75rem | 12px |
| `--space-4` | 1rem | 16px |
| `--space-5` | 1.5rem | 24px |
| `--space-6` | 2rem | 32px |
| `--space-8` | 3rem | 48px |
| `--space-10` | 4rem | 64px |

## Typography

### Font Stack
```css
-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif
```

### Font Sizes
| Token | Value | Pixels |
|-------|-------|--------|
| `--font-size-xs` | 0.75rem | 12px |
| `--font-size-sm` | 0.875rem | 14px |
| `--font-size-base` | 1rem | 16px |
| `--font-size-lg` | 1.125rem | 18px |
| `--font-size-xl` | 1.25rem | 20px |
| `--font-size-2xl` | 1.5rem | 24px |
| `--font-size-3xl` | 2rem | 32px |

### Font Weights
- Normal: 400
- Medium: 500
- Semibold: 600
- Bold: 700

## Border Radius

| Token | Value |
|-------|-------|
| `--radius-sm` | 4px |
| `--radius-md` | 6px |
| `--radius-lg` | 8px |
| `--radius-xl` | 12px |
| `--radius-full` | 9999px (pills/circles) |

## Shadows

| Token | Value |
|-------|-------|
| `--shadow-sm` | `0 1px 2px rgba(0, 0, 0, 0.05)` |
| `--shadow-md` | `0 2px 8px rgba(0, 0, 0, 0.08)` |
| `--shadow-lg` | `0 4px 16px rgba(0, 0, 0, 0.1)` |
| `--shadow-xl` | `0 8px 24px rgba(0, 0, 0, 0.12)` |

## Focus Ring
```css
box-shadow: 0 0 0 3px rgba(94, 129, 172, 0.25);
```

## Component Patterns

### Buttons
- Height: auto (padding-based)
- Padding: `--space-2` `--space-4` (8px 16px)
- Small: `--space-1` `--space-2` (4px 8px)
- Border radius: `--radius-md` (6px)
- Font: `--font-size-sm`, `--font-weight-medium`

### Form Controls
- Padding: `--space-3` (12px)
- Border: 1px solid `--color-border`
- Border radius: `--radius-md` (6px)
- Focus: primary border + focus ring

### Cards
- Background: `--color-bg-primary`
- Border: 1px solid `--color-border-light`
- Border radius: `--radius-lg` (8px)
- Shadow: `--shadow-sm`
- Header padding: `--space-4` `--space-5`
- Body padding: `--space-5`

### Tables
- Header background: `--color-bg-secondary`
- Header text: uppercase, `--font-size-sm`, `--font-weight-semibold`
- Cell padding: `--space-4`
- Row hover: `--color-bg-secondary`
- Border: 1px solid `--color-border-light`

### Badges
- Padding: `--space-1` `--space-3`
- Border radius: `--radius-full`
- Font: `--font-size-xs`, `--font-weight-semibold`

### Pagination
- Link padding: `--space-2` `--space-3`
- Border radius: `--radius-md`
- Active: primary background

## Page Layout Patterns

### Page Header
```html
<div class="page-header">
    <div>
        <h1>Page Title</h1>
        <p class="page-subtitle">Description text</p>
    </div>
    <button class="btn btn-primary">Action</button>
</div>
```

### Form Card
```html
<div class="form-card">
    <EditForm>
        <div class="form-group">...</div>
        <div class="form-actions">
            <button class="btn btn-secondary">Cancel</button>
            <button class="btn btn-primary">Submit</button>
        </div>
    </EditForm>
</div>
```

### Loading State
```html
<div class="loading-state">
    <div class="spinner-border"></div>
    <p>Loading...</p>
</div>
```

### Empty State
```html
<div class="empty-state">
    <div class="empty-state-icon">...</div>
    <h3>No items found</h3>
    <p>Description text</p>
    <button class="btn btn-primary">Action</button>
</div>
```

## Responsive Breakpoints
- Mobile: < 480px
- Tablet: 768px
- Desktop: 1024px
- Wide: 1200px

## Transitions
- Fast: 150ms ease
- Base: 200ms ease
- Slow: 300ms ease
