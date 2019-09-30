// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNetCore.Hosting.IWebHostBuilder
// Assembly: Microsoft.AspNetCore.Hosting.Abstractions, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: 89E901F5-02EC-4DB4-BBC5-F7BFEA24CDDA
// Assembly location: /home/zorbik/.local/share/NuGetPackages/microsoft.aspnetcore.hosting.abstractions/2.2.0/lib/netstandard2.0/Microsoft.AspNetCore.Hosting.Abstractions.dll

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Hosting
{
  /// <summary>
  /// A builder for <see cref="T:Microsoft.AspNetCore.Hosting.IWebHost" />.
  /// </summary>
  public interface IWebHostBuilder
  {
    /// <summary>
    /// Builds an <see cref="T:Microsoft.AspNetCore.Hosting.IWebHost" /> which hosts a web application.
    /// </summary>
    IWebHost Build();

    /// <summary>
    /// Adds a delegate for configuring the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> that will construct an <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" />.
    /// </summary>
    /// <param name="configureDelegate">The delegate for configuring the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> that will be used to construct an <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    /// <remarks>
    /// The <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" /> and <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" /> on the <see cref="T:Microsoft.AspNetCore.Hosting.WebHostBuilderContext" /> are uninitialized at this stage.
    /// The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> is pre-populated with the settings of the <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.
    /// </remarks>
    IWebHostBuilder ConfigureAppConfiguration(
      Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate);

    /// <summary>
    /// Adds a delegate for configuring additional services for the host or web application. This may be called
    /// multiple times.
    /// </summary>
    /// <param name="configureServices">A delegate for configuring the <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

    /// <summary>
    /// Adds a delegate for configuring additional services for the host or web application. This may be called
    /// multiple times.
    /// </summary>
    /// <param name="configureServices">A delegate for configuring the <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    IWebHostBuilder ConfigureServices(
      Action<WebHostBuilderContext, IServiceCollection> configureServices);

    /// <summary>Get the setting value from the configuration.</summary>
    /// <param name="key">The key of the setting to look up.</param>
    /// <returns>The value the setting currently contains.</returns>
    string GetSetting(string key);

    /// <summary>Add or replace a setting in the configuration.</summary>
    /// <param name="key">The key of the setting to add or replace.</param>
    /// <param name="value">The value of the setting to add or replace.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    IWebHostBuilder UseSetting(string key, string value);
  }
}
