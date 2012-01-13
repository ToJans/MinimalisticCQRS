using Ninject;
using SignalR.Infrastructure;
using SignalR.Ninject;
using MinimalisticCQRS.Infrastructure;
using MinimalisticCQRS.Hubs;

[assembly: WebActivator.PreApplicationStartMethod(typeof(MinimalisticCQRS.App_Start.NinjectSignalR), "Start")]

namespace MinimalisticCQRS.App_Start {
    public static class NinjectSignalR {
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() {
            IKernel kernel = CreateKernel();
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel() {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel) {
            var bus = new MiniBus();
            bus.RegisterArType<Account>();
            var qhub = new QueryHub();
            var chub = new CommandHub(bus);
            bus.RegisterNonArInstance(qhub,new AccountUniquenessSaga(),new AccountTransferSaga(bus));
            kernel.Bind<CommandHub>().ToConstant(chub);
            kernel.Bind<QueryHub>().ToConstant(qhub);
        }
    }
}