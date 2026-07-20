import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-hiring',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display: block; }
    .page { padding: 28px; }

    /* KPI strip */
    .kpi-strip { display:grid; grid-template-columns:repeat(4,1fr); gap:14px; margin-bottom:24px; }
    @media(max-width:900px) { .kpi-strip { grid-template-columns:repeat(2,1fr); } }
    @media(max-width:500px) { .kpi-strip { grid-template-columns:1fr; } }

    /* Kanban */
    .kanban { display:grid; grid-template-columns:repeat(5,1fr); gap:14px; }
    @media(max-width:1200px){ .kanban{ grid-template-columns:repeat(3,1fr); } }
    @media(max-width:768px) { .kanban{ grid-template-columns:repeat(2,1fr); } }
    @media(max-width:500px) { .kanban{ grid-template-columns:1fr; } }

    .kanban-col {
      background: var(--gray-25);
      border: 1px solid var(--surface-border);
      border-radius: 10px;
      overflow: hidden;
      min-height: 300px;
    }
    .kanban-header {
      padding: 12px 14px 10px;
      border-bottom: 1px solid var(--surface-border);
      display: flex; align-items: center; justify-content: space-between;
    }
    .kanban-title { font-size:12.5px; font-weight:600; color:var(--gray-700); }
    .kanban-count {
      width:20px; height:20px; border-radius:4px;
      display:flex; align-items:center; justify-content:center;
      font-size:11px; font-weight:700;
    }
    .kanban-body { padding: 10px; display:flex; flex-direction:column; gap:8px; }

    /* Candidate card */
    .candidate-card {
      background: var(--surface-card);
      border: 1px solid var(--surface-border);
      border-radius: 8px;
      padding: 12px;
      cursor: grab;
      box-shadow: var(--shadow-xs);
      transition: box-shadow var(--t-fast), transform var(--t-fast);
    }
    .candidate-card:hover { box-shadow: var(--shadow-sm); transform: translateY(-1px); }
    .candidate-name { font-size:12.5px; font-weight:600; color:var(--gray-900); }
    .candidate-role { font-size:11.5px; color:var(--gray-400); margin-top:2px; }
    .candidate-meta { display:flex; align-items:center; gap:6px; margin-top:8px; flex-wrap:wrap; }
    .candidate-avatar { width:26px; height:26px; border-radius:6px; display:flex; align-items:center; justify-content:center; color:#fff; font-size:10px; font-weight:700; flex-shrink:0; }
  `],
  template: `
<div class="page">
  <div class="page-header">
    <div>
      <h1 class="page-title">Recruitment</h1>
      <p class="page-subtitle">38 open positions · Hiring pipeline</p>
    </div>
    <div style="display:flex;gap:8px;">
      <button class="btn btn-secondary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
        Export
      </button>
      <button class="btn btn-primary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Post Job
      </button>
    </div>
  </div>

  <!-- KPI strip -->
  <div class="kpi-strip">
    <div *ngFor="let k of kpis" class="stat-card card-3d" [style.--stat-accent]="k.color">
      <div class="stat-label">{{ k.label }}</div>
      <div class="stat-value" style="margin-top:8px;">{{ k.value }}</div>
      <div style="margin-top:6px;" class="stat-delta" [class.up]="k.up" [class.down]="!k.up">
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline [attr.points]="k.up ? '18 15 12 9 6 15' : '6 9 12 15 18 9'"/>
        </svg>
        {{ k.delta }}
      </div>
    </div>
  </div>

  <!-- Kanban board -->
  <div class="kanban">
    <div *ngFor="let col of pipeline" class="kanban-col">
      <div class="kanban-header">
        <span class="kanban-title">{{ col.stage }}</span>
        <span class="kanban-count" [style.background]="col.countBg" [style.color]="col.countColor">{{ col.candidates.length }}</span>
      </div>
      <div class="kanban-body">
        <div *ngFor="let c of col.candidates" class="candidate-card">
          <div style="display:flex;align-items:center;gap:8px;">
            <div class="candidate-avatar" [style.background]="c.color">{{ c.initials }}</div>
            <div style="min-width:0;">
              <div class="candidate-name">{{ c.name }}</div>
              <div class="candidate-role">{{ c.role }}</div>
            </div>
          </div>
          <div class="candidate-meta">
            <span class="badge badge-neutral" style="font-size:10px;">{{ c.source }}</span>
            <span style="font-size:10.5px;color:var(--gray-400);margin-left:auto;">{{ c.date }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
  `,
})
export class HiringComponent {
  kpis = [
    { label: 'Total Applicants', value: '248', delta: '+18%',  up: true,  color: '#4f6ef7' },
    { label: 'Time-to-Hire',     value: '24d', delta: '-3d',   up: true,  color: '#16a34a' },
    { label: 'Offer Acceptance', value: '87%', delta: '+5%',   up: true,  color: '#06b6d4' },
    { label: 'Open Roles',       value: '38',  delta: '+12.5%', up: false, color: '#d97706' },
  ];

  pipeline = [
    {
      stage: 'Applied', countBg: 'rgba(79,110,247,.1)', countColor: '#4f6ef7',
      candidates: [
        { name: 'Alex Dumas',    role: 'Frontend Engineer', initials: 'AD', color: '#4f6ef7', source: 'LinkedIn',   date: '2h ago' },
        { name: 'Hana Kobayashi',role: 'UX Designer',       initials: 'HK', color: '#7c5cfc', source: 'Referral',   date: '5h ago' },
        { name: 'Omar Faris',    role: 'Data Analyst',      initials: 'OF', color: '#06b6d4', source: 'Indeed',     date: 'Yesterday' },
        { name: 'Mia Torres',    role: 'Backend Engineer',  initials: 'MT', color: '#d97706', source: 'Glassdoor',  date: '2d ago' },
      ]
    },
    {
      stage: 'Screening', countBg: 'rgba(124,92,252,.1)', countColor: '#7c5cfc',
      candidates: [
        { name: 'Jin Park',      role: 'Product Manager',   initials: 'JP', color: '#7c5cfc', source: 'AngelList',  date: '1d ago' },
        { name: 'Sara Abboud',   role: 'Marketing Lead',    initials: 'SA', color: '#06b6d4', source: 'LinkedIn',   date: '2d ago' },
        { name: 'David Mensah',  role: 'DevOps Engineer',   initials: 'DM', color: '#4f6ef7', source: 'Referral',   date: '3d ago' },
      ]
    },
    {
      stage: 'Interview', countBg: 'rgba(6,182,212,.1)', countColor: '#06b6d4',
      candidates: [
        { name: 'Emma Walsh',    role: 'Senior Designer',   initials: 'EW', color: '#16a34a', source: 'Dribbble',   date: '3d ago' },
        { name: 'Luca Ferrari',  role: 'ML Engineer',       initials: 'LF', color: '#4f6ef7', source: 'LinkedIn',   date: '4d ago' },
      ]
    },
    {
      stage: 'Offer', countBg: 'rgba(22,163,74,.1)', countColor: '#16a34a',
      candidates: [
        { name: 'Yuki Tanaka',   role: 'Backend Engineer',  initials: 'YT', color: '#06b6d4', source: 'Referral',   date: '5d ago' },
        { name: 'Nadia Kovac',   role: 'HR Specialist',     initials: 'NK', color: '#7c5cfc', source: 'LinkedIn',   date: '6d ago' },
      ]
    },
    {
      stage: 'Hired', countBg: 'rgba(22,163,74,.08)', countColor: '#16a34a',
      candidates: [
        { name: 'Carlos Reyes',  role: 'iOS Developer',     initials: 'CR', color: '#dc2626', source: 'LinkedIn',   date: '1w ago' },
        { name: 'Fatima Al-Rashid', role: 'Data Scientist', initials: 'FA', color: '#d97706', source: 'University', date: '1w ago' },
        { name: 'Noah Bennett',  role: 'Sales Manager',     initials: 'NB', color: '#4f6ef7', source: 'Referral',   date: '2w ago' },
      ]
    },
  ];
}
