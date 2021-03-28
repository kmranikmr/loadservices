using Akka.Actor;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Registry
{
    public class RegistryActor : ReceiveActor
    {
        private PreviewRegistry _previewRegistry;
        public RegistryActor(PreviewRegistry previewRegistry)
        {
            _previewRegistry = previewRegistry;
            Receiving();
        }
        
    }
}
