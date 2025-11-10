import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { CurrencyService, CurrencyInfo } from '../../services/currency.service';

@Component({
  selector: 'app-currency-selector',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatIconModule,
    MatTooltipModule,
    TranslateModule,
  ],
  template: `
    <mat-form-field appearance="outline" class="currency-selector">
      <mat-label>{{ 'currency.display_currency' | translate }}</mat-label>
      <mat-select
        [(ngModel)]="selectedCurrency"
        (selectionChange)="onCurrencyChange()"
        [matTooltip]="'currency.select_currency' | translate">
        <mat-option *ngFor="let currency of currencies" [value]="currency.code">
          <span class="currency-option">
            <span class="currency-symbol">{{ currency.symbol }}</span>
            <span class="currency-code">{{ currency.code }}</span>
            <span class="currency-name">- {{ currency.name }}</span>
          </span>
        </mat-option>
      </mat-select>
      <mat-icon matPrefix>currency_exchange</mat-icon>
    </mat-form-field>
  `,
  styles: [
    `
      .currency-selector {
        width: 220px;
      }

      .currency-option {
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .currency-symbol {
        font-weight: 600;
        min-width: 24px;
      }

      .currency-code {
        font-weight: 500;
        min-width: 40px;
      }

      .currency-name {
        color: rgba(0, 0, 0, 0.6);
        font-size: 0.9em;
      }
    `,
  ],
})
export class CurrencySelectorComponent implements OnInit {
  selectedCurrency: string = 'EUR';
  currencies: CurrencyInfo[] = [];

  constructor(private currencyService: CurrencyService) {}

  ngOnInit(): void {
    this.currencies = this.currencyService.getAllCurrencyInfo();
    this.selectedCurrency = this.currencyService.getDisplayCurrency();
  }

  onCurrencyChange(): void {
    this.currencyService.setDisplayCurrency(this.selectedCurrency);
  }
}
