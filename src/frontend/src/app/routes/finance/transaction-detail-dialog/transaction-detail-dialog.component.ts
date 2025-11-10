import { Component, Inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { TranslateModule } from '@ngx-translate/core';
import { Transaction, TransactionType } from '../../../shared/interfaces/transaction.model';
import { TransactionService } from '../../../shared/services/transaction.service';

interface DialogData {
  transaction: Transaction;
  accountId: string;
}

@Component({
  selector: 'app-transaction-detail-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    TranslateModule,
  ],
  templateUrl: './transaction-detail-dialog.component.html',
  styleUrl: './transaction-detail-dialog.component.scss',
})
export class TransactionDetailDialogComponent implements OnInit {
  transaction = signal<Transaction | null>(null);
  counterTransaction = signal<Transaction | null>(null);
  isLoadingCounter = signal(false);
  TransactionType = TransactionType;

  constructor(
    public dialogRef: MatDialogRef<TransactionDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private transactionService: TransactionService
  ) {
    this.transaction.set(data.transaction);
  }

  ngOnInit(): void {
    // Load counter-transaction if it exists
    if (this.data.transaction.counterTransactionId) {
      this.loadCounterTransaction();
    }
  }

  loadCounterTransaction(): void {
    const counterTxId = this.data.transaction.counterTransactionId;
    if (!counterTxId) return;

    this.isLoadingCounter.set(true);

    // We need to fetch the counter transaction - for now we'll skip detailed loading
    // In a real app, we'd need the account ID of the counter transaction
    // For now, we just show the ID
    this.isLoadingCounter.set(false);
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

  close(): void {
    this.dialogRef.close();
  }
}
