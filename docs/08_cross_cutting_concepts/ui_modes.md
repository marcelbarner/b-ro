# User Interface Modes

## Concept Overview

The application provides two distinct user interface modes to accommodate different user skill levels and preferences:

1. **Basic View** - Simplified interface for everyday users
2. **Expert View** - Advanced interface for power users

This dual-mode approach ensures the application is both accessible to beginners while not limiting experienced users.

## Basic View

### Purpose

The Basic View is designed for users who:
- Are new to the application
- Want to complete tasks quickly without complexity
- Prefer guided workflows
- Don't need access to advanced features

### Characteristics

**Simplified Inputs:**
- Only essential fields are shown
- Smart defaults are pre-selected
- Optional fields are hidden
- Tooltips provide contextual help

**Guided Workflows:**
- Step-by-step wizards for complex operations
- Clear call-to-action buttons
- Reduced number of choices at each step
- Progress indicators for multi-step processes

**Interface Design:**
- Clean, uncluttered layouts
- Large, touch-friendly controls
- Limited number of actions per screen
- Prominent primary actions

### Examples

#### Finance - Create Transaction (Basic View)

```
┌─────────────────────────────────┐
│ Add Transaction                 │
├─────────────────────────────────┤
│ Amount*         [_______] EUR   │
│ Type*           [v Income  ]    │
│ Category*       [v Food     ]   │
│ Date            [2025-11-08]    │
│ Description     [__________]    │
│                                 │
│         [Cancel] [Save]         │
└─────────────────────────────────┘
```

**Hidden Fields:**
- Payment method
- Tags
- Recurring settings
- Attachments
- Custom metadata
- Accounting categories

#### Storage - Add Item (Basic View)

```
┌─────────────────────────────────┐
│ Add Item to Storage             │
├─────────────────────────────────┤
│ Item Name*      [__________]    │
│ Quantity*       [____] units    │
│ Location*       [v Kitchen ]    │
│                                 │
│         [Cancel] [Add]          │
└─────────────────────────────────┘
```

**Hidden Fields:**
- Expiration date
- Purchase date
- Purchase price
- Barcode/SKU
- Storage conditions
- Minimum stock level
- Supplier information

## Expert View

### Purpose

The Expert View is designed for users who:
- Are experienced with the application
- Need access to all features and options
- Want full control over data input
- Require advanced configuration options

### Characteristics

**Comprehensive Inputs:**
- All fields are visible (organized logically)
- Advanced options are accessible
- Batch operations available
- Keyboard shortcuts enabled

**Power User Features:**
- Bulk actions
- Advanced filters and search
- Customizable views and columns
- Quick-add shortcuts
- Inline editing

**Interface Design:**
- Information-dense layouts
- Multiple data fields per screen
- Advanced configuration panels
- Customizable dashboards

### Examples

#### Finance - Create Transaction (Expert View)

```
┌─────────────────────────────────────────────────────────┐
│ Add Transaction                        [Switch to Basic]│
├─────────────────────────────────────────────────────────┤
│ Amount*         [_______] [v EUR   ]                    │
│ Type*           [v Income  ]  Recurring: [☐]            │
│ Category*       [v Food     ]  Subcategory: [v Grocery] │
│ Date            [2025-11-08]  Time: [14:30]             │
│ Payment Method  [v Cash     ]  Account: [v Main    ]    │
│ Tags            [#groceries] [#organic] [+ Add]         │
│ Description     [_________________________________]      │
│ Attachments     [📎 Drop files or click to upload]      │
│                                                         │
│ Advanced ▼                                              │
│   Reference No: [__________]  VAT: [__%]                │
│   Cost Center:  [__________]  Project: [__________]     │
│   Notes:        [_________________________________]      │
│                                                         │
│ Recurring Settings (if enabled):                       │
│   Frequency: [v Monthly] Ends: [Never v]                │
│                                                         │
│         [Cancel] [Save] [Save & New]                    │
└─────────────────────────────────────────────────────────┘
```

#### Storage - Add Item (Expert View)

```
┌─────────────────────────────────────────────────────────┐
│ Add Item to Storage                    [Switch to Basic]│
├─────────────────────────────────────────────────────────┤
│ Item Name*      [_____________________]  SKU: [_______] │
│ Quantity*       [____] [v units] Min Stock: [__]        │
│ Location*       [v Kitchen ] Sublocation: [v Pantry  ]  │
│ Category        [v Food    ] Subcategory: [v Pasta   ]  │
│                                                         │
│ Purchase Info:                                          │
│   Date:  [2025-11-08]  Price: [___] EUR                 │
│   Store: [__________]  Receipt: [📎 Upload]             │
│                                                         │
│ Storage Details:                                        │
│   Expiry Date:     [__________]  Opened: [__________]   │
│   Storage Temp:    [v Room Temp]  Humidity: [v Normal] │
│   Opened Lifespan: [__] days                            │
│                                                         │
│ Additional:                                             │
│   Barcode:    [__________] [📷 Scan]                    │
│   Notes:      [_________________________________]        │
│   Tags:       [#pantry] [#staples] [+ Add]              │
│   Documents:  [📎 Manual] [📎 Warranty] [+ Add]         │
│                                                         │
│ Alerts:                                                 │
│   [☑] Low stock alert  [☑] Expiry notification          │
│                                                         │
│         [Cancel] [Save] [Save & Add Another]            │
└─────────────────────────────────────────────────────────┘
```

## View Switching

### Toggle Mechanism

Users can switch between views at any time:

```
┌─────────────────────────────────┐
│ Create Transaction  [⚙ Expert] │  ← Toggle button
└─────────────────────────────────┘
```

### User Preference

- **Default Mode**: Configurable per user in settings
- **Persistence**: Last selected mode is remembered per feature/screen
- **Global Override**: Users can set a global preference
- **First-Time Users**: Default to Basic View

### Implementation Approach

**Frontend (Angular):**
```typescript
// Component determines which template to use
@Component({
  selector: 'bro-transaction-form',
  templateUrl: './transaction-form.component.html'
})
export class TransactionFormComponent {
  viewMode: 'basic' | 'expert' = 'basic';
  
  toggleViewMode() {
    this.viewMode = this.viewMode === 'basic' ? 'expert' : 'basic';
    this.saveViewPreference();
  }
}
```

**Template Structure:**
```html
<!-- Basic View -->
<form *ngIf="viewMode === 'basic'" class="basic-view">
  <!-- Essential fields only -->
</form>

<!-- Expert View -->
<form *ngIf="viewMode === 'expert'" class="expert-view">
  <!-- All fields and advanced options -->
</form>
```

## Design Principles

### Progressive Disclosure

- Start with Basic View by default for new users
- Allow users to "graduate" to Expert View as they gain confidence
- Provide hints about hidden features in Basic View
- Ensure smooth transition between modes

### Consistency

- View toggle always in the same location
- Same field names and labels in both views
- Consistent validation rules
- Unified data model (view is presentation only)

### Accessibility

- Both views must be fully keyboard accessible
- Screen reader support for mode switching
- ARIA labels for all form controls
- Proper focus management when switching modes

### Responsive Design

- Mobile devices default to Basic View
- Expert View adapts layout for smaller screens
- Touch-friendly controls in both modes
- Appropriate use of modals/dialogs for complex inputs on mobile

## Benefits

### For Users

- **Lower Barrier to Entry**: Beginners aren't overwhelmed
- **Growth Path**: Users can expand to Expert View as needed
- **Efficiency**: Power users get direct access to all features
- **Flexibility**: Users choose the experience that suits them

### For Development

- **Incremental Complexity**: Basic View serves as MVP
- **Feature Discovery**: Users discover advanced features gradually
- **Reduced Support Load**: Simpler interface = fewer support questions
- **User Feedback**: Easy to identify which features are truly needed

## Cross-Cutting Implementation

This UI mode concept applies to all three domains:

- **Finance Domain**: Transaction management, budgeting, reporting
- **Documents Domain**: Document upload, search, organization
- **Storage Domain**: Inventory management, location tracking

Each domain implements the same pattern for consistency across the application.
