# Web & Progressive Web App Architecture

Complete guide to Angular PWA implementation with offline support and installability.

---

## PWA Overview

### What is a PWA?

```
Traditional Web App:
- Requires active internet connection
- Can't run offline
- Not installable
- Regular website experience

Progressive Web App (PWA):
✅ Works offline (service worker)
✅ Installable (add to home screen)
✅ App-like experience (fullscreen, no browser UI)
✅ Push notifications (optional)
✅ Faster loading (caching strategy)
✅ Works like native app but built with web tech
```

---

## PWA Implementation for Angular

### 1. PWA Manifest

```json
// public/manifest.webmanifest
{
  "name": "HR Analytics Platform",
  "short_name": "HR Analytics",
  "description": "Employee analytics and management system",
  "start_url": "/",
  "scope": "/",
  "display": "standalone",
  "orientation": "portrait-primary",
  "theme_color": "#1f2937",
  "background_color": "#ffffff",
  "icons": [
    {
      "src": "/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-maskable.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "maskable"
    }
  ],
  "categories": ["business", "productivity"],
  "screenshots": [
    {
      "src": "/screenshots/screenshot-1.png",
      "sizes": "540x720",
      "type": "image/png",
      "form_factor": "narrow"
    },
    {
      "src": "/screenshots/screenshot-wide.png",
      "sizes": "1920x1080",
      "type": "image/png",
      "form_factor": "wide"
    }
  ],
  "shortcuts": [
    {
      "name": "Employees",
      "url": "/employees",
      "icons": [
        {
          "src": "/icons/employees.png",
          "sizes": "96x96",
          "type": "image/png"
        }
      ]
    },
    {
      "name": "Payroll",
      "url": "/payroll",
      "icons": [
        {
          "src": "/icons/payroll.png",
          "sizes": "96x96",
          "type": "image/png"
        }
      ]
    }
  ]
}
```

### 2. HTML Meta Tags

```html
<!-- src/index.html -->
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <title>HR Analytics Platform</title>
    <base href="/">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    
    <!-- PWA Meta Tags -->
    <meta name="theme-color" content="#1f2937">
    <meta name="description" content="HR Analytics Platform - Employee Management System">
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
    <meta name="apple-mobile-web-app-title" content="HR Analytics">
    
    <!-- Icon Links -->
    <link rel="manifest" href="/manifest.webmanifest">
    <link rel="apple-touch-icon" href="/icons/apple-touch-icon.png">
    <link rel="icon" type="image/png" href="/icons/favicon-32x32.png" sizes="32x32">
    <link rel="icon" type="image/png" href="/icons/favicon-16x16.png" sizes="16x16">
    
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
</head>
<body>
    <app-root></app-root>
</body>
</html>
```

### 3. Service Worker Setup

```typescript
// src/service-worker.ts
/// <reference lib="webworker" />
declare const self: ServiceWorkerGlobalScope;

// Install event - cache assets
self.addEventListener('install', (event: ExtendableEvent) => {
  event.waitUntil(
    caches.open('hr-analytics-v1').then((cache) => {
      return cache.addAll([
        '/',
        '/index.html',
        '/styles.css',
        '/main.js',
        '/polyfills.js',
        '/runtime.js',
        '/icons/icon-192x192.png',
        '/icons/icon-512x512.png',
      ]);
    })
  );
  self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', (event: ExtendableEvent) => {
  event.waitUntil(
    caches.keys().then((names) => {
      return Promise.all(
        names.map((name) => {
          if (name !== 'hr-analytics-v1') {
            return caches.delete(name);
          }
          return Promise.resolve();
        })
      );
    })
  );
  self.clients.claim();
});

// Fetch event - Network first, fallback to cache
self.addEventListener('fetch', (event: FetchEvent) => {
  const { request } = event;
  const url = new URL(request.url);

  // API calls - network first
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(
      fetch(request)
        .then((response) => {
          // Cache successful responses
          if (response.ok) {
            const cacheName = 'hr-analytics-api-v1';
            const responseToCache = response.clone();
            caches.open(cacheName).then((cache) => {
              cache.put(request, responseToCache);
            });
          }
          return response;
        })
        .catch(() => {
          // Fallback to cache on network error
          return caches.match(request).then((cached) => {
            return cached || new Response('Offline - please check internet connection', {
              status: 503,
              statusText: 'Service Unavailable',
              headers: new Headers({
                'Content-Type': 'text/plain',
              }),
            });
          });
        })
    );
  }
  // Static assets - cache first
  else {
    event.respondWith(
      caches
        .match(request)
        .then((response) => {
          return response || fetch(request);
        })
        .catch(() => {
          // Offline fallback
          if (request.destination === 'document') {
            return caches.match('/index.html');
          }
          return new Response('Offline', { status: 503 });
        })
    );
  }
});

// Handle messages from clients
self.addEventListener('message', (event: ExtendableMessageEvent) => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
});
```

### 4. Angular Service Worker Integration

```typescript
// src/app/core/services/pwa.service.ts
import { Injectable, inject, OnDestroy } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PwaService implements OnDestroy {
  private swUpdate = inject(SwUpdate);
  private destroy$ = new Subject<void>();
  
  updateAvailable$ = new Subject<void>();

  constructor() {
    if (this.swUpdate.isEnabled) {
      // Check for updates periodically
      setInterval(() => {
        this.swUpdate.checkForUpdates().catch(console.error);
      }, 1000 * 60 * 60); // Every hour

      // Listen for updates
      this.swUpdate.versionUpdates.subscribe((event) => {
        if (event.type === 'VERSION_READY') {
          console.log('New version available');
          this.updateAvailable$.next();
        }
      });
    }
  }

  activateUpdate(): void {
    if (this.swUpdate.isEnabled) {
      this.swUpdate.activateUpdate().then(() => {
        // Refresh page to load new version
        document.location.reload();
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

// Usage in AppComponent
@Component({
  selector: 'app-root',
  template: `
    <div>
      <!-- Show update available banner -->
      @if (updateAvailable$ | async) {
        <div class="bg-blue-500 text-white p-4">
          <p>New version available!</p>
          <button (click)="activateUpdate()" class="btn">Update Now</button>
        </div>
      }
      
      <router-outlet></router-outlet>
    </div>
  `,
})
export class AppComponent {
  private pwaService = inject(PwaService);
  updateAvailable$ = this.pwaService.updateAvailable$;

  activateUpdate(): void {
    this.pwaService.activateUpdate();
  }
}
```

### 5. Install Prompt Handling

```typescript
// src/app/core/services/install-prompt.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

@Injectable({
  providedIn: 'root'
})
export class InstallPromptService {
  private deferredPrompt: BeforeInstallPromptEvent | null = null;
  canInstall$ = new BehaviorSubject<boolean>(false);

  constructor() {
    window.addEventListener('beforeinstallprompt', (e: any) => {
      e.preventDefault();
      this.deferredPrompt = e;
      this.canInstall$.next(true);
    });

    window.addEventListener('appinstalled', () => {
      console.log('App installed');
      this.deferredPrompt = null;
      this.canInstall$.next(false);
    });
  }

  async install(): Promise<boolean> {
    if (!this.deferredPrompt) {
      return false;
    }

    this.deferredPrompt.prompt();
    const { outcome } = await this.deferredPrompt.userChoice;
    
    this.deferredPrompt = null;
    this.canInstall$.next(false);
    
    return outcome === 'accepted';
  }

  isIOS(): boolean {
    return /iPad|iPhone|iPod/.test(navigator.userAgent);
  }

  isAndroid(): boolean {
    return /Android/.test(navigator.userAgent);
  }

  isInstallable(): boolean {
    return !this.isIOS() && !this.isAndroid();
  }
}
```

---

## Offline Support

### Local Database Synchronization

```typescript
// src/app/core/services/offline-sync.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, interval } from 'rxjs';

export interface OfflineChange {
  id: string;
  type: 'CREATE' | 'UPDATE' | 'DELETE';
  table: string;
  data: any;
  timestamp: number;
  synced: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class OfflineSyncService {
  private http = inject(HttpClient);
  private db: IDBDatabase | null = null;
  private changes$ = new BehaviorSubject<OfflineChange[]>([]);
  
  isOnline$ = new BehaviorSubject<boolean>(navigator.onLine);

  async initialize(): Promise<void> {
    // Open IndexedDB for offline changes
    return new Promise((resolve, reject) => {
      const request = indexedDB.open('hr-analytics-offline', 1);

      request.onerror = () => reject(request.error);
      request.onsuccess = () => {
        this.db = request.result;
        resolve();
      };

      request.onupgradeneeded = (event) => {
        const db = (event.target as IDBOpenDBRequest).result;
        if (!db.objectStoreNames.contains('changes')) {
          db.createObjectStore('changes', { keyPath: 'id' });
        }
      };
    });

    // Listen for online/offline events
    window.addEventListener('online', () => {
      this.isOnline$.next(true);
      this.syncChanges();
    });

    window.addEventListener('offline', () => {
      this.isOnline$.next(false);
    });
  }

  async recordChange(change: OfflineChange): Promise<void> {
    if (!this.db) return;

    return new Promise((resolve, reject) => {
      const tx = this.db!.transaction(['changes'], 'readwrite');
      const store = tx.objectStore('changes');
      
      store.add(change);
      
      tx.oncomplete = () => {
        this.loadChanges();
        resolve();
      };
      tx.onerror = () => reject(tx.error);
    });
  }

  private async loadChanges(): Promise<void> {
    if (!this.db) return;

    return new Promise((resolve) => {
      const tx = this.db!.transaction(['changes'], 'readonly');
      const store = tx.objectStore('changes');
      const request = store.getAll();

      request.onsuccess = () => {
        this.changes$.next(request.result as OfflineChange[]);
        resolve();
      };
    });
  }

  async syncChanges(): Promise<void> {
    const unsynced = this.changes$.value.filter(c => !c.synced);

    for (const change of unsynced) {
      try {
        await this.http.post('/api/v1/sync', change).toPromise();
        
        // Mark as synced
        await this.updateChangeSyncStatus(change.id, true);
      } catch (e) {
        console.error('Sync failed for', change.id);
      }
    }
  }

  private async updateChangeSyncStatus(id: string, synced: boolean): Promise<void> {
    if (!this.db) return;

    return new Promise((resolve) => {
      const tx = this.db!.transaction(['changes'], 'readwrite');
      const store = tx.objectStore('changes');
      const request = store.get(id);

      request.onsuccess = () => {
        const change = request.result as OfflineChange;
        change.synced = synced;
        store.put(change);
        resolve();
      };
    });
  }
}
```

---

## Angular Configuration

### angular.json Update

```json
{
  "projects": {
    "hr-analytics": {
      "architect": {
        "build": {
          "configurations": {
            "production": {
              "outputHashing": "all",
              "serviceWorker": "src/service-worker.ts",
              "ngswConfigPath": "ngsw-config.json"
            }
          }
        }
      }
    }
  }
}
```

### ngsw-config.json

```json
{
  "$schema": "./node_modules/@angular/service-worker/config/schema.json",
  "index": "/index.html",
  "assetGroups": [
    {
      "name": "app",
      "installMode": "prefetch",
      "resources": {
        "files": [
          "/favicon.ico",
          "/index.html",
          "/*.css",
          "/*.js"
        ]
      }
    },
    {
      "name": "assets",
      "installMode": "lazy",
      "updateMode": "lazy",
      "resources": {
        "files": [
          "/assets/**",
          "/*.(svg|cur|jpg|jpeg|png|apng|webp|gif|otf|ttf|woff|woff2)"
        ]
      }
    }
  ],
  "dataGroups": [
    {
      "name": "api-production",
      "urls": ["/api/**"],
      "cacheConfig": {
        "strategy": "freshness",
        "maxAge": "1h",
        "maxSize": 100
      }
    }
  ]
}
```

---

## Testing PWA

### PWA Audit (Lighthouse)

```bash
# Run Lighthouse audit
npm run build -- --configuration production
npx http-server dist/

# Open http://localhost:8080 in Chrome
# Run Lighthouse audit (DevTools → Lighthouse)

Criteria:
✅ Installability
✅ Performance
✅ Offline support
✅ Security
✅ Best practices
```

### Manual Testing

```
1. Offline Support
   - Load app
   - Go offline (DevTools → Network → Offline)
   - Verify app still works with cached data
   - Perform actions (changes queued)
   - Go online (verify sync)

2. Install Prompt
   - Load app in Chrome/Edge
   - Click install button (or use 3-dot menu)
   - Verify app installs
   - Verify runs fullscreen

3. Service Worker
   - DevTools → Application → Service Worker
   - Verify registered and active
   - Verify caching working
   - Verify update detection
```

---

**Last Updated:** July 2026
**Status:** PWA Architecture Complete
