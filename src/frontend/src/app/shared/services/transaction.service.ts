import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Transaction,
  CreateTransactionDto,
  CreateTransferDto,
  TransferResult,
} from '../interfaces/transaction.model';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private readonly apiUrl = '/api/finance';

  constructor(private http: HttpClient) {}

  /**
   * Get all transactions for a specific account
   */
  getTransactionsByAccount(accountId: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/accounts/${accountId}/transactions`);
  }

  /**
   * Get a single transaction by ID
   */
  getTransaction(accountId: string, transactionId: string): Observable<Transaction> {
    return this.http.get<Transaction>(
      `${this.apiUrl}/accounts/${accountId}/transactions/${transactionId}`
    );
  }

  /**
   * Create a new transaction
   */
  createTransaction(
    accountId: string,
    transaction: CreateTransactionDto
  ): Observable<Transaction> {
    return this.http.post<Transaction>(
      `${this.apiUrl}/accounts/${accountId}/transactions`,
      transaction
    );
  }

  /**
   * Delete a transaction
   */
  deleteTransaction(accountId: string, transactionId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/accounts/${accountId}/transactions/${transactionId}`
    );
  }

  /**
   * Create a transfer between two accounts
   */
  createTransfer(transfer: CreateTransferDto): Observable<TransferResult> {
    return this.http.post<TransferResult>(`${this.apiUrl}/transfers`, transfer);
  }
}
