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
using ZXing.OneD;
using Xamarin.Essentials;

namespace NetEye.pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class userPage : ContentPage
    {
        List<Request> requests = new List<Request>
        {
            new Request { TechEquipmentId = "3", Description = "Computer not starting up", Status = "0" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "1" },
            new Request { TechEquipmentId = "348-01", Description = "Blue screen of death", Status = "2" },
            new Request { TechEquipmentId = "348-03", Description = "Equipment 1", Status = "3" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "0" },
            new Request { TechEquipmentId = "348-04", Description = "Equipment 3", Status = "1" },
            new Request { TechEquipmentId = "348-02", Description = "Computer overheating", Status = "2" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "3" },
            new Request { TechEquipmentId = "348-05", Description = "Equipment 3", Status = "0" },
            new Request { TechEquipmentId = "348-01", Description = "Hard drive failure", Status = "1" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "2" },
            new Request { TechEquipmentId = "348-06", Description = "Equipment 3", Status = "2" },
            new Request { TechEquipmentId = "348-01", Description = "Monitor not displaying", Status = "3" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "0" },
            new Request { TechEquipmentId = "348-07", Description = "Equipment 3", Status = "1" },
            new Request { TechEquipmentId = "348-01", Description = "Computer running slow", Status = "2" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "3" },
            new Request { TechEquipmentId = "348-08", Description = "Equipment 3", Status = "0" },
            new Request { TechEquipmentId = "348-01", Description = "Virus infection", Status = "1" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "2" },
            new Request { TechEquipmentId = "348-09", Description = "Equipment 3", Status = "3" },
            new Request { TechEquipmentId = "348-01", Description = "Computer not booting up", Status = "0" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "3" },
            new Request { TechEquipmentId = "348-10", Description = "Equipment 3", Status = "2" },
            new Request { TechEquipmentId = "348-01", Description = "Network connection issues", Status = "1" },
            new Request { TechEquipmentId = "348-02", Description = "Equipment 2", Status = "0" },
            new Request { TechEquipmentId = "348-11", Description = "Equipment 3", Status = "1" },
            new Request { TechEquipmentId = "348-01", Description = "Software not working", Status = "2" }
        };

        bool isFirstShowModal = true;
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

            
            requestsList.ItemsSource = requests;

            if (requests.Count == 0)
            {
                frameNotFound.IsVisible = true;
                labelNotFound.Text = "Вы пока не подавали заявок, сделайте это отсканировав QR код";
            }
        }

        public class Request
        {
            public string TechEquipmentId { get; set; }
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
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Camera>();
                status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            }
            if (status != PermissionStatus.Granted)
            {
                
                await DisplayAlert("Нет доступа", "У нас нет доступа к камере, пожалуйста разрешите в настройках вашего устройства доступ к камере для NetEye", "Ок");
                return;
            }


            var scanner = DependencyService.Get<scanningQrCode>();
            var resultScan = await scanner.ScanAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            if (resultScan != null && resultScan != "")
            {
                await Navigation.PushAsync(new addRequestPage(resultScan));
            }           

        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchEntry.Text.Length == 0)
            {
                requestsList.ItemsSource = requests;
                if (requests.Count == 0)
                {
                    frameNotFound.IsVisible = true;
                    labelNotFound.Text = "Вы пока не подавали заявок, сделайте это отсканировав QR код";
                }    
                else
                {
                    frameNotFound.IsVisible = false;
                }
            }
            else
            {
                var filteredObjects = requests.Where(o => o.TechEquipmentId.Contains(searchEntry.Text)).ToList();
                requestsList.ItemsSource = filteredObjects;
                if (filteredObjects.Count == 0)
                {
                    frameNotFound.IsVisible = true;
                    labelNotFound.Text = "Ничего не найдено, проверьте условия поиска";
                }
                else
                    frameNotFound.IsVisible = false;
            }
        }

        private async void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            await modalFrame.TranslateTo(-modalFrame.Width, 0, 250);
            await modalFrame.FadeTo(0, 125);
            modalFrame.IsVisible = false;
            await modalFrame.TranslateTo(0, 0, 250);
            isFirstShowModal= false;
            frameBtnScan.IsVisible = true;
        }

        private async void requestsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRequest = e.CurrentSelection[0] as Request;
            if (selectedRequest != null)
            {
                if (selectedRequest.Status == "2")
                {
                    frameBtnScan.IsVisible = false;
                    modalFrame.IsVisible = true;
                    if (!isFirstShowModal)
                    {                     
                        await modalFrame.FadeTo(250, 0);                       
                    }                    
                        modalId.Text = selectedRequest.TechEquipmentId;
                        modalDescription.Text = selectedRequest.Description;
                        //modal.cancelled ...
                }
                else
                {
                    modalFrame.IsVisible = false;
                    frameBtnScan.IsVisible = true;
                }
            }
        }
    }

}