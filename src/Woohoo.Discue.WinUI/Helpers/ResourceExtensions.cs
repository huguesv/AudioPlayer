// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Helpers;

using Microsoft.Windows.ApplicationModel.Resources;

internal static class ResourceExtensions
{
    private static readonly ResourceLoader ResourceLoader = new();

    public static string GetLocalized(this string resourceKey) => ResourceLoader.GetString(resourceKey);
}
