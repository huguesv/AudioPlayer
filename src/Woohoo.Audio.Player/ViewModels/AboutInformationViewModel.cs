// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels;

using Avalonia;

public class AboutInformationViewModel
{
    public string AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

    public string AvaloniaVersion { get; } = typeof(Application).Assembly.GetName().Version!.ToString();

    public string DotNetFramework { get; } = RuntimeInformation.FrameworkDescription;

    public string ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture.ToString();

    public string RuntimeIdentifier { get; } = RuntimeInformation.RuntimeIdentifier;

    public string OperatingSystem { get; } = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture);
}
