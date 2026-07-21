# Tauri Desktop Application
## Lightweight Alternative to Electron

Complete guide to packaging Angular app with Tauri (Rust-based, smaller bundle size).

---

## Tauri Overview

### Why Tauri?

```
Electron:
- 150MB app size
- High memory usage (~500MB)
- Slower startup
- Full Chromium browser included

Tauri:
✅ 10-50MB app size (3-15x smaller!)
✅ Low memory usage (~100MB)
✅ Fast startup
✅ Uses system WebView
✅ Rust for security
✅ Better performance
```

### When to Use Tauri

```
Choose Tauri if:
✅ App size critical (laptop distribution)
✅ Memory-constrained environments
✅ Want better security (Rust prevents memory bugs)
✅ Don't need extensive OS integrations

Choose Electron if:
✅ Maximum compatibility needed
✅ Complex OS integrations required
✅ Team unfamiliar with Rust
✅ Ecosystem size important
```

---

## Project Setup

### 1. Install Tauri CLI

```bash
npm install --save-dev @tauri-apps/cli@latest
npm install @tauri-apps/api@latest

# Initialize Tauri
npm run tauri init
```

### 2. Directory Structure

```
HR-platform/
├── frontend/              # Angular web app
│   ├── src/
│   ├── dist/             # Built Angular app
│   └── package.json
│
├── src-tauri/            # Tauri Rust backend
│   ├── src/
│   │   ├── main.rs       # Entry point
│   │   └── lib.rs
│   ├── Cargo.toml        # Rust dependencies
│   └── tauri.conf.json   # Configuration
│
└── package.json
```

### 3. Tauri Configuration

```json
// src-tauri/tauri.conf.json
{
  "build": {
    "beforeBuildCommand": "npm run build",
    "beforeDevCommand": "npm run start",
    "devPath": "http://localhost:4200",
    "frontendDist": "../dist"
  },
  "app": {
    "windows": [
      {
        "title": "HR Analytics",
        "width": 1400,
        "height": 900,
        "minWidth": 1000,
        "minHeight": 700,
        "resizable": true,
        "fullscreen": false
      }
    ],
    "security": {
      "csp": null
    }
  },
  "package": {
    "productName": "HR Analytics",
    "version": "1.0.0"
  },
  "tauri": {
    "allowlist": {
      "all": false,
      "shell": {
        "all": false,
        "execute": true,
        "open": true
      },
      "fs": {
        "all": false,
        "readFile": true,
        "writeFile": true,
        "removeFile": true,
        "readDir": true,
        "createDir": true
      },
      "path": {
        "all": true
      },
      "dialog": {
        "all": false,
        "open": true,
        "save": true
      },
      "notification": {
        "all": true
      }
    },
    "windows": [
      {
        "label": "main",
        "title": "HR Analytics",
        "url": "/"
      }
    ],
    "security": {
      "csp": "default-src 'self'"
    }
  }
}
```

---

## Rust Backend Commands

### File Operations

```rust
// src-tauri/src/main.rs
use tauri::command;
use std::fs;
use std::path::PathBuf;

#[command]
async fn open_file(path: String) -> Result<String, String> {
    fs::read_to_string(path)
        .map_err(|e| e.to_string())
}

#[command]
async fn save_file(path: String, content: String) -> Result<(), String> {
    fs::write(path, content)
        .map_err(|e| e.to_string())
}

#[command]
async fn export_to_excel(data: String, filename: String) -> Result<String, String> {
    let path = dirs::download_dir()
        .unwrap()
        .join(&filename);
    
    fs::write(&path, data)
        .map_err(|e| e.to_string())?;
    
    Ok(path.to_string_lossy().to_string())
}

#[command]
async fn get_download_dir() -> Result<String, String> {
    dirs::download_dir()
        .ok_or("Unable to get download directory".to_string())
        .map(|p| p.to_string_lossy().to_string())
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![
            open_file,
            save_file,
            export_to_excel,
            get_download_dir
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
```

---

## Angular Integration

### Tauri Service

```typescript
// src/app/core/services/tauri.service.ts
import { Injectable } from '@angular/core';
import { invoke } from '@tauri-apps/api/tauri';
import { open, save } from '@tauri-apps/api/dialog';

@Injectable({
  providedIn: 'root'
})
export class TauriService {
  isTauri: boolean = (window as any).__TAURI__ !== undefined;

  async openFile(): Promise<string | null> {
    if (!this.isTauri) return null;

    const selected = await open({
      filters: [
        { name: 'Excel Files', extensions: ['xlsx', 'xls'] },
        { name: 'All Files', extensions: ['*'] },
      ],
    });

    if (Array.isArray(selected)) {
      return selected[0];
    }
    return selected;
  }

  async saveFile(content: string, filename: string): Promise<string | null> {
    if (!this.isTauri) return null;

    const path = await save({
      defaultPath: filename,
      filters: [{ name: 'Excel Files', extensions: ['xlsx'] }],
    });

    if (path) {
      await invoke('save_file', {
        path,
        content,
      });
    }

    return path;
  }

  async exportToExcel(data: string, filename: string): Promise<string> {
    if (!this.isTauri) {
      throw new Error('Tauri not available');
    }

    return await invoke('export_to_excel', {
      data,
      filename,
    });
  }

  async getDownloadDir(): Promise<string> {
    if (!this.isTauri) {
      throw new Error('Tauri not available');
    }

    return await invoke('get_download_dir');
  }
}
```

---

## Building & Distribution

### 1. Build Scripts

```json
{
  "scripts": {
    "tauri-dev": "tauri dev",
    "tauri-build": "tauri build",
    "tauri-build:windows": "tauri build --target x86_64-pc-windows-msvc",
    "tauri-build:macos": "tauri build --target universal-apple-darwin",
    "tauri-build:linux": "tauri build --target x86_64-unknown-linux-gnu"
  }
}
```

### 2. Cargo.toml Configuration

```toml
[package]
name = "hr-analytics"
version = "1.0.0"
edition = "2021"

[dependencies]
tauri = { version = "1.5", features = ["shell-open"] }
serde = { version = "1.0", features = ["derive"] }
serde_json = "1.0"
tokio = { version = "1", features = ["full"] }
dirs = "5.0"

[build-dependencies]
tauri-build = "1.5"

[profile.release]
opt-level = "z"
lto = true
strip = true
```

### 3. GitHub Actions for Tauri

```yaml
# .github/workflows/build-tauri.yml
name: Build Tauri App

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            target: x86_64-unknown-linux-gnu
          - os: windows-latest
            target: x86_64-pc-windows-msvc
          - os: macos-latest
            target: universal-apple-darwin

    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install Rust
        uses: dtolnay/rust-toolchain@stable
        with:
          targets: ${{ matrix.target }}

      - name: Setup Linux dependencies
        if: matrix.os == 'ubuntu-latest'
        run: |
          sudo apt-get update
          sudo apt-get install -y libgtk-3-dev libwebkit2gtk-4.0-dev libappindicator3-dev librsvg2-dev patchelf

      - name: Install dependencies
        run: npm ci

      - name: Build Angular
        run: npm run build

      - name: Build Tauri
        run: npm run tauri-build -- --target ${{ matrix.target }}

      - name: Upload artifacts
        uses: actions/upload-artifact@v3
        with:
          name: tauri-release-${{ matrix.os }}
          path: src-tauri/target/release/bundle/
```

---

## Security Considerations

### Content Security Policy

```json
{
  "tauri": {
    "security": {
      "csp": "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';"
    }
  }
}
```

### Allowlist Configuration

```json
{
  "tauri": {
    "allowlist": {
      "all": false,
      "fs": {
        "readFile": true,
        "writeFile": true,
        "removeFile": true
      },
      "shell": {
        "execute": false,
        "open": true
      }
    }
  }
}
```

---

## Performance Comparison

### Bundle Size

```
Electron:   150MB (with all OS binaries)
Tauri:      35MB average
Size Reduction: 77%
```

### Memory Usage (Idle)

```
Electron:   ~500MB
Tauri:      ~100MB
Improvement: 80%
```

### Startup Time

```
Electron:   2-3 seconds
Tauri:      0.5-1 second
Improvement: 2-3x faster
```

---

## Migration Path

### From Electron to Tauri

```bash
# 1. Install Tauri
npm install --save-dev @tauri-apps/cli

# 2. Initialize Tauri project
npm run tauri init

# 3. Configure tauri.conf.json
# (use same Angular build output)

# 4. Create Rust commands for native functionality

# 5. Update Angular services to use @tauri-apps/api

# 6. Build and test
npm run tauri-build
```

### Keep Angular Code 100% Reusable

```typescript
// Your Angular code remains the same
// Only services differ:

// Option 1: Electron
if (this.electronService.isElectron) {
  await this.electronService.saveFile(data);
}

// Option 2: Tauri
if (this.tauriService.isTauri) {
  await this.tauriService.saveFile(data);
}

// Option 3: Web
// Browser's native APIs
```

---

## Decision Matrix

| Aspect | Electron | Tauri |
|--------|----------|-------|
| Bundle Size | 150MB | 35MB ✅ |
| Memory | 500MB | 100MB ✅ |
| Startup | 2-3s | 0.5-1s ✅ |
| Community | Massive | Growing ✅ |
| Security | Good | Better ✅ |
| OS Integration | Complete | Good |
| Migration Effort | None | Low |
| **Recommendation** | **For most** | **For size-critical** |

---

## Recommendation

```
Phase 1: Start with Electron
- Stable, proven, large ecosystem
- Works with any Angular code
- 8-week development timeline

Phase 2: Evaluate Tauri (Optional)
- If file size/performance critical
- Can use same Angular code
- Consider for alternative distribution

Final Decision:
✅ Electron for 90% of deployments
✅ Tauri for lightweight/performance builds
✅ Both use same Angular source code
```

---

**Last Updated:** July 2026
**Status:** Tauri Alternative Complete
