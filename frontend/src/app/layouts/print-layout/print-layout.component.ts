import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { I18nService } from '../../core';

/**
 * Print Layout Component
 * Optimized layout for printing reports and documents
 * Requirements: 9.1, 9.2
 */
@Component({
  selector: 'app-print-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './print-layout.component.html',
  styleUrl: './print-layout.component.css'
})
export class PrintLayoutComponent {
  public i18nService = inject(I18nService);

  /**
   * Print current page
   */
  public printPage(): void {
    if (typeof window !== 'undefined') {
      window.print();
    }
  }

  /**
   * Go back to previous page
   */
  public goBack(): void {
    if (typeof window !== 'undefined') {
      window.history.back();
    }
  }
}

/**
 * Print Layout Component
 * Optimized for printing reports and documents
 * Removes UI elements not needed in print, applies print styles
 */
@Component({
  selector: 'app-print-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div 
      class="bg-white text-slate-900 p-8"
      [dir]="i18nService.isArabic() ? 'rtl' : 'ltr'"
    >
      <!-- Print Header -->
      <header class="border-b border-slate-200 pb-6 mb-6">
        <div class="flex justify-between items-start">
          <div>
            <h1 class="text-3xl font-black text-slate-900">HR Analytics Report</h1>
            <p class="text-sm text-slate-600 mt-1">
              Generated on: {{ today | date: 'full' }}
            </p>
          </div>
          <div class="text-right">
            <p class="font-bold text-slate-900">HR Platform</p>
            <p class="text-sm text-slate-600">Intelligence System</p>
          </div>
        </div>
      </header>

      <!-- Print Content -->
      <main class="print-content">
        <router-outlet></router-outlet>
      </main>

      <!-- Print Footer -->
      <footer class="border-t border-slate-200 mt-12 pt-6 text-center text-sm text-slate-600">
        <p>© {{ today | date: 'yyyy' }} HR Analytics Platform. All rights reserved.</p>
        <p class="mt-1">This document is confidential and intended for authorized personnel only.</p>
      </footer>
    </div>
  `,
  styles: [`
    @media print {
      :host {
        background: white;
      }
      
      body {
        margin: 0;
        padding: 0;
      }
      
      .print-content {
        page-break-inside: avoid;
      }
    }
  `]
})
export class PrintLayoutComponent {
  i18nService = inject(I18nService);
  today = new Date();
}
