﻿namespace NServiceBus.AzureServiceBus.Addressing
{
    using Topology.MetaModel;

    class AddressingLogic
    {
        readonly ICompositionStrategy composition;
        readonly ISanitizationStrategy sanitizationStrategy;

        public AddressingLogic(ISanitizationStrategy sanitizationStrategy, ICompositionStrategy composition)
        {
            this.sanitizationStrategy = sanitizationStrategy;
            this.composition = composition;
        }

        public EntityAddress Apply(string value, EntityType entityType)
        {
            var address = new EntityAddress(value);
           
            var path = composition.GetEntityPath(address.Name, entityType);
            path = sanitizationStrategy.Sanitize(path, entityType);
            return new EntityAddress(path, address.Suffix);
        }
    }
    
}
