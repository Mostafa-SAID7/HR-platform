---
name: Tailwind semantic color tokens
description: The project uses semantic Tailwind tokens (gray, primary, surface, sidebar) mapped to CSS vars, replacing hardcoded slate/indigo classes.
---

# Tailwind Semantic Color Tokens

**Rule:** Use semantic token class names (`gray-700`, `primary`, `surface-card`, etc.) instead of hardcoded `slate-*`/`indigo-*` Tailwind classes. These tokens are defined in `frontend/tailwind.config.ts` and point to CSS variables from `frontend/src/styles.css`.

**Token mapping:**
- `gray-25` … `gray-900` → `var(--gray-25)` … `var(--gray-900)` (dark mode handled by `.dark { --gray-N: ... }` overrides automatically)
- `primary` → `var(--color-primary)` (#4f6ef7)
- `primary-hover` → `var(--color-primary-hover)`
- `primary-dark` → `var(--color-primary-dark)`
- `primary-light` → `var(--color-primary-light)`
- `surface-bg`, `surface-card`, `surface-border`, `surface-divider` → surface CSS vars
- `sidebar-bg`, `sidebar-border`, `sidebar-text`, `sidebar-accent` → sidebar CSS vars
- `success`, `warning`, `error`, `info` → semantic status vars

**Why:** CSS-var-backed tokens mean dark mode works automatically (vars override in `.dark {}`) without needing `dark:` Tailwind prefixes on every color class. This is the design system's intent.

**How to apply:** When adding new UI, use `text-gray-700` not `text-slate-700`, `bg-primary` not `bg-indigo-500`, etc. The `slate` and `indigo` color scales remain in tailwind.config for legacy third-party components only.
