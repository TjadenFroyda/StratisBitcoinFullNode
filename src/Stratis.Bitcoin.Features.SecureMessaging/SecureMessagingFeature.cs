using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stratis.Bitcoin.Builder; 
using Stratis.Bitcoin.Builder.Feature; 
using Stratis.Bitcoin.Features.Wallet; 
using Stratis.Bitcoin.Features.SecureMessaging.Controllers;
using Stratis.Bitcoin.Utilities;

// TODO: Add Logging
// TODO: Add/improve Comments
// TODO: Check coding style guide
// TODO: Safety checks
namespace Stratis.Bitcoin.Features.SecureMessaging
{
    /// <summary>
    /// Secure messaging feature.
    /// </summary>
    public class SecureMessagingFeature : FullNodeFeature 
    {

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger; 

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Stratis.Bitcoin.Features.TestFeature.SecureMessagingFeature"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        public SecureMessagingFeature(ILoggerFactory loggerFactory) 
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName); 
        }

        public override void Initialize()
        {
            this.logger.LogInformation("SecureMessagingFeature Started");
        }
    }

    /// <summary>
    /// Full node builder test feature extension.
    /// </summary>
    public static class FullNodeBuilderTestFeatureExtension
    {
        public static IFullNodeBuilder UseSecureMessagingFeature(this IFullNodeBuilder fullNodeBuilder) 
        {
            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<SecureMessagingFeature>()
                .DependOn<WalletFeature>() 
                .FeatureServices(services =>
                {
                    services.AddSingleton<SecureMessagingController>(); 
                });
            });
            return fullNodeBuilder;
        }
    }
}
