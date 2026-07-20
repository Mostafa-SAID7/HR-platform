import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-workforce',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display: block; }
    .page { padding: 28px; }

    .kpi-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:14px; margin-bottom:24px; }
    @media(max-width:1100px) { .kpi-grid { grid-template-columns:repeat(2,1fr); } }
    @media(max-width:500px)  { .kpi-grid { grid-template-columns:1fr; } }

    .chart-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:16px; margin-bottom:16px; }
    @media(max-width:900px) { .chart-grid { grid-template-columns:1fr; } }

    .card-head { display:flex; align-items:center; justify-content:space-between; padding:16px 20px 12px; border-bottom:1px solid var(--surface-divider); }
    .card-head-title { font-size:14px; font-weight:600; color:var(--gray-900); }
    .card-head-sub { font-size:11.5px; color:var(--gray-400); margin-top:2px; }
    .card-body { padding:16px 20px; }

    /* Fake chart bars */
    .chart-area { position:relative; height:160px; display:flex; align-items:flex-end; gap:4px; padding: 0 0 1px; }
    .chart-line { position:absolute; left:0; right:0; height:1px; background:var(--surface-divider); }
    .bar-col { flex:1; display:flex; flex-direction:column; justify-content:flex-end; align-items:center; gap:0; }
    .bar-seg { width:100%; border-radius:3px 3px 0 0; transition:all var(--t-slow); }
    .bar-seg:hover { filter: brightness(1.1); }
    .x-labels { display:flex; gap:4px; padding:4px 0 0; }
    .x-label { flex:1; text-align:center; font-size:10px; color:var(--gray-400); }

    /* Trend line */
    .trend-area { height:140px; position:relative; }

    /* Insight card */
    .insight-item { display:flex; gap:12px; padding:12px 0; }
    .insight-item + .insight-item { border-top:1px solid var(--surface-divider); }
    .insight-icon { width:36px; height:36px; border-radius:8px; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
    .insight-title { font-size:13px; font-weight:600; color:var(--gray-900); }
    .insight-desc { font-size:12px; color:var(--gray-500); margin-top:3px; line-height:1.5; }

    /* Turnover table */
    .dept-row { display:flex; align-items:center; gap:12px; padding:10px 0; }
    .dept-row + .dept-row { border-top:1px solid var(--surface-divider); }
    .dept-dot { width:10px; height:10px; border-radius:3px; flex-shrink:0; }
    .dept-name { font-size:12.5px; color:var(--gray-700); flex:1; }
    .dept-val  { font-size:13px; font-weight:600; color:var(--gray-900); min-width:36px; text-align:right; }
  `],
  template: `
<div class="page">
  <div class="page-header">
    <div>
      <h1 class="page-title">Analytics</h1>
      <p class="page-subtitle">Workforce analytics & insights · Q2 2026</p>
    </div>
    <div style="display:flex;gap:8px;flex-wrap:wrap;">
      <select class="input" style="width:auto;height:36px;font-size:13px;padding:0 10px;">
        <option>Last 6 months</option>
        <option>Last 12 months</option>
        <option>This quarter</option>
      </select>
      <button class="btn btn-secondary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
        Export
      </button>
    </div>
  </div>

  <!-- KPI strip -->
  <div class="kpi-grid">
    <div *ngFor="let k of kpis" class="stat-card card-3d" [style.--stat-accent]="k.color">
      <div class="stat-label">{{ k.label }}</div>
      <div class="stat-value" style="margin-top:8px;">{{ k.value }}</div>
      <div style="margin-top:6px;" class="stat-delta" [class.up]="k.up" [class.down]="!k.up">
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline [attr.points]="k.up ? '18 15 12 9 6 15' : '6 9 12 15 18 9'"/>
        </svg>
        {{ k.delta }} vs prev period
      </div>
    </div>
  </div>

  <!-- Charts -->
  <div class="chart-grid">

    <!-- Headcount trend -->
    <div class="card-3d">
      <div class="card-head">
        <div>
          <div class="card-head-title">Headcount Growth</div>
          <div class="card-head-sub">Jan – Jul 2026</div>
        </div>
        <span class="badge badge-success">+9.4%</span>
      </div>
      <div class="card-body">
        <div class="chart-area">
          <div class="chart-line" style="bottom:50%;"></div>
          <div class="chart-line" style="bottom:0%;"></div>
          <div *ngFor="let b of headcountBars" class="bar-col">
            <div class="bar-seg" [style.height.px]="b.h" [style.background]="'linear-gradient(to top, #4f6ef7, #7c5cfc)'"></div>
          </div>
        </div>
        <div class="x-labels">
          <span *ngFor="let l of months" class="x-label">{{ l }}</span>
        </div>
      </div>
    </div>

    <!-- Turnover rate -->
    <div class="card-3d">
      <div class="card-head">
        <div>
          <div class="card-head-title">Turnover Rate by Department</div>
          <div class="card-head-sub">Last 6 months average</div>
        </div>
      </div>
      <div class="card-body" style="padding-top:10px;">
        <div *ngFor="let d of turnover" class="dept-row">
          <div class="dept-dot" [style.background]="d.color"></div>
          <span class="dept-name">{{ d.dept }}</span>
          <div class="progress-bar" style="flex:1;max-width:120px;">
            <div class="progress-fill" [style.width.%]="d.pct * 5" [style.background]="d.color"></div>
          </div>
          <span class="dept-val">{{ d.pct }}%</span>
        </div>
      </div>
    </div>

    <!-- Gender diversity stacked -->
    <div class="card-3d">
      <div class="card-head">
        <div>
          <div class="card-head-title">Gender Diversity by Department</div>
          <div class="card-head-sub">% breakdown</div>
        </div>
      </div>
      <div class="card-body">
        <div *ngFor="let d of diversity" style="padding:6px 0;">
          <div style="display:flex;justify-content:space-between;margin-bottom:4px;">
            <span style="font-size:12px;color:var(--gray-600);">{{ d.dept }}</span>
            <span style="font-size:11px;color:var(--gray-400);">{{ d.female }}% ♀ · {{ d.male }}% ♂</span>
          </div>
          <div style="display:flex;height:8px;border-radius:3px;overflow:hidden;gap:1px;">
            <div [style.flex]="d.female" style="background:#7c5cfc;"></div>
            <div [style.flex]="d.male"   style="background:#4f6ef7;"></div>
          </div>
        </div>
        <div style="display:flex;gap:16px;margin-top:12px;">
          <span style="display:flex;align-items:center;gap:6px;font-size:11.5px;color:var(--gray-500);">
            <span style="width:10px;height:10px;background:#7c5cfc;border-radius:3px;display:inline-block;"></span>Female
          </span>
          <span style="display:flex;align-items:center;gap:6px;font-size:11.5px;color:var(--gray-500);">
            <span style="width:10px;height:10px;background:#4f6ef7;border-radius:3px;display:inline-block;"></span>Male
          </span>
        </div>
      </div>
    </div>

    <!-- Hiring vs Attrition -->
    <div class="card-3d">
      <div class="card-head">
        <div>
          <div class="card-head-title">Hiring vs Attrition</div>
          <div class="card-head-sub">Monthly comparison</div>
        </div>
      </div>
      <div class="card-body">
        <div class="chart-area">
          <div class="chart-line" style="bottom:66%;"></div>
          <div class="chart-line" style="bottom:33%;"></div>
          <div class="chart-line" style="bottom:0%;"></div>
          <div *ngFor="let b of hiringAttr" class="bar-col" style="flex-direction:row;align-items:flex-end;gap:2px;">
            <div [style.height.px]="b.hire" style="flex:1;background:#4f6ef7;border-radius:3px 3px 0 0;"></div>
            <div [style.height.px]="b.attr" style="flex:1;background:#dc2626;border-radius:3px 3px 0 0;opacity:.7;"></div>
          </div>
        </div>
        <div class="x-labels">
          <span *ngFor="let l of months" class="x-label">{{ l }}</span>
        </div>
        <div style="display:flex;gap:16px;margin-top:8px;">
          <span style="display:flex;align-items:center;gap:6px;font-size:11.5px;color:var(--gray-500);">
            <span style="width:10px;height:10px;background:#4f6ef7;border-radius:3px;display:inline-block;"></span>Hired
          </span>
          <span style="display:flex;align-items:center;gap:6px;font-size:11.5px;color:var(--gray-500);">
            <span style="width:10px;height:10px;background:#dc2626;border-radius:3px;opacity:.7;display:inline-block;"></span>Left
          </span>
        </div>
      </div>
    </div>
  </div>

  <!-- Insights -->
  <div class="card-3d">
    <div class="card-head"><div class="card-head-title">AI-Powered Insights</div><span class="badge badge-primary">Beta</span></div>
    <div class="card-body" style="padding-top:8px;">
      <div *ngFor="let i of insights" class="insight-item">
        <div class="insight-icon" [style.background]="i.iconBg">
          <span [style.color]="i.color" [innerHTML]="i.icon"></span>
        </div>
        <div>
          <div class="insight-title">{{ i.title }}</div>
          <div class="insight-desc">{{ i.desc }}</div>
        </div>
        <span class="badge" [ngClass]="i.badgeClass" style="margin-left:auto;flex-shrink:0;">{{ i.badge }}</span>
      </div>
    </div>
  </div>
</div>
  `,
})
export class WorkforceComponent {
  private sanitizer = inject(DomSanitizer);
  private svg(s: string): SafeHtml { return this.sanitizer.bypassSecurityTrustHtml(s); }

  kpis = [
    { label: 'Total Headcount',  value: '1,284', delta: '+9.4%',  up: true,  color: '#4f6ef7' },
    { label: 'Avg Tenure',       value: '3.2 yrs', delta: '+0.2y', up: true, color: '#16a34a' },
    { label: 'Turnover Rate',    value: '4.8%',  delta: '-0.6%',  up: true,  color: '#d97706' },
    { label: 'Diversity Index',  value: '0.74',  delta: '+0.03',  up: true,  color: '#7c5cfc' },
  ];

  months = ['Jan','Feb','Mar','Apr','May','Jun','Jul'];
  headcountBars = [
    { h: 90 },{ h: 100 },{ h: 105 },{ h: 115 },{ h: 120 },{ h: 130 },{ h: 140 },
  ];

  turnover = [
    { dept: 'Sales',       pct: 8.2, color: '#dc2626' },
    { dept: 'Engineering', pct: 5.1, color: '#d97706' },
    { dept: 'Marketing',   pct: 4.3, color: '#d97706' },
    { dept: 'Finance',     pct: 2.8, color: '#4f6ef7' },
    { dept: 'HR',          pct: 1.9, color: '#16a34a' },
    { dept: 'Operations',  pct: 3.4, color: '#7c5cfc' },
  ];

  diversity = [
    { dept: 'Engineering', female: 34, male: 66 },
    { dept: 'Sales',       female: 52, male: 48 },
    { dept: 'Marketing',   female: 61, male: 39 },
    { dept: 'Finance',     female: 44, male: 56 },
    { dept: 'HR',          female: 72, male: 28 },
    { dept: 'Operations',  female: 40, male: 60 },
  ];

  hiringAttr = [
    { hire: 50,  attr: 28 },
    { hire: 65,  attr: 32 },
    { hire: 80,  attr: 40 },
    { hire: 100, attr: 44 },
    { hire: 90,  attr: 36 },
    { hire: 110, attr: 30 },
    { hire: 120, attr: 35 },
  ];

  insights = [
    {
      title: 'Engineering turnover risk detected',
      desc: '6 senior engineers have tenure >4 years without promotion. Historical data suggests 68% churn probability in next 90 days.',
      icon: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>`,
      iconBg: 'rgba(220,38,38,.1)', color: '#dc2626', badge: 'High Risk', badgeClass: 'badge-error'
    },
    {
      title: 'Sales team outperforming hiring plan',
      desc: 'Q2 hiring in Sales exceeded targets by 18%. Consider reallocating 3 open headcount to Engineering.',
      icon: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>`,
      iconBg: 'rgba(22,163,74,.1)', color: '#16a34a', badge: 'Opportunity', badgeClass: 'badge-success'
    },
    {
      title: 'Gender parity gap in Engineering',
      desc: 'Female representation in Engineering is 34%, below company target of 45%. 12 open roles are an opportunity to improve.',
      icon: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>`,
      iconBg: 'rgba(79,110,247,.1)', color: '#4f6ef7', badge: 'Action Needed', badgeClass: 'badge-primary'
    },
  ];
}
