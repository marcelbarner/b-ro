import { Component, Inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';
import {
  CreateTransactionDto,
  Transaction,
  TransactionType,
} from '../../../shared/interfaces/transaction.model';
import { TransactionService } from '../../../shared/services/transaction.service';

interface DialogData {
  accountId: string;
  accountCurrency: string;
}

@Component({
  selector: 'app-transaction-create-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    TranslateModule,
  ],
  templateUrl: './transaction-create-dialog.component.html',
  styleUrl: './transaction-create-dialog.component.scss',
})
export class TransactionCreateDialogComponent {
  transactionForm: FormGroup;
  isSubmitting = signal(false);
  TransactionType = TransactionType;

  constructor(
    public dialogRef: MatDialogRef<TransactionCreateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private snackBar: MatSnackBar
  ) {
    this.transactionForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      currency: [data.accountCurrency, Validators.required],
      type: [TransactionType.Deposit, Validators.required],
      description: ['', Validators.required],
      date: [new Date(), Validators.required],
    });

    // Update amount validation based on transaction type
    this.transactionForm.get('type')?.valueChanges.subscribe(type => {
      const amountControl = this.transactionForm.get('amount');
      if (type === TransactionType.Withdrawal) {
        amountControl?.setValidators([Validators.required, Validators.min(0.01)]);
      } else {
        amountControl?.setValidators([Validators.required, Validators.min(0.01)]);
      }
      amountControl?.updateValueAndValidity();
    });
  }

  onSubmit(): void {
    if (this.transactionForm.invalid) {
      return;
    }

    this.isSubmitting.set(true);

    const formValue = this.transactionForm.value;
    let amount = formValue.amount;

    // Convert to negative for withdrawals
    if (formValue.type === TransactionType.Withdrawal) {
      amount = -Math.abs(amount);
    }

    const dto: CreateTransactionDto = {
      amount,
      currency: formValue.currency,
      type: formValue.type,
      description: formValue.description,
      date: formValue.date.toISOString(),
    };

    this.transactionService.createTransaction(this.data.accountId, dto).subscribe({
      next: (transaction: Transaction) => {
        this.snackBar.open('Transaction created successfully', 'Close', { duration: 3000 });
        this.dialogRef.close(transaction);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.snackBar.open(err.message || 'Failed to create transaction', 'Close', {
          duration: 5000,
        });
      },
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  getAmountLabel(): string {
    const type = this.transactionForm.get('type')?.value;
    if (type === TransactionType.Withdrawal) {
      return 'transactions.create.amount_withdrawal';
    }
    return 'transactions.create.amount';
  }
}
