using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public class TreeNode
    {
        private string nodeId;
        private List<TreeNode> children = new List<TreeNode>();

        private Boolean isTerminal;
        private int actingPlayerId;
        private int prevPlayerActed;

        private List<Regret> regrets = new List<Regret>();

        public TreeNode(string nodeId, int actingPlayerId, int prevPlayerActed, Boolean isTerminal = false)
        {
            this.nodeId = nodeId;
            this.isTerminal = isTerminal;
            this.actingPlayerId = actingPlayerId;
            this.prevPlayerActed = prevPlayerActed;
        }

        public TreeNode addChild(string action, int actingPlayerId, int prevPlayerActed, Boolean isTerminal = false)
        {
            string childNodeId = nodeId + ":" + action;
            TreeNode childNode = new TreeNode(childNodeId, actingPlayerId, prevPlayerActed, isTerminal);
            children.Add(childNode);
            return childNode;
        }

        public void initRegrets() {
            List<string> actions = this.getPossibleActions();

            foreach (string action in actions)
            {
                foreach (int hand in GameStructure.getPossibleHands()) {
                    Regret r = new Regret();
                    r.nodeId = nodeId;
                    r.action = action;
                    r.value = 0;
                    r.playerId = actingPlayerId;
                    r.hand = hand;

                    regrets.Add(r);
                }

            }

            foreach (TreeNode node in children) {
                if (!node.getIsTerminal()) {
                    node.initRegrets();
                }
            }
        }

        public List<Regret> getRegrets() {
            return this.regrets;
        } 




        public string getId() {
            return nodeId;
        }

        public List<TreeNode> getChildren()
        {
            return children;
        }


        public List<string> getPossibleActions() { 
            
            List<string> res = new List<string>();

            foreach (TreeNode childNode in children)
            {
                string nodeId = childNode.getId();
                string action = nodeId.Substring(nodeId.LastIndexOf(":") + 1);

                res.Add(action);
            }
        
            return res;
        }

        public int getActingPlayerId()
        {
            return actingPlayerId;
        }

        public Boolean getIsTerminal() {
            return isTerminal;
        }

        public string getLastAction() { 
            
            int index = this.nodeId.LastIndexOf(':');
            return nodeId.Substring(index + 1);
        }

        public double[] getPlayersUtilitiesInTerminalNode() {

            if (!isTerminal) {
                throw new Exception("Not a terminal node");
            }

            double[] res = new double[2];

            long maxBet = getMaxBetFromNodeString();
            long prevBet = getPreMaxBetFromNodeString();


            if (isPlayerFolded(0))
            {
                //There's got to be a raise or a bet in order for a player to fold. So his investment will be equal to previous bet in the node string.
                long p1Investment = prevBet == -1 ? 0 : prevBet;
                res[0] = -p1Investment;
                res[1] = p1Investment + GameStructure.startPot;
            }
            else if (isPlayerFolded(1))
            {
                long p2Investment = prevBet == -1 ? 0 : prevBet;
                res[0] = p2Investment + GameStructure.startPot;
                res[1] = -p2Investment;
            }
            else 
            {
                //we have a call 

                Player p1 = Solver.players[0];
                Player p2 = Solver.players[1];

                if(maxBet == -1) {
                    maxBet = 0;
                }

                if (p1.hand < p2.hand)
                {
                    res[0] += maxBet + GameStructure.startPot;
                    res[1] -= maxBet;
                }
                else
                {
                    res[1] += maxBet + GameStructure.startPot;
                    res[0] -= maxBet;
                }
            }


            return res;
        }

        public Boolean isPlayerFolded(int playerId) {

            int indexOfLastAction = nodeId.LastIndexOf(":");
            string lastAction = nodeId.Substring(indexOfLastAction + 1);

            if (isTerminal && prevPlayerActed == playerId && lastAction == "f")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public long getMaxBetFromNodeString() {

            string pattern = @"[br]\d+";

            var matches = Regex.Matches(nodeId, pattern);

            if (matches.Count > 0)
            {
                // Извлекаем последнее совпадение
                var lastMatch = matches[matches.Count - 1].Value;
                return long.Parse(lastMatch.Substring(1));
            }
            else
            {
                return -1;
            }
        }


        public long getPreMaxBetFromNodeString()
        {

            string pattern = @"[br]\d+";

            var matches = Regex.Matches(nodeId, pattern);

            if (matches.Count > 1)
            {
                var preLastMatch = matches[matches.Count - 2].Value;
                return long.Parse(preLastMatch.Substring(1));
            }
            else
            {
                return -1;
            }
        }

        public void updateRegrets(double additionTerm, string actionName) { 

            Regret r = regrets.Find(x=> x.action == actionName && x.hand == Solver.players[actingPlayerId].hand);
            long itNum = r.iterationNumber; //Solver.currentIterationNumber;
            r.value = ((double)1 / (itNum + 1)) * (itNum * r.value + additionTerm);
            r.iterationNumber++;
            //;
        }

    }
}
