import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { I18nService, ThemeService } from './services';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: I18nService, useValue: { language: () => 'en', isRTL: () => false } },
        { provide: ThemeService, useValue: { theme: () => 'light', isDarkMode: () => false } }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('div')).toBeTruthy();
  });
});
