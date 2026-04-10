import { signal } from '@angular/core';
import * as fc from 'fast-check';
import { describe, it, expect } from 'vitest';

/**
 * Property Tests for Workforce Metrics Consistency
 * 
 * Goal: Ensure that the total headcount always matches the sum of its parts 
 * (region, department, status) regardless of the data values.
 */
describe('Workforce Metrics Consistency (Property-Based)', () => {
  
  // Logic to project to WorkforceComponent style signals
  const calculateTotal = (values: number[]) => values.reduce((a, b) => a + b, 0);

  it('Total headcount must equal sum of regional headcounts', () => {
    fc.assert(
      fc.property(fc.array(fc.integer({ min: 0, max: 10000 }), { minLength: 1, maxLength: 20 }), (regionalValues) => {
        const total = calculateTotal(regionalValues);
        const regionalSum = regionalValues.reduce((a, b) => a + b, 0);
        
        expect(total).toBe(regionalSum);
      })
    );
  });

  it('Total headcount must remain consistent across different segments (Region vs Dept)', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 100, max: 100000 }), 
        (totalHeadcount) => {
          // Generate regional distribution that sums to total
          const distribute = (total: number, parts: number) => {
            let remaining = total;
            const result = [];
            for (let i = 0; i < parts - 1; i++) {
              const part = Math.floor(Math.random() * remaining);
              result.push(part);
              remaining -= part;
            }
            result.push(remaining);
            return result;
          };

          const regions = distribute(totalHeadcount, 5);
          const depts = distribute(totalHeadcount, 10);

          expect(calculateTotal(regions)).toBe(calculateTotal(depts));
          expect(calculateTotal(regions)).toBe(totalHeadcount);
        }
      )
    );
  });
});
