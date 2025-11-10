import { Component, Input, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TransactionService } from '../../../shared/services/transaction.service';
import { Transaction, TransactionType } from '../../../shared/interfaces/transaction.model';
import { TransactionDetailDialogComponent } from '../transaction-detail-dialog/transaction-detail-dialog.component';

@Component({
  selector: 'app-transaction-list',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './transaction-list.component.html',
  styleUrl: './transaction-list.component.scss',
})
export class TransactionListComponent implements OnInit {
  @Input() accountId!: string;
  @Input() accountCurrency!: string;

  transactions = signal<Transaction[]>([]);
  isLoading = signal(false);
  error = signal<string | null>(null);

  // Filter form
  filterForm = new FormGroup({
    fromDate: new FormControl<Date | null>(null),
    toDate: new FormControl<Date | null>(null),
    type: new FormControl<TransactionType | 'all'>('all'),
  });

  // Filtered and sorted transactions
  filteredTransactions = computed(() => {
    let result = this.transactions();

    // Apply date filter
    const fromDate = this.filterForm.value.fromDate;
    const toDate = this.filterForm.value.toDate;
    if (fromDate) {
      result = result.filter(t => new Date(t.date) >= fromDate);
    }
    if (toDate) {
      result = result.filter(t => new Date(t.date) <= toDate);
    }

    // Apply type filter
    const typeFilter = this.filterForm.value.type;
    if (typeFilter !== 'all') {
      result = result.filter(t => t.type === typeFilter);
    }

    // Sort by date (newest first)
    return result.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  });

  displayedColumns: string[] = [
    'date',
    'type',
    'description',
    'amount',
    'counterTransaction',
    'actions',
  ];

  TransactionType = TransactionType;

  constructor(
    private transactionService: TransactionService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadTransactions();

    // Reload when filters change
    this.filterForm.valueChanges.subscribe(() => {
      // Filtering is handled by computed signal
    });
  }

  loadTransactions(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.transactionService.getTransactionsByAccount(this.accountId).subscribe({
      next: transactions => {
        this.transactions.set(transactions);
        this.isLoading.set(false);
      },
      error: err => {
        this.error.set(err.message || 'transactions.error.load_failed');
        this.isLoading.set(false);
        this.snackBar.open('Failed to load transactions', 'Close', { duration: 5000 });
      },
    });
  }

  getTypeLabel(type: TransactionType): string {
    switch (type) {
      case TransactionType.Deposit:
        return 'transactions.type.deposit';
      case TransactionType.Withdrawal:
        return 'transactions.type.withdrawal';
      case TransactionType.Transfer:
        return 'transactions.type.transfer';
      default:
        return 'transactions.type.unknown';
    }
  }

  getTypeColor(type: TransactionType): string {
    switch (type) {
      case TransactionType.Deposit:
        return 'primary';
      case TransactionType.Withdrawal:
        return 'warn';
      case TransactionType.Transfer:
        return 'accent';
      default:
        return '';
    }
  }

  viewTransaction(transaction: Transaction): void {
    this.dialog.open(TransactionDetailDialogComponent, {
      width: '600px',
      data: { transaction, accountId: this.accountId },
    });
  }

  deleteTransaction(transaction: Transaction): void {
    if (!confirm('Are you sure you want to delete this transaction?')) {
      return;
    }

    this.transactionService.deleteTransaction(this.accountId, transaction.transactionId).subscribe({
      next: () => {
        this.snackBar.open('Transaction deleted successfully', 'Close', { duration: 3000 });
        this.loadTransactions();
      },
      error: err => {
        this.snackBar.open(err.message || 'Failed to delete transaction', 'Close', {
          duration: 5000,
        });
      },
    });
  }

  clearFilters(): void {
    this.filterForm.reset({ type: 'all' });
  }
}
