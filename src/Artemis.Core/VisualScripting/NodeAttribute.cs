using System;

namespace Artemis.Core
{
    public class NodeAttribute : Attribute
    {
        #region Properties & Fields

        public string Name { get; }
        public string Description { get; set; }
        public string Category { get; set; }

        #endregion

        #region Constructors

        public NodeAttribute(string name)
        {
            this.Name = name;
        }

        public NodeAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public NodeAttribute(string name, string description, string category)
        {
            this.Name = name;
            this.Description = description;
            this.Category = category;
        }

        #endregion
    }
}
