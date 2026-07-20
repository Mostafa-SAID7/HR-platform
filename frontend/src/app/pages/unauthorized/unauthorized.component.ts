import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display:flex; align-items:center; justify-content:center; min-height:100vh; background:var(--surface-bg); }

    .wrapper {
      display: flex; flex-direction: column; align-items: center;
      justify-content: center; text-align: center;
      padding: 40px 24px; max-width: 520px;
      position: relative; z-index: 1;
    }

    /* Lock illustration */
    .illustration { width: 180px; height: 180px; margin-bottom: 32px; }

    .lock-body {
      animation: scale-in 0.5s cubic-bezier(0.34,1.56,0.64,1) 0.4s both;
    }
    .lock-shackle {
      animation: draw-shackle 0.7s ease-out 0.1s both;
      stroke-dasharray: 200;
      stroke-dashoffset: 200;
    }
    .lock-keyhole {
      animation: scale-in 0.4s cubic-bezier(0.34,1.56,0.64,1) 0.9s both;
      opacity: 0;
    }
    .pulse-ring {
      animation: pulse-ring 2s ease-out 1.2s infinite;
      transform-origin: center;
      opacity: 0;
    }
    .pulse-ring:nth-child(2) { animation-delay: 1.6s; }

    @keyframes scale-in {
      from { opacity:0; transform:scale(0.4); }
      to   { opacity:1; transform:scale(1); }
    }
    @keyframes draw-shackle {
      from { stroke-dashoffset:200; }
      to   { stroke-dashoffset:0; }
    }
    @keyframes pulse-ring {
      0%   { transform:scale(0.85); opacity:0.6; }
      100% { transform:scale(1.5); opacity:0; }
    }
    @keyframes shake-lock {
      0%,100% { transform:translateX(0); }
      20%      { transform:translateX(-4px) rotate(-2deg); }
      40%      { transform:translateX(4px)  rotate(2deg); }
      60%      { transform:translateX(-3px) rotate(-1deg); }
      80%      { transform:translateX(3px)  rotate(1deg); }
    }
    .illustration:hover .lock-group { animation: shake-lock 0.5s ease-in-out; }

    .error-code {
      font-size: 72px; font-weight: 800; line-height:1; letter-spacing:-0.04em;
      background: linear-gradient(135deg, #dc2626, #f59e0b);
      -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
      margin-bottom: 12px;
      animation: fade-up 0.6s ease-out 1.1s both;
    }
    .error-title {
      font-size: 22px; font-weight: 700; color: var(--gray-900); margin-bottom: 10px;
      animation: fade-up 0.6s ease-out 1.25s both;
    }
    .error-desc {
      font-size: 14.5px; color: var(--gray-500); line-height: 1.65; margin-bottom: 28px;
      animation: fade-up 0.6s ease-out 1.4s both;
    }
    .actions {
      display: flex; gap: 10px; justify-content: center; flex-wrap: wrap;
      animation: fade-up 0.6s ease-out 1.55s both;
    }

    @keyframes fade-up {
      from { opacity:0; transform:translateY(14px); }
      to   { opacity:1; transform:translateY(0); }
    }

    /* Background shield pattern */
    .bg-pattern {
      position: fixed; inset:0; pointer-events:none; overflow:hidden; z-index:0;
      opacity: 0.04;
    }
  `],
  template: `
    <!-- Subtle background -->
    <div class="bg-pattern">
      <svg width="100%" height="100%" viewBox="0 0 400 400">
        <defs>
          <pattern id="shield-pat" x="0" y="0" width="80" height="80" patternUnits="userSpaceOnUse">
            <path d="M40 8L8 20v18c0 18 13.6 34.8 32 39 18.4-4.2 32-21 32-39V20L40 8z" fill="#dc2626"/>
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#shield-pat)"/>
      </svg>
    </div>

    <div class="wrapper">
      <!-- Lock SVG -->
      <svg class="illustration" viewBox="0 0 180 180" fill="none">
        <!-- Pulse rings -->
        <circle class="pulse-ring" cx="90" cy="105" r="55" stroke="#dc2626" stroke-width="2"/>
        <circle class="pulse-ring" cx="90" cy="105" r="55" stroke="#dc2626" stroke-width="2"/>

        <g class="lock-group">
          <!-- Shackle arc -->
          <path
            class="lock-shackle"
            d="M62 90V68a28 28 0 0 1 56 0v22"
            stroke="url(#lockGrad)"
            stroke-width="9"
            stroke-linecap="round"
          />
          <!-- Lock body -->
          <rect class="lock-body" x="50" y="90" width="80" height="62" rx="10" fill="url(#lockGrad)"/>
          <!-- Keyhole -->
          <g class="lock-keyhole">
            <circle cx="90" cy="116" r="10" fill="rgba(0,0,0,0.25)"/>
            <rect x="86" y="116" width="8" height="14" rx="3" fill="rgba(0,0,0,0.25)"/>
          </g>
        </g>

        <!-- Stars / sparkles -->
        <g opacity="0.6">
          <path d="M148 48l2 6 6 2-6 2-2 6-2-6-6-2 6-2z" fill="#f59e0b" style="animation:pop-in 0.4s 1.4s both;opacity:0"/>
          <path d="M32 52l1.5 4.5L38 58l-4.5 1.5L32 64l-1.5-4.5L26 58l4.5-1.5z" fill="#4f6ef7" style="animation:pop-in 0.4s 1.6s both;opacity:0"/>
          <path d="M155 130l1 3 3 1-3 1-1 3-1-3-3-1 3-1z" fill="#7c5cfc" style="animation:pop-in 0.4s 1.8s both;opacity:0"/>
        </g>

        <defs>
          <linearGradient id="lockGrad" x1="50" y1="40" x2="130" y2="160" gradientUnits="userSpaceOnUse">
            <stop stop-color="#dc2626"/>
            <stop offset="1" stop-color="#f59e0b"/>
          </linearGradient>
        </defs>
      </svg>

      <div class="error-code">401</div>
      <div class="error-title">Access denied</div>
      <p class="error-desc">
        You don't have permission to view this page.<br>
        Contact your administrator if you believe this is a mistake.
      </p>
      <div class="actions">
        <a routerLink="/dashboard" class="btn btn-primary">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="19 12 12 12 5 12"/>
            <polyline points="12 19 5 12 12 5"/>
          </svg>
          Back to Dashboard
        </a>
        <a routerLink="/login" class="btn btn-secondary">Sign in with another account</a>
      </div>
    </div>
  `,
})
export class UnauthorizedComponent {}
