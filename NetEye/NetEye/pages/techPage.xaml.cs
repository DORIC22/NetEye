using NetEye.res.model;
using NetEye.res.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static NetEye.pages.userPage;

namespace NetEye.pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class techPage : TabbedPage
    {
        TechEquipment equipment = new TechEquipment();
        private HttpClientWithJwt _httpClient;
        AuthUser fUser = new AuthUser();
        RepairRequest repairRequests = null;
        public techPage (AuthUser user)
        {
            InitializeComponent();
            _httpClient = HttpClientWithJwt.GetInstance();
            fUser = user;
            requestsList.ItemsSource = fUser.RepairRequestsReceived;

            picker_status.SelectedIndex = 0;
            Sort();            
            #region Ширина модалки
            modalFrameRequest.WidthRequest = App.Current.MainPage.Width - 20;
            frameSearchRequests.WidthRequest = App.Current.MainPage.Width - 20;
            modalFrameAfterScanning.WidthRequest = App.Current.MainPage.Width - 20;
            #endregion 
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
                Margin = new Thickness(0, -5, 0, 0)
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
        }

        private async void OnExButtonClicked(object sender, EventArgs e) // Выход
        {
            bool answer = await DisplayAlert("Выход", "Вы уверены что хотите выйти?", "Да", "Отмена");
            if (answer)
                await Navigation.PopAsync();

            // дописать стирание данных автовхода
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {           
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Camera>();
                status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            }
            if (status != PermissionStatus.Granted)
            {        
                tabbedPage.CurrentPage = tabbedPage.Children[0];
                await DisplayAlert("Нет доступа", "У нас нет доступа к камере, пожалуйста разрешите в настройках вашего устройства доступ к камере для NetEye", "Ок");                
                return;
            }
            
            //DependencyService.RegisterSingleton<IM>

            var scanner = DependencyService.Get<scanningQrCode>();
            var resultScan = await scanner.ScanAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            if (resultScan != null && resultScan != "")
            {
                equipment = _httpClient.GetTechEquipmentById(resultScan);
                if (equipment != null)
                {
                    modalFrameRequest.IsVisible = false;
                    modalFrameAfterScanning.IsVisible = true;
                    modalAfterScanningLabelIdRequest.Text = resultScan;
                }
                else
                {
                    tabbedPage.CurrentPage = tabbedPage.Children[0];
                    bool answer = await DisplayAlert("Уведомление", "Данное оборудование не " +
                    "добавлено в базу данных.\nДобавить сейчас?", "Да", "Нет");
                }
            }            
            tabbedPage.CurrentPage = tabbedPage.Children[0];
        }

        private void requestsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modalFrameAfterScanning.IsVisible = false;
            var selectedRequest = e.CurrentSelection[0] as RepairRequest;
            if (selectedRequest != null)
            {
                modalFrameRequest.IsVisible= true;
                modalDescription.Text = selectedRequest.Description;  
                modalDate.Text = selectedRequest.CreatedDate.ToString();
                modalLabelIdRequest.Text = selectedRequest.Id.ToString();

                User user = _httpClient.GetUserById(selectedRequest.UserFromId);
                modalUserFrom.Text = user.FullName;
                requestsList.SelectedItem = SelectableItemsView.EmptyViewProperty;
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            modalFrameRequest.IsVisible= false;            
        }

        private void entrySearchRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            Sort();
        }

        public void Sort()
        {
            var filteredRequests = fUser.RepairRequestsReceived;
            if (!string.IsNullOrEmpty(entrySearchRequest.Text))
            {
                filteredRequests = fUser.RepairRequestsReceived.Where(o => o.TechEquipmentId.Contains(entrySearchRequest.Text)).ToList();
                if (filteredRequests.Count == 0) 
                {
                    frameNotFound.IsVisible= true;
                }
                else
                {
                    frameNotFound.IsVisible = false;
                }
            }            
           
            var filtererBySearchAndStatus = filteredRequests.Where(o => o.Status == picker_status.SelectedIndex);
            requestsList.ItemsSource = filtererBySearchAndStatus;
        }

        private void picker_status_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sort();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void ImageButton_Clicked_1(object sender, EventArgs e)
        {
            modalFrameAfterScanning.IsVisible= false;
        }
    }
}