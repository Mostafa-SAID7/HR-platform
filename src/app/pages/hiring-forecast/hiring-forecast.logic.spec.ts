import { describe, it, expect } from 'vitest';
import { HiringForecastComponent } from './hiring-forecast.component';

describe('Hiring Forecast Calculation Logic', () => {
  
  it('should correctly calculate total hires', () => {
    // 12000 count, 5% growth (600), 2.3% turnover (276), 45 planned
    // 600 + 276 + 45 = 921
    expect(HiringForecastComponent.calculateTotalHires(12000, 5, 2.3, 45)).toBe(921);
    
    // Simple case: 1000 count, 10% growth (100), 10% turnover (100), 0 planned = 200
    expect(HiringForecastComponent.calculateTotalHires(1000, 10, 10, 0)).toBe(200);
  });

  it('should handle zero values', () => {
    expect(HiringForecastComponent.calculateTotalHires(12000, 0, 0, 0)).toBe(0);
    expect(HiringForecastComponent.calculateTotalHires(0, 5, 2, 10)).toBe(10);
  });

  it('should round to nearest integer', () => {
    // 100 count, 1.25% growth (1.25), 1.25% turnover (1.25), 0 planned = 2.5 -> 3
    expect(HiringForecastComponent.calculateTotalHires(100, 1.25, 1.25, 0)).toBe(3);
  });
});
