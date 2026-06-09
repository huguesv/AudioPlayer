// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services.Impl;

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Woohoo.Audio.Services;

public sealed class LocalSettingsService : ILocalSettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private JsonNode doc;
    private bool isInitialized;

    public LocalSettingsService()
    {
        this.doc = JsonNode.Parse("{}") ?? new JsonObject();
    }

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public required string FilePath { get; init; }

    public T? ReadSetting<T>(string key)
    {
        this.Initialize();

        if (this.doc is not null && this.doc.AsObject().TryGetPropertyValue(key, out var settingElement))
        {
            return JsonSerializer.Deserialize<T>(settingElement, SerializerOptions);
        }

        return default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        this.Initialize();

        if (value is not null)
        {
            var jsonString = JsonSerializer.Serialize(value, SerializerOptions);
            var element = JsonNode.Parse(jsonString);
            this.doc.AsObject()[key] = element;
        }
        else
        {
            this.doc.AsObject().Remove(key);
        }

        this.Save();

        this.SettingChanged?.Invoke(this, new SettingChangedEventArgs(key));
    }

    private void Initialize()
    {
        if (!this.isInitialized)
        {
            if (File.Exists(this.FilePath))
            {
                using var stream = new FileStream(this.FilePath, FileMode.Open);
                this.doc = JsonNode.Parse(stream) ?? new JsonObject();
            }

            this.isInitialized = true;
        }
    }

    private void Save()
    {
        var folderPath = Path.GetDirectoryName(this.FilePath)
            ?? throw new InvalidOperationException($"Could not get directory name of file: {this.FilePath}");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var jsonString = JsonSerializer.Serialize(this.doc, SerializerOptions);
        File.WriteAllText(this.FilePath, jsonString);
    }
}
