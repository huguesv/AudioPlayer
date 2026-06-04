// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services.Impl;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

public sealed class MruService : IMruService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string MruDataFolder;
    private readonly string MruFilePath;

    private readonly List<MruItem> items = [];

    public event EventHandler<MruItemsChangedEventArgs>? ItemsChanged;

    public MruService(IMruLocationProviderService mruLocationProviderService)
    {
        ArgumentNullException.ThrowIfNull(mruLocationProviderService);

        this.MruFilePath = mruLocationProviderService.MruFilePath;
        this.MruDataFolder = Path.GetDirectoryName(this.MruFilePath) ?? throw new Exception($"Invalid MRU file path: {this.MruFilePath}");

        try
        {
            if (File.Exists(this.MruFilePath))
            {
                string json = File.ReadAllText(this.MruFilePath);
                this.items = JsonSerializer.Deserialize<List<MruItem>>(json) ?? [];
            }
            else
            {
                this.items = [];
            }
        }
        catch
        {
            this.items = [];
        }
    }

    public ImmutableArray<MruItem> GetItems()
    {
        return [.. this.items];
    }

    public void AddItem(MruItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        items.Add(item);
        this.Save();
        this.ItemsChanged?.Invoke(this, new MruItemsChangedEventArgs());
    }

    public void RemoveItem(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var item = this.items.Find(i => i.FilePath == filePath);
        if (item is not null)
        {
            this.items.Remove(item);
            this.Save();
            this.ItemsChanged?.Invoke(this, new MruItemsChangedEventArgs());
        }
    }

    public void ClearItems()
    {
        this.items.Clear();
        this.Save();
        this.ItemsChanged?.Invoke(this, new MruItemsChangedEventArgs());
    }

    public void AddOrUpdateItem(MruItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        int index = this.items.FindIndex(i => i.FilePath == item.FilePath);
        if (index >= 0)
        {
            this.items[index] = item;
        }
        else
        {
            items.Add(item);
        }

        this.Save();
        this.ItemsChanged?.Invoke(this, new MruItemsChangedEventArgs());
    }

    public MruItem? FindItem(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        return this.items.Find(i => i.FilePath == filePath);
    }

    private void Save()
    {
        Directory.CreateDirectory(MruDataFolder);

        string json = JsonSerializer.Serialize(this.items, SerializerOptions);
        File.WriteAllText(this.MruFilePath, json);
    }
}
