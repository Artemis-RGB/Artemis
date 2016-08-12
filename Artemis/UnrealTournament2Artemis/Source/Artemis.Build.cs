using System.IO;

namespace UnrealBuildTool.Rules
{
    public class Artemis : ModuleRules
    {
        public Artemis(TargetInfo Target)
        {
            PublicDependencyModuleNames.AddRange(
                new string[]
                {
                    "Core",
                    "CoreUObject",
                    "Engine",
                    "UnrealTournament",
                    "InputCore",
                    "SlateCore",
                    "Json"
                }
                );
        }
    }
}