import { Injectable, effect, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'pfm-theme';

/** Light/dark toggle, persisted in localStorage and applied via `color-scheme` so Material's `light-dark()` tokens follow it. */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _mode = signal<ThemeMode>(this.readInitialMode());

  readonly mode = this._mode.asReadonly();

  constructor() {
    effect(() => {
      const mode = this._mode();
      document.documentElement.style.colorScheme = mode;
      localStorage.setItem(STORAGE_KEY, mode);
    });
  }

  toggle(): void {
    const next: ThemeMode = this._mode() === 'dark' ? 'light' : 'dark';
    const doc = document as Document & { startViewTransition?: (callback: () => void) => void };

    // Crossfades old vs. new colors instead of an instant, jarring flip. Falls back to a plain
    // switch on browsers without the View Transition API (e.g. Firefox).
    if (typeof doc.startViewTransition === 'function') {
      doc.startViewTransition(() => this._mode.set(next));
    } else {
      this._mode.set(next);
    }
  }

  private readInitialMode(): ThemeMode {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    const prefersDark = typeof window.matchMedia === 'function' && window.matchMedia('(prefers-color-scheme: dark)').matches;
    return prefersDark ? 'dark' : 'light';
  }
}
