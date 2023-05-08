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
using NetEye.res.model;

namespace NetEye.pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class userPage : ContentPage
    {
        TechEquipment equipment = new TechEquipment();
        private HttpClientWithJwt _httpClient;
        AuthUser fUser = new AuthUser();
        RepairRequest repairRequests = null;
        bool isFirstShowModal = true;
        public userPage(AuthUser user)
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
            _httpClient = HttpClientWithJwt.GetInstance();
            fUser = user;
            editorDescriptionRequest.Text = "";
            frameModalAddRequest.WidthRequest = App.Current.MainPage.Width - 20;

            if (fUser.RepairRequestsSubmitted == null)
            {
                fUser.RepairRequestsSubmitted = new HashSet<RepairRequest>(new List<RepairRequest>());
            }

            requestsList.ItemsSource = fUser.RepairRequestsSubmitted;

            if (fUser.RepairRequestsSubmitted.Count == 0)
            {
                frameNotFound.IsVisible = true;
                labelNotFound.Text = "Вы пока не подавали заявок, сделайте это отсканировав QR код";
            }
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
                equipment = _httpClient.GetTechEquipmentById(resultScan);

                if (equipment != null)
                {
                    frameModalAddRequest.IsVisible= true;
                    labelIdEquipment.Text = equipment.Id;
                }          
                else
                {
                    await DisplayAlert("Оборудование не добавлено", "Данное оборудование отсутствует в базе данных." +
                        " Обратитесь к системному администратору для добавления оборудования.", "Ок");
                }
            }           

        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchEntry.Text.Length == 0)
            {
                requestsList.ItemsSource = fUser.RepairRequestsSubmitted;
                if (fUser.RepairRequestsSubmitted.Count == 0)
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
                var filteredObjects = fUser.RepairRequestsSubmitted.Where(o => o.TechEquipmentId.Contains(searchEntry.Text)).ToList();
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
            var selectedRequest = e.CurrentSelection[0] as RepairRequest;
            if (selectedRequest != null)
            {
                if (selectedRequest.Status == 3)
                {
                    frameBtnScan.IsVisible = false;
                    frameModalAddRequest.IsVisible = false;
                    modalFrame.IsVisible = true;
                    
                    if (!isFirstShowModal)
                    {                     
                        await modalFrame.FadeTo(250, 0);                       
                    }                    
                    modalId.Text = selectedRequest.TechEquipmentId;
                    modalDescription.Text = selectedRequest.Description;
                    if (!string.IsNullOrEmpty(selectedRequest.RepairNote))
                        modalDescriptionCancel.Text = selectedRequest.RepairNote;
                    else
                        modalDescriptionCancel.Text = "Не указано.";

                }
                else
                {
                    modalFrame.IsVisible = false;
                    frameBtnScan.IsVisible = true;
                }
            }
        }

        private void editorDescriptionRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (btnSend.IsEnabled == false)
            {
                if (editorDescriptionRequest.Text.Length > 14)
                {
                    btnSend.IsEnabled = true;
                }
            }
        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Отмена", "Отменить подачу заявки?", "Да", "Нет");
            if (answer)
                frameModalAddRequest.IsVisible = false;
        }

        private async void btnSend_Clicked(object sender, EventArgs e)
        {
            string desc = editorDescriptionRequest.Text;
            int userId = Convert.ToInt32(App.Current.Properties["UserId"]);
            string techId = labelIdEquipment.Text;

            if (desc.Length < 14)
            {
                btnSend.IsEnabled = false;
                DependencyService.Get<IToast>().LongToast("Опишите проблему более детально");
                return;
            }

            try
            {
                RepairRequest request = new RepairRequest();
                request.Description = desc;
                request.UserFromId = userId;
                request.Status = 0;
                request.TechEquipmentId = techId;
                _httpClient.PostRepairRequest(request);
                DependencyService.Get<IToast>().LongToast("Заявка успешно подана");
                frameModalAddRequest.IsVisible = false;

                fUser.RepairRequestsSubmitted.Add(request);
                requestsList.ItemsSource = null;
                requestsList.ItemsSource = fUser.RepairRequestsSubmitted;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.ToString(), "Ок");
            }
        }
    }

}