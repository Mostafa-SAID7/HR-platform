---
name: caniuse-lite version pin
description: Why caniuse-lite is pinned to 1.0.30001579 and what breaks if upgraded.
---

# caniuse-lite Version Pin

**Rule:** Keep `caniuse-lite` pinned to `1.0.30001579` in `frontend/package.json`.

**Why:** `browserslist` v4.28.6 (the latest) requires `caniuse-lite/dist/unpacker/agents`. Versions of `caniuse-lite` after `1.0.30001579` removed that file, causing a MODULE_NOT_FOUND crash when `ng serve` starts.

**How to apply:** If a dependency upgrade pulls in a newer `caniuse-lite`, re-pin it: `npm install caniuse-lite@1.0.30001579 --save-exact` inside `frontend/`. Do not upgrade without also verifying `browserslist` has been updated to not require the removed file.
