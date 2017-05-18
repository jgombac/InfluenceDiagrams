using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InfluenceDiagrams
{
    public class MDesigner : DependencyObject
    {
        public static MDesigner instance;

        public static MDesigner Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MDesigner();
                }
                return instance;
            }
        }

        private static bool editEnabled = true;

        public bool EditEnabled
        {
            get { return editEnabled; }
            set { editEnabled = value; }
        }



        public static readonly DependencyProperty NodeSizeProperty =
                               DependencyProperty.Register("NodeSize", typeof(int), typeof(MDesigner));


        public int NodeSize
        {
            get { return (int)this.GetValue(NodeSizeProperty); }
            set { this.SetValue(NodeSizeProperty, value); }
        }

        public static readonly DependencyProperty NodeWidthProperty =
                               DependencyProperty.Register("NodeWidth", typeof(float), typeof(MDesigner));


        public float NodeWidth
        {
            get { return (float)this.GetValue(NodeWidthProperty); }
            set { this.SetValue(NodeWidthProperty, value); }
        }

    }
}
