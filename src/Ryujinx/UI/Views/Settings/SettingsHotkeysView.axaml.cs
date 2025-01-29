using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using DynamicData;
using Ryujinx.Ava.Input;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Input;
using Ryujinx.Input.Assigner;
using System.Linq;
using Button = Ryujinx.Input.Button;
using Key = Ryujinx.Common.Configuration.Hid.Key;

namespace Ryujinx.Ava.UI.Views.Settings
{
    public partial class SettingsHotkeysView : UserControl
    {
        private ButtonKeyAssigner _currentAssigner;
        private readonly IGamepadDriver _avaloniaKeyboardDriver;

        public SettingsHotkeysView()
        {
            InitializeComponent();
            RegisterEvents();
            _avaloniaKeyboardDriver = new AvaloniaKeyboardDriver(this);
            CycleControllers.LayoutUpdated += (_, _1) => RegisterEvents();
        }

        private void RegisterEvents()
        {
            foreach (ILogical visual in SettingButtons.GetLogicalDescendants())
            {
                if (visual is ToggleButton button and not CheckBox)
                {
                    button.IsCheckedChanged -= Button_IsCheckedChanged;
                    button.IsCheckedChanged += Button_IsCheckedChanged;
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (!_currentAssigner?.ToggledButton?.IsPointerOver ?? false)
            {
                _currentAssigner.Cancel();
            }
        }

        private void MouseClick(object sender, PointerPressedEventArgs e)
        {
            bool shouldUnbind = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;

            _currentAssigner?.Cancel(shouldUnbind);

            PointerPressed -= MouseClick;
        }

        private void Button_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                if ((bool)button.IsChecked)
                {
                    if (_currentAssigner != null && button == _currentAssigner.ToggledButton)
                    {
                        return;
                    }

                    if (_currentAssigner == null)
                    {
                        _currentAssigner = new ButtonKeyAssigner(button);

                        this.Focus(NavigationMethod.Pointer);

                        PointerPressed += MouseClick;

                        IKeyboard keyboard = (IKeyboard)_avaloniaKeyboardDriver.GetGamepad("0");
                        IButtonAssigner assigner = new KeyboardKeyAssigner(keyboard);

                        _currentAssigner.ButtonAssigned += (sender, e) =>
                        {
                            if (e.ButtonValue.HasValue)
                            {
                                SettingsViewModel viewModel = (DataContext) as SettingsViewModel;
                                Button buttonValue = e.ButtonValue.Value;

                                switch (button.Name)
                                {
                                    case "ToggleVSyncMode":
                                        viewModel.KeyboardHotkey.ToggleVSyncMode = buttonValue.AsHidType<Key>();
                                        break;
                                    case "Screenshot":
                                        viewModel.KeyboardHotkey.Screenshot = buttonValue.AsHidType<Key>();
                                        break;
                                    case "ShowUI":
                                        viewModel.KeyboardHotkey.ShowUI = buttonValue.AsHidType<Key>();
                                        break;
                                    case "Pause":
                                        viewModel.KeyboardHotkey.Pause = buttonValue.AsHidType<Key>();
                                        break;
                                    case "ToggleMute":
                                        viewModel.KeyboardHotkey.ToggleMute = buttonValue.AsHidType<Key>();
                                        break;
                                    case "ResScaleUp":
                                        viewModel.KeyboardHotkey.ResScaleUp = buttonValue.AsHidType<Key>();
                                        break;
                                    case "ResScaleDown":
                                        viewModel.KeyboardHotkey.ResScaleDown = buttonValue.AsHidType<Key>();
                                        break;
                                    case "VolumeUp":
                                        viewModel.KeyboardHotkey.VolumeUp = buttonValue.AsHidType<Key>();
                                        break;
                                    case "VolumeDown":
                                        viewModel.KeyboardHotkey.VolumeDown = buttonValue.AsHidType<Key>();
                                        break;
                                    case "CustomVSyncIntervalIncrement":
                                        viewModel.KeyboardHotkey.CustomVSyncIntervalIncrement = buttonValue.AsHidType<Key>();
                                        break;
                                    case "CustomVSyncIntervalDecrement":
                                        viewModel.KeyboardHotkey.CustomVSyncIntervalDecrement = buttonValue.AsHidType<Key>();
                                        break;
                                    default:
                                        var index = button.FindAncestorOfType<ItemsControl>().GetLogicalDescendants().OfType<ToggleButton>().IndexOf(button);
                                        if (index >= 0 && viewModel.KeyboardHotkey.CycleControllers != null)
                                        {
                                            viewModel.KeyboardHotkey.CycleControllers[index].Hotkey = buttonValue.AsHidType<Key>();
                                        }
                                        break;
                                }
                            }
                        };

                        _currentAssigner.GetInputAndAssign(assigner, keyboard);
                    }
                    else
                    {
                        if (_currentAssigner != null)
                        {
                            _currentAssigner.Cancel();
                            _currentAssigner = null;
                            button.IsChecked = false;
                        }
                    }
                }
                else
                {
                    _currentAssigner?.Cancel();
                    _currentAssigner = null;
                }
            }
        }

        public void Dispose()
        {
            _currentAssigner?.Cancel();
            _currentAssigner = null;

            _avaloniaKeyboardDriver.Dispose();
        }
    }
}
