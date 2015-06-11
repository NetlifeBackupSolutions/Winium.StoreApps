﻿namespace Winium.StoreApps.Driver
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;

    using Winium.StoreApps.Driver.Automator;
    using Winium.StoreApps.Driver.CommandHelpers;
    using Winium.StoreApps.Driver.EmulatorHelpers;

    #endregion

    internal class Program
    {
        #region Static Fields

        private static readonly List<IDisposable> AppLifetimeDisposables = new List<IDisposable>();

        private static Listener listener;

        #endregion

        #region Methods

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                var options = new CommandLineOptions();
                CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

                var appName = typeof(Program).Assembly.GetName().Name;
                var versionInfo = string.Format("{0}, {1}", appName, new BuildInfo());

                if (options.Version)
                {
                    Console.WriteLine(versionInfo);
                    Environment.Exit(0);
                }

                Automator.Automator.Dependencies = new List<string>();
                if (!string.IsNullOrEmpty(options.Dependency))
                {
                    Automator.Automator.Dependencies.Add(options.Dependency);
                }
                else
                {
                    Console.WriteLine("No dependency");
                }

                if (options.LogPath != null)
                {
                    Logger.TargetFile(options.LogPath, options.Verbose);
                }
                else
                {
                    Logger.TargetConsole(options.Verbose);
                }

                Capabilities.BoundDeviceName = options.BoundDeviceName;

                Logger.Info(versionInfo);

                if (!ExitHandler.SetHandler(OnExitHandler))
                {
                    Logger.Warn("Colud not set OnExit cleanup handlers.");
                }

                var listeningPort = options.Port;
                AppLifetimeDisposables.Add(EmulatorFactory.Instance);
                listener = new Listener(listeningPort);
                Listener.UrnPrefix = options.UrlBase;

                Console.WriteLine("Starting {0} on port {1}\n", appName, listeningPort);

                listener.StartListening();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to start driver: {0}", ex);
                Environment.Exit(ex.HResult);
            }
        }

        private static bool OnExitHandler(ExitHandler.CtrlType signal)
        {
            listener.StopListening();
            foreach (var disposable in AppLifetimeDisposables)
            {
                disposable.Dispose();
            }

            return false;
        }

        #endregion
    }
}
