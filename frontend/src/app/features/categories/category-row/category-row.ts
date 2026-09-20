import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import { Category } from '../../../core/models/category';

/** One row of the category list: colour-tinted icon, name, edit and delete actions. */
@Component({
  selector: 'app-category-row',
  imports: [MatIconModule, MatIconButton],
  templateUrl: './category-row.html',
  styleUrl: './category-row.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryRow {
  readonly category = input.required<Category>();
  readonly edit = output<void>();
  readonly delete = output<void>();
}
