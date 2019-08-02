using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Data;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Text;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Content.Res;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Graphics;
using Android.Graphics.Drawables;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using System.ComponentModel;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Rebex.Net;
using Rebex.IO;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;

namespace Store_Remote_Tool_Android
{
    [Activity(Label = "PS4 PKG Installer", Theme = "@style/AppTheme", MainLauncher = true , Icon = "@drawable/ps4")]

    public class MainActivity : AppCompatActivity
    {
        #region << Permisions>>
        /*We need the following permissions camera and r/w*/
        static readonly int REQUEST_CAMERA = 2;

        static readonly int REQUEST_ReadExternalStorage = 0;

        static readonly int REQUEST_WriteExternalStorage = 1;

        static string[] PERMISSIONS = {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };


        #endregion << Permisions>>


        private string PKGLocation;

        private bool _isWorking;                    // determines whether any operation is running
        private DateTime _lastTransferProgressTime;      // last TransferProgress event call time
        private DateTime _transferTime;             // transfer launch-time
        private bool _cccMode = false;

        public static Context _context;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            _context = this;
            //first check if there are permsions to access files ext
            bool checks = true;
            #region << Check For Permisions >>
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
            {

                // Camera permission has not been granted
                RequestReadWirtePermission();
                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
                {
                    Thread.Sleep(100);
                }
            }
            #endregion << Check For Permisions  >>

            base.OnCreate(savedInstanceState);

            //request the app to be full screen 

            RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.ClearFlags(Android.Views.WindowManagerFlags.Fullscreen); //to hide

            Rebex.Licensing.Key = "==AnKxIZnJ2NXyRRk/MrXLh5vsLbImP/JhMGERReY23qIk==";
            // Set our view from the "main" layout resource

            SetContentView(Resource.Layout.Main);
            this.Title = "PS4 Package Installer";//set the title
            

            Button LoadPkg = FindViewById<Button>(Resource.Id.BrowsePayloadBtn);
            LoadPkg.Click += async delegate
            {
                try
                {
                    FileData fileData = await CrossFilePicker.Current.PickFile();
                    if (fileData == null)
                        return; // user canceled file picking

                    string fileName = fileData.FileName;
                    //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                    PKGLocation = fileData.FilePath;
                    
                    var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(PKGLocation);
                    ImageView pbPkg = FindViewById<ImageView>(Resource.Id.PKGIcon);
                    pbPkg.SetImageBitmap(BytesToBitmap(pkgfile.Image));
                    TextView lblPackageInfo = FindViewById<TextView>(Resource.Id.txtPKGInfo);
                    lblPackageInfo.Text = pkgfile.PS4_Title + "\n" + pkgfile.PKG_Type.ToString() + "\n" +
                                          pkgfile.Param.TitleID; //display whatever info youd like here


                    System.Console.WriteLine("File name chosen: " + fileName);
                    //System.Console.WriteLine("File data: " + contents);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception choosing file: " + ex.ToString());
                }
            };

            Button SendPayloadBtn = FindViewById<Button>(Resource.Id.SendPayloadBtn);
            SendPayloadBtn.Click += delegate
            {
                using (Ftp client = new Ftp())
                {
                    Rebex.Licensing.Key = "==AnKxIZnJ2NXyRRk/MrXLh5vsLbImP/JhMGERReY23qIk==";
                    try
                    {
                        TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
                         if(IPAddressTextBox.Text == "")
                        {
                            return;
                        }

                        // connect and login to the FTP
                        client.Connect(IPAddressTextBox.Text);
                        client.Login("anonymous", "DONT-LOOK@MYCODE");

                        client.StateChanged += StateChanged;
                        client.Traversing += Traversing;
                        client.TransferProgressChanged += TransferProgressChanged;
                        client.DeleteProgressChanged += DeleteProgressChanged;
                        client.ProblemDetected += ProblemDetected;

                        client.PutFile(PKGLocation, @"/user/app/temp.pkg");

                        client.SendCommand("installpkg");

                        var response = client.ReadResponse();

                        SetTex(response.Raw);

                        SetTex("Package Sent");



                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    client.Disconnect();
                }
            };
        }

        public class MessageBox

        {
            public static void Show(string Message)
            {
                new Android.Support.V7.App.AlertDialog.Builder(MainActivity._context)
                .SetMessage(Message)
                .SetNegativeButton("No", (senderAlert, args) =>
                {
                    // SetResult(Result.Canceled);
                })
                .SetPositiveButton("Yes", (senderAlert, args) =>
                {
                    // SetResult(Result.Canceled);
                })
                .Show();
            }
        }

        /// Loads a Bitmap from a byte array
        public static Bitmap BytesToBitmap(byte[] imageBytes)
        {

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

            return bitmap;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok && data != null && requestCode == 100)
            {
                PKGLocation = data.ToUri(IntentUriType.None);
            }
        }

        private void SetTex(string text)
        {
            RunOnUiThread(() => {
                TextView InjectingLbl = FindViewById<TextView>(Resource.Id.InjectingLbl);
                InjectingLbl.Text = text;
                });
        }



        private void TransferCompleted()
        {
            _isWorking = false;
            SetTex("Finished");
            SetProgressValue(0);
        }


        /// <summary>
        /// show transfer status: files, bytes, time, speed
        /// </summary>
        private void ShowTransferStatus(long bytesTransferred, int filesTransferred)
        {
            // unknown bytes transferred


            if (bytesTransferred == 0)
                return;

            // files and bytes transferred
            string outstring = string.Format("{0} file{1} ({2} byte{3}) transferred in",
                filesTransferred, (filesTransferred > 1 ? "s" : null),
                bytesTransferred, (bytesTransferred > 1 ? "s" : null));

            // time spent
            TimeSpan ts = DateTime.Now - _transferTime;

            // speed
            if (ts.TotalSeconds > 1)
            {
                outstring += (ts.Days > 0 ? " " + ts.Days + " day" + (ts.Days > 1 ? "s" : null) : null);
                outstring += (ts.Hours > 0 ? " " + ts.Hours + " hour" + (ts.Hours > 1 ? "s" : null) : null);
                outstring += (ts.Minutes > 0 ? " " + ts.Minutes + " min" + (ts.Minutes > 1 ? "s" : null) : null);
                outstring += (ts.Seconds > 0 ? " " + ts.Seconds + " sec" + (ts.Seconds > 1 ? "s" : null) : null);
            }
            else
            {
                outstring += " " + ts.TotalSeconds + " sec";
            }

            double speed = bytesTransferred / ts.TotalSeconds;
            if (speed < 1)
                outstring += string.Format(" at {0:F3} B/s", speed);
            else if (speed < 1024)
                outstring += string.Format(" at {0:F0} B/s", speed);
            else
                outstring += string.Format(" at {0:F0} KB/s", speed / 1024);

        }


        /// <summary>
        /// handles the transfer progress changed event
        /// </summary>
        void TransferProgressChanged(object sender, FtpTransferProgressChangedEventArgs e)
        {
            string strBatchInfo = string.Format("({0} / {1} file{2} processed)    ",
                e.FilesProcessed, e.FilesTotal, (e.FilesProcessed > 1 ? "s" : null));

            SetProgressValue(Convert.ToInt32(e.ProgressPercentage));

            switch (e.TransferState)
            {
                case TransferProgressState.DataBlockProcessed:
                    strBatchInfo += e.BytesTransferred + " bytes";
                    break;
                case TransferProgressState.DirectoryProcessing:
                    strBatchInfo += "Processing directory...";
                    break;
                case TransferProgressState.FileTransferring:
                    strBatchInfo += "Transferring file...";
                    break;
                case TransferProgressState.FileTransferred:
                    strBatchInfo += "File transferred.";
                    break;
                case TransferProgressState.TransferCompleted:
                    strBatchInfo += "Transfer completed.";
                    ShowTransferStatus(e.BytesTransferred, e.FilesTransferred);
                    break;
            }

            SetTex(strBatchInfo);
        }

        private void SetProgressValue(int value)
        {
            // workaround for progress bar smoothing
            SetTex("Uploading :" + value + "%");
        }

        private void StateChanged(object sender, FtpStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case FtpState.Disconnected:
                case FtpState.Disposed:
                    SetTex("Disconnected");
                    break;
                case FtpState.Ready:
                    SetTex("Ready");
                    break;
            }
        }

        private void DeleteProgressChanged(object sender, FtpDeleteProgressChangedEventArgs e)
        {
            string strDeleteInfo = string.Format("({0} / {1} file{2} deleted)    ",
                e.FilesProcessed, e.FilesTotal, (e.FilesProcessed > 1 ? "s" : null));

            switch (e.DeleteState)
            {
                case DeleteProgressState.DeleteCompleted:
                    strDeleteInfo += "Delete completed.";
                    break;
                case DeleteProgressState.DirectoryDeleted:
                    strDeleteInfo += "Directory deleted.";
                    break;
                case DeleteProgressState.DirectoryProcessing:
                    strDeleteInfo += "Processing directory...";
                    break;
                case DeleteProgressState.FileDeleted:
                    strDeleteInfo += "File deleted.";
                    break;
                case DeleteProgressState.FileDeleting:
                    strDeleteInfo += "Deleting file...";
                    break;
            }

            SetTex(strDeleteInfo);
        }

        private void ProblemDetected(object sender, FtpProblemDetectedEventArgs e)
        {
             MessageBox.Show("Problem Detected " + e);
        }

        void Traversing(object sender, FtpTraversingEventArgs e)
        {
            if (e.Action == TransferAction.Listing)
                return;

            string strBatchInfo = string.Format("({0} file{1} traversed)    ",
                e.FilesTotal, (e.FilesTotal > 1 ? "s" : null));

            switch (e.TraversingState)
            {
                case TraversingState.DirectoryRetrieved:
                    strBatchInfo += "Directory retrieved.";
                    break;
                case TraversingState.DirectoryRetrieving:
                    strBatchInfo += "Retrieving directory...";
                    break;
                case TraversingState.HierarchyRetrieved:
                    strBatchInfo += string.Format("Hierarchy retrieved ({0} byte{1} in {2} file{3}).",
                        e.BytesTotal, (e.BytesTotal > 1 ? "s" : null),
                        e.FilesTotal, (e.FilesTotal > 1 ? "s" : null));
                    break;
                case TraversingState.HierarchyRetrieving:
                    strBatchInfo += "Retrieving hierarchy...";
                    break;
            }

            SetTex(strBatchInfo);
        }

        /// <summary>
        /// Request Permissions to android OS
        /// </summary>
        private void RequestReadWirtePermission()
        {
            Log.Info(this.Title.ToString(), "READWRITE permission has NOT been granted. Requesting permission.");

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage))
            {
                // Provide an additional rationale to the user if the permission was not granted
                // and the user would benefit from additional context for the use of the permission.
                // For example if the user has previously denied the permission.
                Log.Info(this.Title.ToString(), "Displaying READWRITE permission rationale to provide additional context.");

                // Snackbar.Make(layout, "Contacts permissions are needed to demonstrate access",
                //  Snackbar.LengthIndefinite).SetAction("OK", new Action<View>(delegate (View obj) {
                //ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, REQUEST_ReadExternalStorage);
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Camera }, REQUEST_WriteExternalStorage);
                //  })).Show();
            }
            else
            {
                // Camera permission has not been granted yet. Request it directly.
                //ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, REQUEST_ReadExternalStorage);
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Camera }, REQUEST_WriteExternalStorage);
            }

         

        }
    }
}

