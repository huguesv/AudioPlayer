// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.ViewModels;

using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using CommunityToolkit.WinUI;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;

public sealed class SettingsAboutInformationViewModel
{
    public string AppVersion { get; } = string.Format("AppVersionFormat".GetLocalized()!, Assembly.GetExecutingAssembly().GetName().Version!.ToString());

    public string DotNetFramework { get; } = RuntimeInformation.FrameworkDescription;

    public string ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture.ToString();

    public string RuntimeIdentifier { get; } = RuntimeInformation.RuntimeIdentifier;

    public string OperatingSystem { get; } = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture);

    public string WindowsRuntime { get; } = RuntimeInfo.AsString;

    public string WindowsSdk { get; } = ReleaseInfo.AsString;
}
