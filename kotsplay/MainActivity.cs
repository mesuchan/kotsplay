using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using kotsplay.camera;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;

namespace kotsplay
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        string[] Items = new string[]
        {
            "Книга магических рецептов",
            "Библиотека крафта в кибермире"
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ListView listView = FindViewById<ListView>(Resource.Id.listViewBooks);
            listView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, Items);
            listView.ItemClick += ListViewItemClick;

            Button button = FindViewById<Button>(Resource.Id.buttonNewBook);
            button.Click += ButtonClick;
        }

        private void ListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //Переход на активити с камерой и подгрузка книги рецептов
            StartActivity(typeof(CameraViewActivity));
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile();
                if (fileData == null)
                    return;

                string fileName = fileData.FileName;
                string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
            }
            catch (Exception)
            {
                Toast.MakeText(this, "Ошибка загрузки файла", ToastLength.Short);
            }
        }
    }
}

