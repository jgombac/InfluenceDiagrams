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

namespace InfluenceDiagrams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static MCanvas mCanvas;
        Point mouse;
        static StackPanel leftDock;

        public MainWindow()
        {
            InitializeComponent();
            mCanvas = new MCanvas(MainGrid);

            leftDock = new StackPanel { Name="LeftDock", Margin = new Thickness(5) };
            leftDock.Children.Add(new MToolbox());
            MainGrid.Children.Add(leftDock);
            Grid.SetColumn(leftDock, 0);
        }

        public static StackPanel LeftDock
        {
            get { return leftDock; }
        }


        public static MCanvas GetMCanvas
        {
            get { return mCanvas; }
        }

        public static Canvas GetCanvas
        {
            get { return mCanvas.GetCanvas; }
        }

    }
}
