// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Woohoo.Audio.Services;

internal sealed class LocalSettingsService : ILocalSettingsService
{
    private const string DefaultApplicationDataFolder = "Woohoo.Discue.WinUI/ApplicationData";
    private const string DefaultLocalSettingsFile = "LocalSettings.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly LocalSettingsOptions options;

    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string applicationDataFolder;
    private readonly string localsettingsFile;
    private readonly string localsettingsFilePath;

    private JsonNode doc;

    private bool isInitialized;

    public LocalSettingsService(IOptions<LocalSettingsOptions> options)
    {
        this.options = options.Value;

        this.applicationDataFolder = Path.Combine(this.localApplicationData, this.options.ApplicationDataFolder ?? DefaultApplicationDataFolder);
        this.localsettingsFile = this.options.LocalSettingsFile ?? DefaultLocalSettingsFile;
        this.localsettingsFilePath = Path.Combine(this.applicationDataFolder, this.localsettingsFile);

        this.doc = JsonNode.Parse("{}") ?? new JsonObject();
    }

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public string FilePath => this.localsettingsFilePath;

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
            if (File.Exists(this.localsettingsFilePath))
            {
                using var stream = new FileStream(this.localsettingsFilePath, FileMode.Open);
                this.doc = JsonNode.Parse(stream) ?? new JsonObject();
            }

            this.isInitialized = true;
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(this.applicationDataFolder);
        var jsonString = JsonSerializer.Serialize(this.doc, SerializerOptions);
        File.WriteAllText(this.localsettingsFilePath, jsonString);
    }
}
