import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChangeDetectorRef } from '@angular/core';
import { KPICardComponent, KPIData } from './kpi-card.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('KPICardComponent', () => {
  let component: KPICardComponent;
  let fixture: ComponentFixture<KPICardComponent>;
  let cdr: ChangeDetectorRef;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KPICardComponent, BrowserAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(KPICardComponent);
    component = fixture.componentInstance;
    cdr = fixture.debugElement.injector.get(ChangeDetectorRef);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display KPI label', () => {
    component.data = {
      label: 'Total Headcount',
      value: 12000,
    };
    cdr.markForCheck();
    fixture.detectChanges();

    const label = fixture.nativeElement.querySelector('h3');
    expect(label.textContent).toContain('Total Headcount');
  });

  it('should format large numbers correctly', () => {
    component.data = {
      label: 'Test',
      value: 1500000,
    };
    cdr.markForCheck();
    fixture.detectChanges();

    const formatted = component.formatNumber(1500000);
    expect(formatted).toBe('1.5M');
  });

  it('should format thousands correctly', () => {
    component.data = {
      label: 'Test',
      value: 12000,
    };
    cdr.markForCheck();
    fixture.detectChanges();

    const formatted = component.formatNumber(12000);
    expect(formatted).toBe('12.0K');
  });

  it('should display trend indicator when trend is provided', () => {
    component.data = {
      label: 'Test',
      value: 100,
      trend: 'up',
      trendPercentage: 5,
    };
    cdr.markForCheck();
    fixture.detectChanges();

    const trendText = fixture.nativeElement.textContent;
    expect(trendText).toContain('5%');
  });

  it('should show loading state', () => {
    component.data = {
      label: 'Test',
      value: 100,
      loading: true,
    };
    cdr.markForCheck();
    fixture.detectChanges();

    const spinner = fixture.nativeElement.querySelector('svg');
    expect(spinner).toBeTruthy();
  });

  it('should display error message when error is provided', () => {
    component.data = {
      label: 'Test',
      value: 0,
      error: 'Failed to load data',
    };
    cdr.markForCheck();
    fixture.detectChanges();

    const errorText = fixture.nativeElement.textContent;
    expect(errorText).toContain('Failed to load data');
  });
});
