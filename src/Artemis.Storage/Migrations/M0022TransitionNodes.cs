using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;
using Artemis.Storage.Entities.Profile.Nodes;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations;

public class M0022TransitionNodes : IStorageMigration
{
    private void MigrateNodeScript(NodeScriptEntity nodeScript)
    {
        if (nodeScript == null)
            return;

        foreach (NodeEntity node in nodeScript.Nodes)
        {
            if (node.Type == "NumericEasingNode")
                node.Type = "NumericTransitionNode";
            else if (node.Type == "ColorGradientEasingNode")
                node.Type = "ColorGradientTransitionNode";
            else if (node.Type == "SKColorEasingNode")
                node.Type = "SKColorTransitionNode";   
            else if (node.Type == "EasingTypeNode")
                node.Type = "EasingFunctionNode";
        }
    }

    private void MigratePropertyGroup(PropertyGroupEntity propertyGroup)
    {
        if (propertyGroup == null)
            return;

        foreach (PropertyGroupEntity childPropertyGroup in propertyGroup.PropertyGroups)
            MigratePropertyGroup(childPropertyGroup);
        foreach (PropertyEntity property in propertyGroup.Properties)
            MigrateNodeScript(property.DataBinding?.NodeScript);
    }

    private void MigrateDisplayCondition(IConditionEntity conditionEntity)
    {
        if (conditionEntity is EventConditionEntity eventConditionEntity)
            MigrateNodeScript(eventConditionEntity.Script);
        else if (conditionEntity is StaticConditionEntity staticConditionEntity)
            MigrateNodeScript(staticConditionEntity.Script);
    }

    public int UserVersion => 22;

    public void Apply(LiteRepository repository)
    {
        // Migrate profile configuration display conditions
        List<ProfileCategoryEntity> categories = repository.Query<ProfileCategoryEntity>().ToList();
        foreach (ProfileCategoryEntity profileCategoryEntity in categories)
        {
            foreach (ProfileConfigurationEntity profileConfigurationEntity in profileCategoryEntity.ProfileConfigurations)
                MigrateNodeScript(profileConfigurationEntity.ActivationCondition);
            repository.Update(profileCategoryEntity);
        }

        // Migrate profile display conditions and data bindings
        List<ProfileEntity> profiles = repository.Query<ProfileEntity>().ToList();
        foreach (ProfileEntity profileEntity in profiles)
        {
            foreach (LayerEntity layer in profileEntity.Layers)
            {
                MigratePropertyGroup(layer.LayerBrush?.PropertyGroup);
                MigratePropertyGroup(layer.GeneralPropertyGroup);
                MigratePropertyGroup(layer.TransformPropertyGroup);
                foreach (LayerEffectEntity layerEffectEntity in layer.LayerEffects)
                    MigratePropertyGroup(layerEffectEntity?.PropertyGroup);
                MigrateDisplayCondition(layer.DisplayCondition);
            }

            foreach (FolderEntity folder in profileEntity.Folders)
            {
                foreach (LayerEffectEntity folderLayerEffect in folder.LayerEffects)
                    MigratePropertyGroup(folderLayerEffect?.PropertyGroup);
                MigrateDisplayCondition(folder.DisplayCondition);
            }

            repository.Update(profileEntity);
        }
    }
}