import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BadgeComponent } from './badge.component';

describe('BadgeComponent', () => {
  let component: BadgeComponent;
  let fixture: ComponentFixture<BadgeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BadgeComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BadgeComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should apply info variant classes by default', () => {
    fixture.detectChanges();
    const spanElement = fixture.nativeElement.querySelector('span');
    expect(spanElement.className).toContain('bg-blue-100');
  });

  it('should apply success variant classes when specified', () => {
    component.variant = 'success';
    fixture.detectChanges();
    const spanElement = fixture.nativeElement.querySelector('span');
    expect(spanElement.className).toContain('bg-emerald-100');
    expect(spanElement.className).toContain('text-emerald-800');
  });

  it('should apply aria-label when provided', () => {
    component.ariaLabel = 'Status Alert';
    fixture.detectChanges();
    const spanElement = fixture.nativeElement.querySelector('span');
    expect(spanElement.getAttribute('aria-label')).toBe('Status Alert');
  });
});
