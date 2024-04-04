using Android;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V4.App;

using Android.Widget;
using Android.Support.V4.Content;
using Android.Content.PM;
using Android.Media;
using Android.Views;
using System;

namespace AudioRecorder
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        MediaPlayer player = new MediaPlayer();
        MediaRecorder recorder;
        static public bool useNotifications = false;
        static Activity activity = null;

        static public Activity Activity
        {
            get { return (activity); }
        }

        bool recordingExists = false;
        bool isRecording = false;
        bool isPlaying = false;

        public const int RequestPermissionCode = 1;
        string READ_EXTERNAL_STORAGE = Manifest.Permission.ReadExternalStorage;
        string WRITE_EXTERNAL_STORAGE = Manifest.Permission.WriteExternalStorage;
        string RECORD_AUDIO = Manifest.Permission.RecordAudio;

        public void RequestPermission()
        {
            ActivityCompat.RequestPermissions(this, new string[]
                    {READ_EXTERNAL_STORAGE, WRITE_EXTERNAL_STORAGE, RECORD_AUDIO}, RequestPermissionCode);
        }

        public bool CheckPermission()
        {
            int result = (int)ContextCompat.CheckSelfPermission(this, WRITE_EXTERNAL_STORAGE);
            int result1 = (int)ContextCompat.CheckSelfPermission(this, READ_EXTERNAL_STORAGE);
            int result2 = (int)ContextCompat.CheckSelfPermission(this, RECORD_AUDIO);

            return result == (int)Permission.Granted && result1 == (int)Permission.Granted && result2 == (int)Permission.Granted;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            if (CheckPermission())
            {
                SetContentView(Resource.Layout.activity_main);                
            }
            else
            {
                RequestPermission();
            }

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            Button record = FindViewById<Button>(Resource.Id.record);
            Button stop = FindViewById<Button>(Resource.Id.stop);
            Button play = FindViewById<Button>(Resource.Id.play);
            Button stopPlayback = FindViewById<Button>(Resource.Id.stopPlayback);

            record.Click += delegate {
                CreateDirectoryForFiles();
                recorder = new MediaRecorder();

                record.Enabled = false;
                stop.Enabled = true;
                play.Enabled = false;
                stopPlayback.Enabled = false;

                try
                {
                    App.file = new Java.IO.File(App.dir, String.Format("Rec_{0}.mp4", Guid.NewGuid()));
                    App.file.CreateNewFile();

                    if (recorder == null)
                        recorder = new MediaRecorder(); // Initial state.
                    else
                        recorder.Reset();

                    recorder.SetAudioSource(AudioSource.Mic);
                    recorder.SetOutputFormat(OutputFormat.Mpeg4);
                    recorder.SetAudioEncoder(AudioEncoder.Aac);
                    recorder.SetOutputFile(App.file.ToString());
                    recorder.SetAudioEncodingBitRate(16 * 44100);
                    recorder.SetAudioSamplingRate(44100);
                    recorder.Prepare();
                    recorder.Start();
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.StackTrace);
                }
            };

            stop.Click += delegate {
                if (recorder != null)
                {
                    try
                    {
                        recorder.Stop();
                        recorder.Release();
                        recorder = null;

                        record.Enabled = true;
                        stop.Enabled = false;
                        play.Enabled = true;
                        stopPlayback.Enabled = false;

                    }
                    catch (Exception e)
                    {
                        e.StackTrace.ToString();
                    }
                }
            };

            play.Click += delegate {
                try
                {
                    if (player == null)
                    {
                        player = new MediaPlayer();
                    }
                    else
                    {
                        player.Reset();
                    }

                    var uri = Android.Net.Uri.Parse(App.file.ToString());
                    player.SetDataSource(uri.ToString());

                    player.Prepare();
                    player.Start();


                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.StackTrace);
                }

                record.Enabled = false;
                stop.Enabled = false;
                play.Enabled = false;
                stopPlayback.Enabled = true;
            };

            stopPlayback.Click += delegate {
                if ((player != null))
                {
                    if (player.IsPlaying)
                    {
                        player.Stop();
                    }

                    player.Release();
                    player = null;

                    record.Enabled = true;
                    stop.Enabled = false;
                    play.Enabled = true;
                    stopPlayback.Enabled = false;
                }
            };

            player.Completion += delegate {

                stopPlayback.Enabled = false;
                record.Enabled = true;
            };
        }

        private void CreateDirectoryForFiles()
        {
            App.dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic), "Sounds");
            if (!App.dir.Exists())
            {
                App.dir.Mkdirs();
            }
        }
        public static class App
        {
            public static Java.IO.File file;
            public static Java.IO.File dir;
        }

        protected override void OnDestroy() // clean up the recorder
        {
            base.OnDestroy();

            if (recorder != null)
            {
                recorder.Release();
                recorder.Dispose();
                recorder = null;
            }
        }

        protected void OnBackPressed()
        {
            base.OnBackPressed();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}