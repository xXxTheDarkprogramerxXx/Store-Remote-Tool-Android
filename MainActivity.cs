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
using System.ComponentModel.Design;
using System.Drawing;
using System.Xml;
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
    [Activity(Label = "Package Installer", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/ps4")]

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



        Button SelectPluginUI;
        Button SelectPackageUI;


        protected override void OnCreate(Bundle savedInstanceState)
        {
           
            //first check if there are permsions to access files ext
            bool checks = true;
    

            #region << Check For Permisions >>

            // Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) || Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet)

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                    Manifest.Permission.Internet) != (int)Permission.Granted)
            {

                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Internet))
                {



                    ActivityCompat.RequestPermissions(this,
                        new String[] { Manifest.Permission.Internet, Manifest.Permission.Internet }, 1);
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



                    ActivityCompat.RequestPermissions(this,
                        new String[] { Manifest.Permission.AccessWifiState, Manifest.Permission.AccessWifiState }, 1);
                    //  })).Show();
                }

                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) !=
                       (int)Permission.Granted)
                {
                    Thread.Sleep(100);
                }
            }

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                    Manifest.Permission.AccessNetworkState) != (int)Permission.Granted)
            {

                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessNetworkState))
                {



                    ActivityCompat.RequestPermissions(this,
                        new String[] { Manifest.Permission.AccessNetworkState, Manifest.Permission.AccessNetworkState },
                        1);
                    //  })).Show();
                }

                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessNetworkState) !=
                       (int)Permission.Granted)
                {
                    Thread.Sleep(100);
                }
            }

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                    Manifest.Permission.ReadExternalStorage) !=
                (int)Permission.Granted)
            {
                // Camera permission has not been granted
                RequestReadWirtePermission();
                while (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) !=
                       (int)Permission.Granted)
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

            SetContentView(Resource.Layout.selectui);
            this.Title = "Package Installer"; //set the title



            refreshui();

            SelectPackageUI.Click += async delegate
            {
                Intent Next = new Intent(this, typeof(installer));
                StartActivity(Next);
            };

            SelectPluginUI.Click += async delegate
            {
                Intent Next = new Intent(this, typeof(plugin));
                StartActivity(Next);
            };


        }


    protected void refreshui()
    {
        #region << Define UI >>

        SelectPackageUI = FindViewById<Button>(Resource.Id.SelectPackageUI);
        SelectPluginUI = FindViewById<Button>(Resource.Id.SelectPluginUI);

            #endregion
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
