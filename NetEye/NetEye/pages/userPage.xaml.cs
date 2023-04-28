using NetEye.res.service;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZXing;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetEye.pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class userPage : ContentPage
    {
        public userPage()
        {
            InitializeComponent();
            #region navBar
            NavigationPage.SetHasBackButton(this, false);


            var titleView = new StackLayout();
            var titleUndView = new StackLayout();
            titleView.Orientation = StackOrientation.Horizontal;
            titleView.Margin = new Thickness(0, 0, 15, 0);
            titleView.HorizontalOptions = LayoutOptions.FillAndExpand;

            var exButton = new ImageButton()
            {
                Source = "res/image/ic_launcher.png",
                WidthRequest = 45,
                HeightRequest = 45,
                BackgroundColor = Color.FromHex("#F4F4F3")                
            };

            exButton.Clicked += OnExButtonClicked;

            var titleLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                Text = "Net - Eye",
                FontSize = 20,
                TextColor = Color.Black,
                FontFamily = "ur"
            };

            var titleUnderLabel = new Label
            {
                HorizontalOptions = LayoutOptions.End,
                Text = "Mobile",
                FontSize = 12,
                TextColor = Color.FromHex("#839BFF"),
                FontFamily = "ur",
                Margin = new Thickness(0,-5,0,0)
            };
            // Создаем новую картинку
            var iconImage = new Image
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Source = "res/image/logoic.png",
                HeightRequest = 45,
                WidthRequest = 45
            };

            // Добавляем метку и картинку в StackLayout

            titleView.Children.Add(exButton);
            titleUndView.Children.Add(titleLabel);
            titleUndView.Children.Add(titleUnderLabel);
            titleView.Children.Add(titleUndView);
            titleView.Children.Add(iconImage);
            

            NavigationPage.SetTitleView(this, titleView);
            #endregion

            #region Помойка
            var requests = new List<Request>
            {
                new Request { TechEquipmentId = 1, Description = "Equipment 12333333333333333333333333333333333333331", Status = "1" },
                new Request { TechEquipmentId = 2, Description = "Equipment 2", Status = "2" },
                new Request { TechEquipmentId = 3, Description = "Equipment 3222222222222222222222222222sdaffffffdsfsdfsdfsdfsdfsdfsdfsdfdsfsdfsdfsd" +
                "fsdfsdfsdfsd3", Status = "Denied" },
                 new Request { TechEquipmentId = 1, Description = "Equipment 1", Status = "3" },
                new Request { TechEquipmentId = 2, Description = "Equipment 2", Status = "4" },
                new Request { TechEquipmentId = 3, Description = "Equipment 3", Status = "5" },
                 new Request { TechEquipmentId = 1, Description = "Equipment 1", Status = "1" },
                new Request { TechEquipmentId = 2, Description = "Equipment 2", Status = "2" },
                new Request { TechEquipmentId = 3, Description = "Equipment 3", Status = "3" },
                 new Request { TechEquipmentId = 1, Description = "Equipment 1", Status = "4" },
                new Request { TechEquipmentId = 2, Description = "Equipment 2", Status = "2" },
                new Request { TechEquipmentId = 3, Description = "Equipment 3", Status = "2" },
                 new Request { TechEquipmentId = 1, Description = "Equipment 1", Status = "1" },
                new Request { TechEquipmentId = 2, Description = "Equipment 2", Status = "3" },
                new Request { TechEquipmentId = 3, Description = "Equipment 3", Status = "4" }
            };


            requestsList.ItemsSource = requests;
            #endregion
        }

        public class Request
        {
            public int TechEquipmentId { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
        }


        private async void OnExButtonClicked(object sender, EventArgs e) // Выход
        {
            bool answer = await DisplayAlert("Выход", "Вы уверены что хотите выйти?", "Да", "Отмена");
            if (answer)
                await Navigation.PopAsync();

            // дописать стирание данных автовхода
        }

        protected override bool OnBackButtonPressed()
        {            
           return true;
        } 

        private async void btnScan_Clicked(object sender, EventArgs e)
        {
            bool flash = false;
            if (switchFlashLight.IsToggled)
                flash= true;

            
            var scanner = DependencyService.Get<scanningQrCode>();
            var resultScan = await scanner.ScanAsync(flash);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            if (resultScan != null && resultScan != "")
            {
                await Navigation.PushAsync(new addRequestPage(resultScan));
            }

        }

        
    }

}