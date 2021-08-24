using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.Components.Publishers
{
    public class DecisionPublisher
    {
        public delegate void DecisionMade(int decision);
        public DecisionMade DecisionMadeEvent;

        public void Publish(int decision)
        {
            DecisionMadeEvent?.Invoke(decision);
        }
    }
}
