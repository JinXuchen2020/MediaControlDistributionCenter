using System.Windows;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class CustomComboBox : ComboBox
    {
        static CustomComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(typeof(CustomComboBox)));
        }
    }
}
