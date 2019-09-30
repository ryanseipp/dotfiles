// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions
// Assembly: Microsoft.AspNetCore.Hosting, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: 127E718A-C42E-4542-A71B-54CEC3EF156B
// Assembly location: /home/zorbik/.local/share/NuGetPackages/microsoft.aspnetcore.hosting/2.2.0/lib/netstandard2.0/Microsoft.AspNetCore.Hosting.dll

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Microsoft.AspNetCore.Hosting
{
  public static class WebHostBuilderExtensions
  {
    /// <summary>
    /// Specify the startup method to be used to configure the web application.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="configureApp">The delegate that configures the <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder Configure(
      this IWebHostBuilder hostBuilder,
      Action<IApplicationBuilder> configureApp)
    {
      if (configureApp == null)
        throw new ArgumentNullException(nameof (configureApp));
      string name = configureApp.GetMethodInfo().DeclaringType.GetTypeInfo().Assembly.GetName().Name;
      return hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, name).ConfigureServices((Action<IServiceCollection>) (services => services.AddSingleton<IStartup>((Func<IServiceProvider, IStartup>) (sp => (IStartup) new DelegateStartup(sp.GetRequiredService<IServiceProviderFactory<IServiceCollection>>(), configureApp)))));
    }

    /// <summary>Specify the startup type to be used by the web host.</summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="startupType">The <see cref="T:System.Type" /> to be used.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder UseStartup(
      this IWebHostBuilder hostBuilder,
      Type startupType)
    {
      string name = startupType.GetTypeInfo().Assembly.GetName().Name;
      return hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, name).ConfigureServices((Action<IServiceCollection>) (services =>
      {
        if (typeof (IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
          ServiceCollectionServiceExtensions.AddSingleton(services, typeof (IStartup), startupType);
        else
          ServiceCollectionServiceExtensions.AddSingleton(services, typeof (IStartup), (Func<IServiceProvider, object>) (sp =>
          {
            IHostingEnvironment requiredService = sp.GetRequiredService<IHostingEnvironment>();
            return (object) new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, requiredService.EnvironmentName));
          }));
      }));
    }

    /// <summary>Specify the startup type to be used by the web host.</summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <typeparam name="TStartup">The type containing the startup methods for the application.</typeparam>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder UseStartup<TStartup>(
      this IWebHostBuilder hostBuilder)
      where TStartup : class
    {
      return hostBuilder.UseStartup(typeof (TStartup));
    }

    /// <summary>Configures the default service provider</summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="configure">A callback used to configure the <see cref="T:Microsoft.Extensions.DependencyInjection.ServiceProviderOptions" /> for the default <see cref="T:System.IServiceProvider" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder UseDefaultServiceProvider(
      this IWebHostBuilder hostBuilder,
      Action<ServiceProviderOptions> configure)
    {
      return hostBuilder.UseDefaultServiceProvider((Action<WebHostBuilderContext, ServiceProviderOptions>) ((context, options) => configure(options)));
    }

    /// <summary>Configures the default service provider</summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="configure">A callback used to configure the <see cref="T:Microsoft.Extensions.DependencyInjection.ServiceProviderOptions" /> for the default <see cref="T:System.IServiceProvider" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder UseDefaultServiceProvider(
      this IWebHostBuilder hostBuilder,
      Action<WebHostBuilderContext, ServiceProviderOptions> configure)
    {
      return hostBuilder.ConfigureServices((Action<WebHostBuilderContext, IServiceCollection>) ((context, services) =>
      {
        ServiceProviderOptions options = new ServiceProviderOptions();
        configure(context, options);
        services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>((IServiceProviderFactory<IServiceCollection>) new DefaultServiceProviderFactory(options)));
      }));
    }

    /// <summary>
    /// Adds a delegate for configuring the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> that will construct an <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" />.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="configureDelegate">The delegate for configuring the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> that will be used to construct an <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    /// <remarks>
    /// The <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" /> and <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" /> on the <see cref="T:Microsoft.AspNetCore.Hosting.WebHostBuilderContext" /> are uninitialized at this stage.
    /// The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> is pre-populated with the settings of the <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.
    /// </remarks>
    public static IWebHostBuilder ConfigureAppConfiguration(
      this IWebHostBuilder hostBuilder,
      Action<IConfigurationBuilder> configureDelegate)
    {
      return hostBuilder.ConfigureAppConfiguration((Action<WebHostBuilderContext, IConfigurationBuilder>) ((context, builder) => configureDelegate(builder)));
    }

    /// <summary>
    /// Adds a delegate for configuring the provided <see cref="T:Microsoft.Extensions.Logging.ILoggingBuilder" />. This may be called multiple times.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="configureLogging">The delegate that configures the <see cref="T:Microsoft.Extensions.Logging.ILoggingBuilder" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder ConfigureLogging(
      this IWebHostBuilder hostBuilder,
      Action<ILoggingBuilder> configureLogging)
    {
      return hostBuilder.ConfigureServices((Action<IServiceCollection>) (collection => collection.AddLogging(configureLogging)));
    }

    /// <summary>
    /// Adds a delegate for configuring the provided <see cref="T:Microsoft.Extensions.Logging.LoggerFactory" />. This may be called multiple times.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> to configure.</param>
    /// <param name="configureLogging">The delegate that configures the <see cref="T:Microsoft.Extensions.Logging.LoggerFactory" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public static IWebHostBuilder ConfigureLogging(
      this IWebHostBuilder hostBuilder,
      Action<WebHostBuilderContext, ILoggingBuilder> configureLogging)
    {
      return hostBuilder.ConfigureServices((Action<WebHostBuilderContext, IServiceCollection>) ((context, collection) => collection.AddLogging((Action<ILoggingBuilder>) (builder => configureLogging(context, builder)))));
    }
  }
}
