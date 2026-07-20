import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { I18nService, ThemeService } from '../../core';
import { fadeIn } from '../../shared/animations';

/**
 * Auth Layout Component
 * Provides layout for authentication pages (login, signup, forgot password)
 * Requirements: 4.1, 4.2
 */
@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  animations: [fadeIn],
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.css'
})
export class AuthLayoutComponent {
  public i18nService = inject(I18nService);
  public themeService = inject(ThemeService);

  /**
   * Toggle theme
   */
  public toggleTheme(): void {
    const currentTheme = this.themeService.theme();
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    this.themeService.theme.set(newTheme);
  }

  /**
   * Toggle language
   */
  public toggleLanguage(): void {
    const newLang = this.i18nService.isArabic() ? 'en' : 'ar';
    this.i18nService.setLanguage(newLang);
  }
}

/**
 * Authentication Layout Component
 * Used for login, signup, and other auth pages
 */
@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  animations: [fadeIn],
  template: `
    <div 
      class="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-900 to-slate-900 dark:from-slate-950 dark:via-indigo-950 dark:to-slate-950 flex items-center justify-center relative overflow-hidden"
      [dir]="i18nService.isArabic() ? 'rtl' : 'ltr'"
    >
      <!-- Animated Background Elements -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute -top-40 -right-40 w-80 h-80 bg-indigo-500/20 rounded-full blur-3xl animate-pulse"></div>
        <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-violet-500/20 rounded-full blur-3xl animate-pulse delay-2000"></div>
      </div>

      <!-- Content Container -->
      <div class="relative z-10 w-full max-w-md px-6 py-12">
        <!-- Brand Logo -->
        <div class="text-center mb-12">
          <div class="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-tr from-indigo-600 to-violet-500 rounded-3xl mb-4 shadow-lg shadow-indigo-500/30">
            <span class="text-white text-2xl font-black">HR</span>
          </div>
          <h1 class="text-4xl font-black text-white tracking-tighter mt-4">HR Analytics</h1>
          <p class="text-slate-400 text-sm font-bold mt-2 uppercase tracking-widest">Intelligence Platform</p>
        </div>

        <!-- Auth Content Outlet -->
        <div [@fadeIn] class="bg-white/10 dark:bg-slate-900/40 backdrop-blur-2xl rounded-3xl border border-white/20 dark:border-slate-700/50 p-8 shadow-2xl">
          <router-outlet></router-outlet>
        </div>

        <!-- Footer Links -->
        <div class="mt-8 text-center text-slate-400 text-sm">
          <p>
            <span class="font-bold">HR Analytics Platform</span>
            <span class="mx-2">•</span>
            <a href="#" class="hover:text-indigo-400 transition-colors">Privacy Policy</a>
            <span class="mx-2">•</span>
            <a href="#" class="hover:text-indigo-400 transition-colors">Terms of Service</a>
          </p>
        </div>

        <!-- Language & Theme Controls -->
        <div class="mt-6 flex justify-center gap-4">
          <button
            (click)="toggleTheme()"
            class="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-all"
            title="Toggle theme"
          >
            <svg [class.hidden]="!themeService.isDark()" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <svg [class.hidden]="themeService.isDark()" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
          </button>

          <button
            (click)="toggleLanguage()"
            class="px-3 py-2 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-all text-sm font-bold"
            title="Toggle language"
          >
            {{ i18nService.isArabic() ? 'ENG' : 'عربي' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class AuthLayoutComponent {
  i18nService = inject(I18nService);
  themeService = inject(ThemeService);

  toggleLanguage(): void {
    const currentLang = this.i18nService.getLanguage();
    this.i18nService.setLanguage(currentLang === 'en' ? 'ar' : 'en');
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
