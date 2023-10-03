using Gibbed.IO;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace FCBConverterGUI
{
    public class DiscordOwnRPC
    {
        private const string PIPE_NAME = @"discord-ipc-{0}";
        private static readonly int MAX_SIZE = 16 * 1024;
        private static int processID = 0;
        private const string logPrefix = "Discord-RPC: ";

        private static Thread thread;
        private static NamedPipeClientStream _stream;
        private static byte[] _buffer = new byte[MAX_SIZE];
        private static int nonce = 0;
        private static string timestamp = "";
        private static bool end = false;
        private static string largeText = "";
        private static string detailsName = "";
        private static string stateName = "";

        public static void Connect()
        {
            Console.WriteLine(logPrefix + "Initializing...");

            processID = System.Diagnostics.Process.GetCurrentProcess().Id;
            timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            string linuxStr = " (Windows)";
#if LINUXBUILD
            linuxStr = " (Linux)";
#endif

            detailsName = "No workspace opened";
            largeText = MainWindow.appName + linuxStr;

            thread = new Thread(MainLoop);
            thread.Name = "Discord IPC Thread";
            thread.IsBackground = true;
            thread.Start();

            Console.WriteLine(logPrefix + "Initialized.");
        }

        public static void Disconnect()
        {
            Console.WriteLine(logPrefix + "Disconnecting...");

            end = true;

            try
            {
                string handwave = "{\"cmd\": \"DISPATCH\", \"args\": {\"pid\":" + processID + ", \"close_reason\":\"\"}}";
                byte[] handwaveBytes = Encoding.UTF8.GetBytes(handwave);
                var ms = new MemoryStream();
                ms.WriteValueU32(2);
                ms.WriteValueU32((uint)handwaveBytes.Length);
                ms.WriteBytes(handwaveBytes);
                _stream.WriteBytes(ms.ToArray());
                _stream.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(logPrefix + ex.ToString());
            }

            Console.WriteLine(logPrefix + "Disconnected.");
        }

        private static void MainLoop()
        {
            int tries = 0;

            while (!end)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (AttemptConnection(i, false) || AttemptConnection(i, true))
                    {
                        break;
                    }
                }

                BeginReadStream();

                try
                {
                    Console.WriteLine(logPrefix + "Handshaking...");

                    string handshake = "{\"v\":1, \"client_id\": \"1127168424334856224\"}";
                    byte[] handshakeBytes = Encoding.UTF8.GetBytes(handshake);
                    var ms = new MemoryStream();
                    ms.WriteValueU32(0);
                    ms.WriteValueU32((uint)handshakeBytes.Length);
                    ms.WriteBytes(handshakeBytes);
                    _stream.WriteBytes(ms.ToArray());

                    Console.WriteLine(logPrefix + "Handshaked.");

                    Thread.Sleep(1000);

                    Console.WriteLine(logPrefix + "Starting activity sending.");

                    while (_stream.IsConnected && !end)
                    {
                    	stateName = "state";
                    	detailsName = "details";

                        string presence = "{\"cmd\": \"SET_ACTIVITY\", \"args\": {\"pid\":" + processID + ", \"activity\": {\"details\":\"" + detailsName + "\", " + (stateName != "" ? "\"state\":\"" + stateName + "\"," : "") + " \"timestamps\":{\"start\":"+ timestamp + "}, \"assets\":{\"large_image\":\"domino\", \"large_text\":\"" + largeText + "\"}, \"buttons\":[{\"label\":\"Visit FCModding.com\", \"url\":\"https://fcmodding.com/\"}]}}, \"nonce\": \"" + nonce.ToString() + "\"}";
                        byte[] presenceBytes = Encoding.UTF8.GetBytes(presence);
                        var ms2 = new MemoryStream();
                        ms2.WriteValueU32(1);
                        ms2.WriteValueU32((uint)presenceBytes.Length);
                        ms2.WriteBytes(presenceBytes);
                        _stream.WriteBytes(ms2.ToArray());
                        nonce++;

                        Thread.Sleep(1000);

                        //if (nonce > 10) end = true;

                        if (end)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(logPrefix + ex.ToString());
                }

                Console.WriteLine(logPrefix + "Failed to connect or an error happened. Wait 10sec then will try again to connect.");
                Thread.Sleep(10000);
                tries++;

                if (tries > 10)
                {
                    Console.WriteLine(logPrefix + "Limit for tries exceeded. Disabling Discord for this instance.");
                    return;
                }
            }
        }

        private static bool AttemptConnection(int pipe, bool useSandbox)
        {
            try
            {
                Console.WriteLine(logPrefix + "Connecting to pipe " + pipe.ToString() + ", sandbox: " + (useSandbox ? "yes" : "no"));

                string pipename = GetPipeName(0, useSandbox);

                _stream = new NamedPipeClientStream(".", pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
                _stream.Connect(0);

                do { Thread.Sleep(10); } while (!_stream.IsConnected);

                Console.WriteLine(logPrefix + "Connected to pipe " + pipe.ToString() + ", sandbox: " + (useSandbox ? "yes" : "no"));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logPrefix + "Cannot connect to pipe " + pipe.ToString() + ", sandbox: " + (useSandbox ? "yes" : "no") + ex.ToString());
                return false;
            }
        }

        private static void BeginReadStream()
        {
            try
            {
                if (!end)
                {
                    _stream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(EndReadStream), _stream.IsConnected);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logPrefix + ex.ToString());
            }
        }

        private static void EndReadStream(IAsyncResult callback)
        {
            try
            {
                int bytes = 0;

                if (_stream == null || !_stream.IsConnected) return;

                bytes = _stream.EndRead(callback);

                if (bytes > 0)
                {
                    //string data = Encoding.UTF8.GetString(_buffer);
                    //Console.WriteLine(data);
                    /*using (MemoryStream memory = new MemoryStream(_buffer, 0, bytes))
                    {
                    }*/
                }

                if (!end)
                    BeginReadStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine(logPrefix + ex.ToString());
            }
        }







        private static string GetPipeName(int pipe, bool sandbox)
        {
            if (!IsUnix()) return string.Format(PIPE_NAME, pipe);
            return Path.Combine(GetTemporaryDirectory(), (sandbox ? "snap.discord/" : "") + string.Format(PIPE_NAME, pipe));
        }

        private static string GetTemporaryDirectory()
        {
            string temp = null;
            temp = temp ?? Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            temp = temp ?? Environment.GetEnvironmentVariable("TMPDIR");
            temp = temp ?? Environment.GetEnvironmentVariable("TMP");
            temp = temp ?? Environment.GetEnvironmentVariable("TEMP");
            temp = temp ?? "/tmp";
            return temp;
        }

        private static bool IsUnix()
        {
            switch (Environment.OSVersion.Platform)
            {
                default:
                    return false;

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    return true;
            }
        }
    }
}