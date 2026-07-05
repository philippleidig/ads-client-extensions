using TwinCAT.Ads;

namespace TwinCAT.Ads.Extensions.Tests
{
	/// <summary>
	/// Central configuration for the integration test suite.
	/// </summary>
	/// <remarks>
	/// Every test in this project talks to a real TwinCAT system service (AMS port
	/// 10000). The target is configurable via environment variables so the suite can
	/// be pointed at a locally installed TwinCAT XAR (the default loopback target) or
	/// at a remote IPC without editing code:
	///
	///   ADS_TEST_TARGET           AmsNetId of the target       (default "127.0.0.1.1.1")
	///   ADS_TEST_UNREACHABLE      AmsNetId that must NOT exist  (default "111.111.111.111.1.1")
	///   ADS_TEST_SELFHOST_ROUTER  "1" to host an in-process AMS router (CI without TwinCAT)
	///
	/// When no reachable target is configured, integration tests are reported as
	/// <c>Inconclusive</c> (skipped) instead of failing – see <see cref="RequireTarget"/>.
	///
	/// NOTE: the happy-path tests compare against the LOCAL file system
	/// (<c>File.Exists(...)</c>), so they are only meaningful when the target is the
	/// local machine (loopback TwinCAT). Pointing at a remote target still exercises
	/// the wire protocol but the local file-system assertions will not match.
	/// </remarks>
	internal static class Globals
	{
		public static readonly AmsNetId TargetSystem = AmsNetId.Parse(
			Environment.GetEnvironmentVariable("ADS_TEST_TARGET") ?? "127.0.0.1.1.1"
		);

		public static readonly AmsNetId UnreachableSystem = AmsNetId.Parse(
			Environment.GetEnvironmentVariable("ADS_TEST_UNREACHABLE") ?? "111.111.111.111.1.1"
		);

		public static bool SelfHostRouter =>
			Environment.GetEnvironmentVariable("ADS_TEST_SELFHOST_ROUTER") == "1";

		private static bool? _targetAvailable;

		/// <summary>
		/// Skips the calling test (via <see cref="Assert.Inconclusive(string)"/>) unless a
		/// TwinCAT system service is reachable at <see cref="TargetSystem"/>. The reachability
		/// probe runs once per test-run and is cached.
		/// </summary>
		public static void RequireTarget()
		{
			_targetAvailable ??= ProbeTarget();

			if (_targetAvailable != true)
			{
				Assert.Inconclusive(
					$"No reachable TwinCAT system service at {TargetSystem}:{(int)AmsPort.SystemService}. "
						+ "Set ADS_TEST_TARGET to a reachable target (and add an ADS route) to run the "
						+ "integration tests. See test/TwinCAT.Ads.Extensions.Tests/README.md."
				);
			}
		}

		private static bool ProbeTarget()
		{
			try
			{
				using AdsClient client = new AdsClient();
				client.Timeout = 2000;
				client.Connect(TargetSystem, (int)AmsPort.SystemService);
				return client.TryReadState(out _) == AdsErrorCode.NoError;
			}
			catch
			{
				return false;
			}
		}
	}
}
