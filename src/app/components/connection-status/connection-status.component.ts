import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WebSocketService } from '../../services/websocket.service';

/**
 * Connection Status Indicator Component
 *
 * Displays the current WebSocket connection status.
 * Shows visual indicators for connected, disconnected, and reconnecting states.
 *
 * Requirements: 5.3, 5.4
 */
@Component({
  selector: 'app-connection-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium"
      [ngClass]="getStatusClasses()"
    >
      <div class="w-2 h-2 rounded-full" [ngClass]="getIndicatorClasses()"></div>
      <span>{{ getStatusText() }}</span>
    </div>
  `,
})
export class ConnectionStatusComponent implements OnInit {
  connectionStatus$: any;

  constructor(private wsService: WebSocketService) {
    this.connectionStatus$ = this.wsService.connectionStatus$;
  }

  ngOnInit(): void {
    // Component initialized
  }

  getStatusText(): string {
    const status = this.wsService.getConnectionStatus();
    switch (status) {
      case 'connected':
        return 'Connected';
      case 'disconnected':
        return 'Disconnected';
      case 'reconnecting':
        return 'Reconnecting...';
      default:
        return 'Unknown';
    }
  }

  getStatusClasses(): string {
    const status = this.wsService.getConnectionStatus();
    switch (status) {
      case 'connected':
        return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/20 dark:text-emerald-300';
      case 'disconnected':
        return 'bg-red-100 text-red-800 dark:bg-red-500/20 dark:text-red-400';
      case 'reconnecting':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-500/20 dark:text-yellow-300';
      default:
        return 'bg-slate-100 text-slate-800 dark:bg-slate-500/20 dark:text-slate-300';
    }
  }

  getIndicatorClasses(): string {
    const status = this.wsService.getConnectionStatus();
    switch (status) {
      case 'connected':
        return 'bg-emerald-600 dark:bg-emerald-400';
      case 'disconnected':
        return 'bg-red-600 dark:bg-red-400';
      case 'reconnecting':
        return 'bg-yellow-600 dark:bg-yellow-400 animate-pulse';
      default:
        return 'bg-slate-600 dark:bg-slate-400';
    }
  }
}
