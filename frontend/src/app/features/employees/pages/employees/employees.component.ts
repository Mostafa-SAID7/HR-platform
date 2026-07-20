import { Component, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host { display: block; }
    .page { padding: 28px; }

    /* toolbar */
    .toolbar { display:flex; align-items:center; justify-content:space-between; gap:12px; margin-bottom:20px; flex-wrap:wrap; }
    .toolbar-left { display:flex; align-items:center; gap:10px; flex-wrap:wrap; }

    /* search */
    .search-box { position:relative; }
    .search-box svg { position:absolute; left:10px; top:50%; transform:translateY(-50%); color:var(--gray-400); pointer-events:none; }
    .search-box input { padding-left:32px; width:240px; }
    @media(max-width:600px){ .search-box input{ width:100%; } }

    /* filter chips */
    .filter-chip {
      height:34px; padding:0 12px; border-radius:6px; font-size:12.5px; font-weight:500;
      border:1px solid var(--surface-border); background:var(--surface-card); color:var(--gray-600);
      cursor:pointer; transition:all var(--t-fast); display:inline-flex; align-items:center; gap:6px;
    }
    .filter-chip.active { background:var(--color-primary-muted); color:var(--color-primary); border-color:rgba(79,110,247,.3); }
    .filter-chip:hover:not(.active) { background:var(--gray-100); color:var(--gray-800); }

    /* table wrapper */
    .table-card { background:var(--surface-card); border:var(--card-border); border-radius:var(--radius-lg); box-shadow:var(--shadow-sm); overflow:hidden; }
    .table-wrap { overflow-x:auto; }

    /* avatar in table */
    .row-avatar { width:34px; height:34px; border-radius:7px; display:flex; align-items:center; justify-content:center; color:#fff; font-size:12px; font-weight:700; flex-shrink:0; }

    /* actions */
    .row-actions { display:flex; gap:6px; opacity:0; transition:opacity var(--t-fast); }
    tbody tr:hover .row-actions { opacity:1; }

    /* pagination */
    .pagination { display:flex; align-items:center; justify-content:space-between; padding:12px 20px; border-top:1px solid var(--surface-divider); }
    .page-info { font-size:12.5px; color:var(--gray-500); }
    .page-btns { display:flex; gap:4px; }
    .page-btn {
      width:32px; height:32px; border-radius:6px; display:flex; align-items:center; justify-content:center;
      font-size:12.5px; border:1px solid var(--surface-border); background:var(--surface-card);
      color:var(--gray-600); cursor:pointer; transition:all var(--t-fast);
    }
    .page-btn.active { background:var(--color-primary); color:#fff; border-color:var(--color-primary-dark); }
    .page-btn:hover:not(.active):not(:disabled) { background:var(--gray-100); color:var(--gray-900); }
    .page-btn:disabled { opacity:.4; cursor:not-allowed; }
  `],
  template: `
<div class="page">
  <div class="page-header">
    <div>
      <h1 class="page-title">Employees</h1>
      <p class="page-subtitle">{{ filtered().length }} of {{ employees.length }} employees</p>
    </div>
    <div style="display:flex;gap:8px;">
      <button class="btn btn-secondary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
        Export
      </button>
      <button class="btn btn-primary">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Add Employee
      </button>
    </div>
  </div>

  <!-- Toolbar -->
  <div class="toolbar">
    <div class="toolbar-left">
      <div class="search-box">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        <input class="input" type="text" placeholder="Search employees…" [(ngModel)]="searchQuery" style="height:36px;font-size:13.5px;"/>
      </div>
      <button *ngFor="let d of deptFilters" class="filter-chip" [class.active]="activeFilter()===d" (click)="activeFilter.set(d)">{{ d }}</button>
    </div>
    <select class="input" style="width:auto;height:36px;font-size:13px;padding:0 10px;" [(ngModel)]="sortBy">
      <option value="name">Sort: Name</option>
      <option value="dept">Sort: Department</option>
      <option value="score">Sort: Performance</option>
    </select>
  </div>

  <!-- Table -->
  <div class="table-card">
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Employee</th>
            <th>Department</th>
            <th>Role</th>
            <th>Status</th>
            <th>Performance</th>
            <th>Joined</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let e of paginated()">
            <td>
              <div style="display:flex;align-items:center;gap:10px;">
                <div class="row-avatar" [style.background]="e.avatarColor">{{ e.initials }}</div>
                <div>
                  <div style="font-weight:500;color:var(--gray-900);font-size:13.5px;">{{ e.name }}</div>
                  <div style="font-size:11.5px;color:var(--gray-400);">{{ e.email }}</div>
                </div>
              </div>
            </td>
            <td>{{ e.department }}</td>
            <td style="color:var(--gray-600);">{{ e.role }}</td>
            <td><span class="badge" [ngClass]="e.statusClass">{{ e.status }}</span></td>
            <td>
              <div style="display:flex;align-items:center;gap:8px;">
                <div style="flex:1;min-width:60px;" class="progress-bar">
                  <div class="progress-fill" [style.width.%]="e.performance" [style.background]="e.perfColor"></div>
                </div>
                <span style="font-size:12px;font-weight:600;color:var(--gray-700);width:32px;">{{ e.performance }}%</span>
              </div>
            </td>
            <td style="color:var(--gray-500);font-size:12.5px;">{{ e.joined }}</td>
            <td>
              <div class="row-actions">
                <button class="btn btn-ghost btn-sm">View</button>
                <button class="btn btn-secondary btn-sm">Edit</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="pagination">
      <span class="page-info">Showing {{ (currentPage()-1)*pageSize+1 }}–{{ Math.min(currentPage()*pageSize, filtered().length) }} of {{ filtered().length }}</span>
      <div class="page-btns">
        <button class="page-btn" [disabled]="currentPage()===1" (click)="currentPage.set(currentPage()-1)">‹</button>
        <button class="page-btn" *ngFor="let p of pages()" [class.active]="p===currentPage()" (click)="currentPage.set(p)">{{ p }}</button>
        <button class="page-btn" [disabled]="currentPage()===totalPages()" (click)="currentPage.set(currentPage()+1)">›</button>
      </div>
    </div>
  </div>
</div>
  `,
})
export class EmployeesComponent {
  Math = Math;
  searchQuery = '';
  sortBy = 'name';
  pageSize = 8;
  currentPage = signal(1);
  activeFilter = signal('All');
  deptFilters = ['All', 'Engineering', 'Sales', 'Marketing', 'HR', 'Finance'];

  employees = [
    { name: 'Sarah Mitchell',  email: 'sarah@company.com',  department: 'Engineering', role: 'Senior Engineer',     status: 'Active',   statusClass: 'badge-success', performance: 97, perfColor: '#16a34a', joined: 'Jan 2022', initials: 'SM', avatarColor: '#4f6ef7' },
    { name: 'James Okafor',    email: 'james@company.com',  department: 'Sales',       role: 'Account Executive',   status: 'Active',   statusClass: 'badge-success', performance: 94, perfColor: '#16a34a', joined: 'Mar 2021', initials: 'JO', avatarColor: '#7c5cfc' },
    { name: 'Priya Sharma',    email: 'priya@company.com',  department: 'Marketing',   role: 'Marketing Lead',      status: 'Active',   statusClass: 'badge-success', performance: 91, perfColor: '#16a34a', joined: 'Jun 2022', initials: 'PS', avatarColor: '#06b6d4' },
    { name: 'Tom Nakamura',    email: 'tom@company.com',    department: 'Finance',     role: 'Financial Analyst',   status: 'Active',   statusClass: 'badge-success', performance: 89, perfColor: '#d97706', joined: 'Sep 2020', initials: 'TN', avatarColor: '#d97706' },
    { name: 'Layla Hassan',    email: 'layla@company.com',  department: 'HR',          role: 'HR Manager',          status: 'Active',   statusClass: 'badge-success', performance: 88, perfColor: '#d97706', joined: 'Feb 2023', initials: 'LH', avatarColor: '#dc2626' },
    { name: 'Chris Evans',     email: 'chris@company.com',  department: 'Engineering', role: 'Frontend Developer',  status: 'Active',   statusClass: 'badge-success', performance: 85, perfColor: '#d97706', joined: 'Apr 2022', initials: 'CE', avatarColor: '#0284c7' },
    { name: 'Amara Diallo',    email: 'amara@company.com',  department: 'Sales',       role: 'Sales Director',      status: 'On Leave', statusClass: 'badge-warning', performance: 82, perfColor: '#d97706', joined: 'Jul 2019', initials: 'AD', avatarColor: '#7c5cfc' },
    { name: 'Riku Watanabe',   email: 'riku@company.com',   department: 'Engineering', role: 'Backend Engineer',    status: 'Active',   statusClass: 'badge-success', performance: 80, perfColor: '#d97706', joined: 'Nov 2022', initials: 'RW', avatarColor: '#4f6ef7' },
    { name: 'Sofia Rossi',     email: 'sofia@company.com',  department: 'Marketing',   role: 'Content Strategist',  status: 'Active',   statusClass: 'badge-success', performance: 79, perfColor: '#d97706', joined: 'Aug 2023', initials: 'SR', avatarColor: '#06b6d4' },
    { name: 'Ben Carter',      email: 'ben@company.com',    department: 'Finance',     role: 'CFO',                 status: 'Active',   statusClass: 'badge-success', performance: 95, perfColor: '#16a34a', joined: 'Jan 2018', initials: 'BC', avatarColor: '#16a34a' },
    { name: 'Nina Petrova',    email: 'nina@company.com',   department: 'HR',          role: 'Talent Acquisition',  status: 'Active',   statusClass: 'badge-success', performance: 76, perfColor: '#dc2626', joined: 'Oct 2023', initials: 'NP', avatarColor: '#dc2626' },
    { name: 'Kai Thompson',    email: 'kai@company.com',    department: 'Engineering', role: 'DevOps Engineer',     status: 'Inactive', statusClass: 'badge-neutral',  performance: 68, perfColor: '#dc2626', joined: 'May 2021', initials: 'KT', avatarColor: '#667085' },
  ];

  filtered = computed(() => {
    let list = this.employees;
    if (this.activeFilter() !== 'All') list = list.filter(e => e.department === this.activeFilter());
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      list = list.filter(e => e.name.toLowerCase().includes(q) || e.role.toLowerCase().includes(q) || e.department.toLowerCase().includes(q));
    }
    return list;
  });

  totalPages = computed(() => Math.ceil(this.filtered().length / this.pageSize));
  pages = computed(() => Array.from({ length: this.totalPages() }, (_, i) => i + 1));
  paginated = computed(() => {
    const p = this.currentPage() - 1;
    return this.filtered().slice(p * this.pageSize, (p + 1) * this.pageSize);
  });
}
