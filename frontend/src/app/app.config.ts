import { registerLocaleData } from '@angular/common';
import localeDeAt from '@angular/common/locales/de-AT';
import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MAT_ICON_DEFAULT_OPTIONS } from '@angular/material/icon';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { errorInterceptor } from './core/interceptors/error-interceptor';
import { routes } from './app.routes';

registerLocaleData(localeDeAt);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor])),
    { provide: LOCALE_ID, useValue: 'de-AT' },
    provideNativeDateAdapter(),
    { provide: MAT_ICON_DEFAULT_OPTIONS, useValue: { fontSet: 'material-symbols-outlined' } },
  ],
};
