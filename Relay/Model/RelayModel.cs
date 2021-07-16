using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Algorithm.AlgorithmService.v1;
using TradeBot.Common.v1;

namespace Relay.Model
{
    public class RelayModel
    {

        #region Singleton

        private static RelayModel _model;

        public static RelayModel GetInstance()
        {
            if (_model is null)
            {
                //TODO каждому рилею по новому идшнику !!!
                _model = new RelayModel(Guid.NewGuid().ToString());
            }

            return _model;
        }


        #endregion


        #region Model

        public String Id { get; }
        public bool IsOn { get; set; } = false;
        public Config Config { get; set; } = null;

        public RelayModel(string id = null)
        {
            this.Id = id;
        }

        /// <summary>
        /// переключает режим работы рилея на противоположный.
        /// </summary>
        public void Turn()
        {
            IsOn = !IsOn;
        }

        #endregion

    }
}
