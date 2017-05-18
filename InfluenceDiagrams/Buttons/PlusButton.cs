
using System;
using System.Windows;
using System.Windows.Controls;

namespace InfluenceDiagrams.Buttons
{
    [Serializable]
    public class PlusButton : Button
    {

        static PlusButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlusButton),
                   new FrameworkPropertyMetadata(typeof(PlusButton)));
        }
    }
}
