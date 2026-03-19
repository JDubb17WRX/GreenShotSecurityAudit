##PR Title

Stabilize release builds and harden plugin and credential handling

## Summary

This PR does two things:

1. It makes clean Windows/CI builds more deterministic, especially around plugin packaging for the installer.
2. It adds some defense-in-depth hardening around stored secrets, plugin discovery, and restart-manager registration.

The immediate trigger was a GitHub Actions packaging failure where the solution rebuilt successfully, but the installer step could not find plugin DLLs in the expected `Greenshot\bin\Release\net480\Plugins\...` tree. While fixing that, I also cleaned up a few related build assumptions that only worked reliably on developer machines with prebuilt outputs already present.

## Problem

The release build was relying on a few fragile assumptions:

- Plugin post-build copy logic depended on solution-level `OutDir` behavior, which was not stable in CI.
- `Greenshot.csproj` still relied on local binary references for `Greenshot.Base` and `Greenshot.Editor`, instead of project-to-project references.
- The custom build task and resource build path were more brittle on clean agents than on an already-warmed local environment.
- Some security-sensitive areas still used permissive defaults:
  - stored secrets were encrypted with a static symmetric key,
  - plugin discovery searched too broadly,
  - first-run defaults enabled cloud-sharing plugins unless the user disabled them.

## What This PR Changes

### Build and packaging reliability

- Makes plugin output mirroring deterministic by resolving a single app output root for plugin post-build copies instead of depending on solution `OutDir` state.
- Ensures plugin DLLs are copied into the same `Greenshot\bin\Release\net480\Plugins\...` layout the installer expects.
- Normalizes the `MSBuildCommunityTasksPath` import path for clean CI agents.
- Replaces binary references in `Greenshot.csproj` with `ProjectReference`s for `Greenshot.Base` and `Greenshot.Editor`, so dependency ordering works on clean builds.
- Adds the missing `System.Resources.Extensions` package where clean rebuilds need it for non-string `.resx` content.
- Simplifies the custom `UsingTask` declaration so it resolves the built task assembly without relying on `TaskHostFactory`.

### Secret storage hardening

- Replaces the static-key AES path for newly saved secrets with Windows DPAPI (`CurrentUser` scope).
- Preserves backward compatibility by continuing to decrypt legacy values and migrate them forward on the next save.
- Adds the required `System.Security.Cryptography.ProtectedData` package reference for clean builds.

### Plugin loading hardening

- Restricts plugin enumeration to top-level files instead of recursive directory probing.
- Accepts only plugin DLLs that resolve under the installed application roots.
- Adds an allowlist of known built-in plugin assembly names.
- Changes first-run defaults so cloud-sharing plugins start excluded until the user explicitly enables them.

### Restart-manager resilience

- Wraps restart-manager registration in a defensive `try/catch` so Greenshot keeps running even if restart registration is unavailable or fails on a particular host.

## Why This Approach

The build fixes aim to remove hidden dependencies on machine state. A release build should succeed the same way on a fresh GitHub runner as it does on a warmed local box.

The hardening changes are intentionally conservative:

- DPAPI ties stored secrets to the current Windows user instead of a shared static key.
- Plugin discovery now prefers explicit trust boundaries over convenience.
- First-run defaults now avoid automatically enabling cloud-upload destinations.

## Behavior Changes and Compatibility Notes

- Existing encrypted settings continue to load. They are migrated to DPAPI when they are next saved.
- DPAPI-backed secrets become user-profile scoped. That is better for security, but it also means copied config values are no longer portable across users or machines.
- Plugin discovery is stricter now. If Greenshot intentionally supports third-party plugins outside the built-in set, this part may need to be relaxed or made configurable.
- New users will see cloud-sharing plugins excluded by default until enabled.

## Testing

- Reviewed the failing GitHub Actions log from March 19, 2026 and traced the break to the plugin mirror path used during installer packaging.
- Verified that the resolved plugin output root now points to the installer’s expected path: `src\Greenshot\bin\Release\net480\`.
- Confirmed the change set removes the need to special-case individual plugins during packaging.

I was not able to complete a fully representative end-to-end local release rebuild in this environment because the local Visual Studio/.NET toolchain here does not match GitHub’s hosted Windows runner closely enough for a faithful reproduction. CI should be the final authority on the packaging path fix.

## Review Focus

The highest-value review points are:

- Whether the stricter plugin allowlist matches Greenshot’s intended plugin extensibility model.
- Whether DPAPI `CurrentUser` scope is the right tradeoff for Greenshot’s configuration portability expectations.
- Whether the new default exclusion list for cloud plugins matches the desired product behavior for first-run installs.

## Notes

This PR is best described as build stabilization plus security hardening. It is not intended as a claim to fix the currently listed GitHub Security advisories directly; it is defense-in-depth and CI/release reliability work.
