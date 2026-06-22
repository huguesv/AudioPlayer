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
    private readonly Dictionary<Type, JsonSerializerContext> serializationContexts = [];

    private JsonNode doc;
    private bool isInitialized;

    public LocalSettingsService()
    {
        this.RegisterType(typeof(bool), BoolContext.Default);
        this.RegisterType(typeof(bool?), NullableBoolContext.Default);
        this.RegisterType(typeof(string), StringContext.Default);

        this.doc = JsonNode.Parse("{}") ?? new JsonObject();
    }

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public required string FilePath { get; init; }

    public T? ReadSetting<T>(string key)
    {
        this.Initialize();

        if (this.doc is not null && this.doc.AsObject().TryGetPropertyValue(key, out var settingElement))
        {
            if (this.serializationContexts.TryGetValue(typeof(T), out var context))
            {
                return (T?)JsonSerializer.Deserialize(settingElement, typeof(T), context);
            }
            else
            {
                throw new InvalidOperationException($"Type '{typeof(T).AssemblyQualifiedName}' does not have a registered serialization context. Call RegisterType first.");
            }
        }

        return default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        this.Initialize();

        if (value is not null)
        {
            if (this.serializationContexts.TryGetValue(typeof(T), out var context))
            {
                var jsonString = JsonSerializer.Serialize(value, typeof(T), context);
                var element = JsonNode.Parse(jsonString);
                this.doc.AsObject()[key] = element;
            }
            else
            {
                throw new InvalidOperationException($"Type '{typeof(T).AssemblyQualifiedName}' does not have a registered serialization context. Call RegisterType first.");
            }
        }
        else
        {
            this.doc.AsObject().Remove(key);
        }

        this.Save();

        this.SettingChanged?.Invoke(this, new SettingChangedEventArgs(key));
    }

    public void RegisterType(Type type, JsonSerializerContext serializerContext)
    {
        serializationContexts[type] = serializerContext;
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

        var jsonString = JsonSerializer.Serialize(this.doc, JsonNodeContext.Default.JsonNode);
        File.WriteAllText(this.FilePath, jsonString);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(JsonNode))]
public partial class JsonNodeContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(string))]
public partial class StringContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(bool))]
public partial class BoolContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(bool?))]
public partial class NullableBoolContext : JsonSerializerContext
{
}
