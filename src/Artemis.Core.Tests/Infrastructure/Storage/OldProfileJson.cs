using System.Text.Json.Nodes;

namespace Artemis.Core.Tests.Infrastructure.Storage;

/// <summary>
///     Profile JSON the way older versions of Artemis stored and exported it, for testing the profile migrations.
///     <para>
///         Before version 3 profiles were written by JSON.NET with type names turned on: every object starts with a
///         <c>$type</c> naming its .NET class and every list is wrapped in <c>{"$type": ..., "$values": [...]}</c>. Node
///         storage is a JSON string inside the JSON, written the same way. Before version 1 the general nodes lived in
///         the <c>Artemis.VisualScripting.Nodes</c> namespace and nodes had no <c>ProviderId</c>.
///     </para>
/// </summary>
public static class OldProfileJson
{
    public const string ProfileId = "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10";
    public const string CategoryId = "c4f1d1a0-5b3e-4a7d-8e9f-0a1b2c3d4e5f";
    public const string GeneralNodesProviderId = "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78";

    // The nodes in the JSON below, by where they are
    public const string ActivationExitNodeId = "a0000000-0000-0000-0000-000000000001";
    public const string ActivationDataModelNodeId = "a0000000-0000-0000-0000-000000000002";
    public const string FolderEffectNodeId = "b0000000-0000-0000-0000-000000000001";
    public const string FolderConditionNodeId = "c0000000-0000-0000-0000-000000000001";
    public const string LayerConditionNodeId = "c0000000-0000-0000-0000-000000000002";
    public const string LayerTransformNodeId = "d0000000-0000-0000-0000-000000000001";
    public const string LayerBrushNodeId = "e0000000-0000-0000-0000-000000000001";

    /// <summary>
    ///     A profile configuration from before version 1, it has no <c>Version</c> yet. Its activation condition reads a
    ///     data model value.
    /// </summary>
    public const string Version0Configuration =
        """
        {
          "$type": "Artemis.Storage.Entities.Profile.ProfileConfigurationEntity, Artemis.Storage",
          "Name": "Old profile",
          "MaterialIcon": "Keyboard",
          "IconType": 0,
          "IconFill": false,
          "Order": 0,
          "IsSuspended": false,
          "ActivationBehaviour": 1,
          "ActivationCondition": {
            "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeScriptEntity, Artemis.Storage",
            "Name": "Activate profile",
            "Description": "Whether or not the profile should be active",
            "Nodes": {
              "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage]], System.Private.CoreLib",
              "$values": [
                {
                  "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                  "Id": "a0000000-0000-0000-0000-000000000001",
                  "Type": "Artemis.Core.Internal.ExitNode`1",
                  "Name": "Activate profile",
                  "Description": "Whether or not the profile should be active",
                  "IsExitNode": true,
                  "X": 300.0,
                  "Y": 0.0,
                  "Storage": "",
                  "PinCollections": {
                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                    "$values": []
                  }
                },
                {
                  "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                  "Id": "a0000000-0000-0000-0000-000000000002",
                  "Type": "Artemis.VisualScripting.Nodes.DataModel.DataModelNode",
                  "Name": "Data Model-Value",
                  "Description": "Outputs a selectable data model value",
                  "IsExitNode": false,
                  "X": 0.0,
                  "Y": 0.0,
                  "Storage": "{\"$type\":\"Artemis.Storage.Entities.Profile.DataModelPathEntity, Artemis.Storage\",\"Path\":\"IsInGame\",\"DataModelId\":\"Artemis.Plugins.Modules.Game-12345678\"}",
                  "PinCollections": {
                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                    "$values": []
                  }
                }
              ]
            },
            "Connections": {
              "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage]], System.Private.CoreLib",
              "$values": [
                {
                  "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage",
                  "SourceType": "System.Boolean",
                  "SourceNode": "a0000000-0000-0000-0000-000000000002",
                  "TargetNode": "a0000000-0000-0000-0000-000000000001",
                  "SourcePinCollectionId": 0,
                  "SourcePinId": 0,
                  "TargetType": "System.Boolean",
                  "TargetPinCollectionId": 0,
                  "TargetPinId": 0
                }
              ]
            }
          },
          "HotkeyMode": 1,
          "EnableHotkey": {
            "$type": "Artemis.Storage.Entities.Profile.ProfileConfigurationHotkeyEntity, Artemis.Storage",
            "Key": 44,
            "Modifiers": 2
          },
          "DisableHotkey": null,
          "ModuleId": "Artemis.Plugins.Modules.Game-12345678",
          "ProfileCategoryId": "c4f1d1a0-5b3e-4a7d-8e9f-0a1b2c3d4e5f",
          "ProfileId": "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10",
          "FadeInAndOut": true
        }
        """;

    /// <summary>
    ///     A profile from before version 1 with nodes in every place the migrations look for them:
    ///     <list type="bullet">
    ///         <item>the static display condition of the root folder</item>
    ///         <item>a data binding on a property of the blur effect on the root folder</item>
    ///         <item>the event display condition of the layer</item>
    ///         <item>a data binding in the transform properties of the layer</item>
    ///         <item>a data binding in a property group inside the brush properties of the layer</item>
    ///     </list>
    ///     The layer also has adaption hints and LEDs, whose type names have to survive the conversion.
    /// </summary>
    public const string Version0Profile =
        """
        {
          "$type": "Artemis.Storage.Entities.Profile.ProfileEntity, Artemis.Storage",
          "Id": "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10",
          "Name": "Old profile",
          "IsFreshImport": false,
          "Folders": {
            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.FolderEntity, Artemis.Storage]], System.Private.CoreLib",
            "$values": [
              {
                "$type": "Artemis.Storage.Entities.Profile.FolderEntity, Artemis.Storage",
                "Order": 1,
                "Name": "Root folder",
                "IsExpanded": true,
                "Suspended": false,
                "Id": "f0000000-0000-0000-0000-000000000001",
                "ParentId": "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10",
                "LayerEffects": {
                  "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.LayerEffectEntity, Artemis.Storage]], System.Private.CoreLib",
                  "$values": [
                    {
                      "$type": "Artemis.Storage.Entities.Profile.LayerEffectEntity, Artemis.Storage",
                      "ProviderId": "Artemis.Plugins.LayerEffects.Filter.FilterEffectProvider-0c6e3b7d",
                      "EffectType": "BlurEffect",
                      "Name": "Blur",
                      "HasBeenRenamed": false,
                      "Order": 1,
                      "PropertyGroup": {
                        "$type": "Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage",
                        "Identifier": "LayerEffect",
                        "Properties": {
                          "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage]], System.Private.CoreLib",
                          "$values": [
                            {
                              "$type": "Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage",
                              "Identifier": "BlurAmount",
                              "Value": "{\"Width\":10.0,\"Height\":10.0}",
                              "KeyframesEnabled": false,
                              "DataBinding": {
                                "$type": "Artemis.Storage.Entities.Profile.DataBindings.DataBindingEntity, Artemis.Storage",
                                "IsEnabled": true,
                                "NodeScript": {
                                  "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeScriptEntity, Artemis.Storage",
                                  "Name": "Blur amount",
                                  "Description": "The data binding of the blur amount",
                                  "Nodes": {
                                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage]], System.Private.CoreLib",
                                    "$values": [
                                      {
                                        "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                                        "Id": "b0000000-0000-0000-0000-000000000001",
                                        "Type": "Artemis.VisualScripting.Nodes.Mathematics.MathExpressionNode",
                                        "Name": "Math Expression",
                                        "Description": "Outputs the result of a math expression",
                                        "IsExitNode": false,
                                        "X": 0.0,
                                        "Y": 0.0,
                                        "Storage": "\"1 + 1\"",
                                        "PinCollections": {
                                          "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                                          "$values": [
                                            {
                                              "$type": "Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage",
                                              "Id": 0,
                                              "Direction": 0,
                                              "Amount": 2
                                            }
                                          ]
                                        }
                                      }
                                    ]
                                  },
                                  "Connections": {
                                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage]], System.Private.CoreLib",
                                    "$values": []
                                  }
                                }
                              },
                              "KeyframeEntities": {
                                "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.KeyframeEntity, Artemis.Storage]], System.Private.CoreLib",
                                "$values": []
                              }
                            }
                          ]
                        },
                        "PropertyGroups": {
                          "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage]], System.Private.CoreLib",
                          "$values": []
                        }
                      }
                    }
                  ]
                },
                "DisplayCondition": {
                  "$type": "Artemis.Storage.Entities.Profile.Conditions.StaticConditionEntity, Artemis.Storage",
                  "PlayMode": 0,
                  "StopMode": 1,
                  "Script": {
                    "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeScriptEntity, Artemis.Storage",
                    "Name": "Display condition",
                    "Description": "Whether or not the folder should be displayed",
                    "Nodes": {
                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage]], System.Private.CoreLib",
                      "$values": [
                        {
                          "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                          "Id": "c0000000-0000-0000-0000-000000000001",
                          "Type": "Artemis.VisualScripting.Nodes.DataModel.DataModelNode",
                          "Name": "Data Model-Value",
                          "Description": "Outputs a selectable data model value",
                          "IsExitNode": false,
                          "X": 0.0,
                          "Y": 0.0,
                          "Storage": "{\"$type\":\"Artemis.Storage.Entities.Profile.DataModelPathEntity, Artemis.Storage\",\"Path\":\"Player.IsAlive\",\"DataModelId\":\"Artemis.Plugins.Modules.Game-12345678\"}",
                          "PinCollections": {
                            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                            "$values": []
                          }
                        }
                      ]
                    },
                    "Connections": {
                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage]], System.Private.CoreLib",
                      "$values": []
                    }
                  }
                },
                "Timeline": {
                  "$type": "Artemis.Storage.Entities.Profile.TimelineEntity, Artemis.Storage",
                  "StartSegmentLength": "00:00:00",
                  "MainSegmentLength": "00:00:05",
                  "EndSegmentLength": "00:00:00"
                }
              }
            ]
          },
          "Layers": {
            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.LayerEntity, Artemis.Storage]], System.Private.CoreLib",
            "$values": [
              {
                "$type": "Artemis.Storage.Entities.Profile.LayerEntity, Artemis.Storage",
                "Order": 1,
                "Name": "Gradient layer",
                "Suspended": false,
                "Leds": {
                  "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.LedEntity, Artemis.Storage]], System.Private.CoreLib",
                  "$values": [
                    {
                      "$type": "Artemis.Storage.Entities.Profile.LedEntity, Artemis.Storage",
                      "LedName": "Keyboard_A",
                      "DeviceIdentifier": "Artemis.Plugins.Devices.Test-Keyboard",
                      "PhysicalLayout": 1
                    },
                    {
                      "$type": "Artemis.Storage.Entities.Profile.LedEntity, Artemis.Storage",
                      "LedName": "Keyboard_S",
                      "DeviceIdentifier": "Artemis.Plugins.Devices.Test-Keyboard",
                      "PhysicalLayout": null
                    }
                  ]
                },
                "AdaptionHints": {
                  "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.AdaptionHints.IAdaptionHintEntity, Artemis.Storage]], System.Private.CoreLib",
                  "$values": [
                    {
                      "$type": "Artemis.Storage.Entities.Profile.AdaptionHints.CategoryAdaptionHintEntity, Artemis.Storage",
                      "Category": 1,
                      "LimitAmount": true,
                      "Skip": 0,
                      "Amount": 1
                    },
                    {
                      "$type": "Artemis.Storage.Entities.Profile.AdaptionHints.KeyboardSectionAdaptionHintEntity, Artemis.Storage",
                      "Section": 2
                    }
                  ]
                },
                "GeneralPropertyGroup": {
                  "$type": "Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage",
                  "Identifier": "General",
                  "Properties": {
                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage]], System.Private.CoreLib",
                    "$values": [
                      {
                        "$type": "Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage",
                        "Identifier": "ShapeType",
                        "Value": "1",
                        "KeyframesEnabled": false,
                        "DataBinding": null,
                        "KeyframeEntities": {
                          "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.KeyframeEntity, Artemis.Storage]], System.Private.CoreLib",
                          "$values": []
                        }
                      }
                    ]
                  },
                  "PropertyGroups": {
                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage]], System.Private.CoreLib",
                    "$values": []
                  }
                },
                "TransformPropertyGroup": {
                  "$type": "Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage",
                  "Identifier": "Transform",
                  "Properties": {
                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage]], System.Private.CoreLib",
                    "$values": [
                      {
                        "$type": "Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage",
                        "Identifier": "Opacity",
                        "Value": "100.0",
                        "KeyframesEnabled": true,
                        "DataBinding": {
                          "$type": "Artemis.Storage.Entities.Profile.DataBindings.DataBindingEntity, Artemis.Storage",
                          "IsEnabled": false,
                          "NodeScript": {
                            "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeScriptEntity, Artemis.Storage",
                            "Name": "Opacity",
                            "Description": "The data binding of the opacity",
                            "Nodes": {
                              "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage]], System.Private.CoreLib",
                              "$values": [
                                {
                                  "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                                  "Id": "d0000000-0000-0000-0000-000000000001",
                                  "Type": "Artemis.VisualScripting.Nodes.Static.StaticNumericValueNode",
                                  "Name": "Numeric-Value",
                                  "Description": "Outputs a configurable static numeric value",
                                  "IsExitNode": false,
                                  "X": 0.0,
                                  "Y": 0.0,
                                  "Storage": "50.0",
                                  "PinCollections": {
                                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                                    "$values": []
                                  }
                                }
                              ]
                            },
                            "Connections": {
                              "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage]], System.Private.CoreLib",
                              "$values": []
                            }
                          }
                        },
                        "KeyframeEntities": {
                          "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.KeyframeEntity, Artemis.Storage]], System.Private.CoreLib",
                          "$values": [
                            {
                              "$type": "Artemis.Storage.Entities.Profile.KeyframeEntity, Artemis.Storage",
                              "Position": "00:00:01",
                              "Timeline": 0,
                              "Value": "25.0",
                              "EasingFunction": 3
                            }
                          ]
                        }
                      }
                    ]
                  },
                  "PropertyGroups": {
                    "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage]], System.Private.CoreLib",
                    "$values": []
                  }
                },
                "LayerBrush": {
                  "$type": "Artemis.Storage.Entities.Profile.LayerBrushEntity, Artemis.Storage",
                  "ProviderId": "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-ed2e1c49",
                  "BrushType": "ColorBrush",
                  "PropertyGroup": {
                    "$type": "Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage",
                    "Identifier": "LayerBrush",
                    "Properties": {
                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage]], System.Private.CoreLib",
                      "$values": []
                    },
                    "PropertyGroups": {
                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage]], System.Private.CoreLib",
                      "$values": [
                        {
                          "$type": "Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage",
                          "Identifier": "Colors",
                          "Properties": {
                            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage]], System.Private.CoreLib",
                            "$values": [
                              {
                                "$type": "Artemis.Storage.Entities.Profile.PropertyEntity, Artemis.Storage",
                                "Identifier": "Gradient",
                                "Value": "[{\"Color\":\"#ffff0000\",\"Position\":0.0},{\"Color\":\"#ff0000ff\",\"Position\":1.0}]",
                                "KeyframesEnabled": false,
                                "DataBinding": {
                                  "$type": "Artemis.Storage.Entities.Profile.DataBindings.DataBindingEntity, Artemis.Storage",
                                  "IsEnabled": true,
                                  "NodeScript": {
                                    "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeScriptEntity, Artemis.Storage",
                                    "Name": "Gradient",
                                    "Description": "The data binding of the gradient",
                                    "Nodes": {
                                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage]], System.Private.CoreLib",
                                      "$values": [
                                        {
                                          "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                                          "Id": "e0000000-0000-0000-0000-000000000001",
                                          "Type": "Artemis.VisualScripting.Nodes.Color.ColorGradientNode",
                                          "Name": "Color Gradient",
                                          "Description": "Outputs a color gradient with the given colors",
                                          "IsExitNode": false,
                                          "X": 0.0,
                                          "Y": 0.0,
                                          "Storage": "{\"$type\":\"Artemis.Core.ColorGradient, Artemis.Core\",\"$values\":[{\"$type\":\"Artemis.Core.ColorGradientStop, Artemis.Core\",\"Color\":\"#ffff0000\",\"Position\":0.0},{\"$type\":\"Artemis.Core.ColorGradientStop, Artemis.Core\",\"Color\":\"#ff0000ff\",\"Position\":1.0}]}",
                                          "PinCollections": {
                                            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                                            "$values": []
                                          }
                                        }
                                      ]
                                    },
                                    "Connections": {
                                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage]], System.Private.CoreLib",
                                      "$values": []
                                    }
                                  }
                                },
                                "KeyframeEntities": {
                                  "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.KeyframeEntity, Artemis.Storage]], System.Private.CoreLib",
                                  "$values": []
                                }
                              }
                            ]
                          },
                          "PropertyGroups": {
                            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.PropertyGroupEntity, Artemis.Storage]], System.Private.CoreLib",
                            "$values": []
                          }
                        }
                      ]
                    }
                  }
                },
                "Id": "f0000000-0000-0000-0000-000000000002",
                "ParentId": "f0000000-0000-0000-0000-000000000001",
                "LayerEffects": {
                  "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.LayerEffectEntity, Artemis.Storage]], System.Private.CoreLib",
                  "$values": []
                },
                "DisplayCondition": {
                  "$type": "Artemis.Storage.Entities.Profile.Conditions.EventConditionEntity, Artemis.Storage",
                  "TriggerMode": 0,
                  "OverlapMode": 1,
                  "ToggleOffMode": 0,
                  "EventPath": {
                    "$type": "Artemis.Storage.Entities.Profile.DataModelPathEntity, Artemis.Storage",
                    "Path": "Player.Died",
                    "DataModelId": "Artemis.Plugins.Modules.Game-12345678"
                  },
                  "Script": {
                    "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeScriptEntity, Artemis.Storage",
                    "Name": "Event condition",
                    "Description": "Whether or not the event should trigger",
                    "Nodes": {
                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage]], System.Private.CoreLib",
                      "$values": [
                        {
                          "$type": "Artemis.Storage.Entities.Profile.Nodes.NodeEntity, Artemis.Storage",
                          "Id": "c0000000-0000-0000-0000-000000000002",
                          "Type": "Artemis.VisualScripting.Nodes.Static.StaticBooleanValueNode",
                          "Name": "Boolean-Value",
                          "Description": "Outputs a configurable static boolean value",
                          "IsExitNode": false,
                          "X": 0.0,
                          "Y": 0.0,
                          "Storage": "true",
                          "PinCollections": {
                            "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodePinCollectionEntity, Artemis.Storage]], System.Private.CoreLib",
                            "$values": []
                          }
                        }
                      ]
                    },
                    "Connections": {
                      "$type": "System.Collections.Generic.List`1[[Artemis.Storage.Entities.Profile.Nodes.NodeConnectionEntity, Artemis.Storage]], System.Private.CoreLib",
                      "$values": []
                    }
                  }
                },
                "Timeline": {
                  "$type": "Artemis.Storage.Entities.Profile.TimelineEntity, Artemis.Storage",
                  "StartSegmentLength": "00:00:00",
                  "MainSegmentLength": "00:00:02",
                  "EndSegmentLength": "00:00:01"
                }
              }
            ]
          }
        }
        """;

    /// <summary>
    ///     <see cref="Version0Configuration" /> after all migrations, as stored by the current version
    /// </summary>
    public const string MigratedConfiguration =
        """
        {
          "Name": "Old profile",
          "MaterialIcon": "Keyboard",
          "IconType": 0,
          "IconFill": false,
          "Order": 0,
          "IsSuspended": false,
          "ActivationBehaviour": 1,
          "ActivationCondition": {
            "Name": "Activate profile",
            "Description": "Whether or not the profile should be active",
            "Nodes": [
              {
                "Id": "a0000000-0000-0000-0000-000000000001",
                "Type": "Artemis.Core.Internal.ExitNode`1",
                "Name": "Activate profile",
                "Description": "Whether or not the profile should be active",
                "IsExitNode": true,
                "X": 300.0,
                "Y": 0.0,
                "Storage": "",
                "PinCollections": [],
                "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
              },
              {
                "Id": "a0000000-0000-0000-0000-000000000002",
                "Type": "Artemis.Plugins.Nodes.General.Nodes.DataModel.DataModelNode",
                "Name": "Data Model-Value",
                "Description": "Outputs a selectable data model value",
                "IsExitNode": false,
                "X": 0.0,
                "Y": 0.0,
                "Storage": "{\"Path\":\"IsInGame\",\"DataModelId\":\"Artemis.Plugins.Modules.Game-12345678\"}",
                "PinCollections": [],
                "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
              }
            ],
            "Connections": [
              {
                "SourceType": "System.Boolean",
                "SourceNode": "a0000000-0000-0000-0000-000000000002",
                "TargetNode": "a0000000-0000-0000-0000-000000000001",
                "SourcePinCollectionId": 0,
                "SourcePinId": 0,
                "TargetType": "System.Boolean",
                "TargetPinCollectionId": 0,
                "TargetPinId": 0
              }
            ]
          },
          "HotkeyMode": 1,
          "EnableHotkey": {
            "Key": 44,
            "Modifiers": 2
          },
          "DisableHotkey": null,
          "ModuleId": "Artemis.Plugins.Modules.Game-12345678",
          "ProfileCategoryId": "c4f1d1a0-5b3e-4a7d-8e9f-0a1b2c3d4e5f",
          "ProfileId": "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10",
          "FadeInAndOut": true,
          "Version": 4
        }
        """;

    /// <summary>
    ///     <see cref="Version0Profile" /> after all migrations, as stored by the current version
    /// </summary>
    public const string MigratedProfile =
        """
        {
          "Id": "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10",
          "Name": "Old profile",
          "IsFreshImport": false,
          "Folders": [
            {
              "Order": 1,
              "Name": "Root folder",
              "IsExpanded": true,
              "Suspended": false,
              "Id": "f0000000-0000-0000-0000-000000000001",
              "ParentId": "8f6a1c2e-8d0b-4b8e-9c6e-2f2d3f1b7a10",
              "LayerEffects": [
                {
                  "ProviderId": "Artemis.Plugins.LayerEffects.Filter.FilterEffectProvider-0c6e3b7d",
                  "EffectType": "BlurEffect",
                  "Name": "Blur",
                  "HasBeenRenamed": false,
                  "Order": 1,
                  "PropertyGroup": {
                    "Identifier": "LayerEffect",
                    "Properties": [
                      {
                        "Identifier": "BlurAmount",
                        "Value": "{\"Width\":10.0,\"Height\":10.0}",
                        "KeyframesEnabled": false,
                        "DataBinding": {
                          "IsEnabled": true,
                          "NodeScript": {
                            "Name": "Blur amount",
                            "Description": "The data binding of the blur amount",
                            "Nodes": [
                              {
                                "Id": "b0000000-0000-0000-0000-000000000001",
                                "Type": "Artemis.Plugins.Nodes.General.Nodes.Mathematics.MathExpressionNode",
                                "Name": "Math Expression",
                                "Description": "Outputs the result of a math expression",
                                "IsExitNode": false,
                                "X": 0.0,
                                "Y": 0.0,
                                "Storage": "\"1 + 1\"",
                                "PinCollections": [
                                  {
                                    "Id": 0,
                                    "Direction": 0,
                                    "Amount": 2
                                  }
                                ],
                                "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
                              }
                            ],
                            "Connections": []
                          }
                        },
                        "KeyframeEntities": []
                      }
                    ],
                    "PropertyGroups": []
                  }
                }
              ],
              "DisplayCondition": {
                "$type": "Static",
                "PlayMode": 0,
                "StopMode": 1,
                "Script": {
                  "Name": "Display condition",
                  "Description": "Whether or not the folder should be displayed",
                  "Nodes": [
                    {
                      "Id": "c0000000-0000-0000-0000-000000000001",
                      "Type": "Artemis.Plugins.Nodes.General.Nodes.DataModel.DataModelNode",
                      "Name": "Data Model-Value",
                      "Description": "Outputs a selectable data model value",
                      "IsExitNode": false,
                      "X": 0.0,
                      "Y": 0.0,
                      "Storage": "{\"Path\":\"Player.IsAlive\",\"DataModelId\":\"Artemis.Plugins.Modules.Game-12345678\"}",
                      "PinCollections": [],
                      "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
                    }
                  ],
                  "Connections": []
                }
              },
              "Timeline": {
                "StartSegmentLength": "00:00:00",
                "MainSegmentLength": "00:00:05",
                "EndSegmentLength": "00:00:00"
              }
            }
          ],
          "Layers": [
            {
              "Order": 1,
              "Name": "Gradient layer",
              "Suspended": false,
              "Leds": [
                {
                  "LedName": "Keyboard_A",
                  "DeviceIdentifier": "Artemis.Plugins.Devices.Test-Keyboard",
                  "PhysicalLayout": 1
                },
                {
                  "LedName": "Keyboard_S",
                  "DeviceIdentifier": "Artemis.Plugins.Devices.Test-Keyboard",
                  "PhysicalLayout": null
                }
              ],
              "AdaptionHints": [
                {
                  "$type": "Category",
                  "Category": 1,
                  "LimitAmount": true,
                  "Skip": 0,
                  "Amount": 1
                },
                {
                  "$type": "KeyboardSection",
                  "Section": 2
                }
              ],
              "GeneralPropertyGroup": {
                "Identifier": "General",
                "Properties": [
                  {
                    "Identifier": "ShapeType",
                    "Value": "1",
                    "KeyframesEnabled": false,
                    "DataBinding": null,
                    "KeyframeEntities": []
                  }
                ],
                "PropertyGroups": []
              },
              "TransformPropertyGroup": {
                "Identifier": "Transform",
                "Properties": [
                  {
                    "Identifier": "Opacity",
                    "Value": "100.0",
                    "KeyframesEnabled": true,
                    "DataBinding": {
                      "IsEnabled": false,
                      "NodeScript": {
                        "Name": "Opacity",
                        "Description": "The data binding of the opacity",
                        "Nodes": [
                          {
                            "Id": "d0000000-0000-0000-0000-000000000001",
                            "Type": "Artemis.Plugins.Nodes.General.Nodes.Static.StaticNumericValueNode",
                            "Name": "Numeric-Value",
                            "Description": "Outputs a configurable static numeric value",
                            "IsExitNode": false,
                            "X": 0.0,
                            "Y": 0.0,
                            "Storage": "50.0",
                            "PinCollections": [],
                            "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
                          }
                        ],
                        "Connections": []
                      }
                    },
                    "KeyframeEntities": [
                      {
                        "Position": "00:00:01",
                        "Timeline": 0,
                        "Value": "25.0",
                        "EasingFunction": 3
                      }
                    ]
                  }
                ],
                "PropertyGroups": []
              },
              "LayerBrush": {
                "ProviderId": "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-ed2e1c49",
                "BrushType": "ColorBrush",
                "PropertyGroup": {
                  "Identifier": "LayerBrush",
                  "Properties": [],
                  "PropertyGroups": [
                    {
                      "Identifier": "Colors",
                      "Properties": [
                        {
                          "Identifier": "Gradient",
                          "Value": "[{\"Color\":\"#ffff0000\",\"Position\":0.0},{\"Color\":\"#ff0000ff\",\"Position\":1.0}]",
                          "KeyframesEnabled": false,
                          "DataBinding": {
                            "IsEnabled": true,
                            "NodeScript": {
                              "Name": "Gradient",
                              "Description": "The data binding of the gradient",
                              "Nodes": [
                                {
                                  "Id": "e0000000-0000-0000-0000-000000000001",
                                  "Type": "Artemis.Plugins.Nodes.General.Nodes.Color.ColorGradientNode",
                                  "Name": "Color Gradient",
                                  "Description": "Outputs a color gradient with the given colors",
                                  "IsExitNode": false,
                                  "X": 0.0,
                                  "Y": 0.0,
                                  "Storage": "[{\"Color\":\"#ffff0000\",\"Position\":0.0},{\"Color\":\"#ff0000ff\",\"Position\":1.0}]",
                                  "PinCollections": [],
                                  "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
                                }
                              ],
                              "Connections": []
                            }
                          },
                          "KeyframeEntities": []
                        }
                      ],
                      "PropertyGroups": []
                    }
                  ]
                }
              },
              "Id": "f0000000-0000-0000-0000-000000000002",
              "ParentId": "f0000000-0000-0000-0000-000000000001",
              "LayerEffects": [],
              "DisplayCondition": {
                "$type": "Event",
                "TriggerMode": 0,
                "OverlapMode": 1,
                "ToggleOffMode": 0,
                "EventPath": {
                  "Path": "Player.Died",
                  "DataModelId": "Artemis.Plugins.Modules.Game-12345678"
                },
                "Script": {
                  "Name": "Event condition",
                  "Description": "Whether or not the event should trigger",
                  "Nodes": [
                    {
                      "Id": "c0000000-0000-0000-0000-000000000002",
                      "Type": "Artemis.Plugins.Nodes.General.Nodes.Static.StaticBooleanValueNode",
                      "Name": "Boolean-Value",
                      "Description": "Outputs a configurable static boolean value",
                      "IsExitNode": false,
                      "X": 0.0,
                      "Y": 0.0,
                      "Storage": "true",
                      "PinCollections": [],
                      "ProviderId": "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78"
                    }
                  ],
                  "Connections": []
                }
              },
              "Timeline": {
                "StartSegmentLength": "00:00:00",
                "MainSegmentLength": "00:00:02",
                "EndSegmentLength": "00:00:01"
              }
            }
          ]
        }
        """;

    /// <summary>
    ///     Parses one of the JSON strings above into a fresh, editable object
    /// </summary>
    public static JsonObject Parse(string json)
    {
        return JsonNode.Parse(json)!.AsObject();
    }
}
