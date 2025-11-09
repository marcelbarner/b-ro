import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSortModule } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PageHeaderComponent } from '@shared';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '@shared';
import { Account, CreateAccountDto, UpdateAccountDto } from '@shared';
import {
  AccountDialogComponent,
  AccountDialogData,
} from '../account-dialog/account-dialog.component';

@Component({
  selector: 'app-account-list',
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.scss',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSortModule,
    MatDialogModule,
    MatTooltipModule,
    PageHeaderComponent,
  ],
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
    private toastr: ToastrService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private translate: TranslateService
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
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.toastr.error('Failed to load accounts');
        this.isLoading = false;
        this.cdr.markForCheck();
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
            this.toastr.success(this.translate.instant('finance.account_created'));
            this.loadAccounts();
          },
          error: (error) => {
            this.toastr.error(this.translate.instant('finance.account_creation_failed'));
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
            this.toastr.success(this.translate.instant('finance.account_updated'));
            this.loadAccounts();
          },
          error: (error) => {
            this.toastr.error(this.translate.instant('finance.account_update_failed'));
            console.error('Error updating account:', error);
          },
        });
      }
    });
  }

  deleteAccount(account: Account): void {
    const confirmMessage = this.translate.instant('finance.confirm_delete_account');
    if (confirm(confirmMessage)) {
      this.accountService.deleteAccount(account.id).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('finance.account_deleted'));
          this.loadAccounts();
        },
        error: (error) => {
          this.toastr.error(this.translate.instant('finance.account_deletion_failed'));
          console.error('Error deleting account:', error);
        },
      });
    }
  }

  viewAccount(account: Account): void {
    this.router.navigate(['/finance/accounts', account.id]);
  }
}
