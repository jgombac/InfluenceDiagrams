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
    public partial class PropertyEvent : UserControl
    {

        Probability[] probabilities;
        EventNode owner;

        public PropertyEvent(EventNode owner)
        {
            InitializeComponent();
            this.owner = owner;
            probabilities = owner.Probabilities;

            StackPanel first = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (Probability prob in probabilities)
            {
                foreach(Node node in prob.ProbNodes.Concat(prob.CondNodes).ToArray())
                {
                    first.Children.Add(new Label { Content = node.Name, Width = 100, FontWeight = FontWeights.Bold });
                }
                break;
            }
            this.Container.Children.Add(first);
            foreach(Probability prob in probabilities)
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                string[] items = prob.Probabilities.Concat(prob.Conditions).ToArray();
                foreach(string item in items)
                {
                    sp.Children.Add(new Label { Content = item.Substring(owner.Owner.ID.Length+1), Width = 100});
                }
                sp.Children.Add(new TextBox { Text = prob.Value.ToString(), Width = 100});
                this.Container.Children.Add(sp);
            }
            Button save = new Button { Content = "Save Probabilities" };
            save.Click += Save_Click;
            this.Container.Children.Add(save);

            LayoutUpdated += PropertyEvent_LayoutUpdated;

            this.SetBinding(PropertyEvent.AnchorX1Property, new Binding()
            {
                Source = owner.Owner,
                Path = new PropertyPath(Node.AnchorRightXProperty)
            });

            this.SetBinding(PropertyEvent.AnchorY1Property, new Binding()
            {
                Source = owner.Owner,
                Path = new PropertyPath(Node.AnchorRightYProperty)
            });

            this.SetBinding(Canvas.LeftProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(PropertyEvent.AnchorX1Property)
            });

            this.SetBinding(Canvas.TopProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(PropertyEvent.AnchorY1Property)
            });

            MainWindow.GetCanvas.Children.Add(this);
        }

        public void UpdateProbabilities()
        {
            Container.Children.Clear();

            probabilities = owner.Probabilities;

            StackPanel first = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (Probability prob in probabilities)
            {
                foreach (Node node in prob.ProbNodes.Concat(prob.CondNodes).ToArray())
                {
                    first.Children.Add(new Label { Content = node.Name, Width = 100, FontWeight = FontWeights.Bold });
                }
                break;
            }
            this.Container.Children.Add(first);
            foreach (Probability prob in probabilities)
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                string[] items = prob.Probabilities.Concat(prob.Conditions).ToArray();
                foreach (string item in items)
                {
                    sp.Children.Add(new Label { Content = item.Substring(owner.Owner.ID.Length + 1), Width = 100 });
                }
                sp.Children.Add(new TextBox { Text = prob.Value.ToString(), Width = 100 });
                this.Container.Children.Add(sp);
            }
            Button save = new Button { Content = "Save Probabilities" };
            save.Click += Save_Click;
            this.Container.Children.Add(save);
        }

        private void PropertyEvent_LayoutUpdated(object sender, EventArgs e)
        {
            X1Property = (float)owner.Owner.RightXProperty + 15;
            Y1Property = (float)owner.Owner.RightYProperty - 30;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < probabilities.Length; i++)
            {
                Calculator.AddProbability(probabilities[i]);
                double value = 0;
                TextBox tb = (this.Container.Children[i + 1] as StackPanel).Children[(this.Container.Children[i + 1] as StackPanel).Children.Count - 1] as TextBox;
                Double.TryParse(tb.Text, out value);
                probabilities[i].Value = value;
            }
        }


        //--------------------------------------------------
        [NonSerialized]
        public static readonly DependencyProperty AnchorX1Property =
        DependencyProperty.Register(
            "X1Property", typeof(float), typeof(PropertyEvent),
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
            "Y1Property", typeof(float), typeof(PropertyEvent),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float Y1Property
        {
            get { return (float)GetValue(AnchorY1Property); }
            set { SetValue(AnchorY1Property, value); }
        }


    }
}
