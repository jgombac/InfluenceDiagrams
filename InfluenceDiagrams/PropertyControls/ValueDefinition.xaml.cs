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
    public partial class ValueDefinition : UserControl
    {

        ValueNode owner;
        public ValueDefinition(Node owner)
        {
            InitializeComponent();

            this.owner = owner.Control as ValueNode;
            BetterCombo.SelectedIndex = (this.owner.Better == "max") ? 0 : 1;

            this.LayoutUpdated += ValueDefinition_LayoutUpdated;

            this.SetBinding(ValueDefinition.AnchorX1Property, new Binding()
            {
                Source = owner,
                Path = new PropertyPath(Node.AnchorRightXProperty)
            });

            this.SetBinding(ValueDefinition.AnchorY1Property, new Binding()
            {
                Source = owner,
                Path = new PropertyPath(Node.AnchorRightYProperty)
            });

            this.SetBinding(Canvas.LeftProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(ValueDefinition.AnchorX1Property)
            });

            this.SetBinding(Canvas.TopProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(ValueDefinition.AnchorY1Property)
            });

            MainWindow.GetCanvas.Children.Add(this);
        }

        private void ValueDefinition_LayoutUpdated(object sender, EventArgs e)
        {
            X1Property = (float)owner.Owner.RightXProperty + 15;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            owner.Better = (BetterCombo.SelectedIndex == 0) ? "max" : "min";
        }

        [NonSerialized]
        public static readonly DependencyProperty AnchorX1Property =
        DependencyProperty.Register(
            "X1Property", typeof(float), typeof(ValueDefinition),
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
            "Y1Property", typeof(float), typeof(ValueDefinition),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float Y1Property
        {
            get { return (float)GetValue(AnchorY1Property); }
            set { SetValue(AnchorY1Property, value); }
        }
    }
}
