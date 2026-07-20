import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display: block; }
    .page { padding: 28px; }

    .top-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:14px; margin-bottom:24px; }
    @media(max-width:900px) { .top-grid { grid-template-columns:repeat(2,1fr); } }
    @media(max-width:500px) { .top-grid { grid-template-columns:1fr; } }

    .main-grid { display:grid; grid-template-columns:1fr 300px; gap:16px; }
    @media(max-width:900px) { .main-grid { grid-template-columns:1fr; } }

    .card-head { display:flex; align-items:center; justify-content:space-between; padding:16px 20px 12px; border-bottom:1px solid var(--surface-divider); }
    .card-head-title { font-size:14px; font-weight:600; color:var(--gray-900); }
    .card-body { padding:16px 20px; }

    /* Calendar heat */
    .cal-grid { display:grid; grid-template-columns:repeat(7,1fr); gap:4px; }
    .cal-day-label { font-size:10px; font-weight:600; text-align:center; color:var(--gray-400); padding-bottom:4px; text-transform:uppercase; letter-spacing:.04em; }
    .cal-cell {
      aspect-ratio:1; border-radius:4px; display:flex; align-items:center; justify-content:center;
      font-size:10.5px; color:var(--gray-400); cursor:pointer; transition:all var(--t-fast);
      border:1px solid transparent;
    }
    .cal-cell:hover { border-color:var(--color-primary); color:var(--color-primary); }
    .cal-cell.empty { cursor:default; }
    .cal-cell.has-data { color:#fff; font-weight:600; }
    .cal-cell.today { outline:2px solid var(--color-primary); outline-offset:1px; }

    /* Absence reasons */
    .reason-row { display:flex; align-items:center; gap:10px; padding:8px 0; }
    .reason-row + .reason-row { border-top:1px solid var(--surface-divider); }
    .reason-dot { width:10px; height:10px; border-radius:3px; flex-shrink:0; }
    .reason-label { font-size:12.5px; color:var(--gray-700); flex:1; }
    .reason-val { font-size:12.5px; font-weight:600; color:var(--gray-900); }
    .reason-pct { font-size:11px; color:var(--gray-400); margin-left:4px; }

    /* Table */
    .table-card { background:var(--surface-card); border:var(--card-border); border-radius:var(--radius-lg); box-shadow:var(--shadow-sm); overflow:hidden; }
    .att-avatar { width:32px; height:32px; border-radius:7px; display:flex; align-items:center; justify-content:center; color:#fff; font-size:11px; font-weight:700; flex-shrink:0; }
  `],
  template: `
<div class="page">
  <div class="page-header">
    <div>
      <h1 class="page-title">Attendance</h1>
      <p class="page-subtitle">July 2026 — tracking 1,284 employees</p>
    </div>
    <div style="display:flex;gap:8px;">
      <button class="btn btn-secondary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
        Export
      </button>
      <button class="btn btn-primary">Mark Attendance</button>
    </div>
  </div>

  <!-- KPI strip -->
  <div class="top-grid">
    <div *ngFor="let k of kpis" class="stat-card card-3d" [style.--stat-accent]="k.color">
      <div class="stat-label">{{ k.label }}</div>
      <div class="stat-value" style="margin-top:8px;">{{ k.value }}</div>
      <div style="margin-top:6px;" class="stat-delta" [class.up]="k.up" [class.down]="!k.up">
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline [attr.points]="k.up ? '18 15 12 9 6 15' : '6 9 12 15 18 9'"/>
        </svg>
        {{ k.delta }} this month
      </div>
    </div>
  </div>

  <div class="main-grid">
    <!-- Calendar + table -->
    <div style="display:flex;flex-direction:column;gap:16px;">

      <!-- Attendance heat calendar -->
      <div class="card-3d">
        <div class="card-head">
          <div class="card-head-title">July 2026 Attendance Map</div>
          <div style="display:flex;align-items:center;gap:8px;font-size:11.5px;color:var(--gray-400);">
            <span style="display:inline-flex;align-items:center;gap:4px;"><span style="width:10px;height:10px;border-radius:3px;background:#16a34a;display:inline-block;"></span>High</span>
            <span style="display:inline-flex;align-items:center;gap:4px;"><span style="width:10px;height:10px;border-radius:3px;background:#d97706;display:inline-block;"></span>Medium</span>
            <span style="display:inline-flex;align-items:center;gap:4px;"><span style="width:10px;height:10px;border-radius:3px;background:#dc2626;display:inline-block;"></span>Low</span>
          </div>
        </div>
        <div class="card-body">
          <div class="cal-grid">
            <div *ngFor="let d of dayLabels" class="cal-day-label">{{ d }}</div>
            <div *ngFor="let cell of calCells" class="cal-cell"
              [class.empty]="!cell.day"
              [class.has-data]="cell.day && cell.rate !== null"
              [class.today]="cell.isToday"
              [style.background]="cell.day ? cell.bg : ''"
              [style.opacity]="cell.day ? 1 : 0"
              [title]="cell.day ? 'Day '+cell.day+': '+cell.rate+'% attendance' : ''"
            >{{ cell.day || '' }}</div>
          </div>
        </div>
      </div>

      <!-- Today's attendance table -->
      <div class="table-card">
        <div class="card-head" style="padding:14px 18px;">
          <div class="card-head-title">Today's Records</div>
          <span class="badge badge-success">{{ todayDate }}</span>
        </div>
        <div style="overflow-x:auto;">
          <table>
            <thead>
              <tr>
                <th>Employee</th>
                <th>Department</th>
                <th>Check In</th>
                <th>Check Out</th>
                <th>Hours</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let r of records">
                <td>
                  <div style="display:flex;align-items:center;gap:9px;">
                    <div class="att-avatar" [style.background]="r.color">{{ r.initials }}</div>
                    <div>
                      <div style="font-weight:500;font-size:13.5px;color:var(--gray-900);">{{ r.name }}</div>
                    </div>
                  </div>
                </td>
                <td style="color:var(--gray-500);">{{ r.dept }}</td>
                <td style="font-size:13px;color:var(--gray-700);">{{ r.checkIn }}</td>
                <td style="font-size:13px;color:var(--gray-700);">{{ r.checkOut }}</td>
                <td style="font-size:13px;font-weight:600;color:var(--gray-900);">{{ r.hours }}</td>
                <td><span class="badge" [ngClass]="r.statusClass">{{ r.status }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Side panel -->
    <div style="display:flex;flex-direction:column;gap:16px;">
      <!-- Absence reasons -->
      <div class="card-3d">
        <div class="card-head"><div class="card-head-title">Absence Reasons</div></div>
        <div class="card-body" style="padding-top:10px;">
          <div *ngFor="let r of absenceReasons" class="reason-row">
            <div class="reason-dot" [style.background]="r.color"></div>
            <span class="reason-label">{{ r.label }}</span>
            <span class="reason-val">{{ r.count }}</span>
            <span class="reason-pct">({{ r.pct }}%)</span>
          </div>
        </div>
      </div>

      <!-- Late arrivals trend -->
      <div class="card-3d">
        <div class="card-head"><div class="card-head-title">Late Arrivals</div></div>
        <div class="card-body" style="padding-top:8px;">
          <div *ngFor="let l of lateByDept" style="padding:7px 0;border-bottom:1px solid var(--surface-divider);">
            <div style="display:flex;justify-content:space-between;margin-bottom:5px;">
              <span style="font-size:12.5px;color:var(--gray-700);">{{ l.dept }}</span>
              <span style="font-size:12px;font-weight:600;color:var(--gray-800);">{{ l.late }}</span>
            </div>
            <div class="progress-bar">
              <div class="progress-fill" [style.width.%]="l.pct" [style.background]="l.color"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
  `,
})
export class AttendanceComponent {
  todayDate = new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

  kpis = [
    { label: 'Attendance Rate', value: '96.2%', delta: '+0.4%', up: true,  color: '#16a34a' },
    { label: 'Present Today',   value: '1,236', delta: '+14',   up: true,  color: '#4f6ef7' },
    { label: 'Absent Today',    value: '48',    delta: '-6',    up: true,  color: '#d97706' },
    { label: 'Late Arrivals',   value: '22',    delta: '-3',    up: true,  color: '#dc2626' },
  ];

  dayLabels = ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'];

  calCells = (() => {
    const cells = [];
    const rates = [98,96,97,95,94,null,null,97,96,98,96,95,null,null,96,97,95,94,96,null,null,97,98,96,97,94,null,null,96,97,95];
    const colors = (r: number | null) => {
      if (!r) return 'var(--gray-100)';
      if (r >= 97) return '#16a34a';
      if (r >= 94) return '#4f6ef7';
      if (r >= 90) return '#d97706';
      return '#dc2626';
    };
    // July 2026 starts on Wednesday (index 2)
    for (let i = 0; i < 2; i++) cells.push({ day: 0, rate: null, bg: '', isToday: false });
    for (let d = 1; d <= 31; d++) {
      const r = rates[d - 1];
      cells.push({ day: d, rate: r, bg: colors(r), isToday: d === 20 });
    }
    return cells;
  })();

  absenceReasons = [
    { label: 'Sick Leave',       count: 18, pct: 38, color: '#dc2626' },
    { label: 'Personal',         count: 12, pct: 25, color: '#d97706' },
    { label: 'Vacation',         count: 10, pct: 21, color: '#4f6ef7' },
    { label: 'Work from Home',   count: 5,  pct: 10, color: '#7c5cfc' },
    { label: 'Other',            count: 3,  pct: 6,  color: '#667085' },
  ];

  lateByDept = [
    { dept: 'Engineering', late: 8, pct: 64, color: '#dc2626' },
    { dept: 'Sales',       late: 5, pct: 40, color: '#d97706' },
    { dept: 'Marketing',   late: 4, pct: 32, color: '#d97706' },
    { dept: 'Finance',     late: 3, pct: 24, color: '#4f6ef7' },
    { dept: 'HR',          late: 2, pct: 16, color: '#16a34a' },
  ];

  records = [
    { name: 'Sarah Mitchell', dept: 'Engineering', initials: 'SM', color: '#4f6ef7', checkIn: '08:52', checkOut: '—',     hours: '—',    status: 'Present',  statusClass: 'badge-success' },
    { name: 'James Okafor',   dept: 'Sales',       initials: 'JO', color: '#7c5cfc', checkIn: '09:14', checkOut: '—',     hours: '—',    status: 'Present',  statusClass: 'badge-success' },
    { name: 'Amara Diallo',   dept: 'Sales',       initials: 'AD', color: '#7c5cfc', checkIn: '—',     checkOut: '—',     hours: '—',    status: 'On Leave', statusClass: 'badge-warning' },
    { name: 'Tom Nakamura',   dept: 'Finance',     initials: 'TN', color: '#d97706', checkIn: '09:42', checkOut: '—',     hours: '—',    status: 'Late',     statusClass: 'badge-error' },
    { name: 'Layla Hassan',   dept: 'HR',          initials: 'LH', color: '#dc2626', checkIn: '08:58', checkOut: '—',     hours: '—',    status: 'Present',  statusClass: 'badge-success' },
    { name: 'Priya Sharma',   dept: 'Marketing',   initials: 'PS', color: '#06b6d4', checkIn: '—',     checkOut: '—',     hours: '—',    status: 'Absent',   statusClass: 'badge-neutral' },
  ];
}
