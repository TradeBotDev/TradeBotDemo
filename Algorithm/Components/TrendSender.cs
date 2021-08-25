using Algorithm.Components;
using Algorithm.Components.Publishers;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using TradeBot.Former.FormerService.v1;

using static TradeBot.Former.FormerService.v1.FormerService;

namespace Algorithm.DataManipulation
{
    //sends the price to Former
    //all the values are hardcoded for now 
    public class TrendSender : ITrendSender
    {
        private DecisionPublisher _decisionPublisher;
        private Metadata _metadata;
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("FORMER_CONNECTION_STRING"));
        private static readonly FormerServiceClient Client = new FormerServiceClient(Channel);

        public TrendSender(DecisionPublisher decisionPublisher, Metadata metadata)
        {
            _decisionPublisher = decisionPublisher;
            _decisionPublisher.DecisionMadeEvent += SendTrend;
            _metadata = metadata;

        }

        public void SendTrend (int decision)
        {
            var response = Client.SendAlgorithmDecision(new SendAlgorithmDecisionRequest { Decision = decision }, _metadata);
            Log.Information("{@Where}:Sent " + decision + " for user {@User}", "Algorithm", _metadata.GetValue("sessionid"));
        }
    }
}
