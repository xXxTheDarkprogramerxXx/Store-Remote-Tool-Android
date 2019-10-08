using System;
using System.Threading;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Support.V7.App;
using Android.Util;
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
    [Activity(Label = "Package Installer", Theme = "@style/AppTheme", MainLauncher = false, Icon = "@drawable/ps4")]

    public class installer : AppCompatActivity
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
        private string curr;
        public static Context _context;


        Button LoadPkg;
        Button set;
        Button SendPayloadBtn;
        TextView InjectingLbl;
        TextView textView13;
        ProgressBar progressBar1;
        TextView textView7;
        TextView txtPKGInfo;
        ImageView PKGIcon;
        TextView textView12;
        TextView textView6;
        TextView textView11;
        EditText FTPPassword;
        TextView txtLabel;
        EditText FTPUsername;
        TextView textView14;
        TextView txtIpLabel;
        TextView txtLabel66;
        TextView txtLabel77;
        EditText IPAddressTextBox;
        EditText manpath;
        Button SelectPackageUI;

        private string finished;
    Android.App.ProgressDialog progress;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        _context = this;
        //first check if there are permsions to access files ext
        bool checks = true;


        #region << Check For Permisions >>

        // Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) || Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet)

        if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                Manifest.Permission.Internet) != (int) Permission.Granted)
        {

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Internet))
            {



                ActivityCompat.RequestPermissions(this,
                    new String[] {Manifest.Permission.Internet, Manifest.Permission.Internet}, 1);
                //  })).Show();
            }

            while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) !=
                   (int) Permission.Granted)
            {
                Thread.Sleep(100);
            }
        }


        if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                Manifest.Permission.AccessWifiState) != (int) Permission.Granted)
        {

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessWifiState))
            {



                ActivityCompat.RequestPermissions(this,
                    new String[] {Manifest.Permission.AccessWifiState, Manifest.Permission.AccessWifiState}, 1);
                //  })).Show();
            }

            while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) !=
                   (int) Permission.Granted)
            {
                Thread.Sleep(100);
            }
        }

        if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                Manifest.Permission.AccessNetworkState) != (int) Permission.Granted)
        {

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessNetworkState))
            {



                ActivityCompat.RequestPermissions(this,
                    new String[] {Manifest.Permission.AccessNetworkState, Manifest.Permission.AccessNetworkState},
                    1);
                //  })).Show();
            }

            while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessNetworkState) !=
                   (int) Permission.Granted)
            {
                Thread.Sleep(100);
            }
        }

        if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                Manifest.Permission.ReadExternalStorage) !=
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

        SetContentView(Resource.Layout.installer);

        refreshui();
        retrieveset();




        LoadPkg.Click += async delegate
            {
                try
                {



                               String[] types = new String[] { ".pkg" };
                    FileData fileData = await CrossFilePicker.Current.PickFile(types);
                    if (fileData == null)
                        return; // user canceled file picking
                                //com.android.externalstorage.documents



                    System.Console.WriteLine("File name " + fileData.FileName);

                     new Thread(new ThreadStart(delegate
                    {
                        RunOnUiThread(() =>
                        {
                            progress = new ProgressDialog(this);
                            progress.Indeterminate = true;
                            progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
                            progress.SetMessage("Loading... Getting Path...");
                            progress.SetCancelable(false);
                            progress.Show();
                        });

                        FindFileByName(fileData.FileName);

                        System.Console.WriteLine("Found File, Path= " + PKGLocation);

                        RunOnUiThread(() =>
                        {
                            progress.Hide();
                            try
                            {
                                var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(PKGLocation);
                                ImageView pbPkg = FindViewById<ImageView>(Resource.Id.PKGIcon);
                                pbPkg.SetImageBitmap(BytesToBitmap(pkgfile.Icon));
                                TextView lblPackageInfo = FindViewById<TextView>(Resource.Id.txtPKGInfo);
                                lblPackageInfo.Text =
                                    pkgfile.PS4_Title + "\n" + pkgfile.PKG_Type.ToString() + "\n" +
                                    pkgfile.Param.TitleID;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Invaild Package! or Path\n\n Location: " + PKGLocation);
                                // MessageBox.Show("Exception choosing file: " + ex.ToString() + "    " + PKGLocation);
                            }


                        });

                      

                    })).Start();


                    
                






                }
           
                catch (Exception ex)
                {
                    MessageBox.Show("Invaild Package! or Path\n\n Location: " + PKGLocation);
                    // MessageBox.Show("Exception choosing file: " + ex.ToString() + "    " + PKGLocation);
                }
            };

            set.Click += async delegate
            {
                try
                {




                        PKGLocation = manpath.Text;

                       
                            var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(PKGLocation);
                            ImageView pbPkg = FindViewById<ImageView>(Resource.Id.PKGIcon);
                            pbPkg.SetImageBitmap(BytesToBitmap(pkgfile.Icon));
                            TextView lblPackageInfo = FindViewById<TextView>(Resource.Id.txtPKGInfo);
                            lblPackageInfo.Text =
                                pkgfile.PS4_Title + "\n" + pkgfile.PKG_Type.ToString() + "\n" +
                                pkgfile.Param.TitleID;











                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show("Invaild Package! or Path\n\n Location: " + PKGLocation);
                            // MessageBox.Show("Exception choosing file: " + ex.ToString() + "    " + PKGLocation);
                        }
                };





            SendPayloadBtn.Click += delegate
            {
                new Thread(new ThreadStart(delegate
                {
                    using (Ftp client = new Ftp())
                    {
                        Rebex.Licensing.Key = "==AnKxIZnJ2NXyRRk/MrXLh5vsLbImP/JhMGERReY23qIk==";
                        try
                        {
                           // TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
                            if (IPAddressTextBox.Text == "")
                            {
                                RunOnUiThread(() => { MessageBox.Show("Enter an IP Address"); });
                                return;
                            }


                            saveset();

                            //  string paths = GetPathToImage("sa");




                            // connect and login to the FTP
                            client.Connect(IPAddressTextBox.Text);
                            //TextView FTPPassword = FindViewById<TextView>(Resource.Id.FTPPassword);
                            //TextView FTPUsername = FindViewById<TextView>(Resource.Id.FTPUsername);
                            if (FTPPassword.Text == "" && FTPUsername.Text == "")
                            {
                                client.Login("anonymous", "DONT-LOOK@MYCODE");
                            }
                            else
                            {
                                client.Login(FTPUsername.Text, FTPPassword.Text);
                            }


                            client.Traversing += Traversing;
                            client.TransferProgressChanged += TransferProgressChanged;
                            client.DeleteProgressChanged += DeleteProgressChanged;
                            client.ProblemDetected += ProblemDetected;

                            if (!client.DirectoryExists(@"/user/app/"))
                            {
                                client.CreateDirectory(@"/user/app/");
                            }

                            if (client.FileExists(@"/user/app/temp.pkg"))
                            {
                                client.Delete(@"/user/app/temp.pkg", TraversalMode.NonRecursive);
                            }

                            client.ChangeDirectory(@"/user/app/");


                            client.PutFile(PKGLocation, "temp.pkg");

                            try
                            {
                                client.SendCommand("installpkg");
                            }
                            catch (Exception ex)
                            {
                                SetTex("installpkg Failed\n Are you sure your using Inifinx?");
                            }


                            SetTex("Package Sent");

                        }

                        catch (Exception ex)
                        {
                            RunOnUiThread(() => { MessageBox.Show(ex.Message); });
                        }

                   

                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(Application.Context, "Package Sent!", ToastLength.Short).Show();
                        });

                        client.Disconnect();
                    }



                })).Start();
            };

            /* }
             else
             {
                 SendPayloadBtn.Visibility = ViewStates.Invisible;
                 LoadPkg.Visibility = ViewStates.Invisible;
             }*/
        }


        public class MessageBox

        {
            public static void Show(string Message)
            {
                new Android.Support.V7.App.AlertDialog.Builder(installer._context)
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
           // TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
            var prefs = Application.Context.GetSharedPreferences("Package_Installer", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("IP", IPAddressTextBox.Text);
            prefEditor.Commit();

        }

        int FindFileByName(string sss)
        {
           
            string folder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

           // RunOnUiThread(() => MessageBox.Show("[Debug LOG] ExternalStorageDirectory is " + folder));

            try
            {

                String[] files = Directory.GetFiles(folder, sss, SearchOption.AllDirectories);
                foreach (String filez in files)
                {
                    PKGLocation = filez;

                    if (filez == null)
                    {
                        string card = SDPath();

                        String[] morefiles = Directory.GetFiles(card, sss, SearchOption.AllDirectories);
                        foreach (String cardfile in morefiles)
                        {

                            PKGLocation = card;
                           // RunOnUiThread(() => MessageBox.Show("[Debug LOG] SDCard is " + card));
                        }
                    }
                }


                System.Console.WriteLine("File " + PKGLocation);
               

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

          

            return 0;
        }

        private String SDPath()
        {
            String sdcardpath = "";

            //Datas
            if (new Java.IO.File("/data/sdext4/").Exists() && new Java.IO.File("/data/sdext4/").CanRead())
            {
                sdcardpath = "/data/sdext4/";
            }
            if (new Java.IO.File("/data/sdext3/").Exists() && new Java.IO.File("/data/sdext3/").CanRead())
            {
                sdcardpath = "/data/sdext3/";
            }
            if (new Java.IO.File("/data/sdext2/").Exists() && new Java.IO.File("/data/sdext2/").CanRead())
            {
                sdcardpath = "/data/sdext2/";
            }
            if (new Java.IO.File("/data/sdext1/").Exists() && new Java.IO.File("/data/sdext1/").CanRead())
            {
                sdcardpath = "/data/sdext1/";
            }
            if (new Java.IO.File("/data/sdext/").Exists() && new Java.IO.File("/data/sdext/").CanRead())
            {
                sdcardpath = "/data/sdext/";
            }

            //MNTS

            if (new Java.IO.File("mnt/sdcard/external_sd/").Exists() && new Java.IO.File("mnt/sdcard/external_sd/").CanRead())
            {
                sdcardpath = "mnt/sdcard/external_sd/";
            }
            if (new Java.IO.File("mnt/extsdcard/").Exists() && new Java.IO.File("mnt/extsdcard/").CanRead())
            {
                sdcardpath = "mnt/extsdcard/";
            }
            if (new Java.IO.File("mnt/external_sd/").Exists() && new Java.IO.File("mnt/external_sd/").CanRead())
            {
                sdcardpath = "mnt/external_sd/";
            }
            if (new Java.IO.File("mnt/emmc/").Exists() && new Java.IO.File("mnt/emmc/").CanRead())
            {
                sdcardpath = "mnt/emmc/";
            }
            if (new Java.IO.File("mnt/sdcard0/").Exists() && new Java.IO.File("mnt/sdcard0/").CanRead())
            {
                sdcardpath = "mnt/sdcard0/";
            }
            if (new Java.IO.File("mnt/sdcard1/").Exists() && new Java.IO.File("mnt/sdcard1/").CanRead())
            {
                sdcardpath = "mnt/sdcard1/";
            }
            if (new Java.IO.File("mnt/sdcard/").Exists() && new Java.IO.File("mnt/sdcard/").CanRead())
            {
                sdcardpath = "mnt/sdcard/";
            }

            //Storages
            if (new Java.IO.File("/storage/removable/sdcard1/").Exists() && new Java.IO.File("/storage/removable/sdcard1/").CanRead())
            {
                sdcardpath = "/storage/removable/sdcard1/";
            }
            if (new Java.IO.File("/storage/external_SD/").Exists() && new Java.IO.File("/storage/external_SD/").CanRead())
            {
                sdcardpath = "/storage/external_SD/";
            }
            if (new Java.IO.File("/storage/ext_sd/").Exists() && new Java.IO.File("/storage/ext_sd/").CanRead())
            {
                sdcardpath = "/storage/ext_sd/";
            }
            if (new Java.IO.File("/storage/sdcard1/").Exists() && new Java.IO.File("/storage/sdcard1/").CanRead())
            {
                sdcardpath = "/storage/sdcard1/";
            }
            if (new Java.IO.File("/storage/sdcard0/").Exists() && new Java.IO.File("/storage/sdcard0/").CanRead())
            {
                sdcardpath = "/storage/sdcard0/";
            }
            if (new Java.IO.File("/storage/sdcard/").Exists() && new Java.IO.File("/storage/sdcard/").CanRead())
            {
                sdcardpath = "/storage/sdcard/";
            }
            if (new Java.IO.File("/storage/extSdCard/").Exists() && new Java.IO.File("/storage/extSdCard/").CanRead())
            {
                sdcardpath = "/storage/extSdCard/";
            }


            //("SDFinder", "Path: " + sdcardpath);
            return sdcardpath;
        }

        private string GetRealPathFromURI(Android.Net.Uri contentURI)
        {
            ICursor cursor = ContentResolver.Query(contentURI, null, null, null, null);
            cursor.MoveToFirst();
            string documentId = cursor.GetString(0);
            documentId = documentId.Split(':')[1];
            cursor.Close();

            cursor = ContentResolver.Query(
                Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new[] { documentId }, null);
            cursor.MoveToFirst();
            string path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
            cursor.Close();

            return path;
        }


        // Function called from OnCreate
        protected void retrieveset()
        {
            //TextView IPAddressTextBox = FindViewById<TextView>(Resource.Id.IPAddressTextBox);
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

        void wtfisthepath(FileData fileData)
        {


            // var exstor = global::Android.OS.Environment.ExternalStorageDirectory;
            //System.Console.WriteLine(exstor.AbsolutePath);
            string folder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var extstor = GetBaseFolderPath(true);
            Java.IO.File mmm = new Java.IO.File(fileData.FilePath);
            System.Console.WriteLine("folder " + folder);

            if (fileData.FilePath.Contains("com.android.externalstorage.documents"))
            {
                PKGLocation = "/storage/emulated/0/Documents/" + fileData.FileName;
            }
            else if (fileData.FilePath.Contains("com.android.providers.downloads.documents"))
            {
                PKGLocation = "/storage/emulated/0/Download/" + fileData.FileName;
                System.Console.WriteLine("f " + fileData.FileName);

               
            }
            else
            {
                PKGLocation = fileData.FilePath;
            }

            if (Android.OS.Environment.InvokeIsExternalStorageRemovable(mmm))
            {
                System.Console.WriteLine("File is on SD Card");
            }
            else if (!Android.OS.Environment.InvokeIsExternalStorageRemovable(mmm))
            {
                System.Console.WriteLine("File is NOT on SD Card");
            }
        }





        public static string GetBaseFolderPath(bool getSDPath = false)
        {
            string baseFolderPath = "";

            try
            {
                Context context = Application.Context;
                Java.IO.File[] dirs = context.GetExternalFilesDirs(null);

                foreach (Java.IO.File folder in dirs)
                {
                    bool IsRemovable = Android.OS.Environment.InvokeIsExternalStorageRemovable(folder);
                    bool IsEmulated = Android.OS.Environment.InvokeIsExternalStorageEmulated(folder);

                    if (getSDPath ? IsRemovable && !IsEmulated : !IsRemovable && IsEmulated)
                        baseFolderPath = folder.Path;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("GetBaseFolderPath caused the follwing exception: {0}", ex);
            }

            return baseFolderPath;
        }

        protected void refreshui()
        {
            #region << Define UI >>
            LoadPkg = FindViewById<Button>(Resource.Id.BrowsePayloadBtn);
            SendPayloadBtn = FindViewById<Button>(Resource.Id.SendPayloadBtn);
            SelectPackageUI = FindViewById<Button>(Resource.Id.SelectPackageUI);
            txtLabel66 = FindViewById<TextView>(Resource.Id.txtLabel66);
            txtLabel77 = FindViewById<TextView>(Resource.Id.txtLabel77);

            manpath = FindViewById<EditText>(Resource.Id.manpath);


            set = FindViewById<Button>(Resource.Id.set);


            textView13 = FindViewById<TextView>(Resource.Id.textView13);
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            textView7 = FindViewById<TextView>(Resource.Id.textView7);
            txtPKGInfo = FindViewById<TextView>(Resource.Id.txtPKGInfo);
            PKGIcon = FindViewById<ImageView>(Resource.Id.PKGIcon);
            textView12 = FindViewById<TextView>(Resource.Id.textView12);
            textView6 = FindViewById<TextView>(Resource.Id.textView6);
            textView11 = FindViewById<TextView>(Resource.Id.textView11);
            FTPPassword = FindViewById<EditText>(Resource.Id.FTPPassword);
            txtLabel = FindViewById<TextView>(Resource.Id.txtLabel);
            FTPUsername = FindViewById<EditText>(Resource.Id.FTPUsername);
            textView14 = FindViewById<TextView>(Resource.Id.textView14);
            txtIpLabel = FindViewById<TextView>(Resource.Id.txtIpLabel);
            InjectingLbl = FindViewById<TextView>(Resource.Id.InjectingLbl);
            textView13 = FindViewById<TextView>(Resource.Id.textView13);
            IPAddressTextBox = FindViewById<EditText>(Resource.Id.IPAddressTextBox);
            #endregion
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

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string toast = string.Format("The planet is {0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(this, toast, ToastLength.Long).Show();
            curr = toast;
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
                InjectingLbl.Text = text;
            });
        }

        //textView13

        private void SetTexALT(string text)
        {
            RunOnUiThread(() => {
                textView13.Text = text;
            });
        }


        private void TransferCompleted()
        {
            _isWorking = false;
            SetTex("Finished");
            //  SetProgressValue(0);
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
