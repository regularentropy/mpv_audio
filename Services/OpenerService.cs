using System;
using System.Diagnostics;

namespace mpv_audio.Services;

public class OpenerService
{
    /// <summary>
    /// Spawns the process of the MPV
    /// </summary>
    /// <param name="videoPath">Path to the video file</param>
    /// <param name="audioPath">Path to the audio file</param>
    /// <param name="mpvPath">Path to the MPV</param>
    public static void SpawnMPV(string videoPath, string audioPath, string mpvPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo(mpvPath);
            startInfo.ArgumentList.Add(videoPath);
            startInfo.ArgumentList.Add($"--audio-file={audioPath}");

            Process.Start(startInfo);
        }
        catch (Exception e)
        {
            Console.WriteLine($"MPV Launch Exception: {e.Message}");
        }
    }
}