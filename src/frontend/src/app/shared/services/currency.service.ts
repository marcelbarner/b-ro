import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface CurrencyInfo {
  code: string;
  name: string;
  symbol: string;
}

export interface CurrencyInfoResponse {
  currencies: string[];
  lastUpdate: string | null;
}

export interface ExchangeRateResponse {
  fromCurrency: string;
  toCurrency: string;
  rate: number;
  date: string;
}

export interface ConversionResponse {
  amount: number;
  fromCurrency: string;
  toCurrency: string;
  convertedAmount: number;
  rate: number;
  date: string;
}

export interface PortfolioTotalResponse {
  currency: string;
  totalBalance: number;
  accountCount: number;
  calculatedAt: string;
}

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

@Injectable({
  providedIn: 'root',
})
export class CurrencyService {
  private readonly apiUrl = '/api/finance/currencies';
  private readonly accountsUrl = '/api/finance/accounts';

  // BehaviorSubject to track the currently selected display currency
  private selectedCurrencySubject = new BehaviorSubject<string>('EUR');
  public selectedCurrency$ = this.selectedCurrencySubject.asObservable();

  // Common currencies with display information
  private currencyInfo: Record<string, CurrencyInfo> = {
    EUR: { code: 'EUR', name: 'Euro', symbol: '€' },
    USD: { code: 'USD', name: 'US Dollar', symbol: '$' },
    GBP: { code: 'GBP', name: 'British Pound', symbol: '£' },
    CHF: { code: 'CHF', name: 'Swiss Franc', symbol: 'CHF' },
    JPY: { code: 'JPY', name: 'Japanese Yen', symbol: '¥' },
    CNY: { code: 'CNY', name: 'Chinese Yuan', symbol: '¥' },
    CAD: { code: 'CAD', name: 'Canadian Dollar', symbol: 'CA$' },
    AUD: { code: 'AUD', name: 'Australian Dollar', symbol: 'A$' },
    SEK: { code: 'SEK', name: 'Swedish Krona', symbol: 'kr' },
    NOK: { code: 'NOK', name: 'Norwegian Krone', symbol: 'kr' },
    DKK: { code: 'DKK', name: 'Danish Krone', symbol: 'kr' },
    PLN: { code: 'PLN', name: 'Polish Zloty', symbol: 'zł' },
    CZK: { code: 'CZK', name: 'Czech Koruna', symbol: 'Kč' },
    HUF: { code: 'HUF', name: 'Hungarian Forint', symbol: 'Ft' },
  };

  constructor(private http: HttpClient) {
    // Load saved currency preference from localStorage
    const savedCurrency = localStorage.getItem('displayCurrency');
    if (savedCurrency) {
      this.selectedCurrencySubject.next(savedCurrency);
    }
  }

  /**
   * Get list of supported currencies from the API
   */
  getSupportedCurrencies(): Observable<CurrencyInfoResponse> {
    return this.http.get<CurrencyInfoResponse>(this.apiUrl);
  }

  /**
   * Get exchange rate for a specific date and currency pair
   */
  getExchangeRate(
    fromCurrency: string,
    toCurrency: string,
    date?: string
  ): Observable<ExchangeRateResponse> {
    let params = new HttpParams()
      .set('from', fromCurrency)
      .set('to', toCurrency);

    if (date) {
      params = params.set('date', date);
    }

    return this.http.get<ExchangeRateResponse>(`${this.apiUrl}/exchange-rate`, { params });
  }

  /**
   * Convert an amount from one currency to another
   */
  convertAmount(
    amount: number,
    fromCurrency: string,
    toCurrency: string,
    date?: string
  ): Observable<ConversionResponse> {
    let params = new HttpParams()
      .set('amount', amount.toString())
      .set('from', fromCurrency)
      .set('to', toCurrency);

    if (date) {
      params = params.set('date', date);
    }

    return this.http.get<ConversionResponse>(`${this.apiUrl}/convert`, { params });
  }

  /**
   * Get portfolio total in a specific currency
   */
  getPortfolioTotal(currency: string): Observable<PortfolioTotalResponse> {
    const params = new HttpParams().set('currency', currency);
    return this.http.get<PortfolioTotalResponse>(`${this.accountsUrl}/total`, { params });
  }

  /**
   * Get all accounts with balances converted to specified currency
   */
  getAccountsWithConvertedBalances(
    currency: string
  ): Observable<AccountWithConvertedBalance[]> {
    const params = new HttpParams().set('currency', currency);
    return this.http.get<AccountWithConvertedBalance[]>(this.accountsUrl, { params });
  }

  /**
   * Get currency info (name, symbol) for a currency code
   */
  getCurrencyInfo(code: string): CurrencyInfo {
    return (
      this.currencyInfo[code] || {
        code,
        name: code,
        symbol: code,
      }
    );
  }

  /**
   * Get all available currency info
   */
  getAllCurrencyInfo(): CurrencyInfo[] {
    return Object.values(this.currencyInfo);
  }

  /**
   * Set the display currency and save to localStorage
   */
  setDisplayCurrency(currency: string): void {
    this.selectedCurrencySubject.next(currency);
    localStorage.setItem('displayCurrency', currency);
  }

  /**
   * Get the current display currency
   */
  getDisplayCurrency(): string {
    return this.selectedCurrencySubject.value;
  }

  /**
   * Format amount with currency symbol
   */
  formatAmount(amount: number, currencyCode: string): string {
    const info = this.getCurrencyInfo(currencyCode);
    const formattedAmount = amount.toFixed(2);

    // For EUR, USD, GBP - put symbol before
    if (['EUR', 'USD', 'GBP'].includes(currencyCode)) {
      return `${info.symbol}${formattedAmount}`;
    }

    // For others - put symbol after
    return `${formattedAmount} ${info.symbol}`;
  }
}
