import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * KPI Widget Component
 * Displays key performance indicators
 */
export interface KPIData {
  title: string;
  value: number | string;
  unit?: string;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: number;
  icon?: string;
  color?: string;
}

@Component({
  selector: 'app-kpi-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="kpi-widget" [ngClass]="'kpi-' + (data?.color || 'blue')">
      <div class="kpi-header">
        <h3 class="kpi-title">{{ data?.title }}</h3>
        <span class="kpi-icon" *ngIf="data?.icon">{{ data?.icon }}</span>
      </div>
      <div class="kpi-content">
        <p class="kpi-value">
          {{ data?.value }}
          <span class="kpi-unit" *ngIf="data?.unit">{{ data?.unit }}</span>
        </p>
        <div class="kpi-trend" *ngIf="data?.trend" [ngClass]="'trend-' + data?.trend">
          <span class="trend-icon">{{ getTrendIcon() }}</span>
          <span class="trend-value" *ngIf="data?.trendValue">{{ data?.trendValue }}%</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .kpi-widget {
      padding: 1.5rem;
      border-radius: 0.5rem;
      background: white;
      border-left: 4px solid #3b82f6;
    }

    .kpi-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .kpi-title {
      font-size: 0.875rem;
      font-weight: 600;
      color: #6b7280;
      margin: 0;
    }

    .kpi-content {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
    }

    .kpi-value {
      font-size: 2rem;
      font-weight: bold;
      color: #1f2937;
      margin: 0;
    }

    .kpi-unit {
      font-size: 0.875rem;
      color: #9ca3af;
      margin-left: 0.5rem;
    }

    .kpi-trend {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      font-weight: 600;
    }

    .trend-up {
      color: #10b981;
    }

    .trend-down {
      color: #ef4444;
    }

    .trend-neutral {
      color: #f59e0b;
    }

    .kpi-blue {
      border-left-color: #3b82f6;
    }

    .kpi-green {
      border-left-color: #10b981;
    }

    .kpi-red {
      border-left-color: #ef4444;
    }

    .kpi-purple {
      border-left-color: #8b5cf6;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KPIWidgetComponent {
  @Input() data: KPIData | null = null;

  getTrendIcon(): string {
    switch (this.data?.trend) {
      case 'up':
        return '↑';
      case 'down':
        return '↓';
      default:
        return '→';
    }
  }
}
