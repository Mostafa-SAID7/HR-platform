import { Component, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display:flex; align-items:center; justify-content:center; min-height:100vh; background:var(--surface-bg); }

    .wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 40px 24px;
      max-width: 520px;
    }

    /* Animated SVG illustration */
    .illustration { width: 200px; height: 200px; margin-bottom: 32px; }

    .circle-ring {
      animation: draw-ring 1.2s cubic-bezier(0.4,0,0.2,1) forwards;
      stroke-dasharray: 502;
      stroke-dashoffset: 502;
    }

    .four-left, .four-right {
      animation: pop-in 0.5s cubic-bezier(0.34,1.56,0.64,1) forwards;
      opacity: 0;
    }
    .four-left  { animation-delay: 0.7s; }
    .four-right { animation-delay: 0.85s; }
    .zero-group {
      animation: pop-in 0.5s cubic-bezier(0.34,1.56,0.64,1) 0.78s forwards;
      opacity: 0;
    }

    .floating-dot {
      animation: float-dot 3s ease-in-out infinite;
    }
    .floating-dot:nth-child(2) { animation-delay: 0.4s; }
    .floating-dot:nth-child(3) { animation-delay: 0.8s; }

    .glow {
      animation: glow-pulse 2s ease-in-out infinite;
    }

    @keyframes draw-ring {
      from { stroke-dashoffset: 502; opacity: 0; }
      10%  { opacity: 1; }
      to   { stroke-dashoffset: 0; }
    }
    @keyframes pop-in {
      0%   { opacity:0; transform:scale(0.3) translateY(10px); }
      70%  { transform:scale(1.1) translateY(-3px); }
      100% { opacity:1; transform:scale(1) translateY(0); }
    }
    @keyframes float-dot {
      0%,100% { transform:translateY(0); }
      50%      { transform:translateY(-12px); }
    }
    @keyframes glow-pulse {
      0%,100% { opacity:0.15; r:60; }
      50%      { opacity:0.25; r:64; }
    }

    .error-code {
      font-size: 80px;
      font-weight: 800;
      line-height: 1;
      letter-spacing: -0.04em;
      background: linear-gradient(135deg, #4f6ef7, #7c5cfc);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      margin-bottom: 12px;
      animation: fade-up 0.6s ease-out 1.2s both;
    }
    .error-title {
      font-size: 22px;
      font-weight: 700;
      color: var(--gray-900);
      margin-bottom: 10px;
      animation: fade-up 0.6s ease-out 1.35s both;
    }
    .error-desc {
      font-size: 14.5px;
      color: var(--gray-500);
      line-height: 1.65;
      margin-bottom: 28px;
      animation: fade-up 0.6s ease-out 1.5s both;
    }
    .actions {
      display: flex; gap: 10px; justify-content: center; flex-wrap: wrap;
      animation: fade-up 0.6s ease-out 1.65s both;
    }

    @keyframes fade-up {
      from { opacity:0; transform:translateY(14px); }
      to   { opacity:1; transform:translateY(0); }
    }

    /* Floating particles */
    .particles {
      position: fixed; inset: 0; pointer-events: none; overflow: hidden; z-index: 0;
    }
    .particle {
      position: absolute;
      width: 6px; height: 6px;
      border-radius: 50%;
      background: var(--color-primary-muted);
      animation: drift linear infinite;
    }
    @keyframes drift {
      from { transform: translateY(100vh) scale(0); opacity: 0; }
      10%  { opacity: 1; }
      90%  { opacity: 0.4; }
      to   { transform: translateY(-100px) scale(1); opacity: 0; }
    }
  `],
  template: `
    <!-- Floating particles -->
    <div class="particles">
      <div *ngFor="let p of particles" class="particle"
        [style.left.%]="p.x"
        [style.width.px]="p.size"
        [style.height.px]="p.size"
        [style.background]="p.color"
        [style.animation-duration.s]="p.duration"
        [style.animation-delay.s]="p.delay"
      ></div>
    </div>

    <div class="wrapper" style="position:relative;z-index:1;">
      <!-- SVG illustration -->
      <svg class="illustration" viewBox="0 0 200 200" fill="none">
        <!-- Glow -->
        <circle class="glow" cx="100" cy="100" r="60" fill="#4f6ef7"/>

        <!-- Ring -->
        <circle
          class="circle-ring"
          cx="100" cy="100" r="80"
          stroke="url(#ringGrad)"
          stroke-width="4"
          stroke-linecap="round"
          transform="rotate(-90 100 100)"
        />

        <!-- 404 text -->
        <g class="four-left">
          <text x="22" y="118" font-family="system-ui,-apple-system,sans-serif" font-size="52" font-weight="800" fill="url(#textGrad)" text-anchor="middle">4</text>
        </g>
        <g class="zero-group">
          <text x="100" y="118" font-family="system-ui,-apple-system,sans-serif" font-size="52" font-weight="800" fill="url(#textGrad)" text-anchor="middle">0</text>
        </g>
        <g class="four-right">
          <text x="178" y="118" font-family="system-ui,-apple-system,sans-serif" font-size="52" font-weight="800" fill="url(#textGrad)" text-anchor="middle">4</text>
        </g>

        <!-- Floating dots -->
        <circle class="floating-dot" cx="30"  cy="50"  r="5" fill="#4f6ef7" opacity="0.5"/>
        <circle class="floating-dot" cx="170" cy="60"  r="4" fill="#7c5cfc" opacity="0.5"/>
        <circle class="floating-dot" cx="155" cy="160" r="6" fill="#06b6d4" opacity="0.4"/>

        <defs>
          <linearGradient id="ringGrad" x1="0" y1="0" x2="200" y2="200" gradientUnits="userSpaceOnUse">
            <stop stop-color="#4f6ef7"/>
            <stop offset="1" stop-color="#7c5cfc"/>
          </linearGradient>
          <linearGradient id="textGrad" x1="0" y1="0" x2="200" y2="0" gradientUnits="userSpaceOnUse">
            <stop stop-color="#4f6ef7"/>
            <stop offset="1" stop-color="#7c5cfc"/>
          </linearGradient>
        </defs>
      </svg>

      <div class="error-code">404</div>
      <div class="error-title">Page not found</div>
      <p class="error-desc">
        The page you're looking for doesn't exist or has been moved.<br>
        Let's get you back on track.
      </p>
      <div class="actions">
        <a routerLink="/dashboard" class="btn btn-primary">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="19 12 12 12 5 12"/>
            <polyline points="12 19 5 12 12 5"/>
          </svg>
          Back to Dashboard
        </a>
        <button class="btn btn-secondary" onclick="history.back()">Go Back</button>
      </div>
    </div>
  `,
})
export class NotFoundComponent {
  particles = Array.from({ length: 15 }, (_, i) => ({
    x: Math.random() * 100,
    size: Math.random() * 6 + 3,
    color: ['rgba(79,110,247,0.25)', 'rgba(124,92,252,0.2)', 'rgba(6,182,212,0.2)'][i % 3],
    duration: Math.random() * 8 + 6,
    delay: Math.random() * -14,
  }));
}
