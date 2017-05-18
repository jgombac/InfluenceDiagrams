using InfluenceDiagrams.PropertyControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InfluenceDiagrams.Nodes
{
    [Serializable]
    public class Node : DependencyObject
    {
        Anchor n;
        Anchor s;
        Anchor e;
        Anchor w;
        Anchor[] anchors;
        UserControl control;
        UserControl definitionControl;
        UserControl probabilityControl;
        UserControl outcomeControl;
        NodeType type;

        HashSet<Node> children = new HashSet<Node>();
        HashSet<Node> parents = new HashSet<Node>();

        string name = "";
        string id = "";
        string[] definitions = new string[0];

        public SerialNode Serialize()
        {
            SerialNode serial = new SerialNode
            {
                Type = Type,
                Name = Name,
                Id = ID,
                Definitions = Definitions,
            };
            serial.Position = new Point(LeftXProperty, TopYProperty);
            if (Type == NodeType.Event)
                serial.Probabilities = (Control as EventNode).SerializeProbabilities();
            else if(Type == NodeType.Value)
            {
                serial.Better = (Control as ValueNode).Better;
                serial.Outcomes = (Control as ValueNode).SerializeOutcomes();
            }
            return serial;
        }

        public Node(SerialNode serial)
        {
            type = serial.Type;
            definitions = serial.Definitions;
            id = serial.Id;
            name = serial.Name;
            switch (type)
            {
                case NodeType.Decision:
                    control = new DecisionNode(this, serial);
                    definitionControl = new NodeDefinition(this);
                    control.PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
                    break;
                case NodeType.Event:
                    control = new EventNode(this, serial);
                    definitionControl = new NodeDefinition(this);
                    probabilityControl = new PropertyEvent(control as EventNode);
                    control.PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
                    break;
                case NodeType.Value:
                    control = new ValueNode(this, serial);
                    definitionControl = new ValueDefinition(this);
                    outcomeControl = new PropertyValue(control as ValueNode);
                    control.PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
                    break;
            }

            Point pos = serial.Position;
            MainWindow.GetCanvas.Children.Add(control);
            Canvas.SetLeft(control, pos.X - MDesigner.Instance.NodeSize / 2);
            Canvas.SetTop(control, pos.Y - MDesigner.Instance.NodeSize / 2);

            n = new Anchor(this, "North", Node.AnchorTopXProperty, Node.AnchorTopYProperty);
            s = new Anchor(this, "South", Node.AnchorBottomXProperty, Node.AnchorBottomYProperty);
            w = new Anchor(this, "West", Node.AnchorLeftXProperty, Node.AnchorLeftYProperty);
            e = new Anchor(this, "East", Node.AnchorRightXProperty, Node.AnchorRightYProperty);
            anchors = new Anchor[] { n, s, w, e };
            MDiagram.AddNode(this);
        }

        public Node(Point pos, NodeType type)
        {
            this.type = type;
            switch (type)
            {
                case NodeType.Decision:
                    control = new DecisionNode(this);
                    definitionControl = new NodeDefinition(this);
                    control.PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
                    break;
                case NodeType.Event:
                    control = new EventNode(this);
                    definitionControl = new NodeDefinition(this);
                    probabilityControl = new PropertyEvent(control as EventNode);
                    control.PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
                    break;
                case NodeType.Value:
                    control = new ValueNode(this);
                    definitionControl = new ValueDefinition(this);
                    outcomeControl = new PropertyValue(control as ValueNode);
                    control.PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
                    break;
            }
            this.id = RandomString(8);
            MainWindow.GetCanvas.Children.Add(control);
            Canvas.SetLeft(control, pos.X - MDesigner.Instance.NodeSize / 2);
            Canvas.SetTop(control, pos.Y - MDesigner.Instance.NodeSize / 2);

            n = new Anchor(this, "North", Node.AnchorTopXProperty, Node.AnchorTopYProperty);
            s = new Anchor(this, "South", Node.AnchorBottomXProperty, Node.AnchorBottomYProperty);
            w = new Anchor(this, "West", Node.AnchorLeftXProperty, Node.AnchorLeftYProperty);
            e = new Anchor(this, "East", Node.AnchorRightXProperty, Node.AnchorRightYProperty);
            anchors = new Anchor[] { n, s, w, e };
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetMCanvas.Highlight(this);
        }

        public void AddParent(Node parent, bool update)
        {
            parents.Add(parent);
            if (!update)
                return;
            if (type == NodeType.Event)
            {
                (control as EventNode).UpdateProbabilities();
                (probabilityControl as PropertyEvent).UpdateProbabilities();
            }
            else if (type == NodeType.Value)
            {
                (control as ValueNode).UpdateOutcomes();
                (outcomeControl as PropertyValue).UpdateOutcomes();
            }
        }

        public void DeleteClick()
        {
            MainWindow.GetCanvas.Children.Remove(n);
            MainWindow.GetCanvas.Children.Remove(s);
            MainWindow.GetCanvas.Children.Remove(e);
            MainWindow.GetCanvas.Children.Remove(w);
        }


        public void AddChild(Node child)
        {
            children.Add(child);
        }



        private void MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine(type.ToString());
            e.Handled = false;
        }


        public bool DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (!MDesigner.Instance.EditEnabled)
                return false;
            double canvasHeight = MainWindow.GetCanvas.ActualHeight;
            double canvasWidth = MainWindow.GetCanvas.ActualWidth;

            double newLeft = Canvas.GetLeft(control) + +e.HorizontalChange;
            double newTop = Canvas.GetTop(control) + e.VerticalChange;

            if (newLeft < 0)
            {
                newLeft = 0;
                return false;
            }

            if (newTop < 0)
            {
                newTop = 0;
                return false;
            }
            Canvas.SetLeft(control, newLeft);
            Canvas.SetTop(control, newTop);
            return true;
        }


        [NonSerialized]
        public static readonly DependencyProperty AnchorLeftXProperty =
        DependencyProperty.Register(
            "AnchorLeftXProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double LeftXProperty
        {
            get { return (double)GetValue(AnchorLeftXProperty); }
            set { SetValue(AnchorLeftXProperty, value-10); }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorLeftYProperty =
        DependencyProperty.Register(
            "AnchorLeftYProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double LeftYProperty
        {
            get { return (double)GetValue(AnchorLeftYProperty); }
            set { SetValue(AnchorLeftYProperty, value-10); }
        }

        [NonSerialized]
        public static readonly DependencyProperty AnchorRightXProperty =
        DependencyProperty.Register(
            "AnchorRightXProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double RightXProperty
        {
            get { return (double)GetValue(AnchorRightXProperty); }
            set { SetValue(AnchorRightXProperty, value - 10); }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorRightYProperty =
        DependencyProperty.Register(
            "AnchorRightYProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double RightYProperty
        {
            get { return (double)GetValue(AnchorRightYProperty); }
            set { SetValue(AnchorRightYProperty, value - 10); }
        }

        //________________________________________________________________________________

        [NonSerialized]
        public static readonly DependencyProperty AnchorTopXProperty =
        DependencyProperty.Register(
            "AnchorTopXProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double TopXProperty
        {
            get { return (double)GetValue(AnchorTopXProperty); }
            set { SetValue(AnchorTopXProperty, value - 10); }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorTopYProperty =
        DependencyProperty.Register(
            "AnchorTopYProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double TopYProperty
        {
            get { return (double)GetValue(AnchorTopYProperty); }
            set { SetValue(AnchorTopYProperty, value - 10); }
        }

        [NonSerialized]
        public static readonly DependencyProperty AnchorBottomXProperty =
        DependencyProperty.Register(
            "AnchorBottomXProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double BottomXProperty
        {
            get { return (double)GetValue(AnchorBottomXProperty); }
            set { SetValue(AnchorBottomXProperty, value - 10); }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorBottomYProperty =
        DependencyProperty.Register(
            "AnchorBottomYProperty", typeof(double), typeof(Node),
                new FrameworkPropertyMetadata((double)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double BottomYProperty
        {
            get { return (double)GetValue(AnchorBottomYProperty); }
            set { SetValue(AnchorBottomYProperty, value - 10); }
        }


        //___________________________________________________

        public UserControl OutcomeControl
        {
            get { return outcomeControl; }
        }

        public UserControl ProbabilityControl
        {
            get { return probabilityControl; }
        }

        public UserControl DefinitionControl
        {
            get { return definitionControl; }
        }

        public Anchor[] Anchors
        {
            get { return anchors; }
        }

        public Anchor TopAnchor
        {
            get { return n; }
        }

        public Anchor BottomAnchor
        {
            get { return s; }
        }

        public Anchor LeftAnchor
        {
            get { return w; }
        }

        public Anchor RightAnchor
        {
            get { return e; }
        }

        public void RemoveChild(Node child)
        {
            children.RemoveWhere(s => s == child);
        }

        public void RemoveParent(Node parent)
        {
            parents.RemoveWhere(s => s == parent);
        }

        public string[] Definitions
        {
            get { return definitions; }
            set { definitions = value; }
        }

        public HashSet<Node> Parents
        {
            get { return parents; }
        }

        public HashSet<Node> Children
        {
            get { return children; }
        }

        public NodeType Type
        {
            get { return type; }
            set { type = value; }
        }

        public string ID
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public UserControl Control
        {
            get { return control; }
            set
            {
                UserControl newControl = value;
                control = value;
            }
        }


    }
}
