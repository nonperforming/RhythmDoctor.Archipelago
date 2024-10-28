// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

//[assembly: SuppressMessage(
//  "CodeQuality",
//  "IDE0051:Remove unused private members",
//  Justification = "Unity MonoBehaviour methods are not unused",
//  Scope = "member",
//  Target = "~M:RhythmDoctor.Archipelago.Plugin.Start"
//)]

[assembly: SuppressMessage(
  "CodeQuality",
  "IDE0051:Remove unused private members",
  Justification = "Unity MonoBehaviour methods are not unused",
  Scope = "member",
  Target = "~M:RhythmDoctor.Archipelago.Plugin.Awake"
)]

//[assembly: SuppressMessage(
//  "CodeQuality",
//  "IDE0051:Remove unused private members",
//  Justification = "Unity MonoBehaviour methods are not unused",
//  Scope = "member",
//  Target = "~M:RhythmDoctor.Archipelago.Plugin.Update"
//)]

// FIXME: this doesnt actually work lmao
// Related document: https://github.com/dotnet/roslyn-analyzers/blob/main/docs/rules/RS1041.md
[assembly: SuppressMessage(
  "MicrosoftCodeAnalysisCorrectness",
  "RS1041",
  Justification = "Unity libraries fail to load otherwise, we must target netstandard2.1",
  Scope = "module"
)]
