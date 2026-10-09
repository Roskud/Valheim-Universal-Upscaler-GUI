using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Cecil;

namespace ValheimUpscalerPatcher
{
    public static class Patcher
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

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

            // 2. Reliably locate BepInEx folder
            string bepDir = null;
            try
            {
                string patcherLocation = typeof(Patcher).Assembly.Location;
                if (!string.IsNullOrEmpty(patcherLocation) && File.Exists(patcherLocation))
                {
                    // Patcher is located at <BepInEx>\patchers\ValheimUpscalerPatcher.dll
                    string patchersDir = Path.GetDirectoryName(patcherLocation);
                    bepDir = Path.GetDirectoryName(patchersDir);
                }
            }
            catch { }

            if (string.IsNullOrEmpty(bepDir) || !Directory.Exists(bepDir))
            {
                bepDir = Path.Combine(gameDir, "BepInEx");
            }

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

            // 3. Search for bundled dxgi.dll runtimes folder
            string sourceDir = null;
            if (Directory.Exists(bepDir))
            {
                try
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
                catch { }
            }

            if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
            {
                Console.WriteLine("[ValheimUpscalerPatcher] Native runtimes folder not found in: " + bepDir);
                return;
            }

            string[] files = new string[]
            {
                "dxgi.dll",
                "OptiScaler.ini",
                "OptiScaler.dll",
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
                        bool needsCopy = !File.Exists(dst);
                        if (!needsCopy)
                        {
                            FileInfo fiSrc = new FileInfo(src);
                            FileInfo fiDst = new FileInfo(dst);
                            if (fiSrc.Length != fiDst.Length)
                            {
                                needsCopy = true;
                            }
                        }

                        if (needsCopy)
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

            // 4. Eagerly load dxgi.dll so DirectX 12 hooks are established before D3D12 device creation
            try
            {
                string dxgiPath = Path.Combine(gameDir, "dxgi.dll");
                if (File.Exists(dxgiPath))
                {
                    IntPtr hMod = LoadLibrary(dxgiPath);
                    Console.WriteLine("[ValheimUpscalerPatcher] LoadLibrary dxgi.dll handle: " + hMod);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ValheimUpscalerPatcher] LoadLibrary error: " + ex.Message);
            }
        }
    }
}
