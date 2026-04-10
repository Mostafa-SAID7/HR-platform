import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ReportsComponent } from './reports.component';
import { ReportService } from '../../services/report.service';
import { DataService } from '../../services/data.service';
import { of } from 'rxjs';

describe('ReportsComponent Logic', () => {
  let component: ReportsComponent;
  let mockReportService: any;
  let mockDataService: any;

  beforeEach(() => {
    mockReportService = {
      generateReport: vi.fn(),
    };
    
    mockDataService = {
      getReportTemplates: vi.fn().mockReturnValue(of([]))
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: ReportService, useValue: mockReportService },
        { provide: DataService, useValue: mockDataService }
      ]
    });

    TestBed.runInInjectionContext(() => {
      component = new ReportsComponent();
    });
    
    // Initialize component properties
    component.startDate = '2024-01-01';
    component.endDate = '2024-01-31';
    component.reportTitle = 'Test Report';
    component.selectedTemplate = 'performance';
    
    component.reportTemplates.set([
      { id: 'performance', name: 'Performance Template', description: 'Desc', metrics: [] },
      { id: 'workforce', name: 'Workforce Report', description: 'Desc', metrics: [] }
    ]);
  });

  it('should call generateReport with correct config when exportReport is called', () => {
    component.exportReport('pdf');

    expect(mockReportService.generateReport).toHaveBeenCalledWith(
      expect.objectContaining({
        title: 'Test Report',
        language: 'en',
      }),
      'pdf'
    );
  });

  it('should include filters in report config when selected', () => {
    component.selectedDepartment = 'Engineering';
    component.selectedRegion = 'Middle East';
    
    component.exportReport('csv');

    expect(mockReportService.generateReport).toHaveBeenCalledWith(
      expect.objectContaining({
        filters: {
          Department: 'Engineering',
          Region: 'Middle East',
        }
      }),
      'csv'
    );
  });

  it('should update report title when template is selected', () => {
    const template = {
      id: 'workforce',
      name: 'Workforce Report',
      description: 'Desc',
      metrics: []
    };
    
    component.selectTemplate(template);
    
    expect(component.selectedTemplate).toBe('workforce');
    expect(component.reportTitle).toBe('Workforce Report');
  });

  it('should set default date range on init', () => {
    // Manually call ngOnInit
    component.ngOnInit();
    
    expect(component.startDate).toBeTruthy();
    expect(component.endDate).toBeTruthy();
    
    // Check if endDate is today (approx)
    const today = new Date().toISOString().split('T')[0];
    expect(component.endDate).toBe(today);
  });
});
