import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InputComponent } from './input.component';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

describe('InputComponent', () => {
  let component: InputComponent;
  let fixture: ComponentFixture<InputComponent>;
  let inputEl: HTMLInputElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(InputComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should reflect disabled state', () => {
    component.disabled = true;
    fixture.detectChanges();
    const inputEl = fixture.nativeElement.querySelector('input');
    expect(inputEl.disabled).toBe(true);
  });

  it('should reflect label and required text', () => {
    component.label = 'Username';
    component.required = true;
    fixture.detectChanges();
    const labelEl = fixture.nativeElement.querySelector('label');
    expect(labelEl.textContent).toContain('Username');
    expect(labelEl.querySelector('span').textContent).toBe('*');
  });

  it('should emit value changes and call onChange via ControlValueAccessor', () => {
    fixture.detectChanges();
    const inputEl = fixture.nativeElement.querySelector('input');
    let emittedValue = '';
    component.valueChange.subscribe(val => emittedValue = val);
    
    const onChangeSpy = vi.fn();
    component.registerOnChange(onChangeSpy);
    
    inputEl.value = 'test';
    inputEl.dispatchEvent(new Event('input'));
    
    expect(emittedValue).toBe('test');
    expect(component.value).toBe('test');
    expect(onChangeSpy).toHaveBeenCalledWith('test');
  });

  it('should show error and update ARIA attributes when touched and error is set', () => {
    component.error = 'Invalid value';
    component.onTouched = vi.fn(); // Mocking it here as a spy
    component.onBlur(); // sets touched to true
    fixture.detectChanges();
    
    const inputEl = fixture.nativeElement.querySelector('input');
    expect(component.onTouched).toHaveBeenCalled();
    const errorEl = fixture.nativeElement.querySelector('p.text-red-600');
    expect(errorEl.textContent.trim()).toBe('Invalid value');
    expect(inputEl.getAttribute('aria-invalid')).toBe('true');
  });

  it('should pass accessibility checks', async () => {
    const { checkA11y } = await import('../../testing/a11y-utils');
    component.label = 'Username';
    fixture.detectChanges();
    await checkA11y(fixture);
  });
});
