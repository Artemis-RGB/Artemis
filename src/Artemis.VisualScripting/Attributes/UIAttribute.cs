using System;

namespace Artemis.VisualScripting.Attributes
{
    public class UIAttribute : Attribute
    {
        #region Properties & Fields

        public string Name { get; }
        public string Description { get; set; }
        public string Category { get; set; }

        #endregion

        #region Constructors

        public UIAttribute(string name)
        {
            this.Name = name;
        }

        public UIAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public UIAttribute(string name, string description, string category)
        {
            this.Name = name;
            this.Description = description;
            this.Category = category;
        }

        #endregion
    }
}
