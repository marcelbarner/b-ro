# Internationalization (i18n)

## Overview

The application implements comprehensive internationalization support to accommodate users across different locales, languages, and regions. This ensures that content, formatting, and user experience are adapted to local conventions.

## Translation Framework

### ngx-translate

The application uses **@ngx-translate/core v17.0.0** with **@ngx-translate/http-loader v17.0.0** for managing translations.

**Configuration** (`app.config.ts`):
```typescript
provideTranslateService({
  loader: {
    provide: TranslateLoader,
    useClass: TranslateHttpLoader,
  },
})
```

**Translation File Location**: `/public/i18n/*.json`

**Supported Languages**:
- English (en-US) - Default
- Simplified Chinese (zh-CN)
- Traditional Chinese (zh-TW)

### Translation Keys Structure

Translation keys follow a hierarchical dot notation:

```
domain.feature.element
```

**Examples**:
- `finance.accounts` - Finance domain, accounts feature
- `finance.account_created` - Success message for account creation
- `validation.required` - Generic validation message

## Multi-Currency Support

### Currency Handling

The Finance domain supports multiple currencies with proper locale-aware formatting:

**Supported Currencies**:
- EUR (Euro)
- USD (US Dollar)
- GBP (British Pound)
- Any ISO 4217 currency code

### Currency Formatting

**Use Angular's built-in currency pipe** for all monetary values:

```html
{{ account.balance | currency : account.currency : 'symbol' : '1.2-2' }}
```

**Parameters**:
- `account.currency`: ISO 4217 currency code (EUR, USD, GBP)
- `'symbol'`: Display currency symbol (€, $, £)
- `'1.2-2'`: Format with at least 1 digit before decimal, 2-2 digits after

**Output Examples**:
- `5000` EUR → `€5,000.00`
- `7500` USD → `$7,500.00`
- `25000` GBP → `£25,000.00`

### Backend Response

The API returns currency as an ISO 4217 code:

```json
{
  "accountId": "...",
  "name": "Main Checking Account",
  "currency": "EUR",
  "currentBalance": 5000.0
}
```

## Locale Configuration

### Angular Material Date Adapter

**Date-fns adapter** is configured for locale-aware date formatting:

```typescript
provideDateFnsAdapter({
  parse: {
    dateInput: 'yyyy-MM-dd',
  },
  display: {
    dateInput: 'yyyy-MM-dd',
    monthYearLabel: 'yyyy MMM',
  },
})
```

### Locale Provider

Dynamic locale is provided based on user settings:

```typescript
{
  provide: MAT_DATE_LOCALE,
  useFactory: () => inject(SettingsService).getLocale(),
}
```

## Number Formatting

### DecimalPipe Usage

For non-monetary numeric values, use Angular's `DecimalPipe`:

```html
{{ value | number : '1.2-2' }}
```

## Translation Service Usage

### Component Translation

**Inject TranslateService**:

```typescript
constructor(private translate: TranslateService) {}
```

**Access translation**:

```typescript
// Synchronous
const message = this.translate.instant('finance.account_created');

// Asynchronous (observable)
this.translate.get('finance.confirm_delete_account').subscribe(msg => {
  // Use translated message
});

// With parameters
this.translate.get('validation.min', { number: 5 }).subscribe(msg => {
  // "This value should be no less than 5"
});
```

### Template Translation

**Translate directive**:

```html
<h2>{{ 'finance.account_list' | translate }}</h2>
```

**Translate pipe with parameters**:

```html
<span>{{ 'validation.max_length' | translate:{ number: 100 } }}</span>
```

## Finance Domain Translation Keys

### UI Labels
- `finance.account_list`: "Bank Accounts"
- `finance.account_name`: "Account Name"
- `finance.iban`: "IBAN"
- `finance.currency`: "Currency"
- `finance.balance`: "Balance"

### Actions
- `finance.create_account`: "Create Account"
- `finance.edit_account`: "Edit Account"
- `finance.delete_account`: "Delete Account"
- `finance.view_account`: "View Account"

### Messages
- `finance.account_created`: "Account created successfully"
- `finance.account_updated`: "Account updated successfully"
- `finance.account_deleted`: "Account deleted successfully"
- `finance.no_accounts`: "No accounts found."
- `finance.loading_accounts`: "Loading accounts..."

### Placeholders
- `finance.search_placeholder`: "Search by name, IBAN, or currency"
- `finance.account_name_placeholder`: "e.g., Main Checking Account"
- `finance.iban_placeholder`: "e.g., DE89370400440532013000"
- `finance.currency_placeholder`: "e.g., EUR"

## Right-to-Left (RTL) Support

The application includes a Directionality service for RTL language support:

**Service**: `DirectionalityService` (implements Angular CDK `Directionality`)

**Properties**:
- `value`: Current text direction ('ltr' | 'rtl')
- `valueSignal`: Signal-based reactive direction
- `change`: Observable for direction changes

## Best Practices

### 1. Always Use Translation Keys

❌ **Don't hardcode text**:
```html
<h2>Bank Accounts</h2>
```

✅ **Use translation keys**:
```html
<h2>{{ 'finance.account_list' | translate }}</h2>
```

### 2. Use Currency Pipe for Money

❌ **Don't manually format**:
```html
{{ account.balance | number : '1.2-2' }} {{ account.currency }}
```

✅ **Use currency pipe**:
```html
{{ account.balance | currency : account.currency : 'symbol' : '1.2-2' }}
```

### 3. Parameterize Dynamic Content

✅ **Use placeholders for dynamic values**:
```typescript
this.translate.get('finance.account_balance', { 
  balance: account.balance,
  currency: account.currency 
});
```

### 4. Organize Translation Keys Hierarchically

✅ **Group by domain and feature**:
```json
{
  "finance": {
    "accounts": "Accounts",
    "transactions": "Transactions"
  }
}
```

### 5. Extract All User-Facing Text

All user-visible strings should be in translation files:
- Labels
- Button text
- Error messages
- Success notifications
- Placeholders
- Validation messages

## Testing i18n

### Verify Translation Coverage

1. Check all components use `translate` pipe or service
2. Ensure all translation keys exist in `en-US.json`
3. Test with different locales
4. Verify currency formatting with different currencies
5. Test date/time formatting with different locales

### Language Switching

Users can switch languages through the application UI (top toolbar translate button).

## Future Enhancements

1. **Additional Languages**: Add support for more languages (de-DE, fr-FR, es-ES)
2. **Currency Conversion**: Implement real-time currency conversion
3. **Regional Number Formats**: Support different thousand/decimal separators
4. **Translation Management**: Consider translation management system for non-technical translators
5. **Missing Translation Handling**: Improve fallback strategy for missing keys
