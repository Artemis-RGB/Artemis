using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Artemis.UI.Shared.Services.PropertyInput;

/// <summary>
///     Represents the base class for a property input view model that is used to edit layer properties
/// </summary>
/// <typeparam name="T">The type of property this input view model supports</typeparam>
public abstract class PropertyInputViewModel<T> : PropertyInputViewModel
{
    [AllowNull] private T _inputValue;

    private LayerPropertyPreview<T>? _preview;

    private bool _updating;

    /// <summary>
    ///     Creates a new instance of the <see cref="PropertyInputViewModel{T}" /> class
    /// </summary>
    protected PropertyInputViewModel(LayerProperty<T> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
    {
        LayerProperty = layerProperty;
        ProfileEditorService = profileEditorService;
        PropertyInputService = propertyInputService;

        _inputValue = default!;

        this.WhenActivated(d =>
        {
            ProfileEditorService.Time.Subscribe(t => Time = t).DisposeWith(d);
            UpdateInputValue();

            Observable.FromEventPattern<LayerPropertyEventArgs>(x => LayerProperty.Updated += x, x => LayerProperty.Updated -= x)
                .Subscribe(_ => UpdateInputValue())
                .DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyEventArgs>(x => LayerProperty.CurrentValueSet += x, x => LayerProperty.CurrentValueSet -= x)
                .Subscribe(_ => UpdateInputValue())
                .DisposeWith(d);
            Observable.FromEventPattern<DataBindingEventArgs>(x => LayerProperty.DataBinding.DataBindingEnabled += x, x => LayerProperty.DataBinding.DataBindingEnabled -= x)
                .Subscribe(_ => UpdateDataBinding())
                .DisposeWith(d);
            Observable.FromEventPattern<DataBindingEventArgs>(x => LayerProperty.DataBinding.DataBindingDisabled += x, x => LayerProperty.DataBinding.DataBindingDisabled -= x)
                .Subscribe(_ => UpdateDataBinding())
                .DisposeWith(d);

            Disposable.Create(DiscardPreview).DisposeWith(d);
        });
    }

    /// <summary>
    ///     Gets the layer property this view model is editing
    /// </summary>
    public LayerProperty<T> LayerProperty { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the layer property should be enabled
    /// </summary>
    public bool IsEnabled => !LayerProperty.HasDataBinding;

    /// <summary>
    ///     Gets the profile editor service
    /// </summary>
    public IProfileEditorService ProfileEditorService { get; }

    /// <summary>
    ///     Gets the property input service
    /// </summary>
    public IPropertyInputService PropertyInputService { get; }

    /// <summary>
    ///     Gets or boolean indicating whether the current input is being previewed, the value won't be applied until
    ///     <para>
    ///         Only applicable when using something like a <see cref="DraggableFloat" />, see
    ///         <see cref="StartPreview" /> and <see cref="ApplyPreview" />
    ///     </para>
    /// </summary>
    public bool IsPreviewing => _preview != null;

    /// <summary>
    ///     Gets or sets the input value
    /// </summary>
    [MaybeNull]
    public T InputValue
    {
        get => _inputValue;
        set
        {
            this.RaiseAndSetIfChanged(ref _inputValue, value);
            ApplyInputValue();
        }
    }

    /// <summary>
    ///     Gets the prefix to show before input elements
    /// </summary>
    public string? Prefix => LayerProperty.PropertyDescription.InputPrefix;

    /// <summary>
    ///     Gets the affix to show after input elements
    /// </summary>
    public string? Affix => LayerProperty.PropertyDescription.InputAffix;

    /// <summary>
    ///     Gets the current time at which the property is being edited
    /// </summary>
    protected TimeSpan Time { get; private set; }

    internal override object InternalGuard { get; } = new();

    /// <summary>
    ///     Starts the preview of the current property, allowing updates without causing real changes to the property.
    /// </summary>
    public virtual void StartPreview()
    {
        _preview?.DiscardPreview();
        _preview = new LayerPropertyPreview<T>(LayerProperty, Time);
    }

    /// <summary>
    ///     Applies the current preview to the property.
    /// </summary>
    public virtual void ApplyPreview()
    {
        if (_preview == null)
            return;

        if (_preview.DiscardPreview() && _preview.PreviewValue != null)
            ProfileEditorService.ExecuteCommand(new UpdateLayerProperty<T>(LayerProperty, _inputValue, _preview.OriginalValue, Time));
        _preview = null;
    }

    /// <summary>
    ///     Discard the preview of the property.
    /// </summary>
    public virtual void DiscardPreview()
    {
        if (_preview == null)
            return;

        _preview.DiscardPreview();
        _preview = null;
    }

    /// <summary>
    ///     Called when the input value has changed
    /// </summary>
    protected virtual void OnInputValueChanged()
    {
    }

    /// <summary>
    ///     Called when data bindings have been enabled or disabled on the layer property
    /// </summary>
    protected virtual void OnDataBindingsChanged()
    {
    }

    /// <summary>
    ///     Applies the input value to the layer property or the currently active preview.
    /// </summary>
    protected virtual void ApplyInputValue()
    {
        // Avoid reapplying the latest value by checking if we're currently updating
        if (_updating || !ValidationContext.IsValid)
            return;

        if (_preview != null)
            _preview.Preview(_inputValue);
        else
            ProfileEditorService.ExecuteCommand(new UpdateLayerProperty<T>(LayerProperty, _inputValue, Time));
    }

    private void UpdateInputValue()
    {
        // Always run this on the UI thread to avoid race conditions with ApplyInputValue
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                _updating = true;

                // Avoid unnecessary UI updates and validator cycles
                if (Equals(_inputValue, LayerProperty.CurrentValue))
                    return;

                // Override the input value
                _inputValue = LayerProperty.CurrentValue;

                // Notify a change in the input value
                OnInputValueChanged();
                this.RaisePropertyChanged(nameof(InputValue));
            }
            finally
            {
                _updating = false;
            }
        });
    }

    private void UpdateDataBinding()
    {
        this.RaisePropertyChanged(nameof(IsEnabled));
        OnDataBindingsChanged();
    }
}

/// <summary>
///     For internal use only, implement <see cref="PropertyInputViewModel" /> instead.
/// </summary>
public abstract class PropertyInputViewModel : ReactiveValidationObject, IActivatableViewModel, IDisposable
{
    /// <summary>
    ///     Prevents this type being implemented directly, implement
    ///     <see cref="PropertyInputViewModel" /> instead.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    internal abstract object InternalGuard { get; }

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> to release both managed and unmanaged resources;
    ///     <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
    }

    #region Implementation of IActivatableViewModel

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}