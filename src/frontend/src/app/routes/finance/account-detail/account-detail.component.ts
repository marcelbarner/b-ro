import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PageHeaderComponent } from '@shared';
import { AccountService, Account } from '@shared';

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
    PageHeaderComponent,
  ],
})
export class AccountDetailComponent implements OnInit {
  account: Account | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    const accountId = this.route.snapshot.paramMap.get('id');
    if (accountId) {
      this.loadAccount(accountId);
    }
  }

  loadAccount(id: string): void {
    this.isLoading = true;
    this.accountService.getAccount(id).subscribe({
      next: (account) => {
        this.account = account;
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load account';
        this.isLoading = false;
        console.error('Error loading account:', error);
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/finance/accounts']);
  }
}
