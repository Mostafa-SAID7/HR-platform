import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display: block; }
    .page { padding: 28px; }

    .top-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }
    @media(max-width:900px) { .top-grid { grid-template-columns: 1fr; } }

    .main-grid {
      display: grid;
      grid-template-columns: 1fr 320px;
      gap: 16px;
    }
    @media(max-width:900px) { .main-grid { grid-template-columns: 1fr; } }

    .card-head { display:flex; align-items:center; justify-content:space-between; padding:16px 20px 12px; border-bottom:1px solid var(--surface-divider); }
    .card-head-title { font-size:14px; font-weight:600; color:var(--gray-900); }
    .card-body { padding:16px 20px; }

    /* Perf row */
    .perf-row { display:flex; align-items:center; gap:12px; padding:10px 0; }
    .perf-row + .perf-row { border-top:1px solid var(--surface-divider); }
    .perf-avatar { width:36px; height:36px; border-radius:8px; display:flex; align-items:center; justify-content:center; color:#fff; font-size:12px; font-weight:700; flex-shrink:0; }
    .perf-name { font-size:13.5px; font-weight:500; color:var(--gray-900); }
    .perf-dept { font-size:11.5px; color:var(--gray-400); }
    .perf-score { font-size:15px; font-weight:700; min-width:40px; text-align:right; }

    /* Goal item */
    .goal-item { padding:12px 0; }
    .goal-item + .goal-item { border-top:1px solid var(--surface-divider); }
    .goal-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:8px; }
    .goal-name { font-size:13px; font-weight:500; color:var(--gray-800); }

    /* Rating stars */
    .stars { display:flex; gap:3px; }
    .star { font-size:14px; }

    /* Distribution bar */
    .dist-row { display:flex; align-items:center; gap:10px; padding:6px 0; }
    .dist-label { font-size:12px; color:var(--gray-600); width:60px; flex-shrink:0; text-align:right; }
    .dist-bar { flex:1; height:8px; background:var(--gray-100); border-radius:3px; overflow:hidden; }
    .dist-fill { height:100%; border-radius:3px; }
    .dist-count { font-size:12px; font-weight:600; color:var(--gray-700); width:28px; flex-shrink:0; }

    .score-big { font-size:48px; font-weight:800; color:var(--gray-900); line-height:1; letter-spacing:-0.03em; }
    .score-label { font-size:13px; color:var(--gray-400); margin-top:4px; }
  `],
  template: `
<div class="page">
  <div class="page-header">
    <div>
      <h1 class="page-title">Performance</h1>
      <p class="page-subtitle">Q2 2026 review cycle — in progress</p>
    </div>
    <div style="display:flex;gap:8px;">
      <button class="btn btn-secondary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
        Export Report
      </button>
      <button class="btn btn-primary">Start Review</button>
    </div>
  </div>

  <!-- Summary cards -->
  <div class="top-grid">
    <div *ngFor="let s of summary" class="stat-card card-3d" [style.--stat-accent]="s.color">
      <div class="stat-label">{{ s.label }}</div>
      <div class="stat-value" style="margin-top:8px;">{{ s.value }}</div>
      <div style="margin-top:6px;" class="stat-delta" [class.up]="s.up" [class.down]="!s.up">
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline [attr.points]="s.up ? '18 15 12 9 6 15' : '6 9 12 15 18 9'"/>
        </svg>
        {{ s.delta }} vs Q1
      </div>
    </div>
  </div>

  <!-- Main content -->
  <div class="main-grid">

    <!-- Leaderboard -->
    <div class="card-3d">
      <div class="card-head">
        <div class="card-head-title">Performance Leaderboard</div>
        <span class="badge badge-primary">Q2 2026</span>
      </div>
      <div class="card-body" style="padding-top:8px;">
        <div *ngFor="let p of leaders; let i=index" class="perf-row">
          <div style="font-size:13px;font-weight:700;color:var(--gray-300);width:20px;text-align:center;flex-shrink:0;">{{ i+1 }}</div>
          <div class="perf-avatar" [style.background]="p.color">{{ p.initials }}</div>
          <div style="flex:1;min-width:0;">
            <div class="perf-name">{{ p.name }}</div>
            <div class="perf-dept">{{ p.department }}</div>
          </div>
          <div>
            <div class="progress-bar" style="width:100px;margin-bottom:4px;">
              <div class="progress-fill" [style.width.%]="p.score" [style.background]="p.barColor"></div>
            </div>
          </div>
          <div class="perf-score" [style.color]="p.barColor">{{ p.score }}%</div>
        </div>
      </div>
    </div>

    <!-- Score distribution + goals -->
    <div style="display:flex;flex-direction:column;gap:16px;">

      <!-- Avg score -->
      <div class="card-3d" style="padding:20px;text-align:center;">
        <div style="font-size:12px;color:var(--gray-400);font-weight:500;text-transform:uppercase;letter-spacing:.06em;margin-bottom:10px;">Company Average</div>
        <div class="score-big">87.4</div>
        <div class="score-label">out of 100</div>
        <div style="display:flex;justify-content:center;margin-top:12px;" class="stars">
          <span *ngFor="let s of [1,2,3,4,5]" class="star">{{ s <= 4 ? '⭐' : '☆' }}</span>
        </div>
      </div>

      <!-- Score distribution -->
      <div class="card-3d">
        <div class="card-head"><div class="card-head-title">Score Distribution</div></div>
        <div class="card-body" style="padding-top:12px;padding-bottom:12px;">
          <div *ngFor="let d of distribution" class="dist-row">
            <span class="dist-label">{{ d.label }}</span>
            <div class="dist-bar"><div class="dist-fill" [style.width.%]="d.pct" [style.background]="d.color"></div></div>
            <span class="dist-count">{{ d.count }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Goals -->
  <div class="card-3d" style="margin-top:16px;">
    <div class="card-head">
      <div class="card-head-title">Team Goals Progress</div>
      <button class="btn btn-secondary btn-sm">+ Add Goal</button>
    </div>
    <div style="display:grid;grid-template-columns:repeat(2,1fr);gap:0;">
      <div *ngFor="let g of goals; let i=index" class="goal-item card-body" [style.border-right]="i%2===0?'1px solid var(--surface-divider)':''">
        <div class="goal-header">
          <span class="goal-name">{{ g.name }}</span>
          <span class="badge" [ngClass]="g.statusClass">{{ g.status }}</span>
        </div>
        <div class="progress-bar" style="height:8px;">
          <div class="progress-fill" [style.width.%]="g.progress" [style.background]="g.color"></div>
        </div>
        <div style="display:flex;justify-content:space-between;margin-top:6px;">
          <span style="font-size:11.5px;color:var(--gray-400);">{{ g.owner }}</span>
          <span style="font-size:12px;font-weight:600;color:var(--gray-700);">{{ g.progress }}%</span>
        </div>
      </div>
    </div>
  </div>
</div>
  `,
})
export class PerformanceComponent {
  summary = [
    { label: 'Average Score',        value: '87.4%', delta: '+1.8%', up: true,  color: '#4f6ef7' },
    { label: 'Reviews Completed',    value: '847',   delta: '+12.3%', up: true, color: '#16a34a' },
    { label: 'Needs Improvement',    value: '38',    delta: '-5.1%',  up: true, color: '#d97706' },
  ];

  leaders = [
    { name: 'Sarah Mitchell', department: 'Engineering', initials: 'SM', score: 97, color: '#4f6ef7', barColor: '#16a34a' },
    { name: 'Ben Carter',     department: 'Finance',     initials: 'BC', score: 95, color: '#16a34a', barColor: '#16a34a' },
    { name: 'James Okafor',   department: 'Sales',       initials: 'JO', score: 94, color: '#7c5cfc', barColor: '#16a34a' },
    { name: 'Priya Sharma',   department: 'Marketing',   initials: 'PS', score: 91, color: '#06b6d4', barColor: '#16a34a' },
    { name: 'Tom Nakamura',   department: 'Finance',     initials: 'TN', score: 89, color: '#d97706', barColor: '#d97706' },
    { name: 'Layla Hassan',   department: 'HR',          initials: 'LH', score: 88, color: '#dc2626', barColor: '#d97706' },
    { name: 'Chris Evans',    department: 'Engineering', initials: 'CE', score: 85, color: '#0284c7', barColor: '#d97706' },
    { name: 'Amara Diallo',   department: 'Sales',       initials: 'AD', score: 82, color: '#7c5cfc', barColor: '#d97706' },
  ];

  distribution = [
    { label: '90–100', count: 124, pct: 78, color: '#16a34a' },
    { label: '80–89',  count: 386, pct: 61, color: '#4f6ef7' },
    { label: '70–79',  count: 278, pct: 44, color: '#d97706' },
    { label: '60–69',  count: 59,  pct: 18, color: '#dc2626' },
    { label: '<60',    count: 38,  pct: 10, color: '#667085' },
  ];

  goals = [
    { name: 'Reduce employee churn to <5%',    owner: 'L. Hassan',   progress: 72, color: '#4f6ef7', status: 'On Track',   statusClass: 'badge-primary' },
    { name: 'Complete 100% review cycle',       owner: 'All Managers', progress: 66, color: '#16a34a', status: 'In Progress', statusClass: 'badge-info' },
    { name: 'Increase avg score by 3pts',       owner: 'S. Mitchell', progress: 84, color: '#06b6d4', status: 'On Track',   statusClass: 'badge-primary' },
    { name: 'Onboard 20 new engineers',         owner: 'J. Okafor',   progress: 45, color: '#d97706', status: 'At Risk',    statusClass: 'badge-warning' },
    { name: 'Launch mentorship program',        owner: 'P. Sharma',   progress: 30, color: '#7c5cfc', status: 'At Risk',    statusClass: 'badge-warning' },
    { name: 'Achieve 95% attendance rate',      owner: 'T. Nakamura', progress: 96, color: '#16a34a', status: 'Achieved',   statusClass: 'badge-success' },
  ];
}
