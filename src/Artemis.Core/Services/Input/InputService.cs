using System.Collections.Generic;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services
{
    internal class InputService : IInputService
    {
        private readonly ILogger _logger;
        private readonly List<InputProvider> _inputProviders;

        public InputService(ILogger logger)
        {
            _logger = logger;
            _inputProviders = new List<InputProvider>();
        }

        public void AddInputProvider(InputProvider inputProvider)
        {
            _inputProviders.Add(inputProvider);
            inputProvider.KeyboardDataReceived += InputProviderOnKeyboardDataReceived;
        }

        public void RemoveInputProvider(InputProvider inputProvider)
        {
            if (!_inputProviders.Contains(inputProvider))
                return;

            inputProvider.KeyboardDataReceived -= InputProviderOnKeyboardDataReceived;
        }

        private void InputProviderOnKeyboardDataReceived(object? sender, InputProviderKeyboardEventArgs e)
        {
            if (!(sender is InputProvider inputProvider))
                return;

            bool foundLedId = InputKeyUtilities.LedIdMap.TryGetValue(e.Key, out LedId ledId);
            _logger.Verbose("Received keyboard data: LED ID: {ledId}, key: {key}, is down: {isDown}, device: {device} ", ledId, e.Key, e.IsDown, e.Device);
        }
    }

    /// <summary>
    ///     A service that allows you to interact with keyboard and mice input events
    /// </summary>
    public interface IInputService : IArtemisService
    {
        /// <summary>
        ///     Adds an input provided
        /// </summary>
        /// <param name="inputProvider">The input provider the add</param>
        void AddInputProvider(InputProvider inputProvider);

        /// <summary>
        ///     Removes an input provided
        /// </summary>
        /// <param name="inputProvider">The input provider the remove</param>
        void RemoveInputProvider(InputProvider inputProvider);
    }
}