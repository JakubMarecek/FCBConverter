using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace FCBConverterGUI
{
    [PseudoClasses(":normal")]
    public class AnimateTabControl : TabControl
    {
		protected override Type StyleKeyOverride => typeof(TabControl);
		
        public AnimateTabControl()
        {
            PseudoClasses.Add(":normal");
            //this.GetObservable(SelectedContentProperty).Subscribe(OnContentChanged);
        }
		
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectedContentProperty)
            {
           		if (AnimateOnChange)
           		{
           		    PseudoClasses.Remove(":normal");
           		    PseudoClasses.Add(":normal");
           		}
            }
        }

        /*private void OnContentChanged(object obj)
        {
            if (AnimateOnChange)
            {
                PseudoClasses.Remove(":normal");
                PseudoClasses.Add(":normal");
            }
        }*/

        public bool AnimateOnChange
        {
            get => GetValue(AnimateOnChangeProperty);
            set => SetValue(AnimateOnChangeProperty, value);
        }

        public static readonly StyledProperty<bool> AnimateOnChangeProperty =
            AvaloniaProperty.Register<AnimateTabControl, bool>(nameof(AnimateOnChange), true);
    }
}