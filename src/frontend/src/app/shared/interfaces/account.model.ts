/**
 * Account entity representing a bank account
 */
export interface Account {
  id: string;
  name: string;
  iban: string;
  currency: string;
  balance: number;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * DTO for creating a new account
 */
export interface CreateAccountDto {
  name: string;
  iban: string;
  currency: string;
  initialBalance: number;
}

/**
 * DTO for updating an existing account
 */
export interface UpdateAccountDto {
  name?: string;
  iban?: string;
  currency?: string;
}

/**
 * Response DTO from API
 */
export interface AccountResponse {
  accountId: string;
  name: string;
  iban: string;
  currency: string;
  initialBalance: number;
  currentBalance: number;
  createdAt: string;
  updatedAt: string;
}

/**
 * Account with converted balance in another currency
 */
export interface AccountWithConvertedBalance {
  accountId: string;
  name: string;
  iban: string;
  originalCurrency: string;
  originalBalance: number;
  convertedCurrency: string;
  convertedBalance: number;
  createdAt: string;
  updatedAt: string;
}

/**
 * Portfolio total in a specific currency
 */
export interface PortfolioTotal {
  currency: string;
  totalBalance: number;
  accountCount: number;
  calculatedAt: string;
}
