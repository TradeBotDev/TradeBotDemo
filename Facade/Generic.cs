using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Facade
{
    public static class Generalization
    {
        public static async Task<T> ReturnResponse<T>(IMessage<T> message, string methodName) where T : IMessage<T>
        {
            try
            {
                Log.Information("{@Where}: {@MethodName} \n args: response: {@response}\n", "Facade", methodName,message);
                return await Task.FromResult((T)message);
            }
            catch (Exception e)
            {
                Log.Error("{@Where}: {@MethodName}-Exception: {@Exception}\n", "Facade", methodName, e);
                throw;
            }
        }

        public static async Task StreamReadWrite<TMessage, TResponse, TRequest>
            (IMessage<TMessage> message,
            IServerStreamWriter<TResponse> responseStream,
            AsyncServerStreamingCall<TRequest> request,
            ServerCallContext context,
            string methodName) where TMessage : IMessage<TMessage>
        {
            try
            {
                while (await request.ResponseStream.MoveNext(context.CancellationToken))
                {
                    Log.Information("{@Where}: {@MethodName} \n args: response={@response}\n", "Facade", methodName, request.ResponseStream.Current);

                    await responseStream.WriteAsync((TResponse)message);
                }
            }
            catch (Exception e)
            {
                Log.Information("{@Where}: {@MethodName}-Exception: {@Exception}\n", "Facade", methodName, e.Message);
                throw;
            }
        }
        public static async Task ConnectionTester<TRequest>(Func<Task> func, string methodName, IMessage<TRequest> request=null) where TRequest: IMessage<TRequest>
        {
            Log.Information("{@Where}: {@MethodName} \n args: request={@response}\n", "Facade", methodName, request);
            while (true)
            {
                try
                {
                    await func.Invoke();
                    break;
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Cancelled) break;
                    Log.Error("{@Where}: Error {@ExceptionMessage}. Retrying...\r\n{@ExceptionStackTrace}\n", "Facade", e.Message, e.StackTrace);
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
