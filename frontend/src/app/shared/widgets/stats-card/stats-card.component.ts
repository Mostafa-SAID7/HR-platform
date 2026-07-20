import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Stats Card Component
 * Displays statistics in card format
 */
export interface StatsCardData {
  title: string;
  value: number;
  previousValue?: number;
  change?: number;
  label?: string;
  icon?: string;
  color?: 'primary' | 'success' | 'danger' | 'warning' | 'info';
}

@Component({
  selector: 'app-stats-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stats-card" [ngClass]="'card-' + (data?.color || 'primary')">
      <div class="stats-header">
        <h4 class="stats-title">{{ data?.title }}</h4>
        <span class="stats-icon" *ngIf="data?.icon">{{ data?.icon }}</span>
      </div>
      <div class="stats-value">{{ data?.value }}</div>
      <div class="stats-footer" *ngIf="data?.change !== undefined">
        <span class="change-value" [ngClass]="(data?.change ?? 0) >= 0 ? 'positive' : 'negative'">
          {{ (data?.change ?? 0) >= 0 ? '+' : '' }}{{ data?.change }}%
        </span>
        <span class="stats-label" *ngIf="data?.label">{{ data?.label }}</span>
      </div>
    </div>
  `,
  styles: [`
    .stats-card {
      padding: 1.5rem;
      border-radius: 0.5rem;
      background: white;
      border: 1px solid #e5e7eb;
    }

    .stats-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .stats-title {
      margin: 0;
      font-size: 0.875rem;
      font-weight: 600;
      color: #6b7280;
    }

    .stats-value {
      font-size: 1.875rem;
      font-weight: bold;
      color: #1f2937;
      margin-bottom: 0.5rem;
    }

    .stats-footer {
      display: flex;
      justify-content: space-between;
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .change-value {
      font-weight: 600;
    }

    .change-value.positive {
      color: #10b981;
    }

    .change-value.negative {
      color: #ef4444;
    }

    .card-primary { border-left: 4px solid #3b82f6; }
    .card-success { border-left: 4px solid #10b981; }
    .card-danger { border-left: 4px solid #ef4444; }
    .card-warning { border-left: 4px solid #f59e0b; }
    .card-info { border-left: 4px solid #06b6d4; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatsCardComponent {
  @Input() data: StatsCardData | null = null;
}
