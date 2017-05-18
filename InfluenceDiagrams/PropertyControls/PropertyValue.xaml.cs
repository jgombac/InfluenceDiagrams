using InfluenceDiagrams.Calculations;
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
    public partial class PropertyValue : UserControl
    {
        ValueOutcome[] outcomes;
        ValueNode owner;

        public PropertyValue(ValueNode owner)
        {
            InitializeComponent();
            this.owner = owner;
            outcomes = owner.Outcomes;

            StackPanel first = new StackPanel { Orientation = Orientation.Horizontal };

            foreach (ValueOutcome outc in outcomes)
            {
                foreach (Node fac in outc.FacNodes)
                {
                    first.Children.Add(new Label { Content = fac.Name, Width = 100, FontWeight = FontWeights.Bold });
                }
                break;
            }
            first.Children.Add(new Label { Content = owner.Name, Width = 100, FontWeight = FontWeights.Bold });
            this.Container.Children.Add(first);
            foreach (ValueOutcome outc in outcomes)
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                string[] items = outc.Factors.ToArray();
                foreach (string item in items)
                {
                    sp.Children.Add(new Label { Content = item.Substring(owner.Owner.ID.Length+1), Width = 100 });
                }
                sp.Children.Add(new TextBox { Text = outc.Value.ToString(), Width = 100 });
                this.Container.Children.Add(sp);
            }
            if (owner.BestDecisions.Length > 0)
            {
                Container.Children.Add(new Label { Content = "BestDecisions", FontStyle = FontStyles.Italic });
                foreach (string bestDecision in owner.BestDecisions)
                {
                    Container.Children.Add(new Label { Content = bestDecision.Substring(owner.Owner.ID.Length + 1) });
                }
            }
            Button save = new Button { Content = "Save Values" };
            save.Click += Save_Click;
            this.Container.Children.Add(save);

            LayoutUpdated += PropertyValue_LayoutUpdated;

            this.SetBinding(PropertyValue.AnchorX1Property, new Binding()
            {
                Source = owner.Owner,
                Path = new PropertyPath(Node.AnchorRightXProperty)
            });

            this.SetBinding(PropertyValue.AnchorY1Property, new Binding()
            {
                Source = owner.Owner,
                Path = new PropertyPath(Node.AnchorRightYProperty)
            });

            this.SetBinding(Canvas.LeftProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(PropertyValue.AnchorX1Property)
            });

            this.SetBinding(Canvas.TopProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(PropertyValue.AnchorY1Property)
            });

            MainWindow.GetCanvas.Children.Add(this);

        }

        private void PropertyValue_LayoutUpdated(object sender, EventArgs e)
        {
            X1Property = (float)owner.Owner.RightXProperty + 15;
            Y1Property = (float)owner.Owner.RightYProperty - 30;
        }

        public void UpdateOutcomes()
        {
            Container.Children.Clear();
            outcomes = owner.Outcomes;
            StackPanel first = new StackPanel { Orientation = Orientation.Horizontal };

            foreach (ValueOutcome outc in outcomes)
            {
                foreach (Node fac in outc.FacNodes)
                {
                    first.Children.Add(new Label { Content = fac.Name, Width = 100, FontWeight = FontWeights.Bold });
                }
                break;
            }
            first.Children.Add(new Label { Content = owner.Name, Width = 100, FontWeight = FontWeights.Bold });
            this.Container.Children.Add(first);
            foreach (ValueOutcome outc in outcomes)
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                string[] items = outc.Factors.ToArray();
                foreach (string item in items)
                {
                    sp.Children.Add(new Label { Content = item.Substring(owner.Owner.ID.Length + 1), Width = 100 });
                }
                sp.Children.Add(new TextBox { Text = outc.Value.ToString(), Width = 100 });
                this.Container.Children.Add(sp);
            }
            if (owner.BestDecisions.Length > 0)
            {
                Container.Children.Add(new Label { Content = "BestDecisions", FontStyle = FontStyles.Italic });
                foreach (string bestDecision in owner.BestDecisions)
                {
                    Container.Children.Add(new Label { Content = bestDecision.Substring(owner.Owner.ID.Length + 1) });
                }
            }
            Button save = new Button { Content = "Save Values" };
            save.Click += Save_Click;
            this.Container.Children.Add(save);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < outcomes.Length; i++)
            {
                double value = 0;
                TextBox tb = (this.Container.Children[i + 1] as StackPanel).Children[(this.Container.Children[i + 1] as StackPanel).Children.Count - 1] as TextBox;
                Double.TryParse(tb.Text, out value);
                outcomes[i].Value = value;
            }
        }


        //--------------------------------------------------
        [NonSerialized]
        public static readonly DependencyProperty AnchorX1Property =
        DependencyProperty.Register(
            "X1Property", typeof(float), typeof(PropertyValue),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float X1Property
        {
            get
            {
                return (float)GetValue(AnchorX1Property);
            }
            set
            {
                SetValue(AnchorX1Property, value);
            }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorY1Property =
        DependencyProperty.Register(
            "Y1Property", typeof(float), typeof(PropertyValue),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float Y1Property
        {
            get { return (float)GetValue(AnchorY1Property); }
            set { SetValue(AnchorY1Property, value); }
        }
    }
}
