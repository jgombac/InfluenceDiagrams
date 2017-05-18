using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InfluenceDiagrams.Nodes
{
    [Serializable]
    public class Anchor : Grid
    {

        Node node;

        bool isHighlight = false;

        Line l1;
        Line l2;

        public Anchor() { }


        public Anchor(Node node, string name, DependencyProperty xProperty, DependencyProperty yProperty)
        {
            this.node = node;

            this.Name = name;
            this.Background = Brushes.Transparent;
            this.Margin = new Thickness(0);
            this.Width = 16;
            this.Height = 16;

            //listeners

            Viewbox vb = new Viewbox();
            vb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            vb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            vb.Stretch = Stretch.Fill;
            vb.Margin = new System.Windows.Thickness(3);

            Grid ingrid = new Grid();

            l1 = new Line();
            l1.X1 = 0;
            l1.Y1 = 0;
            l1.X2 = 100;
            l1.Y2 = 100;
            l1.Stroke = Brushes.Transparent;
            l1.StrokeThickness = 8;

            l2 = new Line();
            l2.X1 = 0;
            l2.Y1 = 100;
            l2.X2 = 100;
            l2.Y2 = 0;
            l2.Stroke = Brushes.Transparent;
            l2.StrokeThickness = 8;

            ingrid.Children.Add(l1);
            ingrid.Children.Add(l2);
            vb.Child = ingrid;
            this.Children.Add(vb);

            ContextMenu cm1 = new ContextMenu();
            MenuItem item1 = new MenuItem();
            item1.Header = "Add Child";
            item1.Click += Anchor_OnAddChildClick;
            cm1.Items.Add(item1);
            this.ContextMenu = cm1;

            this.MouseEnter += Anchor_MouseEnter;
            this.MouseLeave += Anchor_MouseLeave;
            this.MouseRightButtonDown += Anchor_MouseRightButtonDown;
            this.MouseLeftButtonDown += Anchor_MouseLeftButtonDown;
            this.LayoutUpdated += Anchor_LayoutUpdated;

            this.AllowDrop = true;
            this.Drop += Anchor_Drop;
        

            this.SetBinding(Canvas.LeftProperty, new Binding()
            {
                Source = node,
                Path = new PropertyPath(xProperty)
            });

            this.SetBinding(Canvas.TopProperty, new Binding()
            {
                Source = node,
                Path = new PropertyPath(yProperty)
            });

            Canvas.SetZIndex(this, 100);
            MainWindow.GetCanvas.Children.Add(this);
        }

        public Anchor(Node node, string name, System.Windows.VerticalAlignment vertical, System.Windows.HorizontalAlignment horizontal, System.Windows.Thickness margin)
        {

            this.node = node;

            this.Name = name;
            this.Background = Brushes.Transparent;
            this.VerticalAlignment = vertical;
            this.HorizontalAlignment = horizontal;
            this.Margin = margin;
            this.Width = 16;
            this.Height = 16;

            //listeners

            Viewbox vb = new Viewbox();
            vb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            vb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            vb.Stretch = Stretch.Fill;
            vb.Margin = new System.Windows.Thickness(3);

            Grid ingrid = new Grid();

            l1 = new Line();
            l1.X1 = 0;
            l1.Y1 = 0;
            l1.X2 = 100;
            l1.Y2 = 100;
            l1.Stroke = Brushes.Transparent;
            l1.StrokeThickness = 8;

            l2 = new Line();
            l2.X1 = 0;
            l2.Y1 = 100;
            l2.X2 = 100;
            l2.Y2 = 0;
            l2.Stroke = Brushes.Transparent;
            l2.StrokeThickness = 8;

            ingrid.Children.Add(l1);
            ingrid.Children.Add(l2);
            vb.Child = ingrid;
            this.Children.Add(vb);

            ContextMenu cm1 = new ContextMenu();
            MenuItem item1 = new MenuItem();
            item1.Header = "Add Child";
            item1.Click += Anchor_OnAddChildClick;
            cm1.Items.Add(item1);
            this.ContextMenu = cm1;

            this.MouseEnter += Anchor_MouseEnter;
            this.MouseLeave += Anchor_MouseLeave;
            this.MouseRightButtonDown += Anchor_MouseRightButtonDown;
            this.MouseLeftButtonDown += Anchor_MouseLeftButtonDown;
            //this.LayoutUpdated += Anchor_LayoutUpdated;

            this.AllowDrop = true;
            this.Drop += Anchor_Drop;


        }

        private void Anchor_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MDiagram.ShowAllAnchors();
            MainWindow.GetMCanvas.AddToAnchorLine(node, this);
            MainWindow.GetMCanvas.DragSelection = "Anchor";
            DragDrop.DoDragDrop(this, new DataObject(this), DragDropEffects.Move);
        }


        private void Anchor_Drop(object sender, DragEventArgs e)
        {
            MDiagram.HideAllAnchors();
            MainWindow.GetMCanvas.AddToAnchorLine(node, this);
        }

        private void Anchor_OnAddChildClick(object sender, RoutedEventArgs e)
        {
            isHighlight = false;
            l1.Stroke = new SolidColorBrush(Colors.Transparent);
            l2.Stroke = new SolidColorBrush(Colors.Transparent);
        }

        private void Anchor_LayoutUpdated(object sender, EventArgs e)
        {
            try
            {
                Size size = RenderSize;
                Point ofs = new Point(size.Width / 2, size.Height / 2);
                AnchorPoint = TransformToVisual(MainWindow.GetCanvas).Transform(ofs);
                X1Property = (float)AnchorPoint.X;
                Y1Property = (float)AnchorPoint.Y;
            }catch(InvalidOperationException ioe)
            {
                this.LayoutUpdated -= Anchor_LayoutUpdated;
            }
        }


        private void Anchor_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isHighlight = true;
            l1.Stroke = new SolidColorBrush(Colors.Red);
            l2.Stroke = new SolidColorBrush(Colors.Red);
        }

        private void Anchor_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isHighlight)
                return;
            Hide();
        }

        private void Anchor_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Show();
        }

        public void Show()
        {
            l1.Stroke = new SolidColorBrush(Colors.Black);
            l2.Stroke = new SolidColorBrush(Colors.Black);
        }

        public void Hide()
        {
            l1.Stroke = new SolidColorBrush(Colors.Transparent);
            l2.Stroke = new SolidColorBrush(Colors.Transparent);
        }


        //PROPERTIES----------------------------------------------------------------------------------------------------------------------------------------------------------
        [NonSerialized]
        public static readonly DependencyProperty AnchorPointProperty =
        DependencyProperty.Register(
            "AnchorPoint", typeof(Point), typeof(Node),
                new FrameworkPropertyMetadata(new Point(0, 0),
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Point AnchorPoint
        {
            get { return (Point)GetValue(AnchorPointProperty); }
            set { SetValue(AnchorPointProperty, value); }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorX1Property =
        DependencyProperty.Register(
            "X1Property", typeof(float), typeof(Node),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float X1Property
        {
            get { return (float)GetValue(AnchorX1Property); }
            set { SetValue(AnchorX1Property, value);
            }
        }
        [NonSerialized]
        public static readonly DependencyProperty AnchorY1Property =
        DependencyProperty.Register(
            "Y1Property", typeof(float), typeof(Node),
                new FrameworkPropertyMetadata((float)0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public float Y1Property
        {
            get { return (float)GetValue(AnchorY1Property); }
            set { SetValue(AnchorY1Property, value);
            }
        }
    }
}
