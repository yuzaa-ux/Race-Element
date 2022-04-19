﻿using ACCSetupApp.Util;
using DdsFileTypePlus;
using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.Controls.LiveryBrowser;

namespace ACCSetupApp.Controls.Liveries
{
    internal class DDSutil
    {
        private static Dictionary<string, string> pngsToDDS = new Dictionary<string, string>()
            {
                {"decals_0.dds","decals.png" },
                {"sponsors_0.dds" ,"sponsors.png"},
                {"decals_1.dds","decals.png" },
                {"sponsors_1.dds","sponsors.png" }
            };

        public static void GenerateDDS(LiveryTreeCar livery)
        {
            for (int i = 0; i < pngsToDDS.Count; i++)
            {
                DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + livery.carsRoot.customSkinName);
                FileInfo[] sponsorFiles = customSkinDir.GetFiles(pngsToDDS.ElementAt(i).Value);
                if (sponsorFiles != null && sponsorFiles.Length > 0)
                {
                    MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Generating {pngsToDDS.ElementAt(i).Key}");
                    }));

                    FileInfo sponsorsFile = sponsorFiles[0];
                    FileStream actualFileStream = sponsorsFile.OpenRead();

                    Bitmap bitmap = new Bitmap(actualFileStream);

                    if (pngsToDDS.ElementAt(i).Key.Contains("_1"))
                    {
                        bitmap = ResizeBitmap(bitmap, 2048, 2048);
                    }

                    Surface surface = Surface.CopyFromBitmap(bitmap);

                    FileInfo targetFile = new FileInfo($"{customSkinDir}\\{pngsToDDS.ElementAt(i).Key}");
                    if (targetFile.Exists)
                        targetFile.Delete();

                    FileStream write = targetFile.OpenWrite();
                    DdsFile.Save(write, DdsFileFormat.BC7, DdsErrorMetric.Perceptual, BC7CompressionMode.Slow, true, true, ResamplingAlgorithm.SuperSampling, surface, ProgressChanged);
                    write.Close();
                    actualFileStream.Close();

                    GC.Collect();
                }
            }
        }

        public static bool HasDdsFiles(LiveryTreeCar livery)
        {
            for (int i = 0; i < pngsToDDS.Count; i++)
            {
                KeyValuePair<string, string> kvp = pngsToDDS.ElementAt(i);

                DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + livery.carsRoot.customSkinName);
                if (customSkinDir.Exists)
                {
                    //check if png exists
                    FileInfo[] foundFiles = customSkinDir.GetFiles(kvp.Value);

                    if (foundFiles != null && foundFiles.Length > 0)
                    {
                        foundFiles = customSkinDir.GetFiles(kvp.Key);
                        if (foundFiles == null || foundFiles.Length == 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }

        private static void ProgressChanged(object sender, ProgressEventArgs e)
        {
            //Debug.WriteLine(e.Percent.ToString());
            //progressBar1.Value = (int) Math.Round(e.Percent);
        }

    }
}