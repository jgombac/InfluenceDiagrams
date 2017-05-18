using InfluenceDiagrams.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace InfluenceDiagrams.Relations
{
    [Serializable]
    public class Relation : ArrowLine
    {

        private Node parent;
        private Anchor parentAnchor;
        private Node child;
        private Anchor childAnchor;

        public SerialRelation Serialize()
        {
            SerialRelation serial = new SerialRelation
            {
                Parent = parent.ID,
                Child = child.ID,
                ParentAnchor = parentAnchor.Name,
                ChildAnchor = childAnchor.Name
            };
            return serial;
        }

        public Relation(SerialRelation serial)
        {
            foreach(Node node in MDiagram.Nodes)
            {
                if(node.ID == serial.Parent)
                {
                    parent = node;
                }
                else if(node.ID == serial.Child)
                {
                    child = node;
                }
            }

            foreach(Anchor anchor in parent.Anchors)
            {
                if(anchor.Name == serial.ParentAnchor)
                {
                    parentAnchor = anchor;
                }
            }

            foreach (Anchor anchor in child.Anchors)
            {
                if (anchor.Name == serial.ChildAnchor)
                {
                    childAnchor = anchor;
                }
            }
            child.AddParent(parent, false);
            parent.AddChild(child);

            this.ArrowEnds = ArrowEnds.End;
            this.IsArrowClosed = true;
            this.Stroke = Brushes.Blue;
            this.StrokeThickness = 3;

            ContextMenu contextMenu = new ContextMenu();
            tryTurn = new MenuItem();
            tryTurn.Header = "Try Turning";
            tryTurn.Click += TryTurning;
            tryTurn.IsEnabled = MDiagram.Evaluating;
            contextMenu.Items.Add(tryTurn);
            deleteItem = new MenuItem();
            deleteItem.Header = "Remove";
            deleteItem.Click += DeleteItem_Click;
            contextMenu.Items.Add(deleteItem);
            this.ContextMenu = contextMenu;

            this.SetBinding(ArrowLine.X1Property, new Binding()
            {
                Source = parentAnchor,
                Path = new PropertyPath(Anchor.AnchorX1Property)
            });
            this.SetBinding(ArrowLine.Y1Property, new Binding()
            {
                Source = parentAnchor,
                Path = new PropertyPath(Anchor.AnchorY1Property)
            });
            this.SetBinding(ArrowLine.X2Property, new Binding()
            {
                Source = childAnchor,
                Path = new PropertyPath(Anchor.AnchorX1Property)
            });
            this.SetBinding(ArrowLine.Y2Property, new Binding()
            {
                Source = childAnchor,
                Path = new PropertyPath(Anchor.AnchorY1Property)
            });

            MainWindow.GetCanvas.Children.Add(this);
            MDiagram.AddRelation(this);
        }

        public Relation(Node parent, Anchor parentAnchor, Node child, Anchor childAnchor, bool update)
        {
            this.parent = parent;
            this.parentAnchor = parentAnchor;
            this.child = child;
            this.childAnchor = childAnchor;
            child.AddParent(parent, update);
            parent.AddChild(child);

            this.ArrowEnds = ArrowEnds.End;
            this.IsArrowClosed = true;
            this.Stroke = Brushes.Blue;
            this.StrokeThickness = 3;

            ContextMenu contextMenu = new ContextMenu();
            tryTurn = new MenuItem();
            tryTurn.Header = "Try Turning";
            tryTurn.Click += TryTurning;
            tryTurn.IsEnabled = MDiagram.Evaluating;
            contextMenu.Items.Add(tryTurn);
            deleteItem = new MenuItem();
            deleteItem.Header = "Remove";
            deleteItem.Click += DeleteItem_Click;
            contextMenu.Items.Add(deleteItem);
            this.ContextMenu = contextMenu;

            this.SetBinding(ArrowLine.X1Property, new Binding()
            {
                Source = parentAnchor,
                Path = new PropertyPath(Anchor.AnchorX1Property)
            });
            this.SetBinding(ArrowLine.Y1Property, new Binding()
            {
                Source = parentAnchor,
                Path = new PropertyPath(Anchor.AnchorY1Property)
            });
            this.SetBinding(ArrowLine.X2Property, new Binding()
            {
                Source = childAnchor,
                Path = new PropertyPath(Anchor.AnchorX1Property)
            });
            this.SetBinding(ArrowLine.Y2Property, new Binding()
            {
                Source = childAnchor,
                Path = new PropertyPath(Anchor.AnchorY1Property)
            });

            MainWindow.GetCanvas.Children.Add(this);
            MDiagram.AddRelation(this);
        }


        MenuItem deleteItem;
        MenuItem tryTurn;

        public void DeleteEnabled(bool enabled)
        {
            deleteItem.IsEnabled = enabled;
        }

        public void TurningEnabled(bool enabled)
        {
            tryTurn.IsEnabled = enabled;
        }

        public void TryTurning(object sender, RoutedEventArgs e)
        {
            bool possible = CheckTurn();
            if (possible)
            {
                if (MessageBox.Show("Turning is possible. Proceed?", "Turn Check", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Turn();
            }
            else
            {
                MessageBox.Show("Turning is NOT possible.");
            }
        }

        public bool CheckTurn()
        {
            return parent.Type == NodeType.Event && child.Type == NodeType.Event;
        }

        public void Turn()
        {
            MDiagram.TurnRelation(this);
        }

        public String[] CalculateAngle(Node parent, Node child)
        {
            double x1 = parent.BottomXProperty;
            double y1 = parent.BottomYProperty;
            double x2 = child.BottomXProperty;
            double y2 = child.BottomYProperty;

            double deltaY = y2 - y1;
            double deltaX = x2 - x1;
            double angle = (Math.Atan2(deltaY, deltaX) * 180 / Math.PI) * -1;
            Console.WriteLine(angle.ToString());
            if (angle > -45 && angle <= 45)
                return new String[] { "Right", "Left" };
            else if (angle > 45 && angle <= 135)
                return new String[] { "Top", "Bottom" };
            else if (Math.Abs(angle) >= 135)
                return new String[] { "Left", "Right" };
            else if (angle > -135 && angle <= -45)
                return new String[] { "Bottom", "Top" };
            
            else
                return new String[0];
            
        }

        public void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            child.RemoveParent(parent);
            parent.RemoveChild(child);
            MDiagram.RemoveRelation(this);
            MainWindow.GetCanvas.Children.Remove(this);
        }

        public Anchor ParentAnchor
        {
            get { return parentAnchor; }
        }

        public Anchor ChildAnchor
        {
            get { return childAnchor; }
        }

        public Node Parent
        {
            get { return parent; }
        }

        public Node Child
        {
            get { return child; }
        }

        public Relation Arrow
        {
            get { return this; }
        }

    }
}
