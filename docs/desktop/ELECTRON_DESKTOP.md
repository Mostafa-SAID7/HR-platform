# Electron Desktop Application

Complete guide to packaging Angular app as native Windows/macOS/Linux desktop application.

---

## Electron Overview

### Architecture

```
┌─────────────────────────────────────────┐
│  Main Process (Node.js)                 │
│  • Window management                    │
│  • File system access                   │
│  • System tray                          │
│  • Native dialogs                       │
│  • Auto-update                          │
└─────────────────────────────────────────┘
           │ IPC
           ▼
┌─────────────────────────────────────────┐
│  Renderer Process (Chromium)            │
│  • Angular Application                  │
│  • DOM Rendering                        │
│  • User Interface                       │
└─────────────────────────────────────────┘
```

---

## Project Setup

### 1. Dependencies

```json
{
  "devDependencies": {
    "electron": "^32.0.0",
    "electron-builder": "^25.0.0",
    "electron-updater": "^6.2.0",
    "electron-log": "^5.1.0"
  }
}
```

### 2. Directory Structure

```
HR-platform/
├── frontend/                    # Angular web app
│   ├── src/
│   ├── dist/                   # Built web app
│   └── package.json
│
├── electron/                    # Electron main process
│   ├── main.ts                 # Entry point
│   ├── preload.ts              # Preload script
│   └── tsconfig.json
│
└── electron-builder.config.js  # Build config
```

### 3. Electron Main Process

```typescript
// electron/main.ts
import { app, BrowserWindow, Menu, ipcMain, dialog } from 'electron';
import { autoUpdater } from 'electron-updater';
import log from 'electron-log';
import path from 'path';
import isDev from 'electron-is-dev';

log.transports.file.level = 'info';
autoUpdater.logger = log;

let mainWindow: BrowserWindow | null = null;

function createWindow(): void {
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1000,
    minHeight: 700,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
      enableRemoteModule: false,
    },
    icon: path.join(__dirname, '../public/icons/icon-256x256.png'),
  });

  const startURL = isDev
    ? 'http://localhost:4200'  // Dev server
    : `file://${path.join(__dirname, '../dist/index.html')}`; // Production

  mainWindow.loadURL(startURL);

  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  createMenu();
  setupIPC();
  checkForUpdates();
}

function createMenu(): void {
  const template: Electron.MenuItemConstructorOptions[] = [
    {
      label: 'File',
      submenu: [
        {
          label: 'Exit',
          accelerator: 'CmdOrCtrl+Q',
          click: () => {
            app.quit();
          },
        },
      ],
    },
    {
      label: 'Edit',
      submenu: [
        { role: 'undo' },
        { role: 'redo' },
        { type: 'separator' },
        { role: 'cut' },
        { role: 'copy' },
        { role: 'paste' },
      ],
    },
    {
      label: 'View',
      submenu: [
        { role: 'reload' },
        { role: 'forceReload' },
        { role: 'toggleDevTools' },
      ],
    },
    {
      label: 'Help',
      submenu: [
        {
          label: 'About',
          click: () => {
            dialog.showMessageBox(mainWindow!, {
              type: 'info',
              title: 'About HR Analytics',
              message: 'HR Analytics Platform v1.0.0',
            });
          },
        },
      ],
    },
  ];

  const menu = Menu.buildFromTemplate(template);
  Menu.setApplicationMenu(menu);
}

function setupIPC(): void {
  // File operations
  ipcMain.handle('open-file', async () => {
    const result = await dialog.showOpenDialog(mainWindow!, {
      properties: ['openFile'],
      filters: [
        { name: 'Excel Files', extensions: ['xlsx', 'xls'] },
        { name: 'All Files', extensions: ['*'] },
      ],
    });
    return result;
  });

  // Save file
  ipcMain.handle('save-file', async (event, data: string) => {
    const result = await dialog.showSaveDialog(mainWindow!, {
      defaultPath: 'export.xlsx',
      filters: [{ name: 'Excel Files', extensions: ['xlsx'] }],
    });

    if (!result.canceled) {
      const fs = await import('fs').then(m => m.promises);
      await fs.writeFile(result.filePath!, data);
      return result.filePath;
    }
  });

  // Show notification
  ipcMain.handle('show-notification', (event, { title, body }) => {
    const { Notification } = require('electron');
    new Notification({ title, body });
  });

  // Get app version
  ipcMain.handle('get-app-version', () => {
    return app.getVersion();
  });
}

function checkForUpdates(): void {
  if (isDev) return;

  autoUpdater.checkForUpdatesAndNotify().catch(err => {
    log.error('Error checking for updates:', err);
  });

  autoUpdater.on('update-downloaded', () => {
    dialog.showMessageBox(mainWindow!, {
      type: 'info',
      title: 'Update Available',
      message: 'A new version has been downloaded. Restart to apply.',
      buttons: ['Restart', 'Later'],
    }).then(result => {
      if (result.response === 0) {
        autoUpdater.quitAndInstall();
      }
    });
  });
}

// App lifecycle
app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow();
  }
});
```

### 4. Preload Script

```typescript
// electron/preload.ts
import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('electronAPI', {
  // File operations
  openFile: () => ipcRenderer.invoke('open-file'),
  saveFile: (data: string) => ipcRenderer.invoke('save-file', data),

  // Notifications
  showNotification: (options: { title: string; body: string }) =>
    ipcRenderer.invoke('show-notification', options),

  // App info
  getAppVersion: () => ipcRenderer.invoke('get-app-version'),

  // Listeners
  onUpdateAvailable: (callback: () => void) => {
    ipcRenderer.on('update-available', callback);
  },

  removeUpdateListener: (callback: () => void) => {
    ipcRenderer.removeListener('update-available', callback);
  },
});

declare global {
  interface Window {
    electronAPI: {
      openFile: () => Promise<{ filePaths: string[]; canceled: boolean }>;
      saveFile: (data: string) => Promise<string | undefined>;
      showNotification: (options: { title: string; body: string }) => void;
      getAppVersion: () => Promise<string>;
      onUpdateAvailable: (callback: () => void) => void;
      removeUpdateListener: (callback: () => void) => void;
    };
  }
}

export {};
```

### 5. Angular Service for Electron Integration

```typescript
// src/app/core/services/electron.service.ts
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ElectronService {
  ipcRenderer?: typeof import('electron').ipcRenderer;
  remote?: typeof import('electron').remote;

  get isElectron(): boolean {
    return !!(window as any).electronAPI;
  }

  constructor() {
    if (this.isElectron) {
      (window as any).electronAPI;
    }
  }

  async openFile(): Promise<string | null> {
    if (!this.isElectron) return null;

    const result = await (window as any).electronAPI.openFile();
    return result.filePaths[0] || null;
  }

  async saveFile(data: string): Promise<void> {
    if (!this.isElectron) return;

    await (window as any).electronAPI.saveFile(data);
  }

  showNotification(title: string, body: string): void {
    if (!this.isElectron) return;

    (window as any).electronAPI.showNotification({ title, body });
  }

  async getAppVersion(): Promise<string> {
    if (!this.isElectron) return 'web';

    return await (window as any).electronAPI.getAppVersion();
  }
}
```

---

## Building & Distribution

### 1. Electron Builder Configuration

```javascript
// electron-builder.config.js
module.exports = {
  appId: 'com.hranalytics.app',
  productName: 'HR Analytics',
  directories: {
    buildResources: 'public/icons',
    output: 'dist-electron',
  },
  files: [
    'dist/**/*',
    'electron/**/*',
    'node_modules/**/*',
    'public/**/*',
  ],
  win: {
    target: [
      { target: 'nsis', arch: ['x64', 'ia32'] },
      { target: 'portable', arch: ['x64'] },
      { target: 'msi', arch: ['x64'] },
    ],
    certificateFile: process.env.WIN_CSC_LINK,
    certificatePassword: process.env.WIN_CSC_KEY_PASSWORD,
    signingHashAlgorithms: ['sha256'],
  },
  nsis: {
    oneClick: false,
    allowToChangeInstallationDirectory: true,
    createDesktopShortcut: true,
    createStartMenuShortcut: true,
    shortcutName: 'HR Analytics',
  },
  mac: {
    target: ['dmg', 'zip'],
    category: 'public.app-category.business',
    certificateFile: process.env.MAC_CSC_LINK,
    certificatePassword: process.env.MAC_CSC_KEY_PASSWORD,
    signingIdentity: process.env.MAC_CSC_IDENTITY,
    notarize: {
      teamId: process.env.APPLE_TEAM_ID,
    },
  },
  dmg: {
    sign: false,
  },
  linux: {
    target: ['AppImage', 'deb'],
    category: 'Office',
  },
  publish: {
    provider: 'github',
    owner: 'your-org',
    repo: 'hr-analytics',
  },
};
```

### 2. Build Scripts

```json
{
  "scripts": {
    "electron-dev": "cross-env NODE_ENV=development electron .",
    "electron-build": "npm run build && electron-builder",
    "electron-build:win": "npm run electron-build -- -w",
    "electron-build:mac": "npm run electron-build -- -m",
    "electron-build:linux": "npm run electron-build -- -l",
    "electron-publish": "npm run electron-build -- -p always"
  }
}
```

### 3. Code Signing (macOS)

```bash
#!/bin/bash
# scripts/sign-mac.sh

# Export certificate
echo "$MAC_CSC_LINK" | base64 -d > /tmp/certificate.p12

# Set environment
export CSC_LINK="/tmp/certificate.p12"
export CSC_KEY_PASSWORD="$MAC_CSC_KEY_PASSWORD"
export APPLE_TEAM_ID="$APPLE_TEAM_ID"

# Build
npm run electron-build:mac

# Cleanup
rm /tmp/certificate.p12
```

### 4. Code Signing (Windows)

```bash
#!/bin/bash
# scripts/sign-windows.sh

# Export certificate
echo "$WIN_CSC_LINK" | base64 -d > /tmp/certificate.pfx

# Set environment
export WIN_CSC_LINK="/tmp/certificate.pfx"
export WIN_CSC_KEY_PASSWORD="$WIN_CSC_KEY_PASSWORD"

# Build
npm run electron-build:win

# Cleanup
rm /tmp/certificate.pfx
```

---

## Auto-Updates

### Update Server

```typescript
// Node.js/Express server for updates
import express from 'express';

const app = express();

app.get('/update/win32/:version', (req, res) => {
  const { version } = req.params;
  
  // Check if newer version available
  const latestVersion = '1.2.0';
  
  if (version < latestVersion) {
    res.json({
      url: 'https://releases.hranalytics.com/v1.2.0/HR-Analytics-Setup.exe',
      version: latestVersion,
      releaseDate: new Date().toISOString(),
    });
  } else {
    res.status(204).send(); // No update needed
  }
});

app.listen(3000);
```

### Electron Auto-Update Configuration

```typescript
// electron/main.ts
autoUpdater.checkForUpdatesAndNotify();

// Check every 10 minutes
setInterval(() => {
  autoUpdater.checkForUpdates();
}, 10 * 60 * 1000);
```

---

## GitHub Actions CI/CD

```yaml
# .github/workflows/build-electron.yml
name: Build Electron App

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]

    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci

      - name: Build Angular
        run: npm run build

      - name: Build Electron
        env:
          WIN_CSC_LINK: ${{ secrets.WIN_CSC_LINK }}
          WIN_CSC_KEY_PASSWORD: ${{ secrets.WIN_CSC_KEY_PASSWORD }}
          MAC_CSC_LINK: ${{ secrets.MAC_CSC_LINK }}
          MAC_CSC_KEY_PASSWORD: ${{ secrets.MAC_CSC_KEY_PASSWORD }}
          APPLE_TEAM_ID: ${{ secrets.APPLE_TEAM_ID }}
        run: npm run electron-build -- -p always

      - name: Upload artifacts
        uses: actions/upload-artifact@v3
        with:
          name: releases-${{ matrix.os }}
          path: dist-electron/
```

---

**Last Updated:** July 2026
**Status:** Electron Desktop Complete
