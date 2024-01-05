﻿// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace UnifiedIconLibrary
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using Game.UI;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : IMod
    {
        /// <summary>
        /// The mod's default name.
        /// </summary>
        public const string ModName = "Unified Icon Library";

        // Mod assembly path cache.
        private string s_assemblyPath = null;

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static Mod Instance { get; private set; }

        /// <summary>
        /// Gets the mod directory file path of the currently executing mod assembly.
        /// </summary>
        public string AssemblyPath
        {
            get
            {
                // Update cached path if the existing one is invalid.
                if (string.IsNullOrWhiteSpace(s_assemblyPath))
                {
                    // Update cached path.
                    s_assemblyPath = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                }

                // Return cached path.
                return s_assemblyPath;
            }
        }

        /// <summary>
        /// Gets the mod's active log.
        /// </summary>
        internal static ILog Log { get; private set; }

        /// <summary>
        /// Called by the game when the mod is loaded.
        /// </summary>
        public void OnLoad()
        {
            // Set instance reference.
            Instance = this;

            // Initialize logger.
            Log = LogManager.GetLogger(ModName);
#if DEBUG
            Log.Info("setting logging level to Debug");
            Log.effectivenessLevel = Level.Debug;
#endif

            Log.Info("loading");

            try
            {
                bool needsRefresh = false;

                // Check for icon directory presence.
                string iconsPath = Path.Combine(AssemblyPath, "Icons");
                if (Directory.Exists(iconsPath))
                {
                    // Icon directory present = check for Standard directory.
                    string standardPath = Path.Combine(iconsPath, "Standard");
                    if (Directory.Exists(standardPath))
                    {
                        // Standard directory present - check file count.
                        if (Directory.GetFiles(standardPath).Length != 133)
                        {
                            // File count not up do date - flag files as needing update.
                            Log.Info("Standard file count not current");
                            needsRefresh = true;
                        }
                    }
                    else
                    {
                        // Standard directory not present - flag files as needing update.
                        Log.Info("Standard directory not found");
                        needsRefresh = true;
                    }

                    // If we need to update, delete the Icons directory and any content first.
                    if (needsRefresh)
                    {
                        Log.Info("Deleting existing icons folder");
                        Directory.Delete(iconsPath, true);
                    }
                }
                else
                {
                    // Icon folder not present - flag files as needing update.
                    Log.Info("Icon directory not found");
                    needsRefresh = true;
                }

                // If files need refreshing then extract bundled zip file.
                if (needsRefresh)
                {
                    // Not current - extract bundled zip file.
                    Log.Info("Extracting icons");
                    using Stream embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnifiedIconLibraryLite.Icons.Icons.zip");
                    {
                        using ZipArchive archive = new (embeddedStream, ZipArchiveMode.Read, false);
                        {
                            archive.ExtractToDirectory(AssemblyPath);
                        }
                    }
                }
                else
                {
                    Log.Info("found existing icon directory");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "exception updating or installing icons");
            }
        }

        /// <summary>
        /// Called by the game when the game world is created.
        /// </summary>
        /// <param name="updateSystem">Game update system.</param>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Log.Info("starting OnCreateWorld");

            // Add mod UI resource directory to UI resource handler.
            GameUIResourceHandler uiResourceHandler = GameManager.instance.userInterface.view.uiSystem.resourceHandler as GameUIResourceHandler;
            uiResourceHandler?.HostLocationsMap.Add("uil", new System.Collections.Generic.List<string> { AssemblyPath + "/Icons/" });
        }

        /// <summary>
        /// Called by the game when the mod is disposed of.
        /// </summary>
        public void OnDispose()
        {
            Log.Info("disposing");
            Instance = null;
        }
    }
}
