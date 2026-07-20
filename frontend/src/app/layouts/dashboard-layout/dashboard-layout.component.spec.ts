import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardLayoutComponent } from './dashboard-layout.component';
import { RouterTestingModule } from '@angular/router/testing';
import { I18nService } from '../../services/i18n.service';
import { ThemeService } from '../../services/theme.service';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { vi } from 'vitest';

// Mock components to simplify deeply nested layout testing
@Component({ selector: 'app-breadcrumb', template: '', standalone: true, inputs: ['items'] })
class MockBreadcrumbComponent {}

@Component({ selector: 'app-icon', template: '', standalone: true, inputs: ['name', 'size'] })
class MockIconComponent {}

@Component({ selector: 'app-connection-status', template: '', standalone: true })
class MockConnectionStatusComponent {}

describe('DashboardLayoutComponent', () => {
  let component: DashboardLayoutComponent;
  let fixture: ComponentFixture<DashboardLayoutComponent>;
  let i18nMock: any;
  let themeMock: any;

  beforeEach(async () => {
    i18nMock = { 
        getLanguage: vi.fn().mockReturnValue('en'),
        setLanguage: vi.fn(),
        translate: vi.fn((k) => k),
        isArabic: vi.fn().mockReturnValue(false)
    };
    
    themeMock = {
        isDark: vi.fn().mockReturnValue(false),
        toggleTheme: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [DashboardLayoutComponent, RouterTestingModule],
      providers: [
        { provide: I18nService, useValue: i18nMock },
        { provide: ThemeService, useValue: themeMock }
      ]
    })
    .overrideComponent(DashboardLayoutComponent, {
      set: { 
        imports: [CommonModule, RouterTestingModule, MockBreadcrumbComponent, MockIconComponent, MockConnectionStatusComponent] 
      }
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle sidebar appropriately', () => {
    expect(component.isSidebarOpen()).toBe(false);
    component.toggleSidebar();
    expect(component.isSidebarOpen()).toBe(true);
  });

  it('should allow toggling the language and notify the service', () => {
    i18nMock.getLanguage.mockReturnValue('en');
    component.toggleLanguage();
    expect(i18nMock.setLanguage).toHaveBeenCalledWith('ar');
  });

  it('should rely on ThemeService for theme toggles', () => {
    component.toggleTheme();
    expect(themeMock.toggleTheme).toHaveBeenCalled();
  });
});
