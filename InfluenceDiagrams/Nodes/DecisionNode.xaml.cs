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
    public partial class DecisionNode : UserControl
    {
        Point left;
        Point right;
        Point top;
        Point bottom;
        Node owner;

        string name = "decision";
        HashSet<string> alternatives = new HashSet<string>();
        string[] definitions = new string[0];

        public DecisionNode(Node owner, SerialNode serial)
        {
            InitializeComponent();

            this.owner = owner;
            left = new Point(0, MDesigner.Instance.NodeSize / 2);
            right = new Point(0, MDesigner.Instance.NodeSize / 2);
            top = new Point(0, MDesigner.Instance.NodeSize / 2);
            bottom = new Point(0, MDesigner.Instance.NodeSize / 2);

            this.TryUnbundling.IsEnabled = false;
            this.LayoutUpdated += DecisionNode_LayoutUpdated;
            definitions = owner.Definitions;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => { Name = serial.Name; }));
        }

        public DecisionNode(Node owner)
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


        public void SaveDefinitions(string[] definitions)
        {
            this.definitions = definitions;
        }

        public void UnbundlingEnabled(bool enabled)
        {
            this.TryUnbundling.IsEnabled = enabled;
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
            catch(System.InvalidOperationException oe)
            {
                this.LayoutUpdated -= DecisionNode_LayoutUpdated;
            }
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

        public void TryUnbundle(object sender, RoutedEventArgs e)
        {
            bool possible = CheckUnbundle();
            if (possible)
            {
                if (MessageBox.Show("Unbundling is possible. Proceed?", "Unbundle Check", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Unbundle();
            }
            else
            {
                MessageBox.Show("Unbundling is NOT possible.");
            }
        }

        public bool CheckUnbundle()
        {
            if (owner.Children.Count == 0)
                return false;
            foreach(Node chil in owner.Children)
            {
                if (chil.Type != NodeType.Value)
                    return false;
            }
            Node child = owner.Children.ToArray()[0];
            if (child.Type == NodeType.Value) {

                foreach (Node parent in owner.Parents)
                {
                    if (!parent.Children.Contains(child))
                        return false;
                }       
            }
            return true;
        }

        public void Unbundle()
        {
            MDiagram.UnbundleDecision(owner);
        }

        public void DeleteEnabled(bool enabled)
        {
            DeleteNode.IsEnabled = enabled;
        }

        public void NodeDeleteClick(object sender, RoutedEventArgs e)
        {
            owner.DeleteClick();
            MainWindow.GetMCanvas.Remove(owner, this);
        }

        private void NodeDoubleClick(object sender, RoutedEventArgs e)
        {
            e.Handled = false;
        }

        public void HighlightON()
        {
            NodeContainer.Background = new SolidColorBrush(Colors.LightBlue);
        }

        public void HighlightOFF()
        {
            NodeContainer.Background = new SolidColorBrush(Colors.WhiteSmoke);
        }



        public void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            owner.DragDelta(sender, e);
        }


        public void SaveAlternatives(HashSet<string> newAlternatives)
        {
            this.alternatives = newAlternatives;
            definitions = newAlternatives.ToArray();
            owner.Definitions = definitions;
        }

        private void ConstructDialog()
        {
            UserControl control = new UserControl();
            ListView lv = new ListView();
            lv.HorizontalAlignment = HorizontalAlignment.Stretch;
            foreach(string alt in alternatives)
            {
                TextBox item = new TextBox();
                item.HorizontalAlignment = HorizontalAlignment.Stretch; 
                item.Text = alt;
                lv.Items.Add(item);
            }
            Button addButton = new Button();
            addButton.Content = "Add";
            lv.Items.Add(addButton);
            control.Content = lv;

            Window wind = new Window
            {
                Title = "ABC",
                Content = control,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            wind.Show();
        }

        public HashSet<string> Alternatives
        {
            get { return alternatives; }
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

        public Node Owner
        {
            get { return owner; }
        }

    }
}
