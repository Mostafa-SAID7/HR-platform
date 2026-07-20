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
import { DataTableComponent } from './ui/data-table/data-table.component';
import { FilterPanelComponent } from './ui/filter-panel/filter-panel.component';
export { DataTableComponent, FilterPanelComponent };

// Connection Components
import { ConnectionStatusComponent } from './ui/connection-status/connection-status.component';
export { ConnectionStatusComponent };

// ============================================
// Common Components
// ============================================

import { LoadingSkeletonComponent } from './common/loading-skeleton/loading-skeleton.component';
import { ToastComponent } from './common/toast/toast.component';
import { AlertComponent } from './common/alert/alert.component';
export { LoadingSkeletonComponent, ToastComponent, AlertComponent };
// export { ConfirmDialogComponent } from './common/confirm-dialog/confirm-dialog.component';
// export { BreadcrumbsComponent } from './common/breadcrumbs/breadcrumbs.component';

// UI component groups (based on currently implemented components)
export const UI_FORM_COMPONENTS: any[] = [];
export const UI_DISPLAY_COMPONENTS: any[] = [];
export const UI_COMPLEX_COMPONENTS: any[] = [];
export const UI_DATA_COMPONENTS = [DataTableComponent, FilterPanelComponent];
export const UI_VISUALIZATION_COMPONENTS: any[] = [];
export const UI_CONNECTION_COMPONENTS = [ConnectionStatusComponent];

export const ALL_UI_COMPONENTS = [
  ...UI_DATA_COMPONENTS,
  ...UI_CONNECTION_COMPONENTS,
];

export const COMMON_COMPONENTS = [
  LoadingSkeletonComponent,
  ToastComponent,
  AlertComponent,
];

export const ALL_SHARED_COMPONENTS = [
  ...ALL_UI_COMPONENTS,
  ...COMMON_COMPONENTS,
];
