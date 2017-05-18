using InfluenceDiagrams.Calculations;
using InfluenceDiagrams.PropertyControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace InfluenceDiagrams.Nodes
{
    [Serializable]
    public partial class ValueNode : UserControl
    {
        Point left;
        Point right;
        Point top;
        Point bottom;
        Node owner;

        string name = "value";
        ValueOutcome[] outcomes = new ValueOutcome[0];
        string better = "max";
        string[] bestDecisions = new string[0];

        public ValueNode(Node owner, SerialNode serial)
        {
            InitializeComponent();

            this.owner = owner;
            left = new Point(0, MDesigner.Instance.NodeSize / 2);
            right = new Point(0, MDesigner.Instance.NodeSize / 2);
            top = new Point(0, MDesigner.Instance.NodeSize / 2);
            bottom = new Point(0, MDesigner.Instance.NodeSize / 2);

            this.LayoutUpdated += DecisionNode_LayoutUpdated;
            better = serial.Better;
            outcomes = DeserializeOutcomes(serial.Outcomes);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => { Name = serial.Name; }));
        }

        public SerialValueOutcome[] SerializeOutcomes()
        {
            List<SerialValueOutcome> serials = new List<SerialValueOutcome>();
            foreach (ValueOutcome outcome in outcomes)
                serials.Add(outcome.Serialize());
            return serials.ToArray();
        }

        public ValueOutcome[] DeserializeOutcomes(SerialValueOutcome[] serials)
        {
            List<ValueOutcome> outcomes = new List<ValueOutcome>();
            foreach (SerialValueOutcome serial in serials)
                outcomes.Add(new ValueOutcome(serial));
            return outcomes.ToArray();
        }

        public ValueNode(Node owner)
        {
            InitializeComponent();

            this.owner = owner;
            left = new Point(0, MDesigner.Instance.NodeSize / 2);
            right = new Point(0, MDesigner.Instance.NodeSize / 2);
            top = new Point(0, MDesigner.Instance.NodeSize / 2);
            bottom = new Point(0, MDesigner.Instance.NodeSize / 2);

            this.LayoutUpdated += DecisionNode_LayoutUpdated;
        }

        public void UpdateOutcomes()
        {
            Node[] parents = Parents.ToArray();
            string[] parentNames = new string[0];
            string[][] outcomes = new string[0][];

            foreach (Node parent in parents)
            {
                Array.Resize(ref parentNames, parentNames.Length + 1);
                parentNames[parentNames.Length - 1] = parent.Name;
                Array.Resize(ref outcomes, outcomes.Length + 1);
                outcomes[outcomes.Length - 1] = parent.Definitions;
            }

            var conditionCombinations = CartesianCompute.CartesianProduct(outcomes);
            HashSet<ValueOutcome> values = new HashSet<ValueOutcome>();
            if (conditionCombinations != null && conditionCombinations.Count() > 0)
            {
                foreach (var comb in conditionCombinations)
                {
                    string[] combArr = comb.ToArray();
                    ValueOutcome value = new ValueOutcome(combArr.ToArray(), parents, 0);

                    Console.WriteLine("ValueOutcome: " + value.ToString());
                    values.Add(value);
                }
            }
            this.outcomes = values.ToArray();
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
            MainWindow.GetCanvas.Children.Remove(owner.OutcomeControl);
        }

        private void NodeDoubleClick(object sender, RoutedEventArgs e)
        {
                    
        }

        public void HighlightON()
        {
            NodeRect.Background = new SolidColorBrush(Colors.LightBlue);
        }

        public void HighlightOFF()
        {
            NodeRect.Background = new SolidColorBrush(Colors.WhiteSmoke);
        }

        public void DeleteEnabled(bool enabled)
        {
            DeleteNode.IsEnabled = enabled;
        }

        public void NameDoubleClick(object sender, RoutedEventArgs e)
        {
            TextBox tb = new TextBox
            {
                Text = (this.DragThumb.Template.FindName("NodeName", this.DragThumb) as Label).Content.ToString(),
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

        public void ValuesClick(object sender, RoutedEventArgs e)
        {
            Window wind = new Window
            {
                Title = "ABC",
                Content = new PropertyValue(this),
                SizeToContent = SizeToContent.WidthAndHeight
            };
            wind.Show();
        }

        public void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            owner.DragDelta(sender, e);
        }

        public string Better
        {
            get { return better; }
            set { better = value; }
        }

        public ValueOutcome[] Outcomes
        {
            get { return outcomes; }
            set { outcomes = value; }
        }

        public HashSet<Node> Parents
        {
            get { return owner.Parents; }
        }

        public HashSet<Node> Children
        {
            get { return owner.Children; }
        }

        public Node Owner
        {
            get { return owner; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; (this.DragThumb.Template.FindName("NodeName", this.DragThumb) as Label).Content = value; owner.Name = value; }
        }

        public string[] BestDecisions
        {
            get { return bestDecisions; }
            set { bestDecisions = value; }
        }

    }
}
