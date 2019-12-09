using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SampleActor.Interfaces;

namespace Default.MService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MService : StatefulService
    {
        private const string ACTOR_COLLECTION = "actorCollection";
        private const string ACTOR_TYPE_COLLECTION = "actorTypeCollection";
        private const int ACTOR_CREATION_TIMEOUT = 60; // in secs

        public MService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myTypeDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>(ACTOR_TYPE_COLLECTION);
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, string>>(ACTOR_COLLECTION);
        }

        public async Task<Guid> CreateAsync(string type, CancellationToken cancellationToken)
        {
            var myTypeDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>(ACTOR_TYPE_COLLECTION);
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, string>>(ACTOR_COLLECTION);
            ActorId key = ActorId.CreateRandom();

            // register actor if type doesn't exist
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                if (!await myTypeDictionary.ContainsKeyAsync(tx, type))
                {
                    ActorRuntime.RegisterActorAsync<SampleActor.SampleActor>(
                               (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();
                    await myTypeDictionary.TryAddAsync(tx, type, true);
                }
            }

            // Create actor
            ActorProxy.Create<ISampleActor>(key, $"fabric:/Default.Main/{type}ActorService");

            // Add to collection
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await myDictionary.AddAsync(tx, key.GetGuidId(), type, TimeSpan.FromMilliseconds(ACTOR_CREATION_TIMEOUT * 1000), cancellationToken);
                await tx.CommitAsync();
            }
            return key.GetGuidId();
        }
        //public async Task<int> ReadAsync(Guid id, string value_key, CancellationToken cancellationToken)
        //{
        //    var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, string>>(ACTOR_COLLECTION);
        //    Guid key = Guid.NewGuid();
        //    using (ITransaction tx = this.StateManager.CreateTransaction())
        //    {
        //        await myDictionary.TryGetValueAsync(tx, key, type, TimeSpan.FromMilliseconds(ACTOR_CREATION_TIMEOUT * 1000), cancellationToken);
        //        await tx.CommitAsync();
        //    }
        //    return key;
        //}
    }
}
