// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNetCore.Hosting.WebHostBuilder
// Assembly: Microsoft.AspNetCore.Hosting, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: 127E718A-C42E-4542-A71B-54CEC3EF156B
// Assembly location: /home/zorbik/.local/share/NuGetPackages/microsoft.aspnetcore.hosting/2.2.0/lib/netstandard2.0/Microsoft.AspNetCore.Hosting.dll

using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Microsoft.AspNetCore.Hosting
{
  /// <summary>
  /// A builder for <see cref="T:Microsoft.AspNetCore.Hosting.IWebHost" />
  /// </summary>
  public class WebHostBuilder : IWebHostBuilder
  {
    private readonly HostingEnvironment _hostingEnvironment;
    private readonly List<Action<WebHostBuilderContext, IServiceCollection>> _configureServicesDelegates;
    private IConfiguration _config;
    private WebHostOptions _options;
    private WebHostBuilderContext _context;
    private bool _webHostBuilt;
    private List<Action<WebHostBuilderContext, IConfigurationBuilder>> _configureAppConfigurationBuilderDelegates;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Microsoft.AspNetCore.Hosting.WebHostBuilder" /> class.
    /// </summary>
    public WebHostBuilder()
    {
      this._hostingEnvironment = new HostingEnvironment();
      this._configureServicesDelegates = new List<Action<WebHostBuilderContext, IServiceCollection>>();
      this._configureAppConfigurationBuilderDelegates = new List<Action<WebHostBuilderContext, IConfigurationBuilder>>();
      this._config = (IConfiguration) new ConfigurationBuilder().AddEnvironmentVariables("ASPNETCORE_").Build();
      if (string.IsNullOrEmpty(this.GetSetting(WebHostDefaults.EnvironmentKey)))
        this.UseSetting(WebHostDefaults.EnvironmentKey, Environment.GetEnvironmentVariable("Hosting:Environment") ?? Environment.GetEnvironmentVariable("ASPNET_ENV"));
      if (string.IsNullOrEmpty(this.GetSetting(WebHostDefaults.ServerUrlsKey)))
        this.UseSetting(WebHostDefaults.ServerUrlsKey, Environment.GetEnvironmentVariable("ASPNETCORE_SERVER.URLS"));
      this._context = new WebHostBuilderContext()
      {
        Configuration = this._config
      };
    }

    /// <summary>Get the setting value from the configuration.</summary>
    /// <param name="key">The key of the setting to look up.</param>
    /// <returns>The value the setting currently contains.</returns>
    public string GetSetting(string key)
    {
      return this._config[key];
    }

    /// <summary>Add or replace a setting in the configuration.</summary>
    /// <param name="key">The key of the setting to add or replace.</param>
    /// <param name="value">The value of the setting to add or replace.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public IWebHostBuilder UseSetting(string key, string value)
    {
      this._config[key] = value;
      return (IWebHostBuilder) this;
    }

    /// <summary>
    /// Adds a delegate for configuring additional services for the host or web application. This may be called
    /// multiple times.
    /// </summary>
    /// <param name="configureServices">A delegate for configuring the <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public IWebHostBuilder ConfigureServices(
      Action<IServiceCollection> configureServices)
    {
      if (configureServices == null)
        throw new ArgumentNullException(nameof (configureServices));
      return this.ConfigureServices((Action<WebHostBuilderContext, IServiceCollection>) ((_, services) => configureServices(services)));
    }

    /// <summary>
    /// Adds a delegate for configuring additional services for the host or web application. This may be called
    /// multiple times.
    /// </summary>
    /// <param name="configureServices">A delegate for configuring the <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    public IWebHostBuilder ConfigureServices(
      Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
      if (configureServices == null)
        throw new ArgumentNullException(nameof (configureServices));
      this._configureServicesDelegates.Add(configureServices);
      return (IWebHostBuilder) this;
    }

    /// <summary>
    /// Adds a delegate for configuring the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> that will construct an <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" />.
    /// </summary>
    /// <param name="configureDelegate">The delegate for configuring the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> that will be used to construct an <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" />.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
    /// <remarks>
    /// The <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" /> and <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" /> on the <see cref="T:Microsoft.AspNetCore.Hosting.WebHostBuilderContext" /> are uninitialized at this stage.
    /// The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" /> is pre-populated with the settings of the <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.
    /// </remarks>
    public IWebHostBuilder ConfigureAppConfiguration(
      Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
      if (configureDelegate == null)
        throw new ArgumentNullException(nameof (configureDelegate));
      this._configureAppConfigurationBuilderDelegates.Add(configureDelegate);
      return (IWebHostBuilder) this;
    }

    /// <summary>
    /// Builds the required services and an <see cref="T:Microsoft.AspNetCore.Hosting.IWebHost" /> which hosts a web application.
    /// </summary>
    public IWebHost Build()
    {
      if (this._webHostBuilt)
        throw new InvalidOperationException(Resources.WebHostBuilder_SingleInstance);
      this._webHostBuilt = true;
      AggregateException hostingStartupErrors;
      IServiceCollection serviceCollection1 = this.BuildCommonServices(out hostingStartupErrors);
      IServiceCollection serviceCollection2 = serviceCollection1.Clone();
      IServiceProvider providerFromFactory = GetProviderFromFactory(serviceCollection1);
      if (!this._options.SuppressStatusMessages)
      {
        if (Environment.GetEnvironmentVariable("Hosting:Environment") != null)
          Console.WriteLine("The environment variable 'Hosting:Environment' is obsolete and has been replaced with 'ASPNETCORE_ENVIRONMENT'");
        if (Environment.GetEnvironmentVariable("ASPNET_ENV") != null)
          Console.WriteLine("The environment variable 'ASPNET_ENV' is obsolete and has been replaced with 'ASPNETCORE_ENVIRONMENT'");
        if (Environment.GetEnvironmentVariable("ASPNETCORE_SERVER.URLS") != null)
          Console.WriteLine("The environment variable 'ASPNETCORE_SERVER.URLS' is obsolete and has been replaced with 'ASPNETCORE_URLS'");
      }
      this.AddApplicationServices(serviceCollection2, providerFromFactory);
      WebHost webHost = new WebHost(serviceCollection2, providerFromFactory, this._options, this._config, hostingStartupErrors);
      try
      {
        webHost.Initialize();
        ILogger<WebHost> requiredService = webHost.Services.GetRequiredService<ILogger<WebHost>>();
        foreach (IGrouping<string, string> grouping in this._options.GetFinalHostingStartupAssemblies().GroupBy<string, string>((Func<string, string>) (a => a), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).Where<IGrouping<string, string>>((Func<IGrouping<string, string>, bool>) (g => g.Count<string>() > 1)))
          requiredService.LogWarning(string.Format("The assembly {0} was specified multiple times. Hosting startup assemblies should only be specified once.", (object) grouping));
        return (IWebHost) webHost;
      }
      catch
      {
        webHost.Dispose();
        throw;
      }

      IServiceProvider GetProviderFromFactory(IServiceCollection collection)
      {
        ServiceProvider provider = collection.BuildServiceProvider();
        IServiceProviderFactory<IServiceCollection> service = provider.GetService<IServiceProviderFactory<IServiceCollection>>();
        if (service == null || service is DefaultServiceProviderFactory)
          return (IServiceProvider) provider;
        using (provider)
          return service.CreateServiceProvider(service.CreateBuilder(collection));
      }
    }

    private IServiceCollection BuildCommonServices(
      out AggregateException hostingStartupErrors)
    {
      hostingStartupErrors = (AggregateException) null;
      IConfiguration config = this._config;
      Assembly entryAssembly = Assembly.GetEntryAssembly();
      string applicationNameFallback = (object) entryAssembly != null ? entryAssembly.GetName().Name : (string) null;
      this._options = new WebHostOptions(config, applicationNameFallback);
      if (!this._options.PreventHostingStartup)
      {
        List<Exception> exceptionList = new List<Exception>();
        foreach (string assemblyName in this._options.GetFinalHostingStartupAssemblies().Distinct<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
        {
          try
          {
            foreach (HostingStartupAttribute customAttribute in Assembly.Load(new AssemblyName(assemblyName)).GetCustomAttributes<HostingStartupAttribute>())
              ((IHostingStartup) Activator.CreateInstance(customAttribute.HostingStartupType)).Configure((IWebHostBuilder) this);
          }
          catch (Exception ex)
          {
            exceptionList.Add((Exception) new InvalidOperationException("Startup assembly " + assemblyName + " failed to execute. See the inner exception for more details.", ex));
          }
        }
        if (exceptionList.Count > 0)
          hostingStartupErrors = new AggregateException((IEnumerable<Exception>) exceptionList);
      }
      this._hostingEnvironment.Initialize(this.ResolveContentRootPath(this._options.ContentRootPath, AppContext.BaseDirectory), this._options);
      this._context.HostingEnvironment = (IHostingEnvironment) this._hostingEnvironment;
      ServiceCollection services = new ServiceCollection();
      services.AddSingleton<WebHostOptions>(this._options);
      services.AddSingleton<IHostingEnvironment>((IHostingEnvironment) this._hostingEnvironment);
      services.AddSingleton<Microsoft.Extensions.Hosting.IHostingEnvironment>((Microsoft.Extensions.Hosting.IHostingEnvironment) this._hostingEnvironment);
      services.AddSingleton<WebHostBuilderContext>(this._context);
      IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(this._hostingEnvironment.ContentRootPath).AddConfiguration(this._config);
      foreach (Action<WebHostBuilderContext, IConfigurationBuilder> configurationBuilderDelegate in this._configureAppConfigurationBuilderDelegates)
        configurationBuilderDelegate(this._context, configurationBuilder);
      IConfigurationRoot configurationRoot = configurationBuilder.Build();
      services.AddSingleton<IConfiguration>((IConfiguration) configurationRoot);
      this._context.Configuration = (IConfiguration) configurationRoot;
      DiagnosticListener implementationInstance = new DiagnosticListener("Microsoft.AspNetCore");
      services.AddSingleton<DiagnosticListener>(implementationInstance);
      services.AddSingleton<DiagnosticSource>((DiagnosticSource) implementationInstance);
      services.AddTransient<IApplicationBuilderFactory, ApplicationBuilderFactory>();
      services.AddTransient<IHttpContextFactory, HttpContextFactory>();
      services.AddScoped<IMiddlewareFactory, MiddlewareFactory>();
      services.AddOptions();
      services.AddLogging();
      services.AddTransient<IStartupFilter, AutoRequestServicesStartupFilter>();
      services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();
      services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
      if (!string.IsNullOrEmpty(this._options.StartupAssembly))
      {
        try
        {
          Type startupType = StartupLoader.FindStartupType(this._options.StartupAssembly, this._hostingEnvironment.EnvironmentName);
          if (typeof (IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
            ServiceCollectionServiceExtensions.AddSingleton(services, typeof (IStartup), startupType);
          else
            ServiceCollectionServiceExtensions.AddSingleton(services, typeof (IStartup), (Func<IServiceProvider, object>) (sp =>
            {
              IHostingEnvironment requiredService = sp.GetRequiredService<IHostingEnvironment>();
              return (object) new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, requiredService.EnvironmentName));
            }));
        }
        catch (Exception ex)
        {
          ExceptionDispatchInfo capture = ExceptionDispatchInfo.Capture(ex);
          services.AddSingleton<IStartup>((Func<IServiceProvider, IStartup>) (_ =>
          {
            capture.Throw();
            return (IStartup) null;
          }));
        }
      }
      foreach (Action<WebHostBuilderContext, IServiceCollection> servicesDelegate in this._configureServicesDelegates)
        servicesDelegate(this._context, (IServiceCollection) services);
      return (IServiceCollection) services;
    }

    private void AddApplicationServices(
      IServiceCollection services,
      IServiceProvider hostingServiceProvider)
    {
      DiagnosticListener service = hostingServiceProvider.GetService<DiagnosticListener>();
      services.Replace(ServiceDescriptor.Singleton(typeof (DiagnosticListener), (object) service));
      services.Replace(ServiceDescriptor.Singleton(typeof (DiagnosticSource), (object) service));
    }

    private string ResolveContentRootPath(string contentRootPath, string basePath)
    {
      if (string.IsNullOrEmpty(contentRootPath))
        return basePath;
      if (Path.IsPathRooted(contentRootPath))
        return contentRootPath;
      return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
    }
  }
}
