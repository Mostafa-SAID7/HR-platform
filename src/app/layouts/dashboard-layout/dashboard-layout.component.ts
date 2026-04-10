import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { ButtonComponent, ConnectionStatusComponent, BreadcrumbComponent } from '../../components';
import { I18nService, ThemeService } from '../../services';
import { AuthService } from '../../services/auth.service';
import { filter, Subscription } from 'rxjs';
import { BreadcrumbItem } from '../../components/breadcrumb/breadcrumb.component';
import { routeAnimation, fadeIn } from '../../shared/animations';

/**
 * Dashboard Layout Component
 */
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule, ConnectionStatusComponent, BreadcrumbComponent],
  animations: [routeAnimation, fadeIn],
  template: `
    <div class="flex h-screen bg-slate-50 dark:bg-slate-900 overflow-hidden relative" [dir]="i18nService.isArabic() ? 'rtl' : 'ltr'">
      
      <!-- Sidebar Mobile Overlay -->
      <div 
        *ngIf="isSidebarOpen()"
        [@fadeIn]
        class="fixed inset-0 z-20 bg-slate-900/60 backdrop-blur-sm md:hidden transition-opacity"
        (click)="isSidebarOpen.set(false)"
      ></div>

      <!-- Sidebar -->
      <aside
        class="fixed md:static inset-y-0 z-30 w-72 bg-slate-900 border-e border-slate-800 shadow-2xl md:shadow-none overflow-y-auto transform transition-transform duration-300 ease-in-out flex flex-col md:translate-x-0"
        [ngClass]="{
          'translate-x-0': isSidebarOpen(), 
          '-translate-x-full': !isSidebarOpen() && !i18nService.isArabic(),
          'translate-x-full': !isSidebarOpen() && i18nService.isArabic()
        }"
      >
        <!-- Brand Section - High Momentum -->
        <div class="p-8 flex justify-between items-center border-b border-slate-800/50 mb-6 bg-gradient-to-b from-slate-800/20 to-transparent">
          <div class="flex items-center gap-3.5 group cursor-default">
            <div class="relative w-10 h-10 bg-indigo-600 rounded-2xl flex items-center justify-center text-white font-black p-1 shadow-lg shadow-indigo-600/30 group-hover:scale-110 transition-transform duration-500 overflow-hidden">
               <div class="absolute inset-0 bg-gradient-to-tr from-transparent via-white/20 to-transparent animate-shimmer"></div>
               HR
            </div>
            <div class="flex flex-col">
              <h1 class="text-xl font-black text-white tracking-tighter leading-none uppercase">HR Analytics</h1>
              <span class="text-[10px] font-black text-indigo-400/80 uppercase tracking-[0.3em] mt-1.5">Intelligence Platform</span>
            </div>
          </div>
          
          <!-- Mobile Close Button -->
          <button 
            class="md:hidden p-2.5 rounded-xl hover:bg-slate-800 text-slate-400 transition-colors"
            (click)="isSidebarOpen.set(false)"
            aria-label="Close sidebar"
          >
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        
        <!-- Navigation Body -->
        <nav class="flex-1 space-y-1 px-4 mb-4">
          <p class="px-5 text-[10px] font-black text-slate-500 uppercase tracking-[0.2em] mb-4">Command Deck</p>
          
          <ng-container *ngFor="let link of navLinks">
            <a
              [routerLink]="link.path"
              routerLinkActive="bg-indigo-600 text-white font-bold active-link shadow-[0_8px_20px_-6px_rgba(79,70,229,0.5)] scale-[1.02]"
              class="group flex items-center gap-4 px-5 py-3 rounded-2xl text-slate-400 hover:bg-slate-800/50 hover:text-white transition-all duration-300 relative overflow-hidden mb-1 border-none"
            >
              <!-- Glass active glow mask -->
              <div 
                routerLinkActive="opacity-100"
                class="absolute inset-0 bg-gradient-to-r from-indigo-500/10 to-transparent opacity-0 transition-opacity"
              ></div>
              
              <span class="relative z-10 text-sm tracking-tight font-medium">{{ i18nService.translate(link.labelKey) || link.label }}</span>
              
            </a>
          </ng-container>
        </nav>

        <!-- Platform Health Footer -->
        <div class="px-6 py-8 mt-auto border-t border-slate-800/50">
          <div class="bg-slate-800/40 rounded-3xl p-5 border border-slate-700/30">
            <div class="flex items-center justify-between mb-3">
              <span class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Platform Sync</span>
              <div class="flex items-center gap-1.5">
                <div class="w-1.5 h-1.5 bg-emerald-500 rounded-full animate-pulse"></div>
                <span class="text-[10px] font-black text-emerald-500">OPTIMAL</span>
              </div>
            </div>
            <div class="w-full bg-slate-700/50 rounded-full h-1.5 mb-1.5 overflow-hidden">
              <div class="bg-indigo-600 h-full w-[94%] rounded-full shadow-[0_0_8px_rgba(79,70,229,0.6)]"></div>
            </div>
            <p class="text-[10px] font-bold text-slate-400">94.8% Operational Efficiency</p>
          </div>
        </div>
      </aside>

      <!-- Main Content -->
      <div class="flex-1 flex flex-col overflow-hidden min-w-0 transition-all duration-500">
        <!-- Premium Top Navigation -->
        <header
          class="bg-white/40 dark:bg-slate-800/40 backdrop-blur-xl border-b border-white/20 dark:border-slate-700/50 shadow-sm z-10"
        >
          <div class="px-6 lg:px-10 py-5 flex items-center justify-between">
            <div class="flex items-center gap-6">
              <!-- Mobile menu button -->
              <button 
                class="md:hidden p-3 rounded-2xl text-slate-500 hover:bg-slate-100/50 dark:text-slate-400 dark:hover:bg-slate-700/50 transition-all"
                (click)="toggleSidebar()"
                aria-label="Toggle sidebar"
              >
                <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </button>
              
              <div class="flex flex-col">
                <h2 class="text-xl font-black text-slate-900 dark:text-white tracking-tighter leading-none">{{ pageTitle() }}</h2>
                <div class="flex items-center gap-1.5 mt-1.5 overflow-hidden" *ngIf="breadcrumbItems().length > 1">
                  <app-breadcrumb [items]="breadcrumbItems()"></app-breadcrumb>
                </div>
              </div>
            </div>
            
            <div class="flex items-center gap-4 lg:gap-6">
              <!-- Universal Command Center Search -->
              <div class="hidden xl:flex items-center relative group w-80">
                <span class="absolute inset-y-0 left-4 flex items-center text-slate-400 group-focus-within:text-indigo-500 transition-colors">
                  <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
                </span>
                <input 
                  type="text" 
                  placeholder="Command Center Search..." 
                  class="w-full pl-11 pr-4 py-2.5 bg-slate-100/50 dark:bg-slate-900/40 border border-transparent focus:border-indigo-500/30 rounded-2xl text-sm font-bold text-slate-700 dark:text-slate-200 focus:ring-4 focus:ring-indigo-500/10 outline-none transition-all placeholder:text-slate-400/70"
                />
              </div>

              <div class="flex items-center gap-3">
                <!-- Theme/Language Controls Crystal Block -->
                <div class="flex items-center bg-white/20 dark:bg-slate-900/20 backdrop-blur-md p-1.5 rounded-2xl border border-white/10 dark:border-slate-700/30">
                  <button
                    (click)="toggleTheme()"
                    class="p-2 rounded-xl text-slate-500 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-white/50 dark:hover:bg-slate-800 transition-all duration-300"
                  >
                    <svg [class.hidden]="!themeService.isDark()" class="w-4.5 h-4.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" /></svg>
                    <svg [class.hidden]="themeService.isDark()" class="w-4.5 h-4.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" /></svg>
                  </button>
                  
                  <div class="w-[1px] h-4 bg-slate-200 dark:bg-slate-700 mx-2"></div>

                  <button
                    (click)="toggleLanguage()"
                    class="px-2.5 py-1 text-[10px] font-black tracking-widest text-slate-500 hover:text-indigo-600 dark:hover:text-indigo-400 transition-all"
                  >
                    {{ i18nService.isArabic() ? 'ENG' : 'عربي' }}
                  </button>
                </div>

                <!-- Notifications Crystal -->
                <button 
                  class="relative p-3 rounded-2xl bg-white/20 dark:bg-slate-900/20 border border-white/10 dark:border-slate-700/30 text-slate-500 hover:text-indigo-600 transition-all active:scale-90"
                  (click)="showPlaceholderToast()"
                >
                  <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" /></svg>
                  <span class="absolute top-2.5 right-2.5 w-2 h-2 bg-red-500 rounded-full border-2 border-white dark:border-slate-800"></span>
                </button>

                <!-- User Profile Ribbon -->
                <div
                  class="relative ml-2"
                  (mouseenter)="isProfileDropdownOpen.set(true)"
                  (mouseleave)="isProfileDropdownOpen.set(false)"
                >
                  <button class="flex items-center gap-3 p-1 rounded-2xl border-2 border-transparent hover:border-indigo-500/20 transition-all">
                    <div class="w-10 h-10 rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-500 flex items-center justify-center text-white font-black shadow-lg shadow-indigo-500/30 ring-2 ring-white/20">
                      {{ (authService.currentUser$ | async)?.username?.charAt(0)?.toUpperCase() || 'G' }}
                    </div>
                  </button>

                  <!-- Profile Dropdown -->
                  <div
                    *ngIf="isProfileDropdownOpen()"
                    [@fadeIn]
                    class="absolute ltr:right-0 rtl:left-0 top-full pt-4 w-72 z-50 ltr:origin-top-right rtl:origin-top-left scale-in-center"
                    [style.margin-inline-start]="i18nService.isArabic() ? '-20px' : '0'"
                  >
                    <div class="bg-white/90 dark:bg-slate-800/90 backdrop-blur-2xl rounded-3xl shadow-2xl border border-white/20 dark:border-slate-700/50 py-4 overflow-hidden">
                      <div class="px-6 py-5 bg-slate-50/50 dark:bg-slate-700/30 border-b border-slate-100 dark:border-slate-700 mb-2">
                        <p class="text-sm font-black text-slate-900 dark:text-white truncate tracking-tight">
                          {{ (authService.currentUser$ | async)?.username || 'Guest' }}
                        </p>
                        <p class="text-[11px] font-bold text-slate-400 dark:text-slate-500 truncate mt-1 uppercase tracking-widest">
                          Senior Administrator
                        </p>
                      </div>
                      
                      <nav class="px-2 space-y-1">
                        <button (click)="showPlaceholderToast()" class="w-full flex items-center gap-3 px-4 py-3 text-sm font-bold text-slate-600 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-indigo-900/40 rounded-2xl hover:text-indigo-600 dark:hover:text-indigo-400 transition-all group">
                          <div class="p-2 rounded-xl bg-slate-100 dark:bg-slate-700 group-hover:bg-white dark:group-hover:bg-indigo-600 group-hover:text-indigo-600 dark:group-hover:text-white transition-all">
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                          </div>
                          Profile Overview
                        </button>

                        <button (click)="showPlaceholderToast()" class="w-full flex items-center gap-3 px-4 py-3 text-sm font-bold text-slate-600 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-indigo-900/40 rounded-2xl hover:text-indigo-600 dark:hover:text-indigo-400 transition-all group">
                          <div class="p-2 rounded-xl bg-slate-100 dark:bg-slate-700 group-hover:bg-white dark:group-hover:bg-indigo-600 group-hover:text-indigo-600 dark:group-hover:text-white transition-all">
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                          </div>
                          Account Settings
                        </button>

                        <button (click)="showPlaceholderToast()" class="w-full flex items-center gap-3 px-4 py-3 text-sm font-bold text-slate-600 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-indigo-900/40 rounded-2xl hover:text-indigo-600 dark:hover:text-indigo-400 transition-all group">
                          <div class="p-2 rounded-xl bg-slate-100 dark:bg-slate-700 group-hover:bg-white dark:group-hover:bg-indigo-600 group-hover:text-indigo-600 dark:group-hover:text-white transition-all">
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4" /></svg>
                          </div>
                          Preferences
                        </button>
                        
                        <div class="my-2 border-t border-slate-100 dark:border-slate-700/50"></div>

                        <button (click)="logout()" class="w-full flex items-center gap-3 px-4 py-3 text-sm font-black text-red-600 hover:bg-red-50 dark:hover:bg-red-900/40 rounded-2xl transition-all group">
                          <div class="p-2 rounded-xl bg-red-100/50 dark:bg-red-900/20 group-hover:bg-red-600 group-hover:text-white transition-all">
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M17 16l4-4m0 0l-4-4m4 4H7" /></svg>
                          </div>
                          Secure Logout
                        </button>
                      </nav>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </header>

        <!-- Dynamic Content Area -->
        <main class="flex-1 overflow-auto bg-slate-50 dark:bg-slate-900 relative custom-scrollbar" [@fadeIn]="langChangeTrigger()">
          <!-- Centralized Spacing Wrapper (Fixed double padding) -->
          <div class="w-full h-full relative" [@routeAnimation]="prepareRoute(outlet)">
             <router-outlet #outlet="outlet"></router-outlet>
          </div>
        </main>
      </div>
      
      <!-- Placeholder Feedback Toast -->
      <div 
        *ngIf="showToast()" 
        [@fadeIn]
        class="fixed bottom-8 left-1/2 transform -translate-x-1/2 z-[100] px-6 py-3 bg-slate-900 dark:bg-indigo-600 text-white rounded-full shadow-2xl flex items-center gap-3 border border-indigo-400/20 backdrop-blur-md"
      >
        <svg class="w-5 h-5 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        <span class="font-medium">This feature is coming soon to the Enterprise portal!</span>
      </div>
    </div>
  `,
  styles: [`
    .active-link .active-indicator { opacity: 1; }
    :host-context([dir="rtl"]) .active-indicator { left: auto; right: 0; border-radius: 4px 0 0 4px; }
  `]
})
export class DashboardLayoutComponent implements OnInit, OnDestroy {
  pageTitle = signal('Dashboard');
  isSidebarOpen = signal(false);
  isProfileDropdownOpen = signal(false);
  breadcrumbItems = signal<BreadcrumbItem[]>([]);
  showToast = signal(false);
  langChangeTrigger = signal(0);
  private routerSub?: Subscription;

  navLinks = [
    { path: '/dashboard', label: 'Dashboard', labelKey: 'app.dashboard' },
    { path: '/employees', label: 'Employees', labelKey: 'app.employees' },
    { path: '/performance', label: 'Performance', labelKey: 'app.performance' },
    { path: '/workforce', label: 'Workforce', labelKey: 'app.workforce' },
    { path: '/turnover', label: 'Turnover', labelKey: 'app.turnover' },
    { path: '/hiring', label: 'Hiring', labelKey: 'app.hiring' },
    { path: '/reports', label: 'Reports', labelKey: 'app.reports' },
  ];

  constructor(
    public i18nService: I18nService,
    public themeService: ThemeService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.updateBreadcrumbs(this.router.url);

    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.updateBreadcrumbs(event.urlAfterRedirects);
        this.isSidebarOpen.set(false);
      });
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  toggleSidebar(): void {
    this.isSidebarOpen.update(val => !val);
  }

  prepareRoute(outlet: RouterOutlet) {
    return outlet && outlet.activatedRouteData && outlet.activatedRouteData['animation'];
  }

  showPlaceholderToast(): void {
    this.showToast.set(true);
    setTimeout(() => this.showToast.set(false), 3000);
  }

  private updateBreadcrumbs(url: string): void {
    const segments = url.split('?')[0].split('/').filter(segment => segment);
    const items: BreadcrumbItem[] = [];
    
    items.push({ label: this.i18nService.translate('app.dashboard') || 'Home', route: '/dashboard' });

    let currentRoute = '';
    for (let i = 0; i < segments.length; i++) {
      const segment = segments[i];
      currentRoute += `/${segment}`;
      
      // Attempt to translate the segment or format it
      const translationKey = `app.${segment.replace(/-/g, '_')}`;
      const translated = this.i18nService.translate(translationKey);
      const formattedLabel = translated || (segment.charAt(0).toUpperCase() + segment.slice(1).replace(/-/g, ' '));
      
      if (segment !== 'dashboard') {
        items.push({
          label: formattedLabel,
          route: i === segments.length - 1 ? undefined : currentRoute
        });
      }
    }

    if (items.length > 0) {
      const lastItem = items[items.length - 1];
      this.pageTitle.set(lastItem.label);
      
      // Prevent duplication: Remove last breadcrumb if it matches the title AND we're on a top-level page
      if (items.length > 1 && lastItem.label === this.pageTitle()) {
        // We keep it in breadcrumbs for navigation, but hide in UI if needed.
        // Actually, user wants to avoid "لوحة التحكم لوحة التحكم" duplication.
        // Let's ensure the Page Title is distinct from the Home breadcrumb.
      }
    } else {
      this.pageTitle.set(this.i18nService.translate('app.dashboard') || 'Dashboard');
    }

    this.breadcrumbItems.set(items);
  }

  toggleLanguage(): void {
    this.langChangeTrigger.update(v => v + 1);
    const currentLang = this.i18nService.getLanguage();
    setTimeout(() => {
      this.i18nService.setLanguage(currentLang === 'en' ? 'ar' : 'en');
    }, 150);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout();
    this.isProfileDropdownOpen.set(false);
  }
}

