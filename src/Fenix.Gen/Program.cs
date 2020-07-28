﻿
using Shared;
using System;
using System.IO;
using System.Reflection;

namespace Fenix
{
    class Program
    {
        static void Main(string[] args)
        {
#if !UNITY_5_3_OR_NEWER

            var rootFolder = Directory.GetCurrentDirectory();
            var clientDll = File.ReadAllBytes(Path.Combine(rootFolder, "../../../../../src/Client.App/bin/Debug/netcoreapp3.1/Client.App.dll"));
            var serverDll = File.ReadAllBytes(Path.Combine(rootFolder, "../../../../../bin/netcoreapp3.1/Server.App.dll"));
            Assembly asmClientApp = Assembly.Load(clientDll);
            Assembly asmServerApp = Assembly.Load(serverDll);
            //Assembly asmServerApp = typeof(Server.UModule.Avatar).Assembly;// Assembly.Load(serverDll);

            //Assembly asmRuntime = typeof(Host).Assembly;
            //Assembly asmServerApp = typeof(Server.UModule.Avatar).Assembly;
            //Assembly asmClientApp = typeof(Client.Avatar).Assembly;

            //Assembly asmApp = typeof(ErrCode).Assembly;

            string sharedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Shared/Gen");
            string clientPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Client.App");
            string serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Server.App");

            Gen.AutogenHost(asmServerApp, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Fenix.Runtime/Common"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Client.App"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Server.App"));

            Gen.AutogenActor(asmServerApp, true, sharedPath, clientPath, serverPath); 
            Gen.AutogenActor(asmClientApp, false, sharedPath, clientPath, serverPath);

            var p = new ProtocolCode();
            p.Validate();

#else
            var rootFolder = Directory.GetCurrentDirectory();
            var clientDll = File.ReadAllBytes(Path.Combine(rootFolder, "../../../../../../../Unity/Library/ScriptAssemblies/Client.App.dll"));
            var serverDll = File.ReadAllBytes(Path.Combine(rootFolder, "../../../../../../../bin/netcoreapp3.1/Server.App.dll"));
            Assembly asmClientApp = Assembly.Load(clientDll);
            Assembly asmServerApp = Assembly.Load(serverDll);
            //Assembly asmServerApp = typeof(Server.UModule.Avatar).Assembly;// Assembly.Load(serverDll);

            //Assembly asmRuntime = typeof(Host).Assembly;
            //Assembly asmServerApp = typeof(Server.UModule.Avatar).Assembly;
            //Assembly asmClientApp = typeof(Client.Avatar).Assembly;

            //Assembly asmApp = typeof(ErrCode).Assembly;

            string sharedClientPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../../../Unity/Assets/Scripts/Client.App/Gen"));
            string sharedServerPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../../../src/Server.App/Gen"));

            string clientPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../../../Unity/Assets/Scripts/Client.App"));
            string serverPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../../../src/Server.App"));

            Gen.AutogenHost(asmServerApp, Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Fenix.Runtime/Common")), clientPath, serverPath);

            Gen.AutogenActor(asmServerApp, true, sharedClientPath, sharedServerPath, clientPath, serverPath);
            Gen.AutogenActor(asmClientApp, false, sharedClientPath, sharedServerPath, clientPath, serverPath);

            var p = new ProtocolCode();
            p.Validate();
#endif
        }
    }
}
