import { ChangeDetectionStrategy, Component, inject, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { DeviceService } from './core/services/device.service';
import { ThemeService } from './core/services/theme.service';

interface NavigationItem {
  name: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatSidenavModule,
  ],
  styleUrl: './app.scss',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly deviceService = inject(DeviceService);
  private readonly themeService = inject(ThemeService);

  // A signal (not `async` pipe) so every read in the template agrees within one render pass.
  protected readonly isHandset = toSignal(this.deviceService.isHandset$, { initialValue: false });
  protected readonly drawer = viewChild<MatDrawer>('drawer');
  protected readonly themeMode = this.themeService.mode;

  protected readonly navigation: NavigationItem[] = [
    { name: 'Übersicht', icon: 'space_dashboard', path: '/dashboard' },
    { name: 'Transaktionen', icon: 'receipt_long', path: '/transactions' },
    { name: 'Kategorien', icon: 'category', path: '/categories' },
    { name: 'Budgets', icon: 'savings', path: '/budgets' },
  ];

  protected toggleMenu(): void {
    this.drawer()?.toggle();
  }

  protected closeMenu(): void {
    this.drawer()?.close();
  }

  protected toggleTheme(): void {
    this.themeService.toggle();
  }
}
