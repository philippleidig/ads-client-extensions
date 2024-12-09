using TwinCAT.Ads.SystemService;
using TwinCAT.Ads.TcpRouter;
using TwinCAT.Router;

namespace TwinCAT.Ads.Extensions.Tests.Infrastructure
{
    [TestClass]
    public static class GlobalTestSetup
    {
        private static CancellationTokenSource _cancellationTokenSource;
        private static Task _backgroundTask;

        private static TestLogger _logger;

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            _backgroundTask = Task.Run(() => BackgroundService(cancellationToken));

            _logger = new TestLogger(context);  
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            _cancellationTokenSource.Cancel();
            _backgroundTask.Wait(); 
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
