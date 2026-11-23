import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'money',
  standalone: true
})
export class MoneyPipe implements PipeTransform {
  transform(value: number | null | undefined, currency: string = 'QAR'): string {
    if (value === null || value === undefined) return '0.00 ' + currency;
    return `${Number(value).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;
  }
}
