import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ConnectionStatusComponent } from '../../shared/components';
import { I18nService, ThemeService, AuthService } from '../../core';
import { filter, Subscription } from 'rxjs';
import { routeAnimation, fadeIn } from '../../shared/animations';

interface BreadcrumbItemData {
  label: string;
  path?: string;
}

interface NavLink {
  path: string;
  label: string;
  labelKey: string;
  icon?: SafeHtml;
}

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
}

/**
 * Dashboard Layout Component
 * Responsive layout with sidebar, header, and main content area
 * Supports RTL (Arabic) and Dark mode
 * Requirements: 1.1, 1.2, 1.3
 */
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [
    CommonModule,
    NgFor,
    NgIf,
    RouterOutlet,
    RouterModule,
    ConnectionStatusComponent,
  ],
  animations: [routeAnimation, fadeIn],
  templateUrl: './dashboard-layout.component.html',
  styleUrl: './dashboard-layout.component.css'
})
export class DashboardLayoutComponent implements OnInit, OnDestroy {
  // Services
  public i18nService = inject(I18nService);
  public themeService = inject(ThemeService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);

  private svg(s: string): SafeHtml { return this.sanitizer.bypassSecurityTrustHtml(s); }

  // Signals
  public isSidebarOpen = signal(false);
  public isUserMenuOpen = signal(false);
  public pageTitle = signal('Dashboard');
  public breadcrumbItems = signal<BreadcrumbItemData[]>([]);
  public currentUser = signal<User | null>(null);

  // Navigation Links with inline SVG icons (SafeHtml – assigned in constructor)
  public navLinks: NavLink[] = [];

  private navigationSubscription: Subscription | null = null;

  ngOnInit(): void {
    // Initialize nav links with trusted SVG icons
    this.navLinks = [
      { path: '/dashboard',   label: 'Dashboard',   labelKey: 'nav.dashboard',   icon: this.svg(`<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>`) },
      { path: '/employees',   label: 'Employees',   labelKey: 'nav.employees',   icon: this.svg(`<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`) },
      { path: '/performance', label: 'Performance', labelKey: 'nav.performance', icon: this.svg(`<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>`) },
      { path: '/recruitment', label: 'Recruitment', labelKey: 'nav.recruitment', icon: this.svg(`<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/><line x1="11" y1="8" x2="11" y2="14"/><line x1="8" y1="11" x2="14" y2="11"/></svg>`) },
      { path: '/analytics',   label: 'Analytics',  labelKey: 'nav.analytics',   icon: this.svg(`<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>`) },
      { path: '/attendance',  label: 'Attendance', labelKey: 'nav.attendance',  icon: this.svg(`<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/><polyline points="9 16 11 18 15 14"/></svg>`) },
    ];

    // Close sidebar on mobile on route change
    this.navigationSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.isSidebarOpen.set(false);
        this.isUserMenuOpen.set(false);
        this.updatePageTitle(event.urlAfterRedirects);
        this.updateBreadcrumb(event.urlAfterRedirects);
      });

    // Load current user
    this.loadCurrentUser();
  }

  ngOnDestroy(): void {
    if (this.navigationSubscription) {
      this.navigationSubscription.unsubscribe();
    }
  }

  /**
   * Toggle sidebar open/close
   */
  public toggleSidebar(): void {
    this.isSidebarOpen.update(state => !state);
  }

  /**
   * Close sidebar
   */
  public closeSidebar(): void {
    this.isSidebarOpen.set(false);
  }

  /**
   * Toggle theme
   */
  public toggleTheme(): void {
    const currentTheme = this.themeService.theme();
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    this.themeService.theme.set(newTheme);
  }

  /**
   * Toggle language between English and Arabic
   */
  public toggleLanguage(): void {
    const newLang = this.i18nService.isArabic() ? 'en' : 'ar';
    this.i18nService.setLanguage(newLang);
  }

  /**
   * Logout user
   */
  public logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  /**
   * Load current user data
   */
  private loadCurrentUser(): void {
    // TODO: Load from auth service
    this.currentUser.set({
      id: '1',
      name: 'John Doe',
      email: 'john@example.com',
      role: 'Manager'
    });
  }

  /**
   * Update page title based on route
   */
  private updatePageTitle(url: string): void {
    const titles: { [key: string]: string } = {
      '/dashboard': 'Dashboard',
      '/employees': 'Employees',
      '/performance': 'Performance',
      '/recruitment': 'Recruitment',
      '/analytics': 'Analytics',
      '/attendance': 'Attendance',
      '/settings': 'Settings',
    };

    const title = Object.keys(titles).find(path => url.startsWith(path));
    this.pageTitle.set(title ? titles[title] : 'Dashboard');
  }

  /**
   * Update breadcrumb items based on route
   */
  private updateBreadcrumb(url: string): void {
    const breadcrumbs: { [key: string]: BreadcrumbItemData[] } = {
      '/dashboard': [{ label: 'Dashboard' }],
      '/employees': [
        { label: 'Dashboard', path: '/dashboard' },
        { label: 'Employees' }
      ],
      '/performance': [
        { label: 'Dashboard', path: '/dashboard' },
        { label: 'Performance' }
      ],
      '/recruitment': [
        { label: 'Dashboard', path: '/dashboard' },
        { label: 'Recruitment' }
      ],
      '/analytics': [
        { label: 'Dashboard', path: '/dashboard' },
        { label: 'Analytics' }
      ],
      '/attendance': [
        { label: 'Dashboard', path: '/dashboard' },
        { label: 'Attendance' }
      ],
    };

    const crumbs = Object.keys(breadcrumbs).find(path => url.startsWith(path));
    this.breadcrumbItems.set(crumbs ? breadcrumbs[crumbs] : []);
  }
}
