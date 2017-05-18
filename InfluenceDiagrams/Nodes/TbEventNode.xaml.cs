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

namespace InfluenceDiagrams.Nodes
{
    /// <summary>
    /// Interaction logic for TbEventNode.xaml
    /// </summary>
    public partial class TbEventNode : UserControl
    {
        public TbEventNode()
        {
            InitializeComponent();
        }

        public void MouseLeftDown(object sender, MouseEventArgs e)
        {
            MainWindow.GetMCanvas.DragSelection = "EventNode";
            DragDrop.DoDragDrop(this, MainWindow.GetCanvas, DragDropEffects.Move);
        }
    }
}
