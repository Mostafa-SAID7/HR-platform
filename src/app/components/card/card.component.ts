import { Component, ElementRef, AfterContentInit } from '@angular/core';
import { CommonModule } from '@angular/common';

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
  template: `
    <div class="bg-white dark:bg-slate-800 rounded-lg shadow-md dark:shadow-lg overflow-hidden">
      <div *ngIf="hasHeader" class="border-b border-slate-200 dark:border-slate-700 px-6 py-4">
        <ng-content select="[appCardHeader]"></ng-content>
      </div>
      <div class="px-6 py-4">
        <ng-content></ng-content>
      </div>
      <div
        *ngIf="hasFooter"
        class="border-t border-slate-200 dark:border-slate-700 px-6 py-4 bg-slate-50 dark:bg-slate-900"
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
