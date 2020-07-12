using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;

namespace SpectreBodies
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        private const string BodyListFileName = "SpectreBodyList.txt";
        private const string MetaDataPattern = @"(\w+/)*\w+";

        private string BodyListPath => DirectoryFullName + Path.DirectorySeparatorChar + BodyListFileName;
        private DateTime lastBodyListLoadTime = new DateTime();
        private Dictionary<long, Monster> nearbyMonsters = new Dictionary<long, Monster>();
        private ICollection<string> validSpectreBodies = new List<string>();

        public override bool Initialise()
        {
            EnsureFileCoherence();
            LoadBodyList();
            Input.RegisterKey(Settings.ReloadListKey);
            return base.Initialise();
        }

        private void EnsureFileCoherence()
        {
            if (!File.Exists(BodyListPath))
                File.WriteAllText(BodyListPath, Helpers.FileContent.SpectreBodyList);
        }

        public override void EntityAdded(Entity entity)
        {
            if (entity.Type == EntityType.Monster)
            {
                var monster = entity.AsObject<Monster>();
                if (monster != null && monster.Address != 0x0 && !nearbyMonsters.ContainsKey(monster.Address))
                    nearbyMonsters.Add(monster.Address, monster);
            }
        }

        public override void EntityRemoved(Entity entity)
        {
            if (entity.Type == EntityType.Monster)
            {
                var monster = entity.AsObject<Monster>();
                if (monster != null && monster.Address != 0x0)
                    nearbyMonsters.Remove(monster.Address);
            }
        }

        public override void AreaChange(AreaInstance area)
        {
            base.AreaChange(area);
            nearbyMonsters.Clear();
        }

        public override void Render()
        {
            base.Render();

            if (Input.IsKeyDown(Settings.ReloadListKey.Value) &&
                (DateTime.Now - lastBodyListLoadTime).TotalMilliseconds > 500)
            {
                lastBodyListLoadTime = DateTime.Now;
                LoadBodyList();
                return;
            }

            if (!GameController.InGame || GameController.Area.CurrentArea.IsTown || nearbyMonsters == null ||
                nearbyMonsters.Count == 0)
                return;

            var textColor = Settings.TextColor.Value;
            var backgroundColor = Settings.BackgroundColor.Value;
            var zOffset = Settings.TextOffset.Value;
            var useRenderNames = Settings.UseRenderNames.Value;
            var textSize = Settings.TextSize.Value;
            var drawDistance = Settings.DrawDistance.Value;

            foreach (var monster in nearbyMonsters.Values)
            {
                var entity = monster?.AsObject<Entity>();
                if (entity == null || entity.Address == 0x0 || string.IsNullOrEmpty(entity.Metadata) ||
                    !entity.IsValid || !entity.IsHostile)
                    continue;
                if (entity.HasComponent<Life>())
                {
                    var entityLife = entity.GetComponent<Life>();
                    if (entityLife != null && entityLife.Address != 0x0 && entityLife.CurHP > 0)
                        continue;
                }

                if (Vector3.Distance(entity.Pos, GameController.Player.Pos) > drawDistance)
                    continue;

                if (IsSpectreBody(entity.Metadata))
                {
                    var camera = GameController.Game.IngameState.Camera;
                    var chestScreenCoords = camera.WorldToScreen(entity.Pos.Translate(0, 0, zOffset));
                    if (chestScreenCoords == new Vector2())
                        continue;

                    var iconRect = new Vector2(chestScreenCoords.X, chestScreenCoords.Y);
                    float maxWidth = 0;
                    float maxheight = 0;
                    var size = Graphics.DrawText(GetDisplayName(entity, useRenderNames), iconRect, textColor, textSize,
                        FontAlign.Center);
                    chestScreenCoords.Y += size.Y;
                    maxheight += size.Y;
                    maxWidth = Math.Max(maxWidth, size.X);
                    var background = new RectangleF(chestScreenCoords.X - maxWidth / 2 - 3,
                        chestScreenCoords.Y - maxheight, maxWidth + 6, maxheight);
                    Graphics.DrawBox(background, backgroundColor);
                }
            }
        }

        private string GetDisplayName(Entity entity, bool useRenderNames)
        {
            string GetMetadataDisplayName() =>
                entity.Metadata.Substring(entity.Metadata.LastIndexOf("/", StringComparison.Ordinal) + 1);

            string GetRenderNameDisplayName() => entity.RenderName;

            if (entity == null)
                return string.Empty;
            var displayName = useRenderNames ? GetRenderNameDisplayName() : GetMetadataDisplayName();
            if (string.IsNullOrEmpty(displayName))
                displayName = useRenderNames ? GetMetadataDisplayName() : GetRenderNameDisplayName();
            return displayName;
        }

        private bool IsSpectreBody(string metaData)
        {
            if (string.IsNullOrWhiteSpace(metaData))
                return false;

            return validSpectreBodies.Any(s =>
                !string.IsNullOrWhiteSpace(s) && metaData.IndexOf(s, StringComparison.OrdinalIgnoreCase) > 0);
        }

        private void LoadBodyList()
        {
            if (!File.Exists(BodyListPath))
            {
                LogError($"Missing spectre body list file at {BodyListPath}", 3F);
                return;
            }

            var bodyList = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(BodyListPath))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                            continue;
                        if (Regex.IsMatch(line, MetaDataPattern, RegexOptions.IgnoreCase))
                            bodyList.Add(line);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                LogError($"Failed to read spectre body list file at {BodyListPath}{Environment.NewLine}{ex.Message}",
                    3F);
                return;
            }

            bodyList.Sort();
            validSpectreBodies = bodyList;

            LogMessage($"Loaded {bodyList.Count} spectre bodies from: {BodyListPath}", 2F);
        }
    }
}