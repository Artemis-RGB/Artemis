using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.Layout;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using RGB.NET.Core;
using KeyboardLayoutType = Artemis.Core.KeyboardLayoutType;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;

public partial class LayoutInfoStepViewModel : SubmissionViewModel
{
    private readonly Func<ArtemisLayout, LayoutInfoViewModel> _getLayoutInfoViewModel;
    private ArtemisLayout? _layout;
    [Notify(Setter.Private)] private bool _isKeyboardLayout;
    [Notify] private ObservableCollection<LayoutInfoViewModel> _layoutInfo = new();
    [Notify] private KeyboardLayoutType _physicalLayout;

    public LayoutInfoStepViewModel(Func<ArtemisLayout, LayoutInfoViewModel> getLayoutInfoViewModel)
    {
        _getLayoutInfoViewModel = getLayoutInfoViewModel;

        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<LayoutSelectionStepViewModel>());
        Continue = ReactiveCommand.Create(ExecuteContinue, ValidationContext.Valid);
        Secondary = ReactiveCommand.Create(ExecuteAddLayoutInfo);
        SecondaryText = "Add layout info";

        this.WhenActivated(d =>
        {
            if (State.EntrySource is not LayoutEntrySource layoutEntrySource)
                return;

            _layout = layoutEntrySource.Layout;
            IsKeyboardLayout = _layout.RgbLayout.Type == RGBDeviceType.Keyboard;
            PhysicalLayout = layoutEntrySource.PhysicalLayout;
            LayoutInfo = new ObservableCollection<LayoutInfoViewModel>(layoutEntrySource.LayoutInfo.Select(CreateLayoutInfoViewModel));

            if (!LayoutInfo.Any())
                ExecuteAddLayoutInfo();

            this.ValidationRule(
                vm => vm.PhysicalLayout,
                this.WhenAnyValue(vm => vm.IsKeyboardLayout, vm => vm.PhysicalLayout, (isKeyboard, layout) => !isKeyboard || layout != KeyboardLayoutType.Unknown),
                "A keyboard layout is required"
            ).DisposeWith(d);
            this.ValidationRule(
                vm => vm.LayoutInfo,
                this.WhenAnyValue(vm => vm.LayoutInfo.Count).Select(c => c != 0),
                "At least one layout info is required"
            ).DisposeWith(d);
        });
    }

    private LayoutInfoViewModel CreateLayoutInfoViewModel(LayoutInfo layoutInfo)
    {
        LayoutInfoViewModel vm = _getLayoutInfoViewModel(_layout ?? throw new InvalidOperationException());
        vm.Model = layoutInfo.Model;
        vm.Vendor = layoutInfo.Vendor;
        vm.DeviceProviderId = layoutInfo.DeviceProviderId;
        vm.Remove = ReactiveCommand.Create(() => LayoutInfo.Remove(vm));

        return vm;
    }

    private void ExecuteAddLayoutInfo()
    {
        if (_layout == null)
            return;

        LayoutInfoViewModel layoutInfo = _getLayoutInfoViewModel(_layout);
        layoutInfo.Remove = ReactiveCommand.Create(() => LayoutInfo.Remove(layoutInfo));
        LayoutInfo.Add(layoutInfo);
    }

    private void ExecuteContinue()
    {
        if (State.EntrySource is not LayoutEntrySource layoutEntrySource)
            return;

        layoutEntrySource.PhysicalLayout = PhysicalLayout;
        layoutEntrySource.LayoutInfo = new List<LayoutInfo>(LayoutInfo.Select(i => i.ToLayoutInfo()));

        if (string.IsNullOrWhiteSpace(State.Name))
            State.Name = layoutEntrySource.Layout.RgbLayout.Name ?? "";
        if (string.IsNullOrWhiteSpace(State.Summary))
            State.Summary = !string.IsNullOrWhiteSpace(layoutEntrySource.Layout.RgbLayout.Vendor)
                ? $"{layoutEntrySource.Layout.RgbLayout.Vendor} {layoutEntrySource.Layout.RgbLayout.Type} device layout"
                : $"{layoutEntrySource.Layout.RgbLayout.Type} device layout";
        
        State.Categories = new List<long> {8}; // Device category, yes this could change but why would it
        if (State.EntryId == null)
            State.ChangeScreen<SpecificationsStepViewModel>();
        else
            State.ChangeScreen<UploadStepViewModel>();
    }
}