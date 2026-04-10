import { describe, it, expect } from 'vitest';
import { TurnoverComponent } from './turnover.component';

describe('Turnover Calculation Logic', () => {
  
  it('should correctly calculate turnover rate', () => {
    // (50 departures / 1000 avg employees) * 100 = 5%
    expect(TurnoverComponent.calculateTurnoverRate(50, 1000)).toBe(5);
    
    // (28 departures / 12000 avg employees) * 100 = 0.233... -> 0.2
    expect(TurnoverComponent.calculateTurnoverRate(28, 12000)).toBe(0.2);
    
    // (10 departures / 100 avg employees) * 100 = 10%
    expect(TurnoverComponent.calculateTurnoverRate(10, 100)).toBe(10);
  });

  it('should handle zero average headcount', () => {
    expect(TurnoverComponent.calculateTurnoverRate(50, 0)).toBe(0);
  });

  it('should handle zero departures', () => {
    expect(TurnoverComponent.calculateTurnoverRate(0, 5000)).toBe(0);
  });

  it('should round to one decimal place', () => {
    // (1 departure / 3 employees) * 100 = 33.333...%
    expect(TurnoverComponent.calculateTurnoverRate(1, 3)).toBe(33.3);
  });
});
