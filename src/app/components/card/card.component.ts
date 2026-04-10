import { Component, ElementRef, AfterContentInit, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { fadeIn, slideInUp } from '../../shared/animations';

/**
 * Card Component
 *
 * A reusable card component with header, body, and footer slots.
 * Provides a container for content with consistent styling.
 *
 * Requirements: 2.2, 8.2
 */
@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  animations: [fadeIn, slideInUp],
  template: `
    <div 
      [@slideInUp]
      class="bg-white/80 dark:bg-slate-800/80 backdrop-blur-sm rounded-2xl shadow-sm border border-slate-200/60 dark:border-slate-700/50 overflow-hidden transition-all duration-300 hover:shadow-xl hover:shadow-indigo-500/5 hover:border-indigo-500/20 group"
    >
      <!-- Header -->
      <div 
        *ngIf="hasHeader" 
        class="border-b border-slate-100 dark:border-slate-700/50 px-6 py-5 bg-slate-50/50 dark:bg-slate-900/20 group-hover:bg-indigo-50/30 dark:group-hover:bg-indigo-900/10 transition-colors"
      >
        <ng-content select="[appCardHeader]"></ng-content>
      </div>
      
      <!-- Body -->
      <div class="p-6">
        <ng-content></ng-content>
      </div>
      
      <!-- Footer -->
      <div
        *ngIf="hasFooter"
        class="border-t border-slate-100 dark:border-slate-700/50 px-6 py-4 bg-slate-50/30 dark:bg-slate-900/30"
      >
        <ng-content select="[appCardFooter]"></ng-content>
      </div>
    </div>
  `,
})
export class CardComponent implements AfterContentInit {
  hasHeader = false;
  hasFooter = false;

  constructor(private el: ElementRef) {}

  ngAfterContentInit() {
    // Small delay to ensure content is rendered in JSDOM/Tests
    setTimeout(() => {
      this.hasHeader = !!this.el.nativeElement.querySelector('[appCardHeader]');
      this.hasFooter = !!this.el.nativeElement.querySelector('[appCardFooter]');
    }, 0);
  }
}
