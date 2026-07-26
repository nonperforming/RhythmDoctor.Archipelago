global using Archipelago.MultiClient.Net;
global using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
global using Archipelago.MultiClient.Net.Enums;
global using Archipelago.MultiClient.Net.Helpers;
global using Archipelago.MultiClient.Net.MessageLog.Messages;
global using Archipelago.MultiClient.Net.Models;

global using BepInEx;
global using BepInEx.Configuration;
global using BepInEx.Logging;
#if BEPINEX6
global using BepInEx.Unity.Mono;
#endif

global using HarmonyLib;

global using PulseLib;
global using PulseLib.Extensions;
global using PulseLib.Localization;

global using RDLevelEditor;

global using RhythmDoctor.Archipelago;
global using RhythmDoctor.Archipelago.Client;
global using RhythmDoctor.Archipelago.Client.Components;
global using RhythmDoctor.Archipelago.Client.Components.ItemProcessors;
global using RhythmDoctor.Archipelago.Client.Components.Interfaces;
#if DEBUG
global using RhythmDoctor.Archipelago.Debug;
global using RhythmDoctor.Archipelago.Debug.Patches;
#endif
global using RhythmDoctor.Archipelago.Extensions;
global using RhythmDoctor.Archipelago.Helpers;
global using RhythmDoctor.Archipelago.Modifiers;
global using RhythmDoctor.Archipelago.Modifiers.Archipelago;
global using RhythmDoctor.Archipelago.Modifiers.Archipelago.Powerups;
global using RhythmDoctor.Archipelago.Modifiers.Archipelago.Scales;
global using RhythmDoctor.Archipelago.Modifiers.Archipelago.Traps;
global using RhythmDoctor.Archipelago.Patches;
global using RhythmDoctor.Archipelago.Patches.Gameplay;
global using RhythmDoctor.Archipelago.Patches.Gameplay.ClientAssistPatches;
global using RhythmDoctor.Archipelago.Patches.Menu;
global using RhythmDoctor.Archipelago.Patches.Shared;
global using RhythmDoctor.Archipelago.World;
global using RhythmDoctor.Archipelago.World.Data;

global using System;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Diagnostics.Contracts;
global using System.Collections.ObjectModel;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Reflection.Emit;
global using System.Threading;
global using System.Threading.Tasks;

global using UnityEngine;
global using UnityEngine.UI;

global using Color = UnityEngine.Color;
global using Random = System.Random;