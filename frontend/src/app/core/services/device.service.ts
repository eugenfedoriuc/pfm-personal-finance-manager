import { Injectable, inject } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map, shareReplay } from 'rxjs';

// A plain width query, not `Breakpoints.Handset`: that one switches between a 599.98px portrait
// and a 959.98px landscape threshold, which flips unpredictably while resizing a desktop window
// (the drawer would appear and disappear depending on width/height ratio, not just width).
const HANDSET_QUERY = '(max-width: 959.98px)';

/** Tells the app shell when to collapse the sidenav into a mobile drawer. */
@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly isHandset$ = this.breakpointObserver.observe(HANDSET_QUERY).pipe(
    map((result) => result.matches),
    shareReplay(1),
  );
}
