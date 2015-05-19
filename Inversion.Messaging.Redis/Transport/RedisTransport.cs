﻿using System;
using StackExchange.Redis;

using Inversion.Data.Redis;
using Inversion.Messaging.Model;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public class RedisTransport : RedisStore, ITransport
    {
        private readonly string _queueName;

        public RedisTransport(string connections, int databaseNumber, string queueName)
            : base(connections, databaseNumber)
        {
            _queueName = queueName;
        }

        public RedisTransport(ConnectionMultiplexer connectionMultiplexer, int databaseNumber) : base(connectionMultiplexer, databaseNumber) {}

        public void Push(IEvent ev)
        {
            this.Database.ListRightPush(_queueName, ev.ToJson());
        }

        public IEvent Pop()
        {
            string result = this.Database.ListLeftPop(_queueName);
            
            if (String.IsNullOrEmpty(result))
            {
                return null;
            }

            return this.ConvertDocumentToEvent(result);
        }

        public IEvent Peek()
        {
            string result = this.Database.ListGetByIndex(_queueName, 0);

            if (String.IsNullOrEmpty(result))
            {
                return null;
            }

            return this.ConvertDocumentToEvent(result);
        }

        public long Count()
        {
            return this.Database.ListLength(_queueName);
        }

        protected IEvent ConvertDocumentToEvent(string source)
        {
            return MessagingEvent.FromJson(null, source);
        }
    }
}