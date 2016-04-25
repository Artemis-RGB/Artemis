namespace Artemis.Utilities.ParentChild
{
    /// <summary>
    ///     Defines the contract for an object that has a parent object
    ///     Thomas Levesque - http://www.thomaslevesque.com/2009/06/12/c-parentchild-relationship-and-xml-serialization/
    /// </summary>
    /// <typeparam name="P">Type of the parent object</typeparam>
    public interface IChildItem<P> where P : class
    {
        P Parent { get; set; }
    }
}