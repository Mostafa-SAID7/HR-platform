import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Icon Component
 *
 * Reusable icon component for displaying SVG icons.
 *
 * Requirements: 8.2
 */

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      [class]="'w-' + size + ' h-' + size + ' ' + (filled ? 'fill-current focus:outline-none' : 'stroke-current focus:outline-none')"
      [attr.aria-label]="label"
      [attr.role]="label ? 'img' : 'presentation'"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      viewBox="0 0 24 24"
    >
      <!-- Registry of pre-built icons -->
      <ng-container [ngSwitch]="name">
        <!-- Dashboard Icons -->
        <path *ngSwitchCase="'chart-bar'" stroke-linecap="round" stroke-linejoin="round" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        <path *ngSwitchCase="'users'" stroke-linecap="round" stroke-linejoin="round" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
        
        <!-- Navigation Icons -->
        <path *ngSwitchCase="'chevron-up'" stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7" />
        <path *ngSwitchCase="'chevron-down'" stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" />
        <path *ngSwitchCase="'chevron-left'" stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
        <path *ngSwitchCase="'chevron-right'" stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
        
        <!-- Action/Status Icons -->
        <path *ngSwitchCase="'document-magnifying-glass'" stroke-linecap="round" stroke-linejoin="round" d="M15.75 15.75l-2.489-2.489m0 0a3.375 3.375 0 10-4.773-4.773 3.375 3.375 0 004.773 4.773zM21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        <path *ngSwitchCase="'bell'" stroke-linecap="round" stroke-linejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        
        <!-- Fallback to ng-content if no name matches -->
        <ng-container *ngSwitchDefault>
          <ng-content></ng-content>
        </ng-container>
      </ng-container>
    </svg>
  `,
})
export class IconComponent {
  @Input() name: string | null = null;
  @Input() size = '5';
  @Input() filled = false;
  @Input() label = '';
}
