/**
 * Transaction type enum
 */
export enum TransactionType {
  Deposit = 0,
  Withdrawal = 1,
  Transfer = 2,
}

/**
 * Transaction interface matching backend DTO
 */
export interface Transaction {
  transactionId: string;
  accountId: string;
  amount: number;
  currency: string;
  type: TransactionType;
  description: string;
  date: string;
  counterTransactionId?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * DTO for creating a new transaction
 */
export interface CreateTransactionDto {
  amount: number;
  currency: string;
  type: TransactionType;
  description: string;
  date?: string;
}

/**
 * DTO for creating a transfer between accounts
 */
export interface CreateTransferDto {
  fromAccountId: string;
  toAccountId: string;
  amount: number;
  description: string;
}

/**
 * Transfer result from backend
 */
export interface TransferResult {
  debitTransactionId: string;
  creditTransactionId: string;
  fromAccountBalance: number;
  toAccountBalance: number;
}
