import { Component, Inject, signal, computed, OnInit } from '@angular/core';
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
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';
import {
  CreateTransferDto,
  TransferResult,
} from '../../../shared/interfaces/transaction.model';
import { TransactionService } from '../../../shared/services/transaction.service';
import { AccountService } from '../../../shared/services/account.service';
import { Account } from '../../../shared/interfaces/account.model';

interface DialogData {
  currentAccountId?: string;
}

@Component({
  selector: 'app-transfer-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatDividerModule,
    TranslateModule,
  ],
  templateUrl: './transfer-dialog.component.html',
  styleUrl: './transfer-dialog.component.scss',
})
export class TransferDialogComponent implements OnInit {
  transferForm: FormGroup;
  accounts = signal<Account[]>([]);
  isLoadingAccounts = signal(false);
  isSubmitting = signal(false);
  showPreview = signal(false);

  // Computed values for preview
  fromAccount = computed(() => {
    const fromId = this.transferForm.get('fromAccountId')?.value;
    return this.accounts().find(a => a.id === fromId) || null;
  });

  toAccount = computed(() => {
    const toId = this.transferForm.get('toAccountId')?.value;
    return this.accounts().find(a => a.id === toId) || null;
  });

  transferAmount = computed(() => {
    return this.transferForm.get('amount')?.value || 0;
  });

  canTransfer = computed(() => {
    const from = this.fromAccount();
    const amount = this.transferAmount();
    return from && from.balance >= amount;
  });

  constructor(
    public dialogRef: MatDialogRef<TransferDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private accountService: AccountService,
    private snackBar: MatSnackBar
  ) {
    this.transferForm = this.fb.group({
      fromAccountId: [data.currentAccountId || null, Validators.required],
      toAccountId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      description: ['', Validators.required],
    });

    // Validate that from and to accounts are different
    this.transferForm.get('toAccountId')?.valueChanges.subscribe(() => {
      this.validateAccounts();
    });
    this.transferForm.get('fromAccountId')?.valueChanges.subscribe(() => {
      this.validateAccounts();
    });
  }

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.isLoadingAccounts.set(true);
    this.accountService.getAccounts().subscribe({
      next: accounts => {
        this.accounts.set(accounts);
        this.isLoadingAccounts.set(false);
      },
      error: err => {
        this.snackBar.open('Failed to load accounts', 'Close', { duration: 5000 });
        this.isLoadingAccounts.set(false);
      },
    });
  }

  validateAccounts(): void {
    const fromId = this.transferForm.get('fromAccountId')?.value;
    const toId = this.transferForm.get('toAccountId')?.value;

    if (fromId && toId && fromId === toId) {
      this.transferForm.get('toAccountId')?.setErrors({ sameAccount: true });
    }
  }

  preview(): void {
    if (this.transferForm.invalid) {
      return;
    }
    this.showPreview.set(true);
  }

  backToForm(): void {
    this.showPreview.set(false);
  }

  confirmTransfer(): void {
    if (this.transferForm.invalid || !this.canTransfer()) {
      return;
    }

    this.isSubmitting.set(true);

    const formValue = this.transferForm.value;
    const dto: CreateTransferDto = {
      fromAccountId: formValue.fromAccountId,
      toAccountId: formValue.toAccountId,
      amount: formValue.amount,
      description: formValue.description,
    };

    this.transactionService.createTransfer(dto).subscribe({
      next: (result: TransferResult) => {
        this.snackBar.open('Transfer completed successfully', 'Close', { duration: 3000 });
        this.dialogRef.close(result);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.snackBar.open(err.message || 'Failed to create transfer', 'Close', {
          duration: 5000,
        });
      },
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  getAccountLabel(account: Account): string {
    return `${account.name} (${account.balance.toFixed(2)} ${account.currency})`;
  }
}
