import { ChangeDetectionStrategy, Component, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent, CurrencyFormatPipe } from '@shared';
import { AccountService, Account, CurrencyService } from '@shared';
import { forkJoin } from 'rxjs';
import { TransactionListComponent } from '../transaction-list/transaction-list.component';
import { TransactionCreateDialogComponent } from '../transaction-create-dialog/transaction-create-dialog.component';
import { TransferDialogComponent } from '../transfer-dialog/transfer-dialog.component';

@Component({
  selector: 'app-account-detail',
  templateUrl: './account-detail.component.html',
  styleUrl: './account-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    TranslateModule,
    PageHeaderComponent,
    CurrencyFormatPipe,
    TransactionListComponent,
  ],
})
export class AccountDetailComponent implements OnInit {
  @ViewChild(TransactionListComponent) transactionList?: TransactionListComponent;

  account = signal<Account | null>(null);
  displayCurrency = signal<string>('EUR');
  convertedBalance = signal<number | null>(null);
  exchangeRate = signal<number | null>(null);
  lastRateUpdate = signal<string | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private accountService: AccountService,
    private currencyService: CurrencyService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    const accountId = this.route.snapshot.paramMap.get('id');
    if (accountId) {
      this.loadAccount(accountId);
    }

    // Subscribe to currency changes
    this.currencyService.selectedCurrency$.subscribe(currency => {
      this.displayCurrency.set(currency);
      const acc = this.account();
      if (acc && acc.currency !== currency) {
        this.loadConversionData(acc.balance, acc.currency, currency);
      } else {
        this.convertedBalance.set(null);
        this.exchangeRate.set(null);
      }
    });
  }

  loadAccount(id: string): void {
    this.isLoading.set(true);
    this.accountService.getAccount(id).subscribe({
      next: (account) => {
        this.account.set(account);
        this.isLoading.set(false);

        const displayCurr = this.displayCurrency();
        if (account.currency !== displayCurr) {
          this.loadConversionData(account.balance, account.currency, displayCurr);
        }
      },
      error: (error) => {
        this.error.set('Failed to load account');
        this.isLoading.set(false);
        console.error('Error loading account:', error);
      },
    });
  }

  loadConversionData(amount: number, fromCurrency: string, toCurrency: string): void {
    forkJoin({
      conversion: this.currencyService.convertAmount(amount, fromCurrency, toCurrency),
      currencies: this.currencyService.getSupportedCurrencies()
    }).subscribe({
      next: ({ conversion, currencies }) => {
        this.convertedBalance.set(conversion.convertedAmount);
        this.exchangeRate.set(conversion.rate);
        this.lastRateUpdate.set(currencies.lastUpdate);
      },
      error: (error) => {
        console.error('Error loading conversion data:', error);
        this.convertedBalance.set(null);
        this.exchangeRate.set(null);
      }
    });
  }

  createTransaction(): void {
    const acc = this.account();
    if (!acc) return;

    const dialogRef = this.dialog.open(TransactionCreateDialogComponent, {
      width: '500px',
      data: {
        accountId: acc.id,
        accountCurrency: acc.currency,
      },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Reload account and transactions
        this.loadAccount(acc.id);
        this.transactionList?.loadTransactions();
      }
    });
  }

  createTransfer(): void {
    const acc = this.account();
    if (!acc) return;

    const dialogRef = this.dialog.open(TransferDialogComponent, {
      width: '600px',
      data: {
        currentAccountId: acc.id,
      },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Reload account and transactions
        this.loadAccount(acc.id);
        this.transactionList?.loadTransactions();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/finance/accounts']);
  }
}
