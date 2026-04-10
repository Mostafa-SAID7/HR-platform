import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type BadgeVariant = 'success' | 'warning' | 'error' | 'info';

/**
 * Badge Component
 *
 * A reusable badge component for status indicators.
 * Supports multiple variants for different statuses.
 *
 * Requirements: 2.2, 8.2
 */
@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span [class]="getBadgeClasses()" [attr.aria-label]="ariaLabel">
      <ng-content></ng-content>
    </span>
  `,
})
export class BadgeComponent {
  @Input() variant: BadgeVariant = 'info';
  @Input() ariaLabel: string | null = null;

  getBadgeClasses(): string {
    const baseClasses = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';

    const variantClasses = {
      success: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/20 dark:text-emerald-300',
      warning: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-500/20 dark:text-yellow-300',
      error: 'bg-red-100 text-red-800 dark:bg-red-500/20 dark:text-red-400',
      info: 'bg-blue-100 text-blue-800 dark:bg-blue-500/20 dark:text-blue-300',
    };

    return `${baseClasses} ${variantClasses[this.variant]}`;
  }
}
