import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** Renders a Euro amount as "€ 1.234,56", with the cents in a smaller size. */
@Component({
  selector: 'app-money',
  templateUrl: './money.html',
  styleUrl: './money.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Money {
  readonly amount = input.required<number>();

  private static readonly formatter = new Intl.NumberFormat('de-AT', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  protected readonly parts = computed(() => {
    const value = this.amount();
    const [whole, cents] = Money.formatter.format(Math.abs(value)).split(',');
    return { negative: value < 0, whole, cents };
  });
}
