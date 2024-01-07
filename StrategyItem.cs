using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public class StrategyItem
    {
        public string nodeId;
        public string actionName;
        public int hand;
        public double actionPct;
        public double actionPctSum = 0;
        public double actionPctIterations = 0;


        public string getPctReadable() {

            double number = actionPct * 100;
            string formattedNumber;

            if (number == Math.Round(number))
            {
                formattedNumber = number.ToString("0");
            }
            else
            {
                formattedNumber = number.ToString("0.00");
            }

            formattedNumber += '%';

            return formattedNumber.PadRight(4,' ');
        }

        public string getAvgActionPct() {
            double res = (actionPctSum/actionPctIterations) * 100;
            string formattedNumber = res.ToString("0.00")+"%";
            return formattedNumber;
        }

    }
}
