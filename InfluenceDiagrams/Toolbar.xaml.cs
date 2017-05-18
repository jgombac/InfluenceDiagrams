using InfluenceDiagrams.Nodes;
using InfluenceDiagrams.Relations;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;

namespace InfluenceDiagrams
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar : UserControl
    {
        public Toolbar()
        {
            InitializeComponent();
            NodeSizeSlider.Value = 80;
        }

        public void Serialize(string filename)
        {
            SerializeData data = new SerializeData(filename + ".gombi");
            Node[] nodes = MDiagram.Nodes;
            foreach (Node node in nodes)
            {
                SerialNode serial = node.Serialize();
                data.SerializeObject(serial);
            }
            Relation[] relations = MDiagram.Relations;
            foreach(Relation rel in relations)
            {
                SerialRelation serial = rel.Serialize();
                data.SerializeObject(serial);
            }
            data.CloseStream();

        }

        public void SaveDiagramClick(object sender, RoutedEventArgs e)
        {
            FilenameDialog fd = new FilenameDialog();
            if (fd.ShowDialog() == true)
                Serialize(fd.Filename);
        }

        public void LoadDiagramClick(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "GOMBI Files (*.gombi)|*.gombi" };
            var result = ofd.ShowDialog();
            if (result == false) return;
            Deserialize(ofd.FileName);
        }

        public void Deserialize(string filename)
        {
            SerializeData data = new SerializeData(filename);
            data.DeserializeObjects();
            data.CloseStream();
        }

        public void NodeSizeSliderValueChanged(object sender, RoutedEventArgs e)
        {
            MDesigner.Instance.NodeSize = (int)NodeSizeSlider.Value;
            MDesigner.Instance.NodeWidth = (float)NodeSizeSlider.Value * 1.5f;
        }

        public void EditCheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
                MDesigner.Instance.EditEnabled = true;
            else
                MDesigner.Instance.EditEnabled = false;
        }

        private void NewRelationClick(object sender, RoutedEventArgs e)
        {
            MainWindow.GetMCanvas.LineOpened = true;
        }

        private void EvaluateDiagramClick(object sender, RoutedEventArgs e)
        {
            MDiagram.EvaluateDiagram();
        }
    }
}
