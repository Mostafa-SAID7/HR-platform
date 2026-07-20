import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styles: [`
    .toast {
      padding: 1rem;
      border-radius: 0.5rem;
      color: white;
    }
    .success { background-color: #10b981; }
    .error { background-color: #ef4444; }
    .info { background-color: #3b82f6; }
    .warning { background-color: #f59e0b; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastComponent {
  @Input() message = '';
  @Input() type: 'success' | 'error' | 'info' | 'warning' = 'info';
}
