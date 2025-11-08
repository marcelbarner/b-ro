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
  id: string;
  name: string;
  iban: string;
  currency: string;
  balance: number;
  createdAt: string;
  updatedAt: string;
}
