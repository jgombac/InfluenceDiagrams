using InfluenceDiagrams.Calculations;
using InfluenceDiagrams.PropertyControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace InfluenceDiagrams.Nodes
{
    [Serializable]
    public partial class EventNode : UserControl
    {
        Point left;
        Point right;
        Point top;
        Point bottom;
        Node owner;

        string name = "event";
        string[] definitions = new string[0];

        Probability[] probabilities = new Probability[0];

        public EventNode(Node owner, SerialNode serial)
        {
            InitializeComponent();

            this.owner = owner;
            left = new Point(0, MDesigner.Instance.NodeSize / 2);
            right = new Point(0, MDesigner.Instance.NodeSize / 2);
            top = new Point(0, MDesigner.Instance.NodeSize / 2);
            bottom = new Point(0, MDesigner.Instance.NodeSize / 2);

            this.TryUnbundling.IsEnabled = false;
            this.LayoutUpdated += DecisionNode_LayoutUpdated;
            probabilities = DeserializeProbabilities(serial.Probabilities);
            definitions = owner.Definitions;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => { Name = serial.Name; }));
        }

        public EventNode(Node owner)
        {
            InitializeComponent();

            this.owner = owner;
            left = new Point(0, MDesigner.Instance.NodeSize / 2);
            right = new Point(0, MDesigner.Instance.NodeSize / 2);
            top = new Point(0, MDesigner.Instance.NodeSize / 2);
            bottom = new Point(0, MDesigner.Instance.NodeSize / 2);



            this.TryUnbundling.IsEnabled = false;
            this.LayoutUpdated += DecisionNode_LayoutUpdated;
        }

        public void UpdateProbabilities()
        {
            Node[] parents = Parents.ToArray();
            string[] parentNames = new string[0];
            string[][] outcomes = new string[][] { Definitions };

            bool hasEventParent = false;
            foreach (Node parent in parents)
            {
                if (parent.Type == NodeType.Event)
                    hasEventParent = true;
                Array.Resize(ref parentNames, parentNames.Length + 1);
                parentNames[parentNames.Length - 1] = parent.Name;
                Array.Resize(ref outcomes, outcomes.Length + 1);
                outcomes[outcomes.Length - 1] = parent.Definitions;
            }

            var conditionCombinations = CartesianCompute.CartesianProduct(outcomes);
            HashSet<Probability> probs = new HashSet<Probability>();
            foreach (var comb in conditionCombinations)
            {
                string[] combArr = comb.ToArray();
                Probability prob = null;
                if (hasEventParent)
                    prob = new Probability(new string[] { combArr[0] }, new Node[] { owner }, combArr.Skip(1).ToArray(), parents);
                else
                    prob = new Probability(combArr, new Node[] { owner }.Concat(parents).ToArray(), new string[0], new Node[0]);
                Console.WriteLine("Probability: " + prob.ToString());
                probs.Add(prob);
            }
            probabilities = probs.ToArray();
        }

        public void UnbundlingEnabled(bool enabled)
        {
            this.TryUnbundling.IsEnabled = enabled;
        }

        public void TryUnbundle(object sender, RoutedEventArgs e)
        {
            bool possible = CheckUnbundling();
            if (possible)
            {
                if(MessageBox.Show("Unbundling is possible. Proceed?", "Unbundle Check", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Unbundle();
            }
            else
            {
                MessageBox.Show("Unbundling is NOT possible.");
            }
        }

        public bool CheckUnbundling()
        {
            if (owner.Children.Count == 0)
                return false;
            foreach(Node child in owner.Children)
            {
                if (child.Type != NodeType.Value)
                    return false;
            }

            return true;
        }

        public void Unbundle()
        {
            MDiagram.UnbundleEvent(owner);
        }

        private void DecisionNode_LayoutUpdated(object sender, EventArgs e)
        {
            try
            {
                Size size = RenderSize;
                Point ofs = new Point(0, size.Height / 2);
                left = TransformToVisual(MainWindow.GetCanvas).Transform(ofs);

                Point ofs2 = new Point(size.Width, size.Height / 2);
                right = TransformToVisual(MainWindow.GetCanvas).Transform(ofs2);

                Point ofsbot = new Point(size.Width / 2, size.Height);
                bottom = TransformToVisual(MainWindow.GetCanvas).Transform(ofsbot);

                Point ofstop = new Point(size.Width / 2, 0);
                top = TransformToVisual(MainWindow.GetCanvas).Transform(ofstop);

                owner.RightXProperty = (float)right.X;
                owner.RightYProperty = (float)right.Y;

                owner.LeftXProperty = (float)left.X;
                owner.LeftYProperty = (float)left.Y;

                owner.TopXProperty = (float)top.X;
                owner.TopYProperty = (float)top.Y;

                owner.BottomXProperty = (float)bottom.X;
                owner.BottomYProperty = (float)bottom.Y;
            }
            catch (System.InvalidOperationException oe)
            {
                this.LayoutUpdated -= DecisionNode_LayoutUpdated;
            }
        }

        public void NodeDeleteClick(object sender, RoutedEventArgs e)
        {
            owner.DeleteClick();
            MainWindow.GetMCanvas.Remove(owner, this);
            MainWindow.GetCanvas.Children.Remove(owner.ProbabilityControl);
        }

        private void NodeDoubleClick(object sender, RoutedEventArgs e)
        {
            
        }

        public void EventsClick(object sender, RoutedEventArgs e)
        {

        }

        public void TableClick(object sender, RoutedEventArgs e)
        {
            
            Window wind = new Window
            {
                Title = "ABC",
                Content = new PropertyEvent(this),
                SizeToContent = SizeToContent.WidthAndHeight
            };
            wind.Show();
        }

        public void SaveDefinitions(string[] definitions)
        {
            this.definitions = definitions;
            UpdateProbabilities();
            (Owner.ProbabilityControl as PropertyEvent).UpdateProbabilities();
        }

        public void SaveEvents(Dictionary<string, double> newEvents)
        {

        }

        public void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            owner.DragDelta(sender, e);
        }

        public void HighlightON()
        {
            NodeElipse.Fill = new SolidColorBrush(Colors.LightBlue);
        }

        public void HighlightOFF()
        {
            NodeElipse.Fill = new SolidColorBrush(Colors.WhiteSmoke);
        }

        public void DeleteEnabled(bool enabled)
        {
            DeleteNode.IsEnabled = enabled;
        }

        public void SetName(string name)
        {
            (this.DragThumb.Template.FindName("NodeName", this.DragThumb) as Label).Content = name;
        }

        public void NameDoubleClick(object sender, RoutedEventArgs e)
        {
            TextBox tb = new TextBox
            {
                Text = (this.DragThumb.Template.FindName("NodeName",this.DragThumb) as Label).Content.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Height = (this.DragThumb.Template.FindName("NodeName", this.DragThumb) as Label).ActualHeight
            };
            
            tb.CaretIndex = tb.Text.Length;
            tb.KeyDown += Tb_KeyDown;
            tb.LostFocus += Tb_LostFocus;
            (this.DragThumb.Template.FindName("NameContainer", this.DragThumb) as Grid).Children.Add(tb);
            tb.Focus();
        }

        private void Tb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            this.Name = tb.Text;
            (this.DragThumb.Template.FindName("NameContainer", this.DragThumb) as Grid).Children.Remove(tb);
        }

        private void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                this.Name = tb.Text;
                (this.DragThumb.Template.FindName("NameContainer", this.DragThumb) as Grid).Children.Remove(tb);
            }
        }

        public Probability[] Probabilities
        {
            get { return probabilities; }
            set { probabilities = value; }
        }


        public string[] Definitions
        {
            get { return definitions; }
        }

        public Node Owner
        {
            get { return owner; }
        }

        public Probability[] DeserializeProbabilities(SerialProbability[] serials)
        {
            List<Probability> probs = new List<Probability>();
            foreach (SerialProbability serial in serials)
                probs.Add(new Probability(serial));
            return probs.ToArray();
        }

        public SerialProbability[] SerializeProbabilities()
        {
            List<SerialProbability> probs = new List<SerialProbability>();
            foreach (Probability prob in Probabilities)
                probs.Add(prob.Serialize());
            return probs.ToArray();
        }

        public string Name
        {
            get { return name; }
            set { name = value; (this.DragThumb.Template.FindName("NodeName", this.DragThumb) as Label).Content = value; owner.Name = value; }
        }

        public HashSet<Node> Parents
        {
            get { return owner.Parents; }
        }

    }
}
