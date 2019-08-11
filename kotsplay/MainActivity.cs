using Android.App;
using Android.Database;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using kotsplay.camera;
using kotsplay.Entities;
using kotsplay.Entities.Parsers;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace kotsplay
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private readonly List<Book> books = new List<Book>();
        private ArrayAdapter<Book> booksAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            booksAdapter = new ArrayAdapter<Book>(this, Android.Resource.Layout.SimpleListItem1, books);

            ListView listView = FindViewById<ListView>(Resource.Id.listViewBooks);
            listView.Adapter = booksAdapter;
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
                var book = Book.LoadFromXml(fileData.FilePath);
                booksAdapter.Add(book);
            }
            catch (ParseException)
            {
                Toast.MakeText(this, "Некорректное содержимое файла", ToastLength.Short).Show();
            }
            catch (XmlException)
            {
                Toast.MakeText(this, "Ошибка чтения xml", ToastLength.Short).Show();
            }
            catch (Exception)
            {
                Toast.MakeText(this, "Ошибка открытия файла", ToastLength.Short).Show();
            }
        }
    }
}

