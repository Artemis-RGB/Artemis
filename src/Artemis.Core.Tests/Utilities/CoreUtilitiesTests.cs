using System.Linq.Expressions;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using SkiaSharp;
using CoreUtilities = Artemis.Core.Utilities;

namespace Artemis.Core.Tests.Utilities;

public class CoreUtilitiesTests
{
    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public Person? Partner { get; set; }
        public int Field = 0;
        public int GetAge() => Age;
    }

    #region Utilities

    [Fact]
    public void ApplyUpdate_AsksTheAppToUpdate()
    {
        List<bool> requests = [];
        EventHandler<UpdateEventArgs> handler = (_, e) => requests.Add(e.Silent);
        CoreUtilities.UpdateRequested += handler;
        try
        {
            CoreUtilities.ApplyUpdate(true);
            CoreUtilities.ApplyUpdate(false);
        }
        finally
        {
            CoreUtilities.UpdateRequested -= handler;
        }

        requests.ShouldBe([true, false]);
    }

    [Fact]
    public void CreateAccessibleDirectory_CreatesTheWholePath()
    {
        string root = Path.Combine(Path.GetTempPath(), "Artemis.Core.Tests", Guid.NewGuid().ToString());
        string path = Path.Combine(root, "nested", "folder");
        try
        {
            CoreUtilities.CreateAccessibleDirectory(path);

            Directory.Exists(path).ShouldBeTrue();
            if (OperatingSystem.IsWindows())
            {
                SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);
                new DirectoryInfo(path).GetAccessControl()
                    .GetAccessRules(true, false, typeof(SecurityIdentifier))
                    .Cast<FileSystemAccessRule>()
                    .ShouldContain(r => r.IdentityReference == everyone && r.FileSystemRights.HasFlag(FileSystemRights.Modify));
            }

            // An existing directory is left alone
            File.WriteAllText(Path.Combine(path, "file.txt"), "content");
            CoreUtilities.CreateAccessibleDirectory(path);
            File.Exists(Path.Combine(path, "file.txt")).ShouldBeTrue();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetCurrentLocation_IsTheRunningProgram()
    {
        File.Exists(CoreUtilities.GetCurrentLocation()).ShouldBeTrue();
    }

    #endregion

    #region JSON

    [Fact]
    public void CoreJson_RoundTripsColors()
    {
        string json = CoreJson.Serialize(new Dictionary<string, SKColor> {{"Color", new SKColor(0x11, 0x22, 0x33, 0x44)}});

        json.ShouldBe("{\"Color\":\"#44112233\"}");
        CoreJson.Deserialize<Dictionary<string, SKColor>>(json)!["Color"].ShouldBe(new SKColor(0x11, 0x22, 0x33, 0x44));
    }

    [Fact]
    public void CoreJson_OptionsAreACopy()
    {
        CoreJson.GetJsonSerializerOptions().ShouldNotBeSameAs(CoreJson.GetJsonSerializerOptions());
        CoreJson.GetJsonSerializerOptions().Converters.Count.ShouldBe(2);
    }

    #endregion

    #region Reflection and expressions

    [Fact]
    public void GetPropertyInfo_FindsTheProperty()
    {
        PropertyInfo property = ReflectionUtilities.GetPropertyInfo(new Person(), (Person p) => p.Name);

        property.Name.ShouldBe(nameof(Person.Name));
    }

    [Fact]
    public void GetPropertyInfo_OnlyAcceptsProperties()
    {
        Should.Throw<ArgumentException>(() => ReflectionUtilities.GetPropertyInfo(new Person(), (Person p) => p.GetAge()))
            .Message.ShouldContain("refers to a method");
        Should.Throw<ArgumentException>(() => ReflectionUtilities.GetPropertyInfo(new Person(), (Person p) => p.Field))
            .Message.ShouldContain("refers to a field");
    }

    [Fact]
    public void CreateNullCheckedAccessor_ReadsThePath()
    {
        Func<Person, string> partnerName = CreateAccessor<string>("Partner.Name");
        Func<Person, int> partnerAge = CreateAccessor<int>("Partner.Age");
        Person person = new() {Partner = new Person {Name = "Sam", Age = 30}};

        partnerName(person).ShouldBe("Sam");
        partnerAge(person).ShouldBe(30);
    }

    [Fact]
    public void CreateNullCheckedAccessor_GivesTheDefaultWhenSomethingOnThePathIsMissing()
    {
        Person single = new() {Name = "Alex"};

        CreateAccessor<string>("Partner.Name")(single).ShouldBeNull();
        CreateAccessor<int>("Partner.Age")(single).ShouldBe(0);
        CreateAccessor<string>("Partner.Partner.Name")(single).ShouldBeNull();
        CreateAccessor<string>("Name")(null!).ShouldBeNull();
    }

    [Fact]
    public void CreateNullCheckedAccessor_ThrowsForUnknownMembers()
    {
        Should.Throw<ArgumentException>(() => CreateAccessor<string>("Partner.Nickname"));
    }

    private static Func<Person, T> CreateAccessor<T>(string path)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(Person));
        Expression accessor = ExpressionUtilities.CreateNullCheckedAccessor(parameter, path);
        return Expression.Lambda<Func<Person, T>>(accessor, parameter).Compile();
    }

    #endregion
}
