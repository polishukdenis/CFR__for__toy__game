using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public class Player
    {
        public int id;
        public int hand;
        public long investedInCurrentGame = 0;
        public long totalWinnings = 0;
        public List<StrategyItem> strategy;
        public string lastAction = "";


        public Player(int id)
        {
            this.id = id;
        }

        public void initStrategy(Tree tree) { 
        
            strategy = new List<StrategyItem>();

            TreeNode baseNode = tree.getBaseNode();

            initStrategyForNode(baseNode);
            /*
            if (id == 0) {
                StrategyItem si = strategy.Find(x => x.actionName == "c" && x.hand == 2 && x.nodeId == "r:0:c:b1000");
                si.actionPct = 0.5001;
                StrategyItem si2 = strategy.Find(x => x.actionName == "f" && x.hand == 2 && x.nodeId == "r:0:c:b1000");
                si2.actionPct = 0.4999;
            }*/
            
            ;
        }

        public void initStrategyForNode(TreeNode node) {
            List<string> actions = node.getPossibleActions();

            if (node.getActingPlayerId() == this.id) {
                foreach (string action in actions)
                {
                    foreach(int hand in GameStructure.getPossibleHands())
                    {
                        StrategyItem item = new StrategyItem();
                        double pct = (double) 1 / actions.Count;
                        item.actionName = action;
                        item.actionPct = pct;
                        item.hand = hand;

                        item.nodeId = node.getId();// + ":" + action;

                        strategy.Add(item);
                    }
                }
            }

            foreach(TreeNode child in node.getChildren())
            {
                initStrategyForNode(child);
            }

        }

        public string pickActionAccordingToStrategy(TreeNode node) {

            List<string> actions = node.getPossibleActions();

            if (actions.Count == 1)
            {
                return actions.First();
            }
            else if (actions.Count > 0) { 
                Dictionary<string, double> actionsToPct = new Dictionary<string, double>();
                foreach (string action in actions)
                {
                    actionsToPct[action] = strategy.Find(x => x.actionName == action && x.nodeId == node.getId() && x.hand == hand).actionPct;
                }
                string resAction = chooseAction(actionsToPct);
                return resAction;
            }


            throw new Exception("Cannot pick action in terminal node");
        }


        private static Random rnd = new Random();

        public static string chooseAction(Dictionary<string, double> actionsWithProbabilities)
        {
            double randomPoint = rnd.NextDouble();

            //File.AppendAllText(Solver.logPath, "randomPoint= " + randomPoint + Environment.NewLine);

            double cumulativeProbability = 0.0;
            foreach (var action in actionsWithProbabilities)
            {
                cumulativeProbability += action.Value;
                if (randomPoint < cumulativeProbability)
                {
                    return action.Key;
                }
            }

            throw new Exception("Error when picking an action");
        }


        public void updateInvestments(TreeNode node, string action) {

            investedInCurrentGame = ActionHelper.getCurrentTotalInvestmentFromActionString(action, Solver.players, this.id);

        }

        public double getActionProbability(TreeNode node, string action, int hand) {
            StrategyItem item = strategy.Find(x=> x.nodeId == node.getId() && x.actionName == action && x.hand == hand);
            return item.actionPct;
        }

        public Boolean isHandInRange(int hand) {

            if (id == 0)
            {
                return GameStructure.player0Range.Contains(hand);
            }
            else {
                return GameStructure.player1Range.Contains(hand);
            }
        }

    }
}
