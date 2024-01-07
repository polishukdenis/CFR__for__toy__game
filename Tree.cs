using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfrForToyGame
{
    public class Tree
    {
        private string dumpTreeFilePath = "tree.txt";
        private string baseNodeId = "r:0";
        private TreeNode baseNode;


        public void init() { 
            this.baseNode = new TreeNode(baseNodeId, 0, -1);

            /*TreeNode node1 = baseNode.addChild("b1000", 1);
            node1.addChild("c", 1, true);
            node1.addChild("f", 1, true);*/

            TreeNode node1 = baseNode.addChild("c", 1, 0);
            node1.addChild("c", -1, 1, true);

            TreeNode child = node1.addChild("b1000", 0, 1);
            child.addChild("c", -1, 0, true);
            child.addChild("f", -1, 0, true);

        }
        
        //Должно вызываться только после того как всё дерево полностью инициализировано
        public void initRegrets() { 
            baseNode.initRegrets();
        }

        public void dumpTree() {

            File.Delete(dumpTreeFilePath);
            var myFile = File.Create(dumpTreeFilePath);
            myFile.Close();

            File.AppendAllText(dumpTreeFilePath, baseNode.getId() + Environment.NewLine);

            dumpChildren(baseNode);
        }

        public void dumpChildren(TreeNode node) { 
        
            List<TreeNode> children = node.getChildren();

            foreach (TreeNode child in children)
            {
                File.AppendAllText(dumpTreeFilePath, child.getId() + Environment.NewLine);
                dumpChildren(child);
            }
        }

        public TreeNode getBaseNode() {
            return baseNode;
        }
    }
}
