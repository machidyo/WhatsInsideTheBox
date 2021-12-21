using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePipe;
using VContainer;
using VContainer.Unity;

public class MessagePipeLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var options = builder.RegisterMessagePipe();

        // hello 1
        builder.RegisterMessageBroker<int>(options);
        builder.RegisterComponentInHierarchy<MessagePipeTester>();
        
        // hello 2
        builder.RegisterMessageBroker<PlayerAttackData>(options);
        builder.RegisterComponentInHierarchy<Player>();
        builder.RegisterComponentInHierarchy<Enemy>();
    }
}
