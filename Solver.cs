using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public static class Solver
    {
        public static List<Player> players = new List<Player>();
        public static List<Regret> allRegrets = new List<Regret>();
        public static Tree tree = new Tree();
        

        public static long iterationsNumber = 1000 * 1000 * 100;
        public static string logPath = "log.txt";
        public static string regretDumpPath = "regrets.txt";
        public static int currentIterationNumber = 0;
        private static Random rnd = new Random();


        public static void init()
        {
            players.Add(new Player(0));
            players.Add(new Player(1));

            tree.init();
            tree.dumpTree();
            tree.initRegrets();

            players[0].initStrategy(tree);
            players[1].initStrategy(tree);

            File.Delete(logPath);
            var myFile = File.Create(logPath);
            myFile.Close();

            File.Delete(regretDumpPath);
            myFile = File.Create(regretDumpPath);
            myFile.Close();

            currentIterationNumber = 0;

        }

        public static void solve() { 

            for(currentIterationNumber = 0; currentIterationNumber < iterationsNumber; currentIterationNumber++)
            {
                //playHand();
                //updateRegrets();
                //updateStrategies();


                dealCards();
                double[] utilities = walkCFR(tree.getBaseNode(), 1, 1);

                double p1Utility = utilities[0];
                double p2Utility = utilities[1];


                if(currentIterationNumber % 1000 == 0)
                {
                    File.AppendAllText(regretDumpPath, "i = " + currentIterationNumber + Environment.NewLine);
                    File.AppendAllText(regretDumpPath, "cards p0 = " + players[0].hand + "; cards p1 = " + players[1].hand + Environment.NewLine);
                    File.AppendAllText(regretDumpPath, "u1 = " + utilities[0] + "; u2 = " + utilities[1] + Environment.NewLine);
                    dumpRegrets(tree.getBaseNode());
                    File.AppendAllText(regretDumpPath, Environment.NewLine);
                }
            }

            File.AppendAllText(logPath, iterationsNumber.ToString() + " iterations completed" + Environment.NewLine);
            File.AppendAllText(logPath, "p0 result: " + players[0].totalWinnings + Environment.NewLine);
            File.AppendAllText(logPath, "p1 result: " + players[1].totalWinnings + Environment.NewLine);
        }

        private static void dumpRegrets(TreeNode node) {

            if (!node.getIsTerminal()) {

                File.AppendAllText(regretDumpPath, node.getId() + " p="+ node.getActingPlayerId() + Environment.NewLine);
                if (node.getChildren().Count > 1) {
                    foreach (Regret r in node.getRegrets().OrderBy(x=>x.hand).ToList()) {
                        if (players[r.playerId].isHandInRange(r.hand))
                        {
                            StrategyItem strategyItem = players[r.playerId].strategy.Find(x => x.nodeId == r.nodeId && x.actionName == r.action && r.hand == x.hand);

                            String nodeString = r.nodeId + "->" + r.action;
                            nodeString = nodeString.PadRight(20, ' ');

                            File.AppendAllText(regretDumpPath,  nodeString +
                                 strategyItem.getPctReadable() +" (avg" + strategyItem.getAvgActionPct() + " )" + " h " + r.hand + " p " + r.playerId + " " + r.value + Environment.NewLine);
                        }
                    }
                }
                foreach (TreeNode child in node.getChildren()) {
                    dumpRegrets(child);
                }
            }
        }


        public static double[] walkCFR(TreeNode node, double p1, double p2)
        {
            double[] res = new double[2] { 0, 0 };


            if (node.getIsTerminal())
            {
                res = node.getPlayersUtilitiesInTerminalNode();

            } else {
                if (node.getActingPlayerId() == 0) {

                    updateStrategies(node);

                    double u0 = 0;
                    double u1 = 0;

                    Dictionary<string, double> childUtilsBuffer = new Dictionary<string, double>();
                    foreach (TreeNode childNode in node.getChildren())
                    {

                        string childAction = childNode.getLastAction();
                        string childActionName = childAction.Substring(0, 1);
                        double actionProb = players[0].getActionProbability(node, childAction, players[0].hand);

                        double[] childUtils = walkCFR(childNode, p1 * actionProb, p2);

                        u0 += childUtils[0] * actionProb;
                        u1 += childUtils[1] * actionProb;

                        childUtilsBuffer[childActionName] = childUtils[0];
                    }
                    res[0] = u0;
                    res[1] = u1;

                    if (node.getChildren().Count > 1) {
                        foreach (TreeNode childNode in node.getChildren())
                        {
                            string childAction = childNode.getLastAction();
                            string childActionName = childAction.Substring(0, 1);

                            node.updateRegrets(p2 * (childUtilsBuffer[childActionName] - u0), childActionName);
                        }
                    }

                    

                }
                else {

                    updateStrategies(node);

                    double u1 = 0;
                    double u0 = 0;

                    Dictionary<string, double> childUtilsBuffer = new Dictionary<string, double>();
                    foreach (TreeNode childNode in node.getChildren()) {
                        
                        string childAction = childNode.getLastAction();
                        string childActionName = childAction.Substring(0, 1);
                        double actionProb = players[1].getActionProbability(node, childAction, players[1].hand);

                        double[] childUtils = walkCFR(childNode, p1, p2 * actionProb);

                        u1 += childUtils[1]*actionProb;
                        u0 += childUtils[0]*actionProb;

                        childUtilsBuffer[childActionName] = childUtils[1];
                    }
                    res[1] = u1;
                    res[0] = u0;

                    if (node.getChildren().Count > 1)
                    {
                        foreach (TreeNode childNode in node.getChildren())
                        {

                            string childAction = childNode.getLastAction();
                            string childActionName = childAction.Substring(0, 1);

                            node.updateRegrets(p1 * (childUtilsBuffer[childActionName] - u1), childAction);
                        }
                    }

                    
                }
            }

            return res;
        }


        public static void playHand() {

            players[0].investedInCurrentGame = GameStructure.startPot / 2;
            players[1].investedInCurrentGame = GameStructure.startPot / 2;

            dealCards();

            TreeNode currentNode = tree.getBaseNode();

            while(currentNode.getChildren().Count > 0)
            {
                currentNode = playNode(currentNode);
            }

            calcIncomes(currentNode);
        }

        public static void calcIncomes(TreeNode node) {

            Player p1 = players[0];
            Player p2 = players[1];

            string p1LastAction = p1.lastAction;
            string p2LastAction = p2.lastAction;


            if (p1LastAction == "f")
            {
                p1.totalWinnings -= p1.investedInCurrentGame;
                p2.totalWinnings += p1.investedInCurrentGame;
            }
            else if (p2LastAction == "f")
            {
                p2.totalWinnings -= p2.investedInCurrentGame;
                p1.totalWinnings += p2.investedInCurrentGame;
            }
            else {
                if (p1.hand < p2.hand)
                {
                    p1.totalWinnings += p2.investedInCurrentGame;
                    p2.totalWinnings -= p2.investedInCurrentGame;
                }
                else {
                    p2.totalWinnings += p1.investedInCurrentGame;
                    p1.totalWinnings -= p1.investedInCurrentGame;
                }
            }

            File.AppendAllText(logPath, "i= "+ currentIterationNumber + 
                "; node " + node.getId() + "; p1 " + players[0].totalWinnings + "; p2 " + players[1].totalWinnings +
                "; p1 hand " + p1.hand + "; p2 hand " +p2.hand + Environment.NewLine);
        }

        public static TreeNode playNode(TreeNode node) {

            Player currentPlayer = players[node.getActingPlayerId()];

            string action = currentPlayer.pickActionAccordingToStrategy(node);
            currentPlayer.lastAction = action.Substring(0, 1);
            currentPlayer.updateInvestments(node, action);


            TreeNode nextNode = node.getChildren().Find(x => x.getId() == node.getId() + ':' + action);

            return nextNode;
        }


        public static void updateRegrets()
        {

        }
        public static void updateStrategies(TreeNode node)
        {
            Player currPlayer = players[node.getActingPlayerId()];
            List<StrategyItem> strategyItems = currPlayer.strategy.FindAll(x=>x.nodeId == node.getId() && x.hand == currPlayer.hand);

            List<Regret> regrets = node.getRegrets().FindAll(x=> x.hand == currPlayer.hand);

            Dictionary<string, double> positiveRegrets = new Dictionary<string, double>();
            double regretsSum = 0;
            int regretsAmount = regrets.Count();

            foreach (Regret regret in regrets) {
                positiveRegrets[regret.action] = regret.value > 0 ? regret.value : 0;
                regretsSum += positiveRegrets[regret.action];
            }

            foreach (StrategyItem strategyItem in strategyItems)
            {
                double newValue = 0;
                if (regretsSum > 0)
                {
                    newValue = positiveRegrets[strategyItem.actionName] / regretsSum; 
                    
                }
                else {
                    newValue = (double)1 / regretsAmount;
                    
                }

                strategyItem.actionPct = newValue;
                strategyItem.actionPctSum += newValue;
                strategyItem.actionPctIterations++;
            }
        }


        public static void dealCards()
        {

            if (currentIterationNumber == 0)
            {

                players[0].hand = 2;
                players[1].hand = 1;
            }
            else if (currentIterationNumber == 1)
            {
                players[0].hand = 2;
                players[1].hand = 3;
            }
            else if (currentIterationNumber == 2)
            {
                players[0].hand = 2;
                players[1].hand = 3;
            }
            else if (currentIterationNumber == 3)
            {
                players[0].hand = 2;
                players[1].hand = 1;
            }
            else if (currentIterationNumber == 4)
            {
                players[0].hand = 2;
                players[1].hand = 1;
            }
            else if (currentIterationNumber == 5)
            {
                players[0].hand = 2;
                players[1].hand = 3;
            }
            else {
                players[0].hand = pickHandFromRange(GameStructure.player0Range);
                players[1].hand = pickHandFromRange(GameStructure.player1Range);
            }
            


        }

        private static int pickHandFromRange(List<int> range) {
            
            
            int randomIndex = rnd.Next(range.Count); // Generates a random index
            int randomItem = range[randomIndex];

            return randomItem;
        }

        

    }
}
