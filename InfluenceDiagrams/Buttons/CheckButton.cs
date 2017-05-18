
using System;
using System.Windows;
using System.Windows.Controls;

namespace InfluenceDiagrams.Buttons
{
    [Serializable]
    public class CheckButton : Button
    {
        

        static CheckButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckButton),
                   new FrameworkPropertyMetadata(typeof(CheckButton)));
        }
    }
}
