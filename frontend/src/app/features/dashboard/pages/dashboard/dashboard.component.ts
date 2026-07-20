import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display: block; }

    .page { padding: 28px 28px 40px; max-width: 1400px; }

    /* ── Stat grid ─────────────────── */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }
    @media (max-width: 1200px) { .stats-grid { grid-template-columns: repeat(2, 1fr); } }
    @media (max-width: 600px)  { .stats-grid { grid-template-columns: 1fr; } }

    .stat-card { --stat-accent: var(--kpi-color); }

    .stat-icon-wrap {
      width: 40px; height: 40px;
      border-radius: 9px;
      display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
    }

    /* ── Middle grid ───────────────── */
    .mid-grid {
      display: grid;
      grid-template-columns: 1fr 340px;
      gap: 16px;
      margin-bottom: 16px;
    }
    @media (max-width: 900px) { .mid-grid { grid-template-columns: 1fr; } }

    /* ── Bottom grid ───────────────── */
    .bot-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }
    @media (max-width: 900px) { .bot-grid { grid-template-columns: 1fr; } }

    /* ── Card header ───────────────── */
    .card-head {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px 12px;
      border-bottom: 1px solid var(--surface-divider);
    }
    .card-head-title {
      font-size: 14px; font-weight: 600; color: var(--gray-900);
    }
    .card-head-sub {
      font-size: 11.5px; color: var(--gray-400); margin-top: 2px;
    }
    .card-body { padding: 16px 20px; }

    /* ── Micro bar chart ───────────── */
    .bar-row {
      display: flex; align-items: center; gap: 10px;
      padding: 7px 0;
    }
    .bar-row + .bar-row { border-top: 1px solid var(--surface-divider); }
    .bar-label { font-size: 12.5px; color: var(--gray-600); width: 80px; flex-shrink: 0; }
    .bar-track { flex: 1; height: 7px; background: var(--gray-100); border-radius: 3px; overflow: hidden; }
    .bar-fill  { height: 100%; border-radius: 3px; }
    .bar-val   { font-size: 12px; font-weight: 600; color: var(--gray-700); width: 36px; text-align: right; flex-shrink: 0; }

    /* ── Activity feed ─────────────── */
    .activity-item {
      display: flex; align-items: flex-start; gap: 12px;
      padding: 10px 0;
    }
    .activity-item + .activity-item { border-top: 1px solid var(--surface-divider); }
    .activity-dot {
      width: 8px; height: 8px; border-radius: 50%; margin-top: 5px; flex-shrink: 0;
    }
    .activity-text { font-size: 12.5px; color: var(--gray-700); line-height: 1.5; }
    .activity-time { font-size: 11px; color: var(--gray-400); margin-top: 2px; }

    /* ── Employee row ──────────────── */
    .emp-row {
      display: flex; align-items: center; gap: 12px;
      padding: 9px 0;
    }
    .emp-row + .emp-row { border-top: 1px solid var(--surface-divider); }
    .emp-info { flex: 1; min-width: 0; }
    .emp-name { font-size: 13px; font-weight: 500; color: var(--gray-800); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .emp-role { font-size: 11.5px; color: var(--gray-400); }
    .emp-score { font-size: 13px; font-weight: 600; }

    /* ── Donut chart ───────────────── */
    .donut-wrap { display: flex; justify-content: center; padding: 16px 0; }
    .donut-legend { display: flex; flex-direction: column; gap: 8px; padding: 0 20px 16px; }
    .legend-item {
      display: flex; align-items: center; gap: 8px;
      font-size: 12.5px; color: var(--gray-600);
    }
    .legend-dot { width: 10px; height: 10px; border-radius: 3px; flex-shrink: 0; }
    .legend-val { margin-left: auto; font-weight: 600; color: var(--gray-800); }

    /* ── Animations ────────────────── */
    .stat-card { animation: slideUp 0.4s ease-out forwards; opacity: 0; }
    .stagger .stat-card:nth-child(1) { animation-delay: 0ms; }
    .stagger .stat-card:nth-child(2) { animation-delay: 60ms; }
    .stagger .stat-card:nth-child(3) { animation-delay: 120ms; }
    .stagger .stat-card:nth-child(4) { animation-delay: 180ms; }

    @keyframes slideUp {
      from { opacity: 0; transform: translateY(10px); }
      to   { opacity: 1; transform: translateY(0); }
    }
  `],
  template: `
<div class="page">

  <!-- Page header -->
  <div class="page-header">
    <div>
      <h1 class="page-title">Dashboard</h1>
      <p class="page-subtitle">Welcome back — here's what's happening today.</p>
    </div>
    <div style="display:flex;gap:8px;flex-wrap:wrap;">
      <button class="btn btn-secondary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
          <polyline points="7 10 12 15 17 10"/>
          <line x1="12" y1="15" x2="12" y2="3"/>
        </svg>
        Export
      </button>
      <button class="btn btn-primary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/>
          <line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Add Employee
      </button>
    </div>
  </div>

  <!-- KPI Cards -->
  <div class="stats-grid stagger">
    <div *ngFor="let kpi of kpis" class="stat-card card-3d" [style.--kpi-color]="kpi.color">
      <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:12px;">
        <div>
          <div class="stat-label">{{ kpi.label }}</div>
          <div class="stat-value" style="margin-top:8px;">{{ kpi.value }}</div>
          <div style="margin-top:8px;">
            <span class="stat-delta" [class.up]="kpi.up" [class.down]="!kpi.up">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <polyline [attr.points]="kpi.up ? '18 15 12 9 6 15' : '6 9 12 15 18 9'"/>
              </svg>
              {{ kpi.delta }}
            </span>
            <span style="font-size:11.5px;color:var(--gray-400);margin-left:4px;">vs last month</span>
          </div>
        </div>
        <div class="stat-icon-wrap" [style.background]="kpi.iconBg">
          <span [style.color]="kpi.color" [innerHTML]="kpi.icon"></span>
        </div>
      </div>
      <!-- Mini sparkline -->
      <div style="margin-top:14px;">
        <svg width="100%" height="36" viewBox="0 0 120 36" preserveAspectRatio="none">
          <polyline
            [attr.points]="kpi.spark"
            fill="none"
            [attr.stroke]="kpi.color"
            stroke-width="1.8"
            stroke-linejoin="round"
            stroke-linecap="round"
            opacity="0.7"
          />
        </svg>
      </div>
    </div>
  </div>

  <!-- Middle row -->
  <div class="mid-grid">

    <!-- Department breakdown chart -->
    <div class="card-3d">
      <div class="card-head">
        <div>
          <div class="card-head-title">Headcount by Department</div>
          <div class="card-head-sub">Total 1,284 employees across 6 departments</div>
        </div>
        <span class="badge badge-primary">Live</span>
      </div>
      <div class="card-body">
        <div *ngFor="let d of departments" class="bar-row">
          <span class="bar-label">{{ d.name }}</span>
          <div class="bar-track">
            <div class="bar-fill" [style.width.%]="d.pct" [style.background]="d.color"></div>
          </div>
          <span class="bar-val">{{ d.count }}</span>
        </div>
      </div>
    </div>

    <!-- Hiring funnel donut -->
    <div class="card-3d">
      <div class="card-head">
        <div>
          <div class="card-head-title">Hiring Funnel</div>
          <div class="card-head-sub">Q2 2026</div>
        </div>
      </div>
      <div class="donut-wrap">
        <svg width="130" height="130" viewBox="0 0 130 130">
          <circle cx="65" cy="65" r="50" fill="none" stroke="var(--gray-100)" stroke-width="22"/>
          <circle cx="65" cy="65" r="50" fill="none" stroke="#4f6ef7" stroke-width="22"
            stroke-dasharray="314" stroke-dashoffset="47" stroke-linecap="butt" transform="rotate(-90 65 65)"/>
          <circle cx="65" cy="65" r="50" fill="none" stroke="#7c5cfc" stroke-width="22"
            stroke-dasharray="314" stroke-dashoffset="188" stroke-linecap="butt" transform="rotate(-74 65 65)"/>
          <circle cx="65" cy="65" r="50" fill="none" stroke="#06b6d4" stroke-width="22"
            stroke-dasharray="314" stroke-dashoffset="251" stroke-linecap="butt" transform="rotate(26 65 65)"/>
          <text x="65" y="61" text-anchor="middle" font-size="20" font-weight="700" fill="var(--gray-900)">248</text>
          <text x="65" y="76" text-anchor="middle" font-size="10" fill="var(--gray-400)">Applicants</text>
        </svg>
      </div>
      <div class="donut-legend">
        <div *ngFor="let s of funnelStages" class="legend-item">
          <div class="legend-dot" [style.background]="s.color"></div>
          <span>{{ s.label }}</span>
          <span class="legend-val">{{ s.val }}</span>
        </div>
      </div>
    </div>
  </div>

  <!-- Bottom row -->
  <div class="bot-grid">

    <!-- Top performers -->
    <div class="card-3d">
      <div class="card-head">
        <div class="card-head-title">Top Performers</div>
        <a href="/performance" style="font-size:12px;color:var(--color-primary);">View all →</a>
      </div>
      <div class="card-body" style="padding-top:8px;">
        <div *ngFor="let p of topPerformers" class="emp-row">
          <div class="avatar avatar-md" [style.background]="p.avatarBg" style="color:#fff;">{{ p.initials }}</div>
          <div class="emp-info">
            <div class="emp-name">{{ p.name }}</div>
            <div class="emp-role">{{ p.department }}</div>
          </div>
          <span class="emp-score" [style.color]="p.scoreColor">{{ p.score }}%</span>
        </div>
      </div>
    </div>

    <!-- Recent activity -->
    <div class="card-3d">
      <div class="card-head">
        <div class="card-head-title">Recent Activity</div>
        <span class="badge badge-neutral">Today</span>
      </div>
      <div class="card-body" style="padding-top:8px;">
        <div *ngFor="let a of activities" class="activity-item">
          <div class="activity-dot" [style.background]="a.color"></div>
          <div>
            <div class="activity-text">{{ a.text }}</div>
            <div class="activity-time">{{ a.time }}</div>
          </div>
        </div>
      </div>
    </div>

  </div>
</div>
  `,
})
export class DashboardComponent {
  kpis = [
    {
      label: 'Total Employees', value: '1,284', delta: '3.2%', up: true,
      color: '#4f6ef7', iconBg: 'rgba(79,110,247,0.1)',
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`,
      spark: '0,30 20,22 40,28 60,14 80,18 100,8 120,12'
    },
    {
      label: 'Avg Performance', value: '87.4%', delta: '1.8%', up: true,
      color: '#16a34a', iconBg: 'rgba(22,163,74,0.1)',
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>`,
      spark: '0,28 20,20 40,24 60,16 80,12 100,8 120,4'
    },
    {
      label: 'Open Positions', value: '38', delta: '12.5%', up: false,
      color: '#d97706', iconBg: 'rgba(217,119,6,0.1)',
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>`,
      spark: '0,8 20,14 40,10 60,18 80,14 100,22 120,28'
    },
    {
      label: 'Attendance Rate', value: '96.2%', delta: '0.4%', up: true,
      color: '#0284c7', iconBg: 'rgba(2,132,199,0.1)',
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/><polyline points="9 16 11 18 15 14"/></svg>`,
      spark: '0,20 20,16 40,18 60,10 80,8 100,6 120,4'
    },
  ];

  departments = [
    { name: 'Engineering', count: 312, pct: 78, color: '#4f6ef7' },
    { name: 'Sales',       count: 248, pct: 62, color: '#7c5cfc' },
    { name: 'Marketing',   count: 184, pct: 46, color: '#06b6d4' },
    { name: 'HR',          count: 96,  pct: 24, color: '#16a34a' },
    { name: 'Finance',     count: 280, pct: 70, color: '#d97706' },
    { name: 'Operations',  count: 164, pct: 41, color: '#dc2626' },
  ];

  funnelStages = [
    { label: 'Applied',       val: '248', color: '#4f6ef7' },
    { label: 'Interviewed',   val: '86',  color: '#7c5cfc' },
    { label: 'Offer Sent',    val: '24',  color: '#06b6d4' },
    { label: 'Hired',         val: '14',  color: '#16a34a' },
  ];

  topPerformers = [
    { name: 'Sarah Mitchell', department: 'Engineering', initials: 'SM', score: 97, avatarBg: '#4f6ef7', scoreColor: '#16a34a' },
    { name: 'James Okafor',   department: 'Sales',       initials: 'JO', score: 94, avatarBg: '#7c5cfc', scoreColor: '#16a34a' },
    { name: 'Priya Sharma',   department: 'Marketing',   initials: 'PS', score: 91, avatarBg: '#06b6d4', scoreColor: '#16a34a' },
    { name: 'Tom Nakamura',   department: 'Finance',     initials: 'TN', score: 89, avatarBg: '#d97706', scoreColor: '#d97706' },
    { name: 'Layla Hassan',   department: 'HR',          initials: 'LH', score: 88, avatarBg: '#dc2626', scoreColor: '#d97706' },
  ];

  activities = [
    { text: 'Sarah Mitchell promoted to Senior Engineer', time: '2 min ago',  color: '#4f6ef7' },
    { text: '5 new applicants for Lead Designer role',    time: '18 min ago', color: '#7c5cfc' },
    { text: 'Q2 payroll batch processed successfully',    time: '1 hr ago',   color: '#16a34a' },
    { text: 'Performance review cycle started',           time: '3 hr ago',   color: '#d97706' },
    { text: 'James Okafor completed onboarding',          time: '5 hr ago',   color: '#06b6d4' },
  ];
}
