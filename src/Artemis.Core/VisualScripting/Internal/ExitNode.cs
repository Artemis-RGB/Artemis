namespace Artemis.Core.Internal
{
    internal interface IExitNode : INode
    { }

    internal class ExitNode<T> : Node, IExitNode
    {
        #region Properties & Fields

        public InputPin<T> Input { get; }

        public T? Value { get; private set; }

        public override bool IsExitNode => true;
        
        #endregion

        #region Constructors

        public ExitNode(string name, string description = "")
        {
            this.Name = name;
            this.Description = description;

            Input = CreateInputPin<T>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Value = Input.Value;
        }

        #endregion
    }
}
