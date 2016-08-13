// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.
#pragma once

#include "Core.h"
#include "UnrealTournament.h"
#include "JsonSerializer.h"
#include "JsonObject.h"
#include "JsonReader.h"

#include "Artemis.generated.h"

UCLASS(Blueprintable, Meta = (ChildCanTick))
class AArtemis : public AActor
{
	GENERATED_UCLASS_BODY()

};

struct FArtemis : FTickableGameObject, IModuleInterface
{
	FArtemis();
	virtual void Tick(float DeltaTime) override;
	virtual bool IsTickable() const override { return true; }
	virtual bool IsTickableInEditor() const override { return true; }
	virtual bool IsTickableWhenPaused() const override { return true; }

	virtual void StartupModule() override;
	virtual void ShutdownModule() override;

	// Put a real stat id here
	virtual TStatId GetStatId() const
	{
		return TStatId();
	}

	void WritePipe(FString msg);
	HANDLE pipe;
	float DeltaTimeAccumulator;
	float FrameTimeMinimum;	
	uint64 LastFrameCounter;
};