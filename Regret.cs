using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public class Regret
    {
        public static List<Regret> allRegrets;

       
        public string nodeId;
        public int playerId;
        public string action;
        public int hand;
        public double value = 0;
        public long iterationNumber = 0;

    }
}
