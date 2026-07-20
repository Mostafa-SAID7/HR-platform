import { Component, OnInit, OnDestroy, signal, inject, computed } from '@angular/core';
import { CommonModule, NgClass, NgFor, NgIf } from '@angular/common';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
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
  icon?: string;
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
    NgClass,
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

  // Signals
  public isSidebarOpen = signal(false);
  public isUserMenuOpen = signal(false);
  public pageTitle = signal('Dashboard');
  public breadcrumbItems = signal<BreadcrumbItemData[]>([]);
  public currentUser = signal<User | null>(null);

  // Navigation Links
  public navLinks: NavLink[] = [
    { path: '/dashboard', label: 'Dashboard', labelKey: 'nav.dashboard' },
    { path: '/employees', label: 'Employees', labelKey: 'nav.employees' },
    { path: '/performance', label: 'Performance', labelKey: 'nav.performance' },
    { path: '/recruitment', label: 'Recruitment', labelKey: 'nav.recruitment' },
    { path: '/analytics', label: 'Analytics', labelKey: 'nav.analytics' },
    { path: '/attendance', label: 'Attendance', labelKey: 'nav.attendance' },
  ];

  private navigationSubscription: Subscription | null = null;

  ngOnInit(): void {
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
