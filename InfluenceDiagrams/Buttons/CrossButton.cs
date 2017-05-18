
using System;
using System.Windows;
using System.Windows.Controls;

namespace InfluenceDiagrams.Buttons
{
    [Serializable]
    public class CrossButton : Button
    {

        static CrossButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossButton),
                   new FrameworkPropertyMetadata(typeof(CrossButton)));
        }
    }
}
