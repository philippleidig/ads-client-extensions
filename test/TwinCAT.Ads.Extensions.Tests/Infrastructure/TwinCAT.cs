using TwinCAT.Ads.SystemService;
using TwinCAT.Ads.TcpRouter;
using TwinCAT.Router;

namespace TwinCAT.Ads.Extensions.Tests.Infrastructure
{
    [TestClass]
    public static class GlobalTestSetup
    {
        private static CancellationTokenSource? _cancellationTokenSource;
        private static Task? _backgroundTask;

        private static TestLogger? _logger;

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            _logger = new TestLogger(context);

            // Only host an in-process AMS router when explicitly requested
            // (ADS_TEST_SELFHOST_ROUTER=1). When running against a real TwinCAT
            // system the machine already provides an AMS router on TCP 48898 and
            // hosting a second one would fail to bind. Note: the in-process server
            // provides AMS routing/discovery only, NOT the file system service, so
            // the file/directory tests still require a real TwinCAT target.
            if (!Globals.SelfHostRouter)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            _backgroundTask = Task.Run(() => BackgroundService(cancellationToken));
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();

            try
            {
                _backgroundTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // best-effort shutdown
            }

            _cancellationTokenSource.Dispose();
        }

        private static async Task BackgroundService(CancellationToken cancellationToken)
        {
            AmsTcpIpRouter router = new AmsTcpIpRouter(new AmsNetId("111.111.111.111.1.1"));

            Task routerTask = router.StartAsync(cancellationToken);

            AdsRouterServer adsRouterService = new AdsRouterServer(router, _logger);

            SystemServiceServer systemService = new SystemServiceServer(router, _logger);

            Task systemServiceTask = systemService.ConnectServerAndWaitAsync(cancellationToken);
            Task routerServerTask = adsRouterService.ConnectServerAndWaitAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAll(routerTask, systemServiceTask, routerServerTask);
            }
        }
    }
}
