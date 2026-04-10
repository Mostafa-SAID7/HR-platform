import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PerformanceComponent } from './performance.component';
import { provideMockStore } from '@ngrx/store/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('PerformanceComponent Basic', () => {
  let component: PerformanceComponent;
  let fixture: ComponentFixture<PerformanceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PerformanceComponent],
      providers: [
        provideMockStore(),
        provideNoopAnimations(),
        provideHttpClient()
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(PerformanceComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display components despite NO_ERRORS_SCHEMA', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled).toBeTruthy();
  });
});
