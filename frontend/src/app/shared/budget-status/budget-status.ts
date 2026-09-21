import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Money } from '../money/money';

/** Budget-vs-actual for one category: a progress bar plus the remaining or over-budget amount. */
@Component({
  selector: 'app-budget-status',
  imports: [MatProgressBarModule, Money],
  templateUrl: './budget-status.html',
  styleUrl: './budget-status.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BudgetStatus {
  readonly limit = input<number | null>(null);
  readonly spent = input.required<number>();
  readonly isOverBudget = input(false);
  readonly overBy = input<number | null>(null);

  protected readonly progress = computed(() => {
    const limit = this.limit();
    return limit ? Math.min(100, (this.spent() / limit) * 100) : 0;
  });

  protected readonly remaining = computed(() => {
    const limit = this.limit();
    return limit === null ? null : limit - this.spent();
  });
}
