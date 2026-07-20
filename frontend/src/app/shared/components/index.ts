/**
 * Shared Components Barrel Export
 * All reusable components organized by category
 * 
 * NOTE: Only components that are actually implemented are exported here.
 * Stub/planned components are commented out.
 */

// ============================================
// UI Components (Form, Display, Data, etc.)
// ============================================

// Form Components
// TODO: Implement these components
// export { InputComponent } from './ui/input/input.component';
// export { ButtonComponent } from './ui/button/button.component';
// export { SelectComponent } from './ui/select/select.component';
// export { CheckboxComponent } from './ui/checkbox/checkbox.component';
// export { RadioComponent } from './ui/radio/radio.component';
// export { ToggleComponent } from './ui/toggle/toggle.component';
// export { DatePickerComponent } from './ui/date-picker/date-picker.component';

// Display Components
// export { CardComponent } from './ui/card/card.component';
// export { BadgeComponent } from './ui/badge/badge.component';
// export { IconComponent } from './ui/icon/icon.component';
// export { BreadcrumbComponent } from './ui/breadcrumb/breadcrumb.component';

// Complex Components
// export { DropdownComponent } from './ui/dropdown/dropdown.component';
// export { TabsComponent } from './ui/tabs/tabs.component';
// export { AccordionComponent } from './ui/accordion/accordion.component';
// export { ModalComponent } from './ui/modal/modal.component';

// Data Components
export { DataTableComponent } from './ui/data-table/data-table.component';
export { FilterPanelComponent } from './ui/filter-panel/filter-panel.component';
// export { SearchInputComponent } from './ui/search-input/search-input.component';

// Visualization Components
// export { ChartComponent } from './ui/chart/chart.component';
// export { KPICardComponent } from './ui/kpi-card/kpi-card.component';

// Connection Components
export { ConnectionStatusComponent } from './ui/connection-status/connection-status.component';
// export { NotificationCenterComponent } from './ui/notification-center/notification-center.component';

// ============================================
// Common Components
// ============================================

// export { DataGridComponent } from './common/data-grid/data-grid.component';
// export { ExportButtonComponent } from './common/export-button/export-button.component';
// export { PaginationComponent } from './common/pagination/pagination.component';
// export { EmptyStateComponent } from './common/empty-state/empty-state.component';
export { LoadingSkeletonComponent } from './common/loading-skeleton/loading-skeleton.component';
export { ToastComponent } from './common/toast/toast.component';
export { AlertComponent } from './common/alert/alert.component';
// export { ConfirmDialogComponent } from './common/confirm-dialog/confirm-dialog.component';
// export { BreadcrumbsComponent } from './common/breadcrumbs/breadcrumbs.component';

export const ALL_SHARED_COMPONENTS = [
  ...ALL_UI_COMPONENTS,
  ...COMMON_COMPONENTS,
];
