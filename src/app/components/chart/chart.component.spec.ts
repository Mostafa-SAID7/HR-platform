import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChartComponent, type ChartConfig } from './chart.component';
import * as echarts from 'echarts';
import { By } from '@angular/platform-browser';

describe('ChartComponent', () => {
  let component: ChartComponent;
  let fixture: ComponentFixture<ChartComponent>;

  const mockConfig: ChartConfig = {
    type: 'bar',
    title: 'Test Chart',
    data: {
      categories: ['A', 'B', 'C'],
      values: [10, 20, 30],
    },
    height: '400px',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChartComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize echarts on init', () => {
    // Re-initialize to test spy
    const initSpy = vi.spyOn(echarts, 'init');
    component.ngOnInit();
    expect(initSpy).toHaveBeenCalled();
  });

  it('should update chart when config changes', () => {
    component.chartConfig = mockConfig;
    fixture.detectChanges();
    
    // Config signal should be updated
    expect(component.config().title).toBe('Test Chart');
    expect(component.config().type).toBe('bar');
  });

  it('should emit chartClick event when chart is clicked', () => {
    // Ensure chart is initialized
    component.ngOnInit();
    const chartInstance = (component as any).chart;
    expect(chartInstance).toBeTruthy();

    const emitSpy = vi.spyOn(component.chartClick, 'emit');
    
    // Simulate chart click
    if (chartInstance) {
      // eCharts trigger/on implementation varies in mock, 
      // but in real it's this.chart.on('click', ...)
      // We manually call the registered handler if we can't trigger it
      const clickHandler = (chartInstance.on as any).mock.calls.find((call: any) => call[0] === 'click')[1];
      clickHandler({ name: 'A', value: 10 });
      expect(emitSpy).toHaveBeenCalledWith({ name: 'A', value: 10 });
    }
  });

  it('should cleanup on destroy', () => {
    component.ngOnInit();
    const chartInstance = (component as any).chart;
    expect(chartInstance).toBeTruthy();

    const disposeSpy = vi.spyOn(chartInstance, 'dispose');
    const disconnectSpy = vi.spyOn((component as any).resizeObserver, 'disconnect');

    component.ngOnDestroy();
    
    expect(disposeSpy).toHaveBeenCalled();
    expect(disconnectSpy).toHaveBeenCalled();
  });

  it('should have correct height from config', () => {
    component.chartConfig = { ...mockConfig, height: '500px' };
    fixture.detectChanges();
    
    const container = fixture.debugElement.query(By.css('.w-full')).nativeElement;
    expect(container.style.height).toBe('500px');
  });
});
