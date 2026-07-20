---
name: Sidebar responsive CSS approach
description: Why the sidebar uses component CSS media queries instead of Tailwind responsive class bindings.
---

# Sidebar Responsive CSS Approach

**Rule:** The `DashboardLayoutComponent` sidebar responsive behavior (fixed on mobile, static icon-only on tablet, full on desktop) must live in `dashboard-layout.component.css` using `@media` queries, NOT in Tailwind responsive prefixes applied via Angular `[class]` bindings.

**Why:** Angular's `[class.X]="expr"` bindings apply classes unconditionally — the expression is evaluated in JS, not CSS. Tailwind responsive prefixes like `md:static` only win if they appear later in the CSS cascade. When Angular adds `-translate-x-full` via class binding, and the root has `overflow-hidden`, the transformed static-positioned sidebar stops participating in the flex layout correctly across breakpoints. The result is the sidebar disappearing at desktop widths.

**How to apply:**
- Keep `@media (max-width: 767px)` for `position: fixed` (mobile overlay)
- Keep `@media (min-width: 768px) and (max-width: 1023px)` for `width: 70px`, `position: static` (tablet icon-only)
- Keep `@media (min-width: 1024px)` for `width: 240px`, `position: static` (desktop full)
- Use Angular `[class.sidebar-open]`, `[class.sidebar-closed-ltr]`, `[class.sidebar-closed-rtl]` bindings for CSS classes defined in the component file (these apply translate only on mobile via media query)
- Tailwind classes are fine for all OTHER layout elements (header, content, dropdowns, etc.)
