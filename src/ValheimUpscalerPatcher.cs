using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace ValheimUpscalerPatcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs
        {
            get { return new string[0]; }
        }

        public static void Initialize()
        {
            try
            {
                DeployFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ValheimUpscalerPatcher] Initialize error: " + ex);
            }
        }

        public static void Patch(AssemblyDefinition assembly)
        {
        }

        private static void DeployFiles()
        {
            string gameDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(gameDir)) gameDir = Directory.GetCurrentDirectory();

            // 1. Remove conflicting version.dll if present to prevent assertion crashes
            string versionDll = Path.Combine(gameDir, "version.dll");
            if (File.Exists(versionDll))
            {
                try { File.Delete(versionDll); } catch { }
            }

            // 2. Locate runtimes in BepInEx folder (plugins or patchers)
            string bepDir = Path.Combine(gameDir, "BepInEx");
            if (!Directory.Exists(bepDir))
            {
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--doorstop-target-assembly" && i + 1 < args.Length)
                    {
                        string preloaderPath = args[i + 1];
                        bepDir = Path.GetDirectoryName(Path.GetDirectoryName(preloaderPath));
                        break;
                    }
                }
            }

            string sourceDir = null;
            if (Directory.Exists(bepDir))
            {
                string[] found = Directory.GetFiles(bepDir, "dxgi.dll", SearchOption.AllDirectories);
                foreach (string f in found)
                {
                    string dir = Path.GetDirectoryName(f);
                    if (!dir.Equals(gameDir, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceDir = dir;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir)) return;

            string[] files = new string[]
            {
                "dxgi.dll",
                "OptiScaler.ini",
                "amd_fidelityfx_dx12.dll",
                "amd_fidelityfx_framegeneration_dx12.dll",
                "amd_fidelityfx_upscaler_dx12.dll",
                "libxess.dll",
                "libxess_fg.dll",
                "fakenvapi.dll",
                "fakenvapi.ini"
            };

            foreach (string file in files)
            {
                string src = Path.Combine(sourceDir, file);
                string dst = Path.Combine(gameDir, file);
                if (File.Exists(src))
                {
                    try
                    {
                        if (!File.Exists(dst))
                        {
                            File.Copy(src, dst, true);
                            Console.WriteLine("[ValheimUpscalerPatcher] Auto-deployed: " + file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[ValheimUpscalerPatcher] Deploy error for " + file + ": " + ex.Message);
                    }
                }
            }
        }
    }
}
