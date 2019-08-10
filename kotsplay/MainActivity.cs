using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;

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
            Toast.MakeText(this, "item clicked", ToastLength.Short);
            //Переход на активити с камерой и подгрузка книги рецептов
        }

        private void ButtonClick(object sender, System.EventArgs e)
        {
            //Переход на активити с камерой и подгрузка новой книги рецептов
        }
    }
}

