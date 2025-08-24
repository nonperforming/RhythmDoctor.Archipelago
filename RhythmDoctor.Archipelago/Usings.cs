global using Archipelago.MultiClient.Net;
global using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
global using Archipelago.MultiClient.Net.Enums;
global using Archipelago.MultiClient.Net.Helpers;
global using Archipelago.MultiClient.Net.MessageLog.Messages;
global using Archipelago.MultiClient.Net.Models;

global using BepInEx;
global using BepInEx.Logging;

global using HarmonyLib;

global using PulseLib;
global using PulseLib.Extensions;

global using RhythmDoctor.Archipelago;
global using RhythmDoctor.Archipelago.Client;
#if DEBUG
global using RhythmDoctor.Archipelago.Debug;
global using RhythmDoctor.Archipelago.Debug.Patches;
#endif
global using RhythmDoctor.Archipelago.Helpers;
global using RhythmDoctor.Archipelago.Interfaces;
global using RhythmDoctor.Archipelago.Patches;
global using RhythmDoctor.Archipelago.Patches.Gameplay;
global using RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;
global using RhythmDoctor.Archipelago.Patches.Gameplay.Traps;
global using RhythmDoctor.Archipelago.Patches.Menu;
global using RhythmDoctor.Archipelago.World;
global using RhythmDoctor.Archipelago.World.Data;

global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Diagnostics.Contracts;
global using System.IO;
global using System.Linq;
global using System.Reflection.Emit;
global using System.Threading.Tasks;

global using UnityEngine;
global using UnityEngine.UI;
