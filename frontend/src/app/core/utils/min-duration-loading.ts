import { Signal, signal } from '@angular/core';

/** Once shown, a loading indicator stays up at least this long so a fast response doesn't flash it. */
const DEFAULT_MIN_VISIBLE_MS = 900;

export interface MinDurationLoading {
  readonly loading: Signal<boolean>;
  /** Call when the request starts. */
  start(): void;
  /** Call when the request settles (success or error). May keep `loading` true a bit longer. */
  stop(): void;
}

/**
 * Wraps a loading flag so it stays `true` for at least `minVisibleMs` once set, instead of
 * flipping back to `false` a few milliseconds later on a fast (e.g. local) response. Genuinely
 * slow requests are unaffected: the minimum only ever adds a delay to hiding, never to showing.
 */
export function createMinDurationLoading(minVisibleMs = DEFAULT_MIN_VISIBLE_MS): MinDurationLoading {
  const _loading = signal(false);
  let shownAt = 0;
  let hideTimer: ReturnType<typeof setTimeout> | undefined;

  return {
    loading: _loading.asReadonly(),
    start(): void {
      clearTimeout(hideTimer);
      shownAt = Date.now();
      _loading.set(true);
    },
    stop(): void {
      const remaining = minVisibleMs - (Date.now() - shownAt);
      if (remaining <= 0) {
        _loading.set(false);
      } else {
        hideTimer = setTimeout(() => _loading.set(false), remaining);
      }
    },
  };
}
