import 'zone.js';
import 'zone.js/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { CardComponent } from './card.component';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

@Component({
  template: `
    <app-card>
      <div appCardHeader class="header-text">Header Content</div>
      <p class="body-text">Body Content</p>
      <div appCardFooter class="footer-text">Footer Content</div>
    </app-card>
  `,
  imports: [CardComponent],
  standalone: true
})
class TestHostComponent {}

describe('CardComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    vi.useFakeTimers();
    await TestBed.configureTestingModule({
      imports: [CardComponent, TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    const card = fixture.debugElement.query(By.directive(CardComponent));
    expect(card).toBeTruthy();
  });

  it('should create and render content slots correctly', async () => {
    vi.advanceTimersByTime(100); 
    fixture.detectChanges();
    
    // Check component internal state
    const cardDe = fixture.debugElement.query(By.directive(CardComponent));
    const cardInstance = cardDe.componentInstance;
    expect(cardInstance.hasHeader).toBe(true);
    expect(cardInstance.hasFooter).toBe(true);
    
    fixture.detectChanges();

    const header = fixture.nativeElement.querySelector('.header-text');
    const footer = fixture.nativeElement.querySelector('.footer-text');
    const body = fixture.nativeElement.querySelector('.body-text');

    expect(header).toBeTruthy();
    expect(header.textContent).toContain('Header Content');
    expect(footer).toBeTruthy();
    expect(footer.textContent).toContain('Footer Content');
    expect(body).toBeTruthy();
    expect(body.textContent).toContain('Body Content');
  });
});
