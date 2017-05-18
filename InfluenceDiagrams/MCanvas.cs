using InfluenceDiagrams.Nodes;
using InfluenceDiagrams.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InfluenceDiagrams
{
    public class MCanvas
    {
        Canvas mCanvas;

        string dragSelection = "";

        Node highlighted = null;

        HashSet<Node> endNodes = new HashSet<Node>();

        private bool lineOpened = false;
        HashSet<Node> lineNodes = new HashSet<Node>();
        List<KeyValuePair<Node, Anchor>> anchorLine = new List<KeyValuePair<Node, Anchor>>();

        public MCanvas(Grid mainGrid)
        {

            ScrollViewer scroller = new ScrollViewer();
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

            mCanvas = new CustomCanvas();
            mCanvas.Margin = new Thickness(5);
            mCanvas.Background = new SolidColorBrush(Colors.Transparent);
            mCanvas.Width = Double.NaN;
            mCanvas.Height = Double.NaN;
            mCanvas.AllowDrop = true;
            mCanvas.SizeChanged += MCanvas_SizeChanged;


            scroller.Content = mCanvas;
            scroller.CanContentScroll = true;

            mainGrid.Children.Add(scroller);
            Grid.SetColumn(scroller, 1);


            mCanvas.Drop += MCanvas_Drop;


        }

        public void Highlight(Node node)
        {
            if (highlighted != null)
            {
                if (highlighted.Type == NodeType.Event)
                    (highlighted.Control as EventNode).HighlightOFF();
                else if (highlighted.Type == NodeType.Decision)
                    (highlighted.Control as DecisionNode).HighlightOFF();
                else if (highlighted.Type == NodeType.Value)
                    (highlighted.Control as ValueNode).HighlightOFF();
            }
            if (node.Type == NodeType.Event)
                (node.Control as EventNode).HighlightON();
            else if (node.Type == NodeType.Decision)
                (node.Control as DecisionNode).HighlightON();
            else if (node.Type == NodeType.Value)
                (node.Control as ValueNode).HighlightON();
            highlighted = node;

        }

        private void MCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine(mCanvas.ActualWidth.ToString());
            mCanvas.InvalidateMeasure();
        }

        public void AddEndNode(Node end)
        {
            endNodes.Add(end);
        }



        public void AddToAnchorLine(Node node, Anchor anchor)
        {
            anchorLine.Add(new KeyValuePair<Node, Anchor>(node, anchor));
            if(anchorLine.Count >= 2)
            {
                KeyValuePair<Node, Anchor> first = anchorLine[0];
                KeyValuePair<Node, Anchor> second = anchorLine[1];
                Relation rel = new Relation(first.Key, first.Value, second.Key, second.Value, true);
                ClearAnchorLine();
            }
        }

        public void ClearAnchorLine()
        {
            anchorLine.Clear();
        }


        private void MCanvas_Drop(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(TextBlock))
                return;
            Point mouse = e.GetPosition(mCanvas);
            Console.WriteLine(mouse.X + "  " + mouse.Y);

            MDiagram.HideAllAnchors();
            ClearAnchorLine();

            if (dragSelection == "DecisionNode")
            {
                InstantiateDecisionNode(mouse);
            }
            else if (dragSelection == "EventNode")
            {
                InstantiateEventNode(mouse);
            }
            else if (dragSelection == "ValueNode")
            {
                InstantiateValueNode(mouse);
            }
            

        }

        public void ReplaceNodes(Node oldNode, Node newNode)
        {

        }

        public void Remove(Node node, UserControl control)
        {
            MDiagram.RemoveNode(node);
            mCanvas.Children.Remove(control);
            mCanvas.Children.Remove(node.DefinitionControl);
        }

        void InstantiateValueNode(Point mouse)
        {
            Node valueNode = new Node(mouse, NodeType.Value);
            MDiagram.AddNode(valueNode);
        }

        void InstantiateDecisionNode(Point mouse)
        {
            Node decisionNode = new Node(mouse, NodeType.Decision);
            MDiagram.AddNode(decisionNode);
        }

        void InstantiateEventNode(Point mouse)
        {
            Node eventNode = new Node(mouse, NodeType.Event);
            MDiagram.AddNode(eventNode);
        }

        public String DragSelection
        {
            get { return dragSelection; }
            set { dragSelection = value; }
        }

        public Canvas GetCanvas
        {
            get { return mCanvas; }
        }

        public bool LineOpened
        {
            get { return lineOpened; }
            set { lineOpened = value; }
        }
    }
}
