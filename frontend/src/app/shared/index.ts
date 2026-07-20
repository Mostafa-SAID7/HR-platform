/**
 * Shared Module Barrel Export
 * Exports all shared components, pipes, directives, widgets, models, and utilities
 *
 * Usage:
 * // Import specific components (standalone)
 * import { ButtonComponent, InputComponent } from '@app/shared/components/ui';
 *
 * // Import component groups (standalone)
 * import { UI_FORM_COMPONENTS, COMMON_COMPONENTS } from '@app/shared/components';
 *
 * // Import module (NgModule)
 * import { SharedModule } from '@app/shared';
 *
 * // Import pipes/directives/utilities directly
 * import { SafeHtmlPipe, ClickOutsideDirective } from '@app/shared/pipes';
 */

// ============================================
// Components (Organized by Category)
// ============================================

export * from './components/ui/index';
export * from './components/common/index';
// Layout components are in app/layouts, not in shared
// export * from './components/layout/index';
export * from './components/index'; // Component groups and all exports

// ============================================
// Pipes
// ============================================

export {
  SafeHtmlPipe,
  CurrencyFormatPipe,
  DateFormatPipe,
  TruncatePipe,
  HighlightPipe,
  TranslatePipe,
} from './pipes/index';

// ============================================
// Directives
// ============================================

export {
  ClickOutsideDirective,
  HasPermissionDirective,
  LoadingDirective,
  HighlightTextDirective,
  AutoFocusDirective,
} from './directives/index';

// ============================================
// Widgets
// ============================================

export {
  KPIWidgetComponent,
  StatsCardComponent,
  ChartWidgetComponent,
  ProgressWidgetComponent,
  ActivityFeedComponent,
} from './widgets/index';

// ============================================
// Models
// ============================================

// Explicitly export non-conflicting model types
// (FilterCriteria, SearchResult, Notification are already exported via components/ui/index)
export type { TableColumn, TableConfig, TableSort, TableFilter } from './models/table.model';
export type { FilterOperator, AdvancedFilter } from './models/filter.model';
export type { ToastType, AlertType, Toast, Modal, ModalButton, Breadcrumb } from './models/ui.model';

// ============================================
// Utilities
// ============================================

export * from './utils/index';

// ============================================
// Animations & Styles
// ============================================

export { trigger, transition, style, animate } from '@angular/animations';
export * from './animations';

// ============================================
// SharedModule (for NgModule-based components)
// ============================================

export { SharedModule } from './shared.module';

// ============================================
// Convenience Exports for Common Use Cases
// ============================================

/**
 * All UI Components - Forms, Displays, Data, Visualization, Connection
 * Useful when you want to import all UI components at once
 */
export {
  ALL_UI_COMPONENTS,
} from './components/index';

/**
 * All Common Components - DataGrid, Export, Pagination, etc.
 * Used for common patterns across features
 */
export { COMMON_COMPONENTS, ALL_SHARED_COMPONENTS } from './components/index';

/**
 * All Pipes - Translation, Formatting, Filtering
 * Use for data transformation in templates
 */
export const ALL_SHARED_PIPES = [
  'safeHtml',
  'currencyFormat',
  'dateFormat',
  'truncate',
  'highlight',
  'translate',
];

/**
 * All Directives - Permissions, Loading, Focus, ClickOutside
 * Use for DOM manipulation and behavior
 */
export const ALL_SHARED_DIRECTIVES = [
  'appClickOutside',
  'appHasPermission',
  'appLoading',
  'appHighlightText',
  'appAutoFocus',
];
