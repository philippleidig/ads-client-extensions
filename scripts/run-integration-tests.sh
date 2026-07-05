#!/usr/bin/env bash
#
# Runs the integration test suite against a real TwinCAT system service.
#
# Usage:
#   scripts/run-integration-tests.sh [TARGET_AMSNETID] [-- <extra dotnet test args>]
#
# Examples:
#   scripts/run-integration-tests.sh                       # loopback TwinCAT (127.0.0.1.1.1)
#   scripts/run-integration-tests.sh 5.62.31.13.1.1        # explicit target
#   scripts/run-integration-tests.sh 5.62.31.13.1.1 -- --filter FullyQualifiedName~RenameFileAsync
#
# The target can also be set via the ADS_TEST_TARGET environment variable.
# See test/TwinCAT.Ads.Extensions.Tests/README.md for prerequisites (ADS route etc.).
set -euo pipefail

TARGET="${1:-${ADS_TEST_TARGET:-127.0.0.1.1.1}}"
if [[ "${1:-}" == "--" ]]; then TARGET="${ADS_TEST_TARGET:-127.0.0.1.1.1}"; else shift || true; fi
if [[ "${1:-}" == "--" ]]; then shift; fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/../test/TwinCAT.Ads.Extensions.Tests"

echo "Running integration tests against ADS target: $TARGET"
ADS_TEST_TARGET="$TARGET" dotnet test "$PROJECT" "$@"
