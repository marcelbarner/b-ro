import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSortModule } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { Account, CreateAccountDto, UpdateAccountDto } from '../../models/account.model';
import {
  AccountDialogComponent,
  AccountDialogData,
} from '../account-dialog/account-dialog.component';

@Component({
  selector: 'app-account-list',
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.scss',
})
export class AccountListComponent implements OnInit {
  accounts: Account[] = [];
  filteredAccounts: Account[] = [];
  displayedColumns: string[] = ['name', 'iban', 'currency', 'balance', 'actions'];
  searchTerm = '';
  isLoading = false;

  constructor(
    private accountService: AccountService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.isLoading = true;
    this.accountService.getAccounts().subscribe({
      next: (accounts) => {
        this.accounts = accounts;
        this.filteredAccounts = accounts;
        this.isLoading = false;
      },
      error: (error) => {
        this.snackBar.open('Failed to load accounts', 'Close', {
          duration: 3000,
        });
        this.isLoading = false;
        console.error('Error loading accounts:', error);
      },
    });
  }

  applyFilter(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredAccounts = this.accounts.filter(
      (account) =>
        account.name.toLowerCase().includes(term) ||
        account.iban.toLowerCase().includes(term) ||
        account.currency.toLowerCase().includes(term)
    );
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open<AccountDialogComponent, AccountDialogData>(
      AccountDialogComponent,
      {
        width: '500px',
        data: { mode: 'create' },
      }
    );

    dialogRef.afterClosed().subscribe((result: CreateAccountDto) => {
      if (result) {
        this.accountService.createAccount(result).subscribe({
          next: () => {
            this.snackBar.open('Account created successfully', 'Close', {
              duration: 3000,
            });
            this.loadAccounts();
          },
          error: (error) => {
            this.snackBar.open('Failed to create account', 'Close', {
              duration: 3000,
            });
            console.error('Error creating account:', error);
          },
        });
      }
    });
  }

  editAccount(account: Account): void {
    const dialogRef = this.dialog.open<AccountDialogComponent, AccountDialogData>(
      AccountDialogComponent,
      {
        width: '500px',
        data: { mode: 'edit', account },
      }
    );

    dialogRef.afterClosed().subscribe((result: UpdateAccountDto) => {
      if (result) {
        this.accountService.updateAccount(account.id, result).subscribe({
          next: () => {
            this.snackBar.open('Account updated successfully', 'Close', {
              duration: 3000,
            });
            this.loadAccounts();
          },
          error: (error) => {
            this.snackBar.open('Failed to update account', 'Close', {
              duration: 3000,
            });
            console.error('Error updating account:', error);
          },
        });
      }
    });
  }

  deleteAccount(account: Account): void {
    if (confirm(`Delete account "${account.name}"?`)) {
      this.accountService.deleteAccount(account.id).subscribe({
        next: () => {
          this.snackBar.open('Account deleted successfully', 'Close', {
            duration: 3000,
          });
          this.loadAccounts();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete account', 'Close', {
            duration: 3000,
          });
          console.error('Error deleting account:', error);
        },
      });
    }
  }

  viewAccount(account: Account): void {
    this.router.navigate(['/finance/accounts', account.id]);
  }
}
