using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public static class ActionHelper
    {
        public static long getCurrentTotalInvestmentFromActionString(string action, List<Player> players, int actingPlayerId) {

            long currentPlayerInvesment = players[actingPlayerId].investedInCurrentGame;
            long otherPlayerInvestment = players.Find(x => x.id != actingPlayerId).investedInCurrentGame;


            if (action == "c") {
                return otherPlayerInvestment;
            }
            else if (action == "f") {
                return currentPlayerInvesment;
            }
            else if ((action[0] == 'b') || (action[0] == 'r')) {

                int betSize = Int32.Parse(action.Substring(1));
                return currentPlayerInvesment + betSize;
            }
            throw new Exception("Unexpected action");

        }

    }
}
