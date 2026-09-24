import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Centered branded loader for a screen's first load; background refreshes use a plain progress bar instead. */
@Component({
  selector: 'app-loading-indicator',
  templateUrl: './loading-indicator.html',
  styleUrl: './loading-indicator.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingIndicator {
  readonly label = input('Wird geladen …');
}
