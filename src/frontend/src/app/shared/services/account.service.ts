import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  Account,
  AccountResponse,
  CreateAccountDto,
  UpdateAccountDto,
} from '../interfaces/account.model';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private readonly apiUrl = '/api/finance/accounts';

  constructor(private http: HttpClient) {}

  /**
   * Get all accounts
   */
  getAccounts(): Observable<Account[]> {
    return this.http
      .get<AccountResponse[]>(this.apiUrl)
      .pipe(map((accounts) => accounts.map(this.mapToAccount)));
  }

  /**
   * Get a single account by ID
   */
  getAccount(id: string): Observable<Account> {
    return this.http
      .get<AccountResponse>(`${this.apiUrl}/${id}`)
      .pipe(map(this.mapToAccount));
  }

  /**
   * Create a new account
   */
  createAccount(account: CreateAccountDto): Observable<Account> {
    return this.http
      .post<AccountResponse>(this.apiUrl, account)
      .pipe(map(this.mapToAccount));
  }

  /**
   * Update an existing account
   */
  updateAccount(id: string, account: UpdateAccountDto): Observable<Account> {
    return this.http
      .put<AccountResponse>(`${this.apiUrl}/${id}`, account)
      .pipe(map(this.mapToAccount));
  }

  /**
   * Delete an account
   */
  deleteAccount(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Map API response to Account model (convert ISO date strings to Date objects)
   */
  private mapToAccount(response: AccountResponse): Account {
    return {
      ...response,
      createdAt: new Date(response.createdAt),
      updatedAt: new Date(response.updatedAt),
    };
  }
}
