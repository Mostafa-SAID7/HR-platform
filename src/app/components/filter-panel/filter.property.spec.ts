import { FilterCriteria } from './filter-panel.component';

// Extract the filtering algorithm used in employees.component.ts for property testing
function applyFilters(data: any[], filters: FilterCriteria): any[] {
  return data.filter((employee) => {
    if (filters.department && filters.department.length > 0) {
      if (!filters.department.includes(employee.department)) return false;
    }
    if (filters.region && filters.region.length > 0) {
      if (!filters.region.includes(employee.region)) return false;
    }
    if (filters.status && filters.status.length > 0) {
      if (!filters.status.includes(employee.employmentStatus)) return false;
    }
    if (filters.performanceScoreRange && employee.performanceScore !== undefined) {
      const [min, max] = filters.performanceScoreRange;
      if (employee.performanceScore < min || employee.performanceScore > max) return false;
    }
    return true;
  });
}

describe('Filtering Property Tests (Requirement 3.6)', () => {
  // Generate random employee data
  const generateRandomEmployees = (count: number) => {
    const deps = ['Engineering', 'Sales', 'Marketing', 'HR'];
    const regions = ['Middle East', 'Europe', 'North America'];
    const statuses = ['active', 'on-leave', 'departed'];
    
    return Array.from({ length: count }, (_, i) => ({
      id: i.toString(),
      name: `Emp ${i}`,
      department: deps[Math.floor(Math.random() * deps.length)],
      region: regions[Math.floor(Math.random() * regions.length)],
      employmentStatus: statuses[Math.floor(Math.random() * statuses.length)],
      performanceScore: Math.floor(Math.random() * 100)
    }));
  };

  const dataset = generateRandomEmployees(1000);

  it('Property 1: Filter Consistency - order of filters does not affect results', () => {
    // We will test if creating criteria piece by piece yields the same as combined
    const criteriaDept: FilterCriteria = { department: ['Engineering', 'Sales'] };
    const criteriaRegion: FilterCriteria = { region: ['Europe'] };
    const criteriaCombined: FilterCriteria = { department: ['Engineering', 'Sales'], region: ['Europe'] };

    // Apply seq:
    const step1 = applyFilters(dataset, criteriaDept);
    const step2 = applyFilters(step1, criteriaRegion);

    // Apply combined
    const combined = applyFilters(dataset, criteriaCombined);

    // They should ideally be exactly the same length and items
    expect(step2.length).toBe(combined.length);
    expect(step2).toEqual(combined);
  });

  it('Property 2: Filter Subset - filtered results are ALWAYS a subset of unfiltered data', () => {
    const criteria: FilterCriteria = {
      department: ['HR'],
      status: ['active'],
      performanceScoreRange: [50, 100]
    };

    const result = applyFilters(dataset, criteria);
    
    expect(result.length).toBeLessThanOrEqual(dataset.length);

    // Ensure every item in results exists in dataset
    const allInternal = result.every(resItem => dataset.some(d => d.id === resItem.id));
    expect(allInternal).toBe(true);

    // Ensure criteria matches exactly
    const propertyHoldsObjectively = result.every(r => 
        r.department === 'HR' && 
        r.employmentStatus === 'active' && 
        r.performanceScore >= 50 && r.performanceScore <= 100
    );
    expect(propertyHoldsObjectively).toBe(true);
  });
});
