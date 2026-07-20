import { Injectable, inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

/**
 * Authorization Service - Validates: Requirements 30.2, 30.3, 30.4
 * 
 * Implements Role-Based Access Control (RBAC) with:
 * - Permission checking
 * - Resource-level authorization
 * - UI element visibility control
 */

export interface Permission {
  resource: string;
  action: string;
}

export interface RolePermissions {
  [role: string]: Permission[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  
  // Define role-based permissions
  private rolePermissions: RolePermissions = {
    admin: [
      { resource: 'users', action: 'create' },
      { resource: 'users', action: 'read' },
      { resource: 'users', action: 'update' },
      { resource: 'users', action: 'delete' },
      { resource: 'employees', action: 'create' },
      { resource: 'employees', action: 'read' },
      { resource: 'employees', action: 'update' },
      { resource: 'employees', action: 'delete' },
      { resource: 'performance', action: 'create' },
      { resource: 'performance', action: 'read' },
      { resource: 'performance', action: 'update' },
      { resource: 'performance', action: 'delete' },
      { resource: 'reports', action: 'create' },
      { resource: 'reports', action: 'read' },
      { resource: 'reports', action: 'update' },
      { resource: 'reports', action: 'delete' },
      { resource: 'reports', action: 'export' },
      { resource: 'audit', action: 'read' },
      { resource: 'system', action: 'configure' },
    ],
    hr_manager: [
      { resource: 'employees', action: 'create' },
      { resource: 'employees', action: 'read' },
      { resource: 'employees', action: 'update' },
      { resource: 'employees', action: 'delete' },
      { resource: 'performance', action: 'create' },
      { resource: 'performance', action: 'read' },
      { resource: 'performance', action: 'update' },
      { resource: 'reports', action: 'create' },
      { resource: 'reports', action: 'read' },
      { resource: 'reports', action: 'export' },
    ],
    department_manager: [
      { resource: 'employees', action: 'read' },
      { resource: 'performance', action: 'read' },
      { resource: 'reports', action: 'read' },
    ],
    analyst: [
      { resource: 'employees', action: 'read' },
      { resource: 'performance', action: 'read' },
      { resource: 'reports', action: 'create' },
      { resource: 'reports', action: 'read' },
      { resource: 'reports', action: 'export' },
    ],
    employee: [
      { resource: 'employees', action: 'read' }, // Own profile only
      { resource: 'performance', action: 'read' }, // Own performance only
    ],
  };

  private authService = inject(AuthService);

  constructor() {}

  /**
   * Check if user can perform an action on a resource
   */
  canAccess(resource: string, action: string): boolean {
    const user = this.authService.getCurrentUser();
    if (!user) return false;

    return user.roles.some(role => {
      const permissions = this.rolePermissions[role] || [];
      return permissions.some(p => p.resource === resource && p.action === action);
    });
  }

  /**
   * Check if user can perform any of the specified actions
   */
  canAccessAny(resource: string, actions: string[]): boolean {
    return actions.some(action => this.canAccess(resource, action));
  }

  /**
   * Check if user can perform all specified actions
   */
  canAccessAll(resource: string, actions: string[]): boolean {
    return actions.every(action => this.canAccess(resource, action));
  }

  /**
   * Check if user can view resource
   */
  canView(resource: string): boolean {
    return this.canAccess(resource, 'read');
  }

  /**
   * Check if user can create resource
   */
  canCreate(resource: string): boolean {
    return this.canAccess(resource, 'create');
  }

  /**
   * Check if user can edit resource
   */
  canEdit(resource: string): boolean {
    return this.canAccess(resource, 'update');
  }

  /**
   * Check if user can delete resource
   */
  canDelete(resource: string): boolean {
    return this.canAccess(resource, 'delete');
  }

  /**
   * Check if user can export resource
   */
  canExport(resource: string): boolean {
    return this.canAccess(resource, 'export');
  }

  /**
   * Get all permissions for current user
   */
  getUserPermissions(): Permission[] {
    const user = this.authService.getCurrentUser();
    if (!user) return [];

    const permissions: Permission[] = [];
    user.roles.forEach(role => {
      const rolePerms = this.rolePermissions[role] || [];
      permissions.push(...rolePerms);
    });

    // Remove duplicates
    return Array.from(new Map(permissions.map(p => [`${p.resource}:${p.action}`, p])).values());
  }

  /**
   * Check if user is admin
   */
  isAdmin(): boolean {
    return this.authService.hasRole('admin');
  }

  /**
   * Check if user is HR manager
   */
  isHRManager(): boolean {
    return this.authService.hasRole('hr_manager');
  }

  /**
   * Check if user is department manager
   */
  isDepartmentManager(): boolean {
    return this.authService.hasRole('department_manager');
  }

  /**
   * Check if user is analyst
   */
  isAnalyst(): boolean {
    return this.authService.hasRole('analyst');
  }

  /**
   * Check if user is regular employee
   */
  isEmployee(): boolean {
    return this.authService.hasRole('employee');
  }
}
