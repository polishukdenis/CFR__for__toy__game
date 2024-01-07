using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public static class GameStructure
    {
        public static long startPot = 100;
        public static List<int> getPossibleHands() {
            return new List<int>() { 1, 2, 3 };
        }

        public static List<int> player0Range = new List<int>() { 2 };
        public static List<int> player1Range = new List<int>() { 1, 3 };
    }
}
