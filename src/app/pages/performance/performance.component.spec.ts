import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PerformanceComponent } from './performance.component';
import { provideMockStore } from '@ngrx/store/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { I18nService, ThemeService } from '../../services';

describe('PerformanceComponent Simple', () => {
  let component: PerformanceComponent;
  let fixture: ComponentFixture<PerformanceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PerformanceComponent],
      providers: [
        provideMockStore(),
        provideNoopAnimations(),
        provideHttpClient(),
        { provide: I18nService, useValue: { language: () => 'en', isRTL: () => false, translate: (k: string) => k } },
        { provide: ThemeService, useValue: { theme: () => 'light', isDarkMode: () => false } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PerformanceComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
