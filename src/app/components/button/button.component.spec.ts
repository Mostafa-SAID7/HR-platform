import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ButtonComponent } from './button.component';
import { vi } from 'vitest';

describe('ButtonComponent', () => {
  let component: ButtonComponent;
  let fixture: ComponentFixture<ButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should apply primary variant classes by default', () => {
    fixture.detectChanges();
    const buttonElement = fixture.nativeElement.querySelector('button');
    expect(buttonElement.className).toContain('bg-indigo-600');
  });

  it('should apply danger variant classes when specified', () => {
    component.variant = 'danger';
    fixture.detectChanges();
    const buttonElement = fixture.nativeElement.querySelector('button');
    expect(buttonElement.className).toContain('bg-red-600');
  });

  it('should apply sizing classes', () => {
    component.size = 'lg';
    fixture.detectChanges();
    const buttonElement = fixture.nativeElement.querySelector('button');
    expect(buttonElement.className).toContain('px-6 py-3');
  });

  it('should emit onClick event when clicked but not when disabled', () => {
    const emitSpy = vi.spyOn(component.onClick, 'emit');
    const buttonElement = fixture.nativeElement.querySelector('button');
    
    // Normal click
    buttonElement.click();
    expect(emitSpy).toHaveBeenCalled();
    
    // Disabled click
    component.disabled = true;
    fixture.detectChanges();
    emitSpy.mockClear();
    
    buttonElement.click();
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('should apply aria-label if provided', () => {
    component.ariaLabel = 'Test Button';
    fixture.detectChanges();
    const buttonElement = fixture.nativeElement.querySelector('button');
    expect(buttonElement.getAttribute('aria-label')).toBe('Test Button');
  });
});
