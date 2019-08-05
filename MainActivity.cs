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
using System.Drawing;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Rebex.Net;
using Rebex.IO;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;
using Bitmap = Android.Graphics.Bitmap;
using Android.Database;
using Android.Provider;

namespace Store_Remote_Tool_Android
{
    [Activity(Label = "PS4 PKG Installer", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/ps4")]

    public class MainActivity : AppCompatActivity
    {
        #region << Permisions>>

        /*We need the following permissions camera and r/w*/
        static readonly int REQUEST_CAMERA = 2;

        static readonly int REQUEST_ReadExternalStorage = 0;

        static readonly int REQUEST_WriteExternalStorage = 1;
        private bool _isWorking;                    // determines whether any operation is running
        private DateTime _transferTime;             // transfer launch-time



        static string[] PERMISSIONS =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.AccessNetworkState,
            Manifest.Permission.AccessWifiState,
            Manifest.Permission.Internet,
            Manifest.Permission.Camera
        };


        #endregion << Permisions>>


        private string PKGLocation;

        public static Context _context;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _context = this;
            //first check if there are permsions to access files ext
            bool checks = true;

            #region << Check For Permisions >>

            // Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) || Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet)

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                    Manifest.Permission.Internet) != (int)Permission.Granted)
            {

                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Internet))
                {



                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.Internet, Manifest.Permission.Internet }, 1);
                    //  })).Show();
                }
                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) !=
                       (int)Permission.Granted)
                {
                    Thread.Sleep(100);
                }
            }


            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                    Manifest.Permission.AccessWifiState) != (int)Permission.Granted)
            {

                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessWifiState))
                {



                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessWifiState, Manifest.Permission.AccessWifiState }, 1);
                    //  })).Show();
                }
                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) !=
                       (int)Permission.Granted)
                {
                    Thread.Sleep(100);
                }
            }

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                    Manifest.Permission.AccessNetworkState) != (int) Permission.Granted)
            {

                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessNetworkState))
                {


   
                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessNetworkState, Manifest.Permission.AccessNetworkState }, 1);
                    //  })).Show();
                }
                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessNetworkState) !=
                       (int)Permission.Granted)
                {
                    Thread.Sleep(100);
                }
            }

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) !=
                (int) Permission.Granted)
             { 
                            // Camera permission has not been granted
                            RequestReadWirtePermission();
                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) !=
                       (int) Permission.Granted)
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
            this.Title = "PS4 Package Installer"; //set the title

            retrieveset();

            Button LoadPkg = FindViewById<Button>(Resource.Id.BrowsePayloadBtn);
            LoadPkg.Click += async delegate
            {
                try
                {
                    


                    String[] types = new String[] {".pkg"};
                    FileData fileData = await CrossFilePicker.Current.PickFile(types);
                    if (fileData == null)
                        return; // user canceled file picking
                    //com.android.externalstorage.documents

                    if (fileData.FilePath.Contains("com.android.providers.downloads.documents"))
                    {
                        PKGLocation = "/storage/emulated/0/Download/" + fileData.FileName;
                    }
                    else
                    {
                        PKGLocation = fileData.FilePath;
                    }



                    /* if (fileData.FilePath.Contains("com.android.externalstorage.documents"))
                     {
                         PKGLocation = "/storage/emulated/" + fileData.FileName;
                     }*/


                    var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(PKGLocation);
                    ImageView pbPkg = FindViewById<ImageView>(Resource.Id.PKGIcon);
                    pbPkg.SetImageBitmap(BytesToBitmap(pkgfile.Icon));
                    TextView lblPackageInfo = FindViewById<TextView>(Resource.Id.txtPKGInfo);
                    lblPackageInfo.Text = pkgfile.PS4_Title + "\n" + pkgfile.PKG_Type.ToString() + "\n" +
                                          pkgfile.Param.TitleID; //display whatever info youd like here

                    // System.Console.WriteLine("File name chosen: " + fileName);
                    //System.Console.WriteLine("File data: " + contents);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invaild Package! or Path\n\n Location: " + PKGLocation);
                    // MessageBox.Show("Exception choosing file: " + ex.ToString() + "    " + PKGLocation);
                }
            };

            

            Button SendPayloadBtn = FindViewById<Button>(Resource.Id.SendPayloadBtn);
            SendPayloadBtn.Click += delegate
            {
                new Thread(new ThreadStart(delegate
                {
                    using (Ftp client = new Ftp())
                    {
                        Rebex.Licensing.Key = "==AnKxIZnJ2NXyRRk/MrXLh5vsLbImP/JhMGERReY23qIk==";
                        try
                        {
                            TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
                            if (IPAddressTextBox.Text == "")
                            {
                                return;
                            }


                            saveset();

                            //  string paths = GetPathToImage("sa");




                            // connect and login to the FTP
                            client.Connect(IPAddressTextBox.Text);
                            client.Login("anonymous", "DONT-LOOK@MYCODE");

                            client.Traversing += Traversing;
                            client.TransferProgressChanged += TransferProgressChanged;
                            client.DeleteProgressChanged += DeleteProgressChanged;
                            client.ProblemDetected += ProblemDetected;

                            client.PutFile(PKGLocation, @"/user/app/temp.pkg");

                        }


                        catch (Exception ex)
                        {
                            RunOnUiThread(() =>
                            {
                                MessageBox.Show(ex.Message);
                            });
                        }


                        client.SendCommand("installpkg");

                        SetTex("Package Sent");

                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(Application.Context, "Package Sent!", ToastLength.Short).Show();
                        });

                        client.Disconnect();
                    }



                })).Start();
            };
        }
    

    public class MessageBox

        {
            public static void Show(string Message)
            {
                new Android.Support.V7.App.AlertDialog.Builder(MainActivity._context)
                .SetMessage(Message)
                .SetPositiveButton("OK", (senderAlert, args) =>
                {
                   // SetResult(Result.Ok);
                })
                .Show();
            }
        }

    // Function called from OnDestroy
    protected void saveset()
    {

            //store
            TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
            var prefs = Application.Context.GetSharedPreferences("Package_Installer", FileCreationMode.Private);
        var prefEditor = prefs.Edit();
        prefEditor.PutString("IP", IPAddressTextBox.Text);
        prefEditor.Commit();

    }

    // Function called from OnCreate
    protected void retrieveset()
    {
        TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
            //retreive 
            var prefs = Application.Context.GetSharedPreferences("Package_Installer", FileCreationMode.Private);
        var somePref = prefs.GetString("IP", "");

        RunOnUiThread(() => IPAddressTextBox.Text = somePref);

        if (somePref != "")
        {
            //Show a toast
            RunOnUiThread(() => Toast.MakeText(this, "Saved IP Restored: " + somePref, ToastLength.Long).Show());
        }

    }

        private string GetActualPathFromFile(Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(this, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    string[] split = docId.Split(chars);
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {
                    string id = DocumentsContract.GetDocumentId(uri);

                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                                    Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    //System.Diagnostics.Debug.WriteLine(contentUri.ToString());

                    return getDataColumn(this, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    String[] split = docId.Split(chars);

                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[]
                    {
                split[1]
                    };

                    return getDataColumn(this, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {

                // Return the remote address
                if (isGooglePhotosUri(uri))
                    return uri.LastPathSegment;

                return getDataColumn(this, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }


        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection, String[] selectionArgs)
            {
                ICursor cursor = null;
                String column = "_data";
                String[] projection =
                {
                    column
                };

                try
                {
                    cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        int index = cursor.GetColumnIndexOrThrow(column);
                        return cursor.GetString(index);
                    }
                }
                finally
                {
                    if (cursor != null)
                        cursor.Close();
                }
                return null;
            }

            //Whether the Uri authority is ExternalStorageProvider.
            public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is DownloadsProvider.
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is MediaProvider.
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is Google Photos.
        public static bool isGooglePhotosUri(Android.Net.Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
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
            if (resultCode == Result.Ok && data != null && requestCode == 1)
            {
             

            }

        }
    


        private void SetTex(string text)
        {
            RunOnUiThread(() => {
                TextView InjectingLbl = FindViewById<TextView>(Resource.Id.InjectingLbl);
                InjectingLbl.Text = text;
            });
        }

        //textView13

        private void SetTexALT(string text)
        {
            RunOnUiThread(() => {
                TextView textView13 = FindViewById<TextView>(Resource.Id.textView13);
                textView13.Text = text;
            });
        }


        private void TransferCompleted()
        {
            _isWorking = false;
            SetTex("Finished");
          //  SetProgressValue(0);
        }

        private string GetRealPathFromURI(Android.Net.Uri uri)
        {
            string doc_id = "";
            using (var c1 = ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                string document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = ContentResolver.Query(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
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
        ///
        ///
        ///
        private int prre;
        void TransferProgressChanged(object sender, FtpTransferProgressChangedEventArgs e)
        {

            string strBatchInfo = string.Format("({0} / {1} file{2} processed)    ",
                e.FilesProcessed, e.FilesTotal, (e.FilesProcessed > 1 ? "s" : null));


          
            SetProgressValue("             %" + Convert.ToInt32(e.ProgressPercentage));


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

            SetTexALT(strBatchInfo);
        }

        private void SetProgressValue(string text)
        {
            RunOnUiThread(() => {
                TextView InjectingLbl = FindViewById<TextView>(Resource.Id.InjectingLbl);
                InjectingLbl.Text = text;
            });

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

            SetTexALT(strDeleteInfo);
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

            SetTexALT(strBatchInfo);
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