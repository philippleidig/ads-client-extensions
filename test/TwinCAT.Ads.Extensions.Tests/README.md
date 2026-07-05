# Integration tests

Every test in this project drives the extension methods against a **real TwinCAT
system service** (AMS port `10000`). There is no way to fully exercise the ADS
file/directory services without a running TwinCAT system, so the suite is written
as an integration suite that is **skipped by default** and **opt-in** via
configuration.

## Behaviour without a target

Running `dotnet test` with no configuration probes the default target
(`127.0.0.1.1.1`) once. If nothing answers, every target-dependent test is reported
as **Inconclusive** (skipped) rather than failed, so the suite stays green in CI
and on developer machines that do not have TwinCAT installed.

```
Passed: 2, Failed: 0, Skipped: 112
```

## Running against a real TwinCAT system

### Prerequisites

1. A reachable TwinCAT system running the **system service** on AMS port `10000`:
   - a local **TwinCAT XAR** (Windows engineering/runtime, or TwinCAT/BSD), or
   - a remote IPC / PLC.
2. An **ADS route** from the machine running the tests to the target
   (TwinCAT: *Router → Edit Routes*, or `TcAmsRemoteMgr` / `tcadsroute`).
3. On non-Windows test hosts, a local AMS router (the TwinCAT/BSD system router, or
   the in-process router — see below).

### Configuration (environment variables)

| Variable                   | Meaning                                             | Default              |
| -------------------------- | --------------------------------------------------- | -------------------- |
| `ADS_TEST_TARGET`          | AmsNetId of the target system                       | `127.0.0.1.1.1`      |
| `ADS_TEST_UNREACHABLE`     | An AmsNetId that must **not** exist (negative tests) | `111.111.111.111.1.1` |
| `ADS_TEST_SELFHOST_ROUTER` | `1` to host an in-process AMS router (routing only) | *(off)*              |

> **Important:** the happy-path tests assert against the **local** file system
> (`File.Exists(...)`), so they only match when the target is the **local machine**
> (loopback TwinCAT). Pointing `ADS_TEST_TARGET` at a *remote* target still verifies
> the wire protocol and the error paths, but the local file-system assertions will
> not hold for content that lives on the remote.

### Run

Local (loopback) TwinCAT XAR — nothing to set, just run:

```bash
dotnet test
```

Explicit / remote target:

```bash
# bash
ADS_TEST_TARGET=5.62.31.13.1.1 dotnet test
```

```powershell
# PowerShell
$env:ADS_TEST_TARGET = "5.62.31.13.1.1"; dotnet test
```

Or use the helper scripts (they set the variable and forward extra `dotnet test`
arguments):

```bash
scripts/run-integration-tests.sh 5.62.31.13.1.1
scripts/run-integration-tests.ps1 -Target 5.62.31.13.1.1
```

### Run a single test / class

```bash
dotnet test --filter "FullyQualifiedName~FileExtensionsTests.RenameFileAsync_ShouldRenameFile"
dotnet test --filter "FullyQualifiedName~DirectoryExtensionsTests"
```

## Notes on the in-process router (`ADS_TEST_SELFHOST_ROUTER=1`)

The assembly setup can host an `AmsTcpIpRouter` + `SystemServiceServer`. This
provides AMS **routing and discovery** only — it does **not** implement the file
system service (FOPEN/FREAD/FDELETE/FFILEFIND/…). It is therefore useful for the
routing/reachability tests but **not** for the file/directory happy-path tests,
which still require a real TwinCAT target. Do not enable it on a machine that
already runs a TwinCAT router (port `48898` would clash).

## UTF-8 tests

The `utf8: true` code path (non-ASCII file/directory names) is covered by the
`*_Utf8_*` tests. Because they create files with non-ASCII names on the target,
they require a target whose file system is the local machine (loopback) just like
the other happy-path tests.
