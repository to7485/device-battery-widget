# Gate 6 Provider Isolation Spec 01

- Date: 2026-08-16
- Scope: NFR-STAB-005 exception isolation
- Method: deterministic Application specification

## Verified behavior

- Production App starts each `IBatteryProvider` through `ProviderRunner.RunIsolatedAsync`.
- A deliberately failing Provider exception is captured and reported to its failure callback.
- A concurrently executed healthy Provider completes and its event remains readable from the shared channel.
- Cancellation requested by normal application shutdown is treated as expected completion rather than a Provider failure.
- The App logs only Provider ID and exception type for an isolated failure; raw IDs and exception payload data are not logged.

## Result

**PASS**

This deterministic failure-injection specification validates isolation mechanics. It does not replace long-duration real-device stability testing.
