using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Blockchains.BasicBaseChain;
using AElf.Cryptography;
using AElf.Kernel;
using AElf.Kernel.Account.Application;
using AElf.Kernel.Consensus.Application;
using AElf.Kernel.SmartContract;
using AElf.Kernel.SmartContract.Application;
using AElf.Kernel.TransactionPool.Infrastructure;
using AElf.Modularity;
using AElf.OS.Network.Application;
using AElf.OS.Network.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Volo.Abp.Modularity;

namespace AElf.Contracts.TestBase
{
    [DependsOn(
        typeof(BasicBaseChainAElfModule),
        typeof(KernelTestAElfModule)
    )]
    public class ContractTestAElfModule : AElfModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var mockService = new Mock<IWebHostEnvironment>();
            mockService.SetupGet(m => m.ContentRootPath).Returns("");
            context.Services.AddSingleton(typeof(IWebHostEnvironment), mockService.Object);
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            services.AddSingleton(o => Mock.Of<IAElfNetworkServer>());
            services.AddSingleton(o => Mock.Of<IPeerPool>());

            services.AddSingleton(o => Mock.Of<INetworkService>());

            // When testing contract and packaging transactions, no need to generate and schedule real consensus stuff.
            context.Services.AddSingleton(o => Mock.Of<IConsensusService>());
            context.Services.AddSingleton(o => Mock.Of<IConsensusScheduler>());

            context.Services.AddSingleton<ITxHub, MockTxHub>();

            Configure<ContractOptions>(options => { options.IsTxExecutionTimeoutEnabled = false; });

            var ecKeyPair = CryptoHelper.GenerateKeyPair();

            context.Services.AddTransient(o =>
            {
                var mockService = new Mock<IAccountService>();
                mockService.Setup(a => a.SignAsync(It.IsAny<byte[]>())).Returns<byte[]>(data =>
                    Task.FromResult(CryptoHelper.SignWithPrivateKey(ecKeyPair.PrivateKey, data)));

                mockService.Setup(a => a.GetPublicKeyAsync()).ReturnsAsync(ecKeyPair.PublicKey);

                return mockService.Object;
            });
            
            context.Services.RemoveAll<IPreExecutionPlugin>();
            
            Configure<ContractOptions>(options =>
            {
                options.ContractFeeStrategyAcsList = new List<string>{"acs1"};
            });
        }
    }
}