using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using kotsplay.camera;

namespace kotsplay
{
    [Activity(Label = "RecipesActivity")]
    public class RecipesActivity : AppCompatActivity
    {
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Toast.MakeText(this, item.ItemId, ToastLength.Long);

            if (item.ItemId == Resource.Id.menu_ingredients)
                StartActivity(typeof(IngredientsActivity));

            if (item.ItemId == Resource.Id.menu_exit)
                StartActivity(typeof(CameraViewActivity));

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_recipes);

            string[] Items = new string[] { };

            ListView listView = FindViewById<ListView>(Resource.Id.listViewRecipes);
            listView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, Items);
        }
    }
}