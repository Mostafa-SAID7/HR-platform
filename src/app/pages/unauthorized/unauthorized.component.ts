import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { I18nService } from '../../services/i18n.service';
import { fadeIn, slideInUp } from '../../shared/animations';

/**
 * Unauthorized Page Component
 *
 * Displayed when user lacks required permissions to access a resource.
 *
 * Requirements: 30.4
 */
@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterLink],
  animations: [fadeIn, slideInUp],
  template: `
     <div 
      class="min-h-[90vh] flex flex-col items-center justify-center p-6 text-center select-none"
      [@fadeIn]
    >
      <div class="relative group">
        <!-- Abstract Illustration -->
        <div class="text-[12rem] md:text-[16rem] font-black text-red-50 dark:text-red-900/10 leading-none select-none pointer-events-none transform group-hover:scale-105 transition-transform duration-700">
          403
        </div>
        
        <!-- Hover Floating Icons -->
        <div class="absolute inset-0 flex items-center justify-center">
          <div 
            [@slideInUp]
            class="bg-white dark:bg-slate-800 p-8 rounded-3xl shadow-2xl border border-slate-100 dark:border-slate-700 max-w-sm transform hover:-translate-y-2 transition-transform duration-500"
          >
            <div class="w-20 h-20 bg-red-50 dark:bg-red-900/40 rounded-2xl flex items-center justify-center mx-auto mb-6 text-red-600 dark:text-red-400">
              <svg class="w-10 h-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m0 0v2m0-2h2m-2 0H10m11 3a9 9 0 11-18 0 9 9 0 0118 0zM12 5V3m0 2v2m0-2h2m-2 0H10" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v4m-3.5 1.5l.5-.5m3.5.5l-.5-.5" />
              </svg>
            </div>
            
            <h1 class="text-2xl font-bold text-slate-900 dark:text-white mb-3">
              {{ i18n.translate('error.unauthorized') || 'Access Denied' }}
            </h1>
            
            <p class="text-slate-600 dark:text-slate-400 mb-8 leading-relaxed">
               {{ i18n.translate('error.unauthorized_desc') || "You've reached a high-security perimeter. This sensitive data is restricted to authorized personnel only." }}
            </p>
            
            <a 
              routerLink="/dashboard"
              class="inline-flex items-center justify-center gap-2 px-8 py-3.5 bg-red-600 hover:bg-red-700 text-white font-bold rounded-2xl shadow-lg shadow-red-600/20 hover:shadow-red-600/40 transform active:scale-95 transition-all duration-300 w-full uppercase tracking-widest text-[10px]"
            >
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              Return Home
            </a>
          </div>
        </div>
      </div>

      <!-- Background Blobs -->
      <div class="fixed top-1/2 left-1/4 w-96 h-96 bg-red-400/10 blur-[100px] rounded-full -z-10 animate-pulse"></div>
      <div class="fixed bottom-1/4 right-1/4 w-64 h-64 bg-slate-400/10 blur-[80px] rounded-full -z-10"></div>
    </div>
  `,
})
export class UnauthorizedComponent {
  public i18n = inject(I18nService);
}
