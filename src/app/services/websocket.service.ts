import { Injectable } from '@angular/core';
import { Subject, Observable, BehaviorSubject, interval, timer, Subscription } from 'rxjs';
import { filter, map, retryWhen, delay, take } from 'rxjs/operators';

/**
 * WebSocket Service
 */
@Injectable({
  providedIn: 'root',
})
export class WebSocketService {
  private ws: WebSocket | null = null;
  private messageSubject = new Subject<any>();
  private connectionStatus = new BehaviorSubject<'connected' | 'disconnected' | 'reconnecting' | 'mocked'>(
    'disconnected',
  );
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 3;
  private reconnectDelay = 2000;
  private mockSubscription?: Subscription;

  messages$: Observable<any> = this.messageSubject.asObservable();
  connectionStatus$: Observable<'connected' | 'disconnected' | 'reconnecting' | 'mocked'> =
    this.connectionStatus.asObservable();

  constructor() {
    this.connect();
  }

  connect(url: string = 'ws://localhost:8080'): void {
    if (this.ws && (this.ws.readyState === WebSocket.OPEN || this.ws.readyState === WebSocket.CONNECTING)) {
      return;
    }

    this.stopMocking();

    try {
      this.ws = new WebSocket(url);

      this.ws.onopen = () => {
        console.log('[WebSocketService] Connected to real WebSocket server');
        this.connectionStatus.next('connected');
        this.reconnectAttempts = 0;
      };

      this.ws.onmessage = (event: MessageEvent) => {
        try {
          const data = JSON.parse(event.data);
          this.messageSubject.next(data);
        } catch (error) {
          console.error('[WebSocketService] Failed to parse message:', error);
        }
      };

      this.ws.onerror = () => {
        this.connectionStatus.next('disconnected');
      };

      this.ws.onclose = () => {
        this.connectionStatus.next('disconnected');
        this.attemptReconnect(url);
      };
    } catch (error) {
      this.connectionStatus.next('disconnected');
      this.attemptReconnect(url);
    }
  }

  private attemptReconnect(url: string): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.warn('[WebSocketService] Max reconnection attempts reached. Switching to Mock Mode for UI demonstration.');
      this.startMocking();
      return;
    }

    this.reconnectAttempts++;
    this.connectionStatus.next('reconnecting');

    setTimeout(() => {
      this.connect(url);
    }, this.reconnectDelay);
  }

  private startMocking(): void {
    if (this.connectionStatus.value === 'mocked') return;
    
    this.connectionStatus.next('mocked');
    
    // Simulate real-time updates every 5 seconds
    this.mockSubscription = timer(0, 5000).subscribe(() => {
      const types = ['kpi_update', 'notification', 'chart_update'];
      const randomType = types[Math.floor(Math.random() * types.length)];
      
      let mockData: any = { type: randomType, timestamp: new Date().toISOString() };
      
      if (randomType === 'kpi_update') {
        mockData.data = {
          headcount: 12000 + Math.floor(Math.random() * 50),
          turnover: (4.2 + (Math.random() * 0.4 - 0.2)).toFixed(2),
          satisfaction: (88 + (Math.random() * 2 - 1)).toFixed(1)
        };
      } else if (randomType === 'notification') {
        mockData.data = {
          id: Math.random().toString(36).substr(2, 9),
          title: 'Live Insight',
          message: 'Real-time satisfaction score updated in Engineering department.',
          priority: 'low'
        };
      }
      
      this.messageSubject.next(mockData);
    });
  }

  private stopMocking(): void {
    this.mockSubscription?.unsubscribe();
  }

  send(data: any): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(data));
    } else if (this.connectionStatus.value === 'mocked') {
      console.log('[WebSocketService] Mock Send (Silent):', data);
    } else {
      console.warn('[WebSocketService] WebSocket is not connected');
    }
  }

  subscribe<T>(messageType: string): Observable<T> {
    return this.messages$.pipe(
      filter((msg) => msg.type === messageType),
      map((msg) => msg.data as T),
    );
  }

  disconnect(): void {
    this.stopMocking();
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    this.connectionStatus.next('disconnected');
    this.reconnectAttempts = 0;
  }

  isConnected(): boolean {
    return this.connectionStatus.value === 'connected' || this.connectionStatus.value === 'mocked';
  }

  getConnectionStatus(): 'connected' | 'disconnected' | 'reconnecting' | 'mocked' {
    return this.connectionStatus.value;
  }
}
