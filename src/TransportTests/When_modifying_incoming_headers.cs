﻿namespace NServiceBus.AzureServiceBus.TransportTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.TransportTests;
    using NUnit.Framework;
    using Transport;

    // TODO: remove these tests when pulled into TransportTests
    public class When_modifying_incoming_headers : NServiceBusTransportTest
    {
        [TestCase(TransportTransactionMode.None)]
        [TestCase(TransportTransactionMode.ReceiveOnly)]
        [TestCase(TransportTransactionMode.SendsAtomicWithReceive)]
        [TestCase(TransportTransactionMode.TransactionScope)]
        public async Task Should_roll_back_header_modifications_for_immediate_retries(TransportTransactionMode transactionMode)
        {
            var messageRetries = new TaskCompletionSource<MessageContext>();
            var firstInvocation = true;

            await StartPump(context =>
            {
                if (firstInvocation)
                {
                    context.Headers["test-header"] = "modified";
                    firstInvocation = false;
                    throw new Exception();
                }

                messageRetries.SetResult(context);
                return Task.FromResult(0);
            },
                context => Task.FromResult(ErrorHandleResult.RetryRequired),
                transactionMode);

            await SendMessage(InputQueueName, new Dictionary<string, string>
            {
                {"test-header", "original"}
            });

            var retriedMessage = await messageRetries.Task;

            Assert.AreEqual("original", retriedMessage.Headers["test-header"]);
        }

        [TestCase(TransportTransactionMode.None)]
        [TestCase(TransportTransactionMode.ReceiveOnly)]
        [TestCase(TransportTransactionMode.SendsAtomicWithReceive)]
        [TestCase(TransportTransactionMode.TransactionScope)]
        public async Task Should_roll_back_header_modifications_before_handling_error(TransportTransactionMode transactionMode)
        {
            var errorHandled = new TaskCompletionSource<ErrorContext>();

            await StartPump(context =>
            {
                context.Headers["test-header"] = "modified";
                throw new Exception();
            },
                context =>
                {
                    errorHandled.SetResult(context);
                    return Task.FromResult(ErrorHandleResult.Handled);
                },
                transactionMode);

            await SendMessage(InputQueueName, new Dictionary<string, string>
            {
                {"test-header", "original"}
            });

            var errorContext = await errorHandled.Task;

            Assert.AreEqual("original", errorContext.Message.Headers["test-header"]);
        }

        [TestCase(TransportTransactionMode.None)]
        [TestCase(TransportTransactionMode.ReceiveOnly)]
        [TestCase(TransportTransactionMode.SendsAtomicWithReceive)]
        [TestCase(TransportTransactionMode.TransactionScope)]
        public async Task Should_roll_back_header_modifications_from_handling_error(TransportTransactionMode transactionMode)
        {
            var messageRetries = new TaskCompletionSource<MessageContext>();
            var firstInvocation = true;

            await StartPump(context =>
            {
                if (firstInvocation)
                {
                    firstInvocation = false;
                    throw new Exception();
                }

                messageRetries.SetResult(context);
                return Task.FromResult(0);
            },
                context =>
                {
                    context.Message.Headers["test-header"] = "modified";
                    return Task.FromResult(ErrorHandleResult.RetryRequired);
                },
                transactionMode);

            await SendMessage(InputQueueName, new Dictionary<string, string>
            {
                {"test-header", "original"}
            });

            var retriedMessage = await messageRetries.Task;

            Assert.AreEqual("original", retriedMessage.Headers["test-header"]);
        }
    }
}