# Gate 7 Release Readiness

- Date opened: 2026-08-18
- Candidate: v1.0.0 / win-x64
- Authorization: prepare through immediately before external deployment
- Current status: **WINDOWS 11 PROTOTYPE DISTRIBUTION APPROVED — PRODUCTION NOT APPROVED**

## Approved release shape

- Primary: self-contained win-x64, per-user signed installer
- Secondary: framework-dependent win-x64 portable diagnostic profile
- No trimming and no single-file bundling for v1.0.0
- Installer runs with normal user privileges and installs below LocalAppData
- GitHub Pre-release `v1.0.0-rc.2` is approved only for Windows 11 validation
- Signed Production Release creation remains subject to separate final approval

## Implemented preparation

- Application/assembly/file version fixed at 1.0.0
- `asInvoker` application manifest
- Self-contained and framework-dependent publish profiles
- Repeatable Release Candidate build, ZIP, and SHA-256 generation script
- Inno Setup per-user installer definition
- Uninstall cleanup for the application-owned HKCU Run value

## Validation checklist

- [ ] Current widget process closed before publish
- [x] Release solution build, warnings 0 / errors 0
- [x] Automated specifications 66/66 PASS
- [x] Self-contained publish succeeds
- [x] Framework-dependent publish succeeds
- [x] Published executable file/product version is 1.0.0
- [x] Self-contained portable launch/lifecycle smoke PASS
- [x] Framework-dependent portable launch/lifecycle smoke PASS
- [x] Duplicate-instance launch is rejected cleanly
- [x] SHA-256 manifest generated and independently rechecked
- [x] Inno Setup 6.7.3 installed and unsigned installer build PASS
- [ ] Code-signing certificate supplied; executable and installer signatures PASS
- [x] Isolated per-user install/reinstall/uninstall and installed-app smoke PASS
- [x] Windows 10 22H2 build 19045 portable RC lifecycle validation PASS
- [ ] Windows 11 validation PASS
- [ ] Autostart after installed-path move PASS
- [x] No known Critical/Major code defect open after duplicate-instance fix
- [ ] Release notes and known limitations approved
- [x] Unsigned Windows 11 test prototype distribution approval received
- [x] GitHub Pre-release v1.0.0-rc.1 uploaded and remote SHA-256 verified (superseded)
- [x] GitHub Pre-release v1.0.0-rc.2 desktop-shortcut build uploaded and remote SHA-256 verified
- [ ] Signed Production deployment approval received

## Preserved release conditions

- NFR-STAB-001/002/003 remain Deferred with owner-accepted residual risk; they are not PASS.
- DualSense Bluetooth precision is a 10% estimated bucket.
- Exclusive fullscreen overlay is not guaranteed by Windows Topmost.
- Same-model two-device physical validation remains a limitation.
- Windows 11 evidence, signed installer, and clean install/uninstall are release blockers until completed.

## Local environment findings

- Current kernel reports Windows 10 build 19045.
- Inno Setup 6.7.3 was installed per-user and the installer build passed.
- No current-user code-signing certificate was found at Gate 7 start.
- Signature validation remains blocked pending a production signing identity.
