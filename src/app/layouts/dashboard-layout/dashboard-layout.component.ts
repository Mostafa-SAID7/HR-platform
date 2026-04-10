import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { ButtonComponent, ConnectionStatusComponent, BreadcrumbComponent } from '../../components';
import { I18nService, ThemeService } from '../../services';
import { AuthService } from '../../services/auth.service';
import { filter, Subscription } from 'rxjs';
import { BreadcrumbItem } from '../../components/breadcrumb/breadcrumb.component';

/**
 * Dashboard Layout Component
 *
 * Main layout for the dashboard with:
 * - Responsive sidebar navigation
 * - Top navigation bar with user profile and settings
 * - Notifications placeholder
 * - Language and theme switchers
 * - Connection status indicator
 *
 * Requirements: 3.1, 5.3, 9.1, 10.1
 */
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule, ConnectionStatusComponent, BreadcrumbComponent],
  template: `
    <div class="flex h-screen bg-slate-50 dark:bg-slate-900 overflow-hidden relative">
      
      <!-- Sidebar Mobile Overlay -->
      <div 
        *ngIf="isSidebarOpen()"
        class="fixed inset-0 z-20 bg-slate-900/50 backdrop-blur-sm md:hidden transition-opacity"
        (click)="isSidebarOpen.set(false)"
      ></div>

      <!-- Sidebar -->
      <aside
        class="fixed md:static inset-y-0 left-0 z-30 w-64 bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700 shadow-sm overflow-y-auto transform transition-transform duration-300 ease-in-out"
        [ngClass]="{'translate-x-0': isSidebarOpen(), '-translate-x-full md:translate-x-0': !isSidebarOpen()}"
      >
        <div class="p-6 flex justify-between items-center">
          <h1 class="text-2xl font-bold text-indigo-600 dark:text-indigo-400">HR Analytics</h1>
          <!-- Mobile Close Button -->
          <button 
            class="md:hidden text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200 focus:outline-none"
            (click)="isSidebarOpen.set(false)"
            aria-label="Close sidebar"
          >
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <nav class="mt-4 md:mt-8 space-y-2 px-4">
          <a
            routerLink="/dashboard"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.dashboard') || 'Dashboard' }}
          </a>
          <a
            routerLink="/employees"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.employees') || 'Employees' }}
          </a>
          <a
            routerLink="/performance"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.performance') || 'Performance' }}
          </a>
          <a
            routerLink="/workforce"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.workforce') || 'Workforce' }}
          </a>
          <a
            routerLink="/turnover"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.turnover') || 'Turnover' }}
          </a>
          <a
            routerLink="/hiring-forecast"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.hiring') || 'Hiring Forecast' }}
          </a>
          <a
            routerLink="/reports"
            routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400"
            class="block px-4 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            {{ i18nService.translate('app.reports') || 'Reports' }}
          </a>
        </nav>
      </aside>

      <!-- Main Content -->
      <div class="flex-1 flex flex-col overflow-hidden min-w-0">
        <!-- Top Navigation -->
        <header
          class="bg-white dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700 shadow-sm z-10"
        >
          <div class="px-4 md:px-6 py-4 flex items-center justify-between">
            <div class="flex items-center gap-3">
              <!-- Mobile menu button -->
              <button 
                class="md:hidden p-2 -ml-2 rounded-md text-slate-500 hover:text-slate-700 hover:bg-slate-100 dark:text-slate-400 dark:hover:text-slate-200 dark:hover:bg-slate-700 focus:outline-none transition-colors"
                (click)="toggleSidebar()"
                aria-label="Toggle sidebar"
                [attr.aria-expanded]="isSidebarOpen()"
              >
                <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </button>
              <h2 class="text-xl font-semibold text-slate-900 dark:text-white hidden sm:block">{{ pageTitle() }}</h2>
            </div>
            
            <div class="flex items-center gap-2 md:gap-4">
              <!-- Connection Status -->
              <div class="hidden sm:block">
                <app-connection-status></app-connection-status>
              </div>

              <!-- Notifications -->
              <button class="relative p-2 rounded-lg text-slate-500 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-700 transition-colors" aria-label="Notifications">
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                </svg>
                <span class="absolute top-1.5 right-1.5 flex h-2 w-2">
                  <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                  <span class="relative inline-flex rounded-full h-2 w-2 bg-red-500"></span>
                </span>
              </button>

              <!-- Language Switcher -->
              <button
                (click)="toggleLanguage()"
                class="px-3 py-2 rounded-lg text-sm font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
                [attr.aria-label]="
                  'Switch language to ' + (i18nService.isArabic() ? 'English' : 'Arabic')
                "
              >
                {{ i18nService.isArabic() ? 'EN' : 'AR' }}
              </button>

              <!-- Theme Switcher -->
              <button
                (click)="toggleTheme()"
                class="p-2 rounded-lg text-slate-500 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-700 transition-colors"
                [attr.aria-label]="'Switch to ' + (themeService.isDark() ? 'light' : 'dark') + ' mode'"
              >
                <!-- Sun icon: shown in dark mode to switch to light -->
                <svg [class.hidden]="!themeService.isDark()" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                <!-- Moon icon: shown in light mode to switch to dark -->
                <svg [class.hidden]="themeService.isDark()" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              </button>

              <!-- User Profile Dropdown -->
              <div
                class="relative pl-2 md:pl-4 border-l border-slate-200 dark:border-slate-700"
                (mouseenter)="isProfileDropdownOpen.set(true)"
                (mouseleave)="isProfileDropdownOpen.set(false)"
              >
                <button
                  class="flex items-center gap-2 md:gap-3 focus:outline-none"
                  aria-haspopup="true"
                  [attr.aria-expanded]="isProfileDropdownOpen()"
                >
                  <div class="relative">
                    <div
                      class="w-8 h-8 md:w-10 md:h-10 rounded-full bg-gradient-to-tr from-indigo-600 to-indigo-400 flex items-center justify-center text-white font-bold shadow-md flex-shrink-0 transition-transform duration-200"
                      [class.scale-105]="isProfileDropdownOpen()"
                    >
                      <ng-container *ngIf="authService.currentUser$ | async as user; else guestFallback">
                        {{ user.username?.charAt(0)?.toUpperCase() || 'U' }}
                      </ng-container>
                      <ng-template #guestFallback>G</ng-template>
                    </div>
                    <!-- Online Status Indicator -->
                    <span class="absolute bottom-0 right-0 w-2.5 h-2.5 md:w-3 md:h-3 bg-emerald-500 border-2 border-white dark:border-slate-800 rounded-full"></span>
                  </div>

                  <span class="text-sm font-medium text-slate-700 dark:text-slate-300 hidden md:flex items-center gap-1 group">
                    <span class="truncate max-w-[120px]">{{ (authService.currentUser$ | async)?.username || 'Guest' }}</span>
                    <svg class="h-4 w-4 text-slate-400 group-hover:text-slate-600 dark:group-hover:text-slate-200 transition-colors" [class.rotate-180]="isProfileDropdownOpen()" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                    </svg>
                  </span>
                </button>

                <!-- Dropdown Menu: pt-2 bridges the gap between button and panel so mouseleave doesn't fire -->
                <div
                  *ngIf="isProfileDropdownOpen()"
                  class="absolute right-0 top-full pt-2 w-56 z-50"
                >
                <div class="bg-white dark:bg-slate-800 rounded-xl shadow-xl border border-slate-100 dark:border-slate-700 py-2">
                  <div class="px-4 py-3 border-b border-slate-100 dark:border-slate-700 mb-1">
                    <p class="text-sm font-semibold text-slate-900 dark:text-white truncate">
                      {{ (authService.currentUser$ | async)?.username || 'Guest User' }}
                    </p>
                    <p class="text-xs text-slate-500 dark:text-slate-400 truncate mt-0.5">
                      {{ (authService.currentUser$ | async)?.email || 'guest@example.com' }}
                    </p>
                  </div>
                  
                  <a href="#" class="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700/50 transition-colors">
                    <svg class="w-4 h-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    Profile Settings
                  </a>
                  
                  <a href="#" class="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700/50 transition-colors">
                    <svg class="w-4 h-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    Preferences
                  </a>

                  <div class="border-t border-slate-100 dark:border-slate-700 my-1"></div>

                  <button 
                    (click)="logout()" 
                    class="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors text-left"
                  >
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                    </svg>
                    Sign out
                  </button>
                </div>
                </div>
              </div>
            </div>
          </div>
        </header>

        <!-- Page Content -->
        <main class="flex-1 overflow-auto bg-slate-50 dark:bg-slate-900">
          <div class="p-4 md:p-6 space-y-4">
            <!-- Breadcrumbs -->
            <app-breadcrumb [items]="breadcrumbItems()"></app-breadcrumb>
            
            <div class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-slate-200 dark:border-slate-700 min-h-[calc(100vh-12rem)]">
              <router-outlet></router-outlet>
            </div>
          </div>
        </main>
      </div>
    </div>
  `,
})
export class DashboardLayoutComponent implements OnInit, OnDestroy {
  pageTitle = signal('Dashboard');
  isSidebarOpen = signal(false);
  isProfileDropdownOpen = signal(false);
  breadcrumbItems = signal<BreadcrumbItem[]>([]);
  private routerSub?: Subscription;

  constructor(
    public i18nService: I18nService,
    public themeService: ThemeService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Generate initial breadcrumbs
    this.updateBreadcrumbs(this.router.url);

    // Listen to router events for breadcrumbs and sidebar auto-close
    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.updateBreadcrumbs(event.urlAfterRedirects);
        // Auto close sidebar on mobile navigation
        this.isSidebarOpen.set(false);
      });
  }

  ngOnDestroy(): void {
    if (this.routerSub) {
      this.routerSub.unsubscribe();
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen.update(val => !val);
  }

  private updateBreadcrumbs(url: string): void {
    const segments = url.split('?')[0].split('/').filter(segment => segment);
    const items: BreadcrumbItem[] = [];
    
    // Default Home
    items.push({ label: this.i18nService.translate('app.dashboard') || 'Home', route: '/dashboard' });

    let currentRoute = '';
    for (let i = 0; i < segments.length; i++) {
      const segment = segments[i];
      currentRoute += `/${segment}`;
      
      // Basic formatting: capitalize first letter and replace hyphens with spaces
      const formattedLabel = segment.charAt(0).toUpperCase() + segment.slice(1).replace(/-/g, ' ');
      
      // Avoid duplicating 'dashboard' if it's the first segment
      if (segment !== 'dashboard') {
        items.push({
          label: formattedLabel,
          route: i === segments.length - 1 ? undefined : currentRoute
        });
      }
    }

    // Set page title to the last segment's label or default to Dashboard
    if (items.length > 0) {
      const lastItem = items[items.length - 1];
      this.pageTitle.set(lastItem.label);
    } else {
      this.pageTitle.set(this.i18nService.translate('app.dashboard') || 'Dashboard');
    }

    this.breadcrumbItems.set(items);
  }

  toggleLanguage(): void {
    const currentLang = this.i18nService.getLanguage();
    const newLang = currentLang === 'en' ? 'ar' : 'en';
    this.i18nService.setLanguage(newLang);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout();
    this.isProfileDropdownOpen.set(false);
  }
}
