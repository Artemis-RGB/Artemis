using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    internal class DataModelStore
    {
        private static readonly List<DataModelRegistration> Registrations = new List<DataModelRegistration>();

        public static DataModelRegistration Add(DataModel dataModel)
        {
            DataModelRegistration registration;
            lock (Registrations)
            {
                if (Registrations.Any(r => r.DataModel == dataModel))
                    throw new ArtemisCoreException($"Data model store already contains data model '{dataModel.DataModelDescription}'");

                registration = new DataModelRegistration(dataModel, dataModel.Implementation.Instance) {IsInStore = true};
                Registrations.Add(registration);
            }

            OnDataModelAdded(new DataModelStoreEvent(registration));
            return registration;
        }

        public static void Remove(DataModelRegistration registration)
        {
            lock (Registrations)
            {
                if (!Registrations.Contains(registration))
                    throw new ArtemisCoreException($"Data model store does not contain data model '{registration.DataModel.DataModelDescription}'");

                Registrations.Remove(registration);
                registration.IsInStore = false;
            }

            OnDataModelRemoved(new DataModelStoreEvent(registration));
        }

        public static List<DataModelRegistration> GetAll()
        {
            lock (Registrations)
            {
                return new List<DataModelRegistration>(Registrations);
            }
        }

        public static DataModelRegistration Get(Guid pluginGuid)
        {
            lock (Registrations)
            {
                return Registrations.FirstOrDefault(d => d.PluginImplementation.PluginInfo.Guid == pluginGuid);
            }
        }

        #region Events

        public static event EventHandler<DataModelStoreEvent> DataModelAdded;
        public static event EventHandler<DataModelStoreEvent> DataModelRemoved;

        private static void OnDataModelAdded(DataModelStoreEvent e)
        {
            DataModelAdded?.Invoke(null, e);
        }

        private static void OnDataModelRemoved(DataModelStoreEvent e)
        {
            DataModelRemoved?.Invoke(null, e);
        }

        #endregion
    }
}