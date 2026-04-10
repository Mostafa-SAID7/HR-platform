import { ComponentFixture } from '@angular/core/testing';
import { axe } from 'vitest-axe';
import * as matchers from 'vitest-axe/matchers';
import { expect } from 'vitest';

/**
 * Accessibility Testing Utility
 * 
 * Provides a standard way to check for accessibility violations in components.
 * Uses axe-core and vitest-axe.
 */

// Extend expect for vitest-axe
expect.extend(matchers);

/**
 * Runs accessibility checks on a component fixture
 * @param fixture The component fixture to test
 * @param options Axe configuration options (optional)
 */
export async function checkA11y<T>(
  fixture: ComponentFixture<T>,
  options: any = {}
): Promise<void> {
  // Ensure the fixture is stable
  await fixture.whenStable();
  
  // Get the native element
  const element = fixture.nativeElement;
  
  // Run axe on the element
  const results = await axe(element, options);
  
  // Assert no violations
  (expect(results) as any).toHaveNoViolations();
}
