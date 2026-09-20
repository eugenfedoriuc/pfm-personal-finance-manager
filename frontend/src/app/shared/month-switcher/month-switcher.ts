import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepicker, MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MonthStateService } from '../../core/state/month-state.service';

/** Previous/next buttons plus a "year view" datepicker for jumping to an arbitrary month. */
@Component({
  selector: 'app-month-switcher',
  imports: [MatButtonModule, MatIconModule, MatDatepickerModule],
  templateUrl: './month-switcher.html',
  styleUrl: './month-switcher.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MonthSwitcher {
  protected readonly monthState = inject(MonthStateService);

  protected readonly pickerDate = computed(() => {
    const { year, month } = this.monthState.selected();
    return new Date(year, month - 1, 1);
  });

  protected readonly label = computed(() =>
    this.pickerDate().toLocaleDateString('de-AT', { month: 'long', year: 'numeric' }),
  );

  protected onMonthSelected(date: Date, picker: MatDatepicker<Date>): void {
    this.monthState.set({ year: date.getFullYear(), month: date.getMonth() + 1 });
    // The year view lets the user drill further into a day; closing here keeps this a month picker.
    picker.close();
  }
}
