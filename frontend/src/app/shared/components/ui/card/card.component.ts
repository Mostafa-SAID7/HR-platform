import { Component, ElementRef, AfterContentInit, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { fadeIn, slideInUp } from '@app/shared/animations';

/**
 * Card Component
 *
 * A reusable card component with header, body, and footer slots.
 * Provides a container for content with consistent styling.
 *
 * Requirements: 2.2, 8.2
 */
@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  animations: [fadeIn, slideInUp],
  templateUrl: './card.component.html',
})
export class CardComponent implements AfterContentInit {
  hasHeader = false;
  hasFooter = false;

  constructor(private el: ElementRef) {}

  ngAfterContentInit() {
    // Small delay to ensure content is rendered in JSDOM/Tests
    setTimeout(() => {
      this.hasHeader = !!this.el.nativeElement.querySelector('[appCardHeader]');
      this.hasFooter = !!this.el.nativeElement.querySelector('[appCardFooter]');
    }, 0);
  }
}
