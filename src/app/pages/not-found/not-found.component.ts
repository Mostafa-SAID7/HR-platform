import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { I18nService } from '../../services/i18n.service';
import { fadeIn, slideInUp } from '../../shared/animations';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterModule],
  animations: [fadeIn, slideInUp],
  template: `
    <div 
      class="min-h-[80vh] flex flex-col items-center justify-center p-6 text-center select-none"
      [@fadeIn]
    >
      <div class="relative group">
        <!-- Abstract Illustration -->
        <div class="text-[12rem] md:text-[16rem] font-black text-slate-100 dark:text-slate-800/50 leading-none select-none pointer-events-none transform group-hover:scale-105 transition-transform duration-700">
          404
        </div>
        
        <!-- Hover Floating Icons -->
        <div class="absolute inset-0 flex items-center justify-center">
          <div 
            [@slideInUp]
            class="bg-white dark:bg-slate-800 p-8 rounded-3xl shadow-2xl border border-slate-100 dark:border-slate-700 max-w-sm transform hover:-translate-y-2 transition-transform duration-500"
          >
            <div class="w-20 h-20 bg-indigo-50 dark:bg-indigo-900/40 rounded-2xl flex items-center justify-center mx-auto mb-6 text-indigo-600 dark:text-indigo-400">
              <svg class="w-10 h-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 9.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            
            <h1 class="text-2xl font-bold text-slate-900 dark:text-white mb-3">
              {{ i18n.translate('error.not_found') || 'Resource Not Found' }}
            </h1>
            
            <p class="text-slate-600 dark:text-slate-400 mb-8 leading-relaxed">
               {{ i18n.translate('error.not_found_desc') || "The page you're looking for seems to have wandered off the grid. Let's get you back to headquarters." }}
            </p>
            
            <a 
              routerLink="/dashboard"
              class="inline-flex items-center justify-center gap-2 px-8 py-3.5 bg-indigo-600 hover:bg-indigo-700 text-white font-bold rounded-2xl shadow-lg shadow-indigo-600/20 hover:shadow-indigo-600/40 transform active:scale-95 transition-all duration-300 w-full"
            >
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              Go to Dashboard
            </a>
          </div>
        </div>
      </div>

      <!-- Background Blobs for Atmosphere -->
      <div class="fixed top-1/2 left-1/4 w-96 h-96 bg-indigo-400/10 blur-[100px] rounded-full -z-10 animate-pulse"></div>
      <div class="fixed bottom-1/4 right-1/4 w-64 h-64 bg-slate-400/10 blur-[80px] rounded-full -z-10"></div>
    </div>
  `,
})
export class NotFoundComponent {
  public i18n = inject(I18nService);
}
