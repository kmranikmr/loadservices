﻿using Akka.Actor;
using DataAnalyticsPlatform.Actors.messages;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Preview
{
    public class PreviewsActor : ReceiveActor
    {
       
      // private IActorRef ProductActor { get; }
      private PreviewRegistry previewRegistry { get; set; }
        public PreviewsActor(PreviewRegistry previewRegistry)
        {
            // this.ProductActor = productActor;
            this.previewRegistry = previewRegistry;
             ReceiveAny(m => {
                if (m is MsgUserId)
                {
                     Console.WriteLine(" previews actor ");
                    var envelope = m as MsgUserId;
                    var previewActor = Context.Child(envelope.userId.ToString()) is Nobody ?
                        Context.ActorOf(Props.Create(() => new PreviewActor(previewRegistry)), envelope.userId.ToString()) :
                        Context.Child(envelope.userId.ToString());
                    previewActor.Forward(m);
                } 
            });
        }


    }
}
