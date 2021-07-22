using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facade
{
    public class Repeater
    {
        private static Repeater _repeater;
        private Repeater()
        {

        }

        static Repeater getInstance()
        {
           return _repeater ??= new Repeater();
        }



    }
}
