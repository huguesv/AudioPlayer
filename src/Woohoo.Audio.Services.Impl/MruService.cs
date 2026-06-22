// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services.Impl;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

public sealed class MruService : IMruService
{
    private readonly List<MruItem> items = [];

    private bool isInitialized;

    public MruService()
    {
    }

    public event EventHandler<EventArgs>? ItemsChanged;

    public required string MruFilePath { get; init; }

    public ImmutableArray<MruItem> GetItems()
    {
        this.EnsureInitialized();

        return [.. this.items];
    }

    public void AddItem(MruItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        this.EnsureInitialized();

        this.items.Add(item);
        this.Save();
        this.ItemsChanged?.Invoke(this, new EventArgs());
    }

    public void RemoveItem(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        this.EnsureInitialized();

        var item = this.items.Find(i => i.FilePath == filePath);
        if (item is not null)
        {
            this.items.Remove(item);
            this.Save();
            this.ItemsChanged?.Invoke(this, new EventArgs());
        }
    }

    public void ClearItems()
    {
        this.EnsureInitialized();

        this.items.Clear();
        this.Save();
        this.ItemsChanged?.Invoke(this, new EventArgs());
    }

    public void AddOrUpdateItem(MruItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        this.EnsureInitialized();

        int index = this.items.FindIndex(i => i.FilePath == item.FilePath);
        if (index >= 0)
        {
            this.items[index] = item;
        }
        else
        {
            this.items.Add(item);
        }

        this.Save();
        this.ItemsChanged?.Invoke(this, new EventArgs());
    }

    public MruItem? FindItem(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        this.EnsureInitialized();

        return this.items.Find(i => i.FilePath == filePath);
    }

    private void EnsureInitialized()
    {
        if (this.isInitialized)
        {
            return;
        }

        try
        {
            this.items.Clear();

            if (File.Exists(this.MruFilePath))
            {
                string json = File.ReadAllText(this.MruFilePath);
                var deserialized = JsonSerializer.Deserialize<List<MruItem>>(json, MruListJsonContext.Default.ListMruItem) ?? [];
                this.items.AddRange(deserialized);
            }
        }
        catch
        {
            this.items.Clear();
        }
        finally
        {
            this.isInitialized = true;
        }
    }

    private void Save()
    {
        var folderPath = Path.GetDirectoryName(this.MruFilePath)
           ?? throw new Exception($"Invalid MRU file path: {this.MruFilePath}");

        Directory.CreateDirectory(folderPath);

        string json = JsonSerializer.Serialize(this.items, MruListJsonContext.Default.ListMruItem);
        File.WriteAllText(this.MruFilePath, json);
    }
}
