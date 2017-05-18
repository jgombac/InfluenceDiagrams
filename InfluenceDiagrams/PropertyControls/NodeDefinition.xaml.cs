using InfluenceDiagrams.Buttons;
using InfluenceDiagrams.Nodes;
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

namespace InfluenceDiagrams.PropertyControls
{
    [Serializable]
    public partial class NodeDefinition : UserControl
    {

        private Node owner;

        public NodeDefinition() { }

        public NodeDefinition(Node owner)
        {
            InitializeComponent();
            this.owner = owner;
            if (owner.Definitions.Length >= 2)
                ValidDefinitions(true);
            else
                ValidDefinitions(false);
            if (owner.Definitions != null && owner.Definitions.Length > 0) { 
                foreach (string def in owner.Definitions)
                {
                    StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                    sp.Children.Add(new TextBox { Text = def.Substring(owner.ID.Length + 1), Margin = new Thickness(2), MinWidth = 100 });
                    CrossButton removeBtn = new CrossButton { Width = 15, Height = 15 };
                    removeBtn.Click += RemoveClick;
                    sp.Children.Add(removeBtn);
                    this.DefinitionContainer.Items.Add(sp);
                }
            }

            LayoutUpdated += NodeDefinition_LayoutUpdated;

            this.SetBinding(NodeDefinition.AnchorX1Property, new Binding()
            {
                Source = owner,
                Path = new PropertyPath(Node.AnchorRightXProperty)
            });

            this.SetBinding(NodeDefinition.AnchorY1Property, new Binding()
            {
                Source = owner,
                Path = new PropertyPath(Node.AnchorRightYProperty)
            });

            this.SetBinding(Canvas.LeftProperty, new Binding() {
                Source = this,
                Path = new PropertyPath(NodeDefinition.AnchorX1Property)
            });

            this.SetBinding(Canvas.TopProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(NodeDefinition.AnchorY1Property)
            });

            MainWindow.GetCanvas.Children.Add(this);
        }

        private void NodeDefinition_LayoutUpdated(object sender, EventArgs e)
        {
            X1Property = (float)owner.RightXProperty + 15;
        }

        public void AddClick(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBox { Margin = new Thickness(2), MinWidth = 100 });
            CrossButton removeBtn = new CrossButton { Width = 15, Height = 15 };
            removeBtn.Click += RemoveClick;
            sp.Children.Add(removeBtn);
            this.DefinitionContainer.Items.Add(sp);
            ValidDefinitions(false);
        }

        public void RemoveClick(object sender, RoutedEventArgs e)
        {
            var item = (sender as CrossButton);
            var parent = VisualTreeHelper.GetParent(item);
            int index = DefinitionContainer.Items.IndexOf(parent);
            this.DefinitionContainer.Items.RemoveAt(index);
            if (owner.Definitions.SequenceEqual(GetDefinitions()))
                ValidDefinitions(true);
            else
                ValidDefinitions(false);
        }

        public void ValidDefinitions(bool valid)
        {
            if (valid)
                DefBorder.BorderBrush = new SolidColorBrush(Colors.LightGray);
            else
                DefBorder.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        public string[] GetDefinitions()
        {
            HashSet<string> newDefinitions = new HashSet<string>();
            foreach (StackPanel sp in this.DefinitionContainer.Items)
            {
                TextBox tb = sp.Children[0] as TextBox;
                newDefinitions.Add(owner.ID + ";" + tb.Text);
            }
            return newDefinitions.ToArray();
        }

        public void SaveClick(object sender, RoutedEventArgs e)
        {
            if(DefinitionContainer.Items.Count <= 1)
            {
                ValidDefinitions(false);
                return;
            }
            HashSet<string> newDefinitions = new HashSet<string>();
            foreach (StackPanel sp in this.DefinitionContainer.Items)
            {
                TextBox tb = sp.Children[0] as TextBox;
                if(tb.Text == "")
                {
                    ValidDefinitions(false);
                    return;
                }
                newDefinitions.Add(owner.ID + ";" + tb.Text);
            }
            ValidDefinitions(true);
            if(owner.Type == NodeType.Event)
                (owner.Control as EventNode).SaveDefinitions(newDefinitions.ToArray());
            else if (owner.Type == NodeType.Decision)
                (owner.Control as DecisionNode).SaveDefinitions(newDefinitions.ToArray());
            owner.Definitions = newDefinitions.ToArray();
        }



        //--------------------------------------------------
        [NonSerialized]
        public static readonly DependencyProperty AnchorX1Property =
        DependencyProperty.Register(
            "X1Property", typeof(float), typeof(NodeDefinition),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float X1Property
        {
            get {
                return (float)GetValue(AnchorX1Property); }
            set { SetValue(AnchorX1Property, value);
            }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorY1Property =
        DependencyProperty.Register(
            "Y1Property", typeof(float), typeof(NodeDefinition),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float Y1Property
        {
            get { return (float)GetValue(AnchorY1Property); }
            set { SetValue(AnchorY1Property, value); }
        }

    }
}
