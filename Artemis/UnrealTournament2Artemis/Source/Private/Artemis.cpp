// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#include "Artemis.h"

#include "UnrealTournament.h"
#include "UTPlayerController.h"
#include "UTGameState.h"
#include "UTArmor.h"
#include "UTTimedPowerup.h"

DEFINE_LOG_CATEGORY_STATIC(LogUTKBLightShow, Log, All);

AArtemis::AArtemis(const FObjectInitializer& ObjectInitializer)
	: Super(ObjectInitializer)
{
}

FArtemis::FArtemis()
{
	FrameTimeMinimum = 0.03f;
	DeltaTimeAccumulator = 0;
}

IMPLEMENT_MODULE(FArtemis, Artemis)

void FArtemis::StartupModule()
{
	WritePipe(FString(TEXT("Unreal Tournament plugin loaded")));
}

void FArtemis::ShutdownModule()
{

}

void FArtemis::Tick(float DeltaTime)
{
	if (GIsEditor)
	{
		return;
	}

	// Avoid double ticking
	if (LastFrameCounter > 0 && LastFrameCounter == GFrameCounter)
	{
		return;
	}
		
	LastFrameCounter = GFrameCounter;

	// We may be going 120hz, don't spam the pipe
	DeltaTimeAccumulator += DeltaTime;
	if (DeltaTimeAccumulator < FrameTimeMinimum)
	{
		return;
	}
	DeltaTimeAccumulator = 0;
	
	// Setup JSON object
	TSharedRef<FJsonObject> RootJson(new FJsonObject());
	TSharedRef<FJsonObject> PlayerJson(new FJsonObject());
	TSharedRef<FJsonObject> EnvironmentJson(new FJsonObject());
	RootJson->SetObjectField("Player", PlayerJson);
	RootJson->SetObjectField("Environment", EnvironmentJson);
	// Setup JSON writer to be used before returning
	FString Buffer;
	TSharedRef<TJsonWriter<> > Writer = TJsonWriterFactory<>::Create(&Buffer);

	AUTPlayerController* UTPC = nullptr;
	AUTGameState* GS = nullptr;
	const TIndirectArray<FWorldContext>& AllWorlds = GEngine->GetWorldContexts();
	for (const FWorldContext& Context : AllWorlds)
	{
		UWorld* World = Context.World();
		if (World && World->WorldType == EWorldType::Game)
		{
			UTPC = Cast<AUTPlayerController>(GEngine->GetFirstLocalPlayerController(World));
			if (UTPC)
			{
				UUTLocalPlayer* UTLP = Cast<UUTLocalPlayer>(UTPC->GetLocalPlayer());
				if (UTLP == nullptr || UTLP->IsMenuGame())
				{
					UTPC = nullptr;
					continue;
				}

				GS = World->GetGameState<AUTGameState>();
				break;
			}
		}
	}

	if (!UTPC || !GS)
	{
		RootJson->SetStringField("State", "MainMenu");
		FJsonSerializer::Serialize(RootJson, Writer);
		WritePipe(Buffer);
		return;
	}

	// Update environment data	
	if (GS->GetGameModeClass())
	{
		EnvironmentJson->SetStringField("GameMode", GS->GetGameModeClass()->GetName());
	}
	EnvironmentJson->SetBoolField("MatchStarted", GS->HasMatchStarted());
	EnvironmentJson->SetNumberField("GoalScore", GS->GoalScore);
	// Insert GameState JsonReport
	GS->MakeJsonReport(EnvironmentJson);
	// The JsonReport may contain all players, which is a bit too much
	if (EnvironmentJson->HasField("Players")) 
	{
		EnvironmentJson->RemoveField("Players");
	}

	// Update player data
	// If character not found player must be spectating(?)
	if (!UTPC->GetUTCharacter()) 
	{
		RootJson->SetStringField("State", "Spectating");
	}
	// If dead, don't try reading HP/Armor
	else if (UTPC->GetUTCharacter()->IsDead()) 
	{
		RootJson->SetStringField("State", "Dead");
		PlayerJson->SetNumberField("Health", 0);
		PlayerJson->SetNumberField("Armor", 0);
	}
	// Player is found and alive
	else 
	{
		// Update HP and armor
		RootJson->SetStringField("State", "Alive");
		PlayerJson->SetNumberField("Health", UTPC->GetUTCharacter()->Health);
		// TODO: Crashes the game
		// PlayerJson->SetNumberField("Armor", UTPC->GetUTCharacter()->GetArmorAmount());

		// Update player powerups data
		TSharedRef<FJsonObject> InventoryJson(new FJsonObject());
		PlayerJson->SetObjectField("Inventory", InventoryJson);
		InventoryJson->SetBoolField("HasJumpBoots", false);
		InventoryJson->SetBoolField("HasInvisibility", false);
		InventoryJson->SetBoolField("HasBerserk", false);
		InventoryJson->SetBoolField("HasUDamage", false);
		InventoryJson->SetBoolField("HasThighPads", false);
		InventoryJson->SetBoolField("HasShieldBelt", false);
		InventoryJson->SetBoolField("HasChestArmor", false);
		InventoryJson->SetBoolField("HasHelmet", false);
				

		for (TInventoryIterator<> It(UTPC->GetUTCharacter()); It; ++It)
		{
			AUTInventory* InventoryItem = (*It);
			// Using Contains here because pickups might have slighty different names in different contexts
			if (InventoryItem->GetClass()->GetName().Contains("Armor_ThighPads")) 
			{
				InventoryJson->SetBoolField("HasThighPads", true);
			}
			else if (InventoryItem->GetClass()->GetName().Contains("Armor_ShieldBelt"))
			{
				InventoryJson->SetBoolField("HasShieldBelt", true);
			}
			else if (InventoryItem->GetClass()->GetName().Contains("Armor_Chest")) 
			{
				InventoryJson->SetBoolField("HasChestArmor", true);
			}
			else if (InventoryItem->GetClass()->GetName().Contains("Armor_Helmet")) 
			{
				InventoryJson->SetBoolField("HasHelmet", true);
			}
			else if (InventoryItem->GetClass()->GetName().Contains("JumpBoots")) 
			{
				InventoryJson->SetBoolField("HasJumpBoots", true);
			} 
			else if (InventoryItem->GetClass()->GetName().Contains("Invis")) 
			{
				InventoryJson->SetBoolField("HasInvisibility", true);
			} 
			else if (InventoryItem->GetClass()->GetName().Contains("Berserk")) 
			{
				InventoryJson->SetBoolField("HasBerserk", true);
			}
			else if (InventoryItem->GetClass()->GetName().Contains("UDamage")) 
			{
				InventoryJson->SetBoolField("HasUDamage", true);
			}
		}

		// Update player weapon data
		TSharedRef<FJsonObject> WeaponJson(new FJsonObject());
		PlayerJson->SetObjectField("Weapon", WeaponJson);
		if (UTPC->GetUTCharacter()->GetWeapon()) 
		{
			WeaponJson->SetStringField("Name", UTPC->GetUTCharacter()->GetWeapon()->GetClass()->GetName());
			WeaponJson->SetNumberField("Ammo", UTPC->GetUTCharacter()->GetWeapon()->Ammo);
			WeaponJson->SetNumberField("MaxAmmo", UTPC->GetUTCharacter()->GetWeapon()->MaxAmmo);
			WeaponJson->SetBoolField("IsFiring", UTPC->GetUTCharacter()->GetWeapon()->IsFiring());
			WeaponJson->SetNumberField("FireMode", UTPC->GetUTCharacter()->GetWeapon()->GetCurrentFireMode());
			WeaponJson->SetNumberField("ZoomState", UTPC->GetUTCharacter()->GetWeapon()->ZoomState);
		}
		else {
			WeaponJson->SetStringField("Name", "None");
		}
	}

	// Insert PlayerState JsonReport
	TSharedRef<FJsonObject> PlayerStateJson(new FJsonObject());
	PlayerJson->SetObjectField("State", PlayerStateJson);
	if (UTPC->UTPlayerState) 
	{
		UTPC->UTPlayerState->MakeJsonReport(PlayerStateJson);
	}

	FJsonSerializer::Serialize(RootJson, Writer);
	WritePipe(Buffer);
}

void FArtemis::WritePipe(FString msg)
{
	pipe = CreateFile(TEXT("\\\\.\\pipe\\artemis"), GENERIC_WRITE, 0, nullptr, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, nullptr);
	if (pipe == nullptr || pipe == INVALID_HANDLE_VALUE)
	{
		return;
	}

	uint32 BytesWritten = 0;
	WriteFile(pipe, TCHAR_TO_ANSI(*msg), msg.Len(), (::DWORD*)&BytesWritten, nullptr);
}