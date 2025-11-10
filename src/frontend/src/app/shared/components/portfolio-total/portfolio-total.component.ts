import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { CurrencyService } from '../../services/currency.service';
import { AccountService } from '../../services/account.service';
import { CurrencyFormatPipe } from '../../pipes/currency-format.pipe';
import { PortfolioTotal } from '../../interfaces/account.model';

@Component({
  selector: 'app-portfolio-total',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    TranslateModule,
    CurrencyFormatPipe,
  ],
  template: `
    <mat-card class="portfolio-card">
      <mat-card-header>
        <mat-icon mat-card-avatar>account_balance_wallet</mat-icon>
        <mat-card-title>{{ 'finance.portfolio_total' | translate }}</mat-card-title>
        <mat-card-subtitle>{{ 'finance.all_accounts' | translate }}</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        @if (isLoading) {
          <div class="loading-container">
            <mat-spinner diameter="40"></mat-spinner>
          </div>
        } @else if (portfolioTotal) {
          <div class="total-container">
            <div class="total-amount">
              {{ portfolioTotal.totalBalance | currencyFormat:portfolioTotal.currency }}
            </div>
            <div class="account-count">
              {{ portfolioTotal.accountCount }}
              {{ portfolioTotal.accountCount === 1 ? ('finance.account' | translate) : ('finance.accounts' | translate) }}
            </div>
            <div class="calculated-at" [matTooltip]="portfolioTotal.calculatedAt | date:'medium'">
              {{ 'finance.last_updated' | translate }}: {{ portfolioTotal.calculatedAt | date:'short' }}
            </div>
          </div>
        } @else {
          <div class="error-container">
            <mat-icon>error_outline</mat-icon>
            <p>{{ 'finance.failed_to_load_portfolio' | translate }}</p>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [
    `
      .portfolio-card {
        max-width: 400px;
        margin: 16px;

        mat-card-header {
          margin-bottom: 16px;

          mat-icon[mat-card-avatar] {
            font-size: 40px;
            width: 40px;
            height: 40px;
            color: #1976d2;
          }
        }

        .loading-container {
          display: flex;
          justify-content: center;
          align-items: center;
          min-height: 120px;
        }

        .total-container {
          text-align: center;

          .total-amount {
            font-size: 2.5em;
            font-weight: 600;
            color: #1976d2;
            margin-bottom: 8px;
          }

          .account-count {
            font-size: 1.1em;
            color: rgba(0, 0, 0, 0.6);
            margin-bottom: 12px;
          }

          .calculated-at {
            font-size: 0.85em;
            color: rgba(0, 0, 0, 0.54);
            font-style: italic;
          }
        }

        .error-container {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          min-height: 120px;
          color: #d32f2f;

          mat-icon {
            font-size: 48px;
            width: 48px;
            height: 48px;
            margin-bottom: 8px;
          }

          p {
            margin: 0;
          }
        }
      }
    `,
  ],
})
export class PortfolioTotalComponent implements OnInit, OnDestroy {
  portfolioTotal: PortfolioTotal | null = null;
  isLoading = false;
  private destroy$ = new Subject<void>();

  constructor(
    private currencyService: CurrencyService,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    // React to currency changes
    this.currencyService.selectedCurrency$
      .pipe(
        takeUntil(this.destroy$),
        switchMap((currency: string) => {
          this.isLoading = true;
          return this.accountService.getPortfolioTotal(currency);
        })
      )
      .subscribe({
        next: (portfolio) => {
          this.portfolioTotal = portfolio;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading portfolio total:', error);
          this.portfolioTotal = null;
          this.isLoading = false;
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
