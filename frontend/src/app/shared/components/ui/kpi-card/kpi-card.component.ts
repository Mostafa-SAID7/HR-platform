import {
  Component,
  Input,
  ChangeDetectionStrategy,
  signal,
  effect,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { fadeIn, slideInUp } from '@app/shared/animations';
import { BadgeComponent } from '../badge/badge.component';

/**
 * KPI Card Component
 *
 * Displays key performance indicators with:
 * - Animated number transitions
 * - Trend indicators (up/down arrows)
 * - Loading and error states
 * - Real-time data updates
 *
 * Requirements: 1.1, 1.2, 1.6, 2.1, 2.3
 */

export interface KPIData {
  label: string;
  value: number;
  previousValue?: number;
  unit?: string;
  trend?: 'up' | 'down' | 'neutral';
  trendPercentage?: number;
  loading?: boolean;
  error?: string;
}

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    slideInUp,
    trigger('numberChange', [
      transition(':enter', [style({ opacity: 0, transform: 'scale(0.8)' }), animate('300ms ease-out')]),
      transition(':leave', [animate('300ms ease-in', style({ opacity: 0, transform: 'scale(0.8)' }))]),
    ]),
  ],
  templateUrl: './kpi-card.component.html',
})
export class KPICardComponent implements OnInit {
  @Input() data: KPIData = {
    label: 'Metric',
    value: 0,
  };

  Math = Math;

  ngOnInit(): void {
    // Component initialization
  }

  formatNumber(value: number): string {
    if (value >= 1000000) {
      return (value / 1000000).toFixed(1) + 'M';
    }
    if (value >= 1000) {
      return (value / 1000).toFixed(1) + 'K';
    }
    return value.toLocaleString();
  }
}
