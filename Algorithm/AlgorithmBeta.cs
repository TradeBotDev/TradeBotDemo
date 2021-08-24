using Algorithm.Components;
using Algorithm.Components.Publishers;
using Algorithm.DataManipulation;
using Algorithm.Services;
using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;

namespace Algorithm.Analysis
{
    public class AlgorithmBeta
    {
        //This algo looks back several seconds (set by user) and looks for price peaks and pits 
        //we analyse the trend by calculating SMAs (simple moving average) of several subsets withing the timeframe
        //if the general price trend is rising/falling, but the current price is doing the opposite we conclude that this might be a local pit or a peak
        //pits are good for buying (lowest price we could find)
        //peaks are good for selling (highest price we could find)
        //this decision is then sent to Former service which does what it can with it 

        //algo components
        private DataCollector _dc;
        private PointMaker _pm;
        private DecisionMaker _dm;
        private ITrendSender _ps;

        //publishers to coordinate the components
        private PointPublisher _pointPublisher;
        private DecisionPublisher _decisionPublisher;

        private bool _isStopped = true;
        private int _precision = 0;

        private Metadata _metadata;

        public bool GetState()
        {
            return _isStopped;
        }

        //when an algo is created it's immediately subscribed to new points 
        public AlgorithmBeta(Metadata metadata)
        {
            _metadata = metadata;
            _pointPublisher = new();
            _decisionPublisher = new();

            _dm = new(_decisionPublisher, _pointPublisher);
            _dc = new(_pointPublisher);
            _pm = new();
            _ps = new TrendSender(_decisionPublisher, _metadata);

            Log.Information("{@Where}: Algorithm for user {@User} has been created", "Algorithm", metadata.GetValue("sessionid"));
        }

        public void NewOrderAlert(OrderWrapper order, Metadata metadata)
        {
            if (metadata.GetValue("slot") == _metadata.GetValue("slot"))
            {
                _dc.AddNewOrder(order);
            }
        }       

        public void ChangeSetting(AlgorithmInfo settings)
        {
            _precision = settings.Sensitivity;
            _pm.SetPointInterval((int)settings.Interval.Seconds * 1000 / 5);
            _dc.ClearAllData();
            Log.Information("{@Where}:Settings changed", "Algorithm");
        }
        public void ChangeState()
        {
            if (_isStopped)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
        private void Stop()
        {
            _isStopped = true;
            Log.Information("{@Where}:Algorithm has been stopped", "Algorithm");
            _pm.Stop();
        }

        private void Start()
        {
            _isStopped = false;
            Log.Information("{@Where}:Algorithm has been launched", "Algorithm");
            _pm.Launch(_pointPublisher, _dc);
        }
    }
}
