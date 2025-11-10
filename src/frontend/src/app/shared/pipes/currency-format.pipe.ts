import { Pipe, PipeTransform } from '@angular/core';
import { CurrencyService } from '../services/currency.service';

/**
 * Pipe to format amounts with currency symbol
 * Usage: {{ 1234.56 | currencyFormat:'USD' }}
 * Output: $1,234.56
 */
@Pipe({
  name: 'currencyFormat',
  standalone: true,
})
export class CurrencyFormatPipe implements PipeTransform {
  constructor(private currencyService: CurrencyService) {}

  transform(value: number | null | undefined, currencyCode?: string): string {
    if (value === null || value === undefined) {
      return '-';
    }

    if (!currencyCode) {
      currencyCode = this.currencyService.getDisplayCurrency();
    }

    return this.currencyService.formatAmount(value, currencyCode);
  }
}
