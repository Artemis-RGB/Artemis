using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Tests.Services;

/// <summary>
///     Profile categories and the profile configurations inside them, as managed by the profile service
/// </summary>
public class ProfileCategoryTests : ProfileServiceTestBase
{
    [Fact]
    public void ANewDatabase_GetsTheDefaultCategories()
    {
        ProfileService.ProfileCategories.Select(c => (c.Name, c.Order)).ShouldBe([("Games", 1), ("Applications", 2), ("General", 3)]);
        GetStoredCategories().Select(c => c.Name).ShouldBe(["Games", "Applications", "General"]);
    }

    [Fact]
    public void Restarting_LoadsTheStoredCategoriesInOrder()
    {
        ProfileCategory games = ProfileService.ProfileCategories.Single(c => c.Name == "Games");
        games.Order = 10;
        ProfileService.SaveProfileCategory(games);

        IProfileService restarted = CreateNewProfileService();

        // The default categories are not created again
        restarted.ProfileCategories.Select(c => c.Name).ShouldBe(["Applications", "General", "Games"]);
    }

    [Fact]
    public void Restarting_WithoutAnyCategories_CreatesTheDefaultCategoriesAgain()
    {
        foreach (ProfileCategory category in ProfileService.ProfileCategories.ToList())
            ProfileService.DeleteProfileCategory(category);

        IProfileService restarted = CreateNewProfileService();

        restarted.ProfileCategories.Select(c => c.Name).ShouldBe(["Games", "Applications", "General"]);
    }

    [Fact]
    public void CreateProfileCategory_AddsItAtTheBottom()
    {
        List<ProfileCategory> added = [];
        ProfileService.ProfileCategoryAdded += (_, e) => added.Add(e.ProfileCategory);

        ProfileCategory category = ProfileService.CreateProfileCategory("Tests");

        category.Order.ShouldBe(4);
        ProfileService.ProfileCategories.Last().ShouldBeSameAs(category);
        added.ShouldBe([category]);
        GetStoredCategories().Last().Name.ShouldBe("Tests");
    }

    [Fact]
    public void CreateProfileCategory_AtTheTop_MovesTheOtherCategoriesDown()
    {
        ProfileCategory category = ProfileService.CreateProfileCategory("Tests", true);

        category.Order.ShouldBe(1);
        ProfileService.ProfileCategories.Where(c => c != category).Select(c => c.Order).ShouldBe([2, 3, 4]);
        GetStoredCategories().Select(c => c.Name).ShouldBe(["Tests", "Games", "Applications", "General"]);
    }

    [Fact]
    public void CreateProfileCategory_AtTheTop_IsListedFirst()
    {
        // Profiles are updated and rendered in list order, the first category ends up on top
        ProfileCategory category = ProfileService.CreateProfileCategory("Tests", true);

        ProfileService.ProfileCategories.First().ShouldBeSameAs(category);
    }

    [Fact]
    public void SaveProfileCategory_StoresItsChanges()
    {
        ProfileCategory category = ProfileService.CreateProfileCategory("Tests");
        category.Name = "Renamed";
        category.IsCollapsed = true;
        category.IsSuspended = true;

        ProfileService.SaveProfileCategory(category);

        ProfileCategory reloaded = CreateNewProfileService().ProfileCategories.Single(c => c.EntityId == category.EntityId);
        reloaded.Name.ShouldBe("Renamed");
        reloaded.IsCollapsed.ShouldBeTrue();
        reloaded.IsSuspended.ShouldBeTrue();
    }

    [Fact]
    public void ChangingTheOrder_LikeTheSidebarDoes_ReordersTheCategories()
    {
        // Moving General up one place, the sidebar gives every category its new order and saves the ones that changed
        List<ProfileCategory> categories = ProfileService.ProfileCategories.ToList();
        categories.Remove(General);
        categories.Insert(1, General);
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].Order == i + 1)
                continue;
            categories[i].Order = i + 1;
            ProfileService.SaveProfileCategory(categories[i]);
        }

        ProfileService.ProfileCategories.Select(c => c.Name).ShouldBe(["Games", "General", "Applications"]);
        CreateNewProfileService().ProfileCategories.Select(c => c.Name).ShouldBe(["Games", "General", "Applications"]);
    }

    [Fact]
    public void DeleteProfileCategory_RemovesTheCategoryAndItsProfiles()
    {
        ProfileCategory category = ProfileService.CreateProfileCategory("Tests");
        ProfileConfiguration first = ProfileService.CreateProfileConfiguration(category, "First", "TestTube");
        ProfileConfiguration second = ProfileService.CreateProfileConfiguration(category, "Second", "TestTube");
        ProfileService.ActivateProfile(first);
        List<ProfileConfiguration> removedProfiles = [];
        List<ProfileCategory> removedCategories = [];
        ProfileService.ProfileRemoved += (_, e) => removedProfiles.Add(e.ProfileConfiguration);
        ProfileService.ProfileCategoryRemoved += (_, e) => removedCategories.Add(e.ProfileCategory);

        ProfileService.DeleteProfileCategory(category);

        ProfileService.ProfileCategories.ShouldNotContain(category);
        removedProfiles.ShouldBe([second, first], ignoreOrder: true);
        removedCategories.ShouldBe([category]);
        first.Profile.ShouldBeNull();
        GetStoredCategories().ShouldNotContain(c => c.Name == "Tests");
        GetStoredProfiles().ShouldBeEmpty();
    }

    [Fact]
    public void CreateProfileConfiguration_AddsItToTheTopOfTheCategory()
    {
        List<ProfileConfiguration> added = [];
        ProfileService.ProfileAdded += (_, e) => added.Add(e.ProfileConfiguration);

        ProfileConfiguration first = ProfileService.CreateProfileConfiguration(General, "First", "TestTube");
        ProfileConfiguration second = ProfileService.CreateProfileConfiguration(General, "Second", "Abacus");

        General.ProfileConfigurations.ShouldBe([second, first]);
        second.Order.ShouldBe(0);
        first.Order.ShouldBe(1);
        added.ShouldBe([first, second]);
    }

    [Fact]
    public void CreateProfileConfiguration_IsStoredButNotActivated()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");

        configuration.Profile.ShouldBeNull();
        configuration.Category.ShouldBeSameAs(General);
        configuration.ProfileId.ShouldNotBe(Guid.Empty);
        configuration.Icon.IconName.ShouldBe("TestTube");

        ProfileConfiguration reloaded = CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Single();
        reloaded.Name.ShouldBe("Test profile");
        reloaded.ProfileId.ShouldBe(configuration.ProfileId);
        reloaded.Icon.IconName.ShouldBe("TestTube");
    }

    [Fact]
    public void SaveProfileCategory_StoresTheSettingsOfItsProfiles()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.Name = "Renamed";
        configuration.IsSuspended = true;
        configuration.FadeInAndOut = true;
        configuration.ActivationBehaviour = ActivationBehaviour.DisableOthersBelow;
        configuration.HotkeyMode = ProfileConfigurationHotkeyMode.EnableDisable;
        configuration.EnableHotkey = new Hotkey(KeyboardKey.F1, KeyboardModifierKey.Shift);
        configuration.DisableHotkey = new Hotkey(KeyboardKey.F2, null);
        configuration.Icon.SetIconByStream(new MemoryStream([1, 2, 3]));

        ProfileService.SaveProfileCategory(General);

        ProfileConfiguration reloaded = CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Single();
        reloaded.Name.ShouldBe("Renamed");
        reloaded.IsSuspended.ShouldBeTrue();
        reloaded.FadeInAndOut.ShouldBeTrue();
        reloaded.ActivationBehaviour.ShouldBe(ActivationBehaviour.DisableOthersBelow);
        reloaded.HotkeyMode.ShouldBe(ProfileConfigurationHotkeyMode.EnableDisable);
        reloaded.EnableHotkey!.Key.ShouldBe(KeyboardKey.F1);
        reloaded.EnableHotkey.Modifiers.ShouldBe(KeyboardModifierKey.Shift);
        reloaded.DisableHotkey!.Key.ShouldBe(KeyboardKey.F2);
        reloaded.DisableHotkey.Modifiers.ShouldBeNull();
        reloaded.Icon.IconType.ShouldBe(ProfileConfigurationIconType.BitmapImage);
        reloaded.Icon.IconBytes.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void AddProfileConfiguration_WithATarget_PutsItInFrontOfTheTarget()
    {
        ProfileConfiguration first = ProfileService.CreateProfileConfiguration(General, "First", "TestTube");
        ProfileConfiguration second = ProfileService.CreateProfileConfiguration(General, "Second", "TestTube");
        ProfileConfiguration third = ProfileService.CreateProfileConfiguration(General, "Third", "TestTube");
        // Listed as third, second, first

        General.AddProfileConfiguration(first, second);
        ProfileService.SaveProfileCategory(General);

        General.ProfileConfigurations.ShouldBe([third, first, second]);
        General.ProfileConfigurations.Select(c => c.Order).ShouldBe([0, 1, 2]);
        CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Select(c => c.Name).ShouldBe(["Third", "First", "Second"]);
    }

    [Fact]
    public void MovingAProfileToAnotherCategory_LikeTheSidebarDoes_StoresTheMove()
    {
        ProfileCategory games = ProfileService.ProfileCategories.Single(c => c.Name == "Games");
        ProfileConfiguration staying = ProfileService.CreateProfileConfiguration(General, "Staying", "TestTube");
        ProfileConfiguration moving = ProfileService.CreateProfileConfiguration(General, "Moving", "TestTube");
        List<ProfileConfiguration> removedFromGeneral = [];
        General.ProfileConfigurationRemoved += (_, e) => removedFromGeneral.Add(e.ProfileConfiguration);

        // The sidebar adds it to the new category and saves both categories
        games.AddProfileConfiguration(moving, null);
        ProfileService.SaveProfileCategory(games);
        ProfileService.SaveProfileCategory(General);

        moving.Category.ShouldBeSameAs(games);
        General.ProfileConfigurations.ShouldBe([staying]);
        staying.Order.ShouldBe(0);
        removedFromGeneral.ShouldBe([moving]);

        IProfileService restarted = CreateNewProfileService();
        restarted.ProfileCategories.Single(c => c.Name == "Games").ProfileConfigurations.Select(c => c.Name).ShouldBe(["Moving"]);
        restarted.ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Select(c => c.Name).ShouldBe(["Staying"]);
    }

    [Fact]
    public void RemoveProfileConfiguration_DeactivatesAndRemovesIt()
    {
        ProfileConfiguration first = ProfileService.CreateProfileConfiguration(General, "First", "TestTube");
        ProfileConfiguration second = ProfileService.CreateProfileConfiguration(General, "Second", "TestTube");
        ProfileService.ActivateProfile(second);
        List<ProfileConfiguration> removed = [];
        ProfileService.ProfileRemoved += (_, e) => removed.Add(e.ProfileConfiguration);

        ProfileService.RemoveProfileConfiguration(second);

        second.Profile.ShouldBeNull();
        General.ProfileConfigurations.ShouldBe([first]);
        first.Order.ShouldBe(0);
        removed.ShouldBe([second]);
        GetStoredProfiles().Select(p => p.ProfileConfiguration.Name).ShouldBe(["First"]);
    }

    [Fact]
    public void CloneProfileConfiguration_HasItsOwnCopyOfTheStoredData()
    {
        ProfileConfiguration original = ProfileService.CreateProfileConfiguration(General, "Original", "TestTube");

        ProfileConfiguration clone = ProfileService.CloneProfileConfiguration(original);

        clone.ShouldNotBeSameAs(original);
        clone.Name.ShouldBe("Original");
        clone.Category.ShouldBeSameAs(General);
        clone.ProfileId.ShouldBe(original.ProfileId);
        // The clone is not added to the category
        General.ProfileConfigurations.ShouldBe([original]);

        clone.Entity.ShouldNotBeSameAs(original.Entity);
        clone.Entity.Profile.ShouldNotBeSameAs(original.Entity.Profile);
        clone.Name = "Changed through the clone";
        clone.Save();
        original.Entity.ProfileConfiguration.Name.ShouldBe("Original");
    }

    [Fact]
    public void ProfileCategories_AreStoredWithTheirProfiles()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");

        ProfileCategoryEntity stored = GetStoredCategories().Single(c => c.Name == "General");

        stored.ProfileConfigurations.Single().ProfileConfiguration.ProfileId.ShouldBe(configuration.ProfileId);
        stored.ProfileConfigurations.Single().ProfileConfiguration.ProfileCategoryId.ShouldBe(General.EntityId);
    }
}
