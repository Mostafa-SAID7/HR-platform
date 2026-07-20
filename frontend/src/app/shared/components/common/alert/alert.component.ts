import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './alert.component.html',
  styles: [`
    .alert {
      padding: 1rem;
      border-radius: 0.5rem;
      border-left: 4px solid;
    }
    .alert-success {
      background-color: #d1fae5;
      border-left-color: #10b981;
      color: #065f46;
    }
    .alert-error {
      background-color: #fee2e2;
      border-left-color: #ef4444;
      color: #7f1d1d;
    }
    .alert-info {
      background-color: #dbeafe;
      border-left-color: #3b82f6;
      color: #1e3a8a;
    }
    .alert-warning {
      background-color: #fef3c7;
      border-left-color: #f59e0b;
      color: #78350f;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AlertComponent {
  @Input() message = '';
  @Input() type: 'success' | 'error' | 'info' | 'warning' = 'info';
}
