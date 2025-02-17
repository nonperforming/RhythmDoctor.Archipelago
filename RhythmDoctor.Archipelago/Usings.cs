global using Archipelago.MultiClient.Net;
global using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
global using Archipelago.MultiClient.Net.Enums;
global using Archipelago.MultiClient.Net.Helpers;
global using Archipelago.MultiClient.Net.MessageLog.Messages;
global using Archipelago.MultiClient.Net.Models;

global using BepInEx;
global using BepInEx.Logging;

global using HarmonyLib;

global using RhythmDoctor.Archipelago;
global using RhythmDoctor.Archipelago.Client;
#if DEBUG
global using RhythmDoctor.Archipelago.Debug;
global using RhythmDoctor.Archipelago.Debug.Patches;
#endif
global using RhythmDoctor.Archipelago.Helpers;
global using RhythmDoctor.Archipelago.Patches;
global using RhythmDoctor.Archipelago.World;
global using RhythmDoctor.Archipelago.World.Enums;
global using RhythmDoctor.Archipelago.World.Structures;

global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Reflection.Emit;
global using System.Runtime.Serialization;

global using UnityEngine;
global using UnityEngine.Assertions;
global using UnityEngine.UI;

global using YamlDotNet.Serialization;
global using YamlDotNet.Serialization.NamingConventions;
