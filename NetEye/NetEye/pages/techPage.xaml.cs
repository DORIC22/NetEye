using NetEye.res.model;
using NetEye.res.service;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetEye.pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class techPage : Xamarin.Forms.TabbedPage
    {
        TechEquipment equipment = new TechEquipment();
        RepairRequest selectedRequest = new RepairRequest();
        TechEquipment selectedTechEquipment = new TechEquipment();
        private HttpClientWithJwt _httpClient;
        AuthUser fUser = new AuthUser();
        List<TechEquipment> techEquipmentList = new List<TechEquipment>();
        RepairRequest repairRequests = null;
        public techPage (AuthUser user)
        {
            InitializeComponent();
            _httpClient = HttpClientWithJwt.GetInstance();
            fUser = user;
            if (fUser.RepairRequestsReceived == null)
            {
                RepairRequest request = new RepairRequest();
                fUser.RepairRequestsReceived = new List<RepairRequest>();
            }
            requestsList.ItemsSource = fUser.RepairRequestsReceived;           
            techEquipmentList = _httpClient.GetAllTechEquipment();
            techEquipmentCollection.ItemsSource= techEquipmentList;

            picker_status.SelectedIndex = 0;
            //Sort();            
            #region Ширина модалки
            modalFrameAddTechEquipment.WidthRequest = App.Current.MainPage.Width - 20;
            modalFrameRequest.WidthRequest = App.Current.MainPage.Width - 20;
            frameSearchRequests.WidthRequest = App.Current.MainPage.Width - 20;
            modalFrameAfterScanning.WidthRequest = App.Current.MainPage.Width - 20;
            modalFrameAddRepairRequest.WidthRequest = App.Current.MainPage.Width - 20;
            modalFrameDetailsTechEquipment.WidthRequest = App.Current.MainPage.Width - 20;
            #endregion
            #region navBar
            Xamarin.Forms.NavigationPage.SetHasBackButton(this, false);


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


            Xamarin.Forms.NavigationPage.SetTitleView(this, titleView);
            #endregion
        }

        private async void OnExButtonClicked(object sender, EventArgs e) // Выход
        {
            bool answer = await DisplayAlert("Выход", "Вы уверены что хотите выйти?", "Да", "Отмена");
            if (answer)
            {
                App.Current.Properties.Remove("email");
                App.Current.Properties.Remove("password");
                App.Current.Properties.Remove("rememberMe");
                await Navigation.PopAsync();
            }
                

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
                modalFrameAfterScanning.IsVisible = false;
                modalFrameAddTechEquipment.IsVisible = false;
                if (equipment != null)
                {
                    modalFrameAddRepairRequest.IsVisible = false;
                    modalFrameRequest.IsVisible = false;
                    modalFrameAfterScanning.IsVisible = true;
                    modalAfterScanningLabelIdRequest.Text = resultScan;
                    tabbedPage.CurrentPage = tabbedPage.Children[0];
                }
                else
                {
                    tabbedPage.CurrentPage = tabbedPage.Children[0];
                    bool answer = await DisplayAlert("Уведомление", "Данное оборудование не " +
                    "добавлено в базу данных.\nДобавить сейчас?", "Да", "Нет");
                    if (answer)
                    {
                        modalFrameDetailsTechEquipment.IsVisible= false;
                        tabbedPage.CurrentPage = tabbedPage.Children[1];
                        pickerTypeTechEquipmentForAdd.SelectedIndex= 0;
                        modalFrameAddTechEquipment.IsVisible = true;
                        modalFrameAddTechEquipmentLabelId.Text = resultScan;
                    }
                }
            }
            else
            {
                tabbedPage.CurrentPage = tabbedPage.Children[0];
            }
        }

        private void requestsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modalFrameAddRepairRequest.IsVisible = false;
            modalFrameAfterScanning.IsVisible = false;
            selectedRequest = e.CurrentSelection[0] as RepairRequest;
            if (selectedRequest != null)
            {
                modalFrameRequest.IsVisible= true;
                modalDescription.Text = selectedRequest.Description;  
                modalDate.Text = selectedRequest.CreatedDate.ToString();
                modalLabelIdRequest.Text = selectedRequest.Id.ToString();
                pickerStatus.SelectedIndex = selectedRequest.Status;
                if (pickerStatus.SelectedIndex == 2 || pickerStatus.SelectedIndex == 3)
                {
                    pickerStatus.IsEnabled= false;
                }
                else
                    pickerStatus.IsEnabled = true;

                User user = _httpClient.GetUserById(selectedRequest.UserFromId);
                modalUserFrom.Text = user.FullName;
                //requestsList.SelectedItem = SelectableItemsView.EmptyViewProperty;

                //
                if (pickerStatus.SelectedIndex == 2 || pickerStatus.SelectedIndex == 3)
                {
                    if (string.IsNullOrEmpty(selectedRequest.RepairNote))
                        selectedRequest.RepairNote = "";
                    editorRepairNote.Text = selectedRequest.RepairNote;
                    frameRepairNote.IsVisible = true;
                }
                else
                {
                    frameRepairNote.IsVisible = false;
                }
                //

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
            else
            {
                if (filteredRequests.Count == 0)
                {
                    frameNotFound.IsVisible = true;
                }
                else
                {
                    frameNotFound.IsVisible = false;
                }
            }

            if (filteredRequests.Count != 0)
            {
                var filtererBySearchAndStatus = filteredRequests.Where(o => o.Status == picker_status.SelectedIndex);
                requestsList.ItemsSource = filtererBySearchAndStatus;
            }
            else
            {
                var filtererBySearchAndStatus = filteredRequests.Where(o => o.Status == picker_status.SelectedIndex);
                requestsList.ItemsSource = filtererBySearchAndStatus;
            }
        }

        public void SortEquipment()
        {
            var filteredTechEquipment = techEquipmentList;
            if (!string.IsNullOrEmpty(entrySearchTechEquipment.Text))
            {
                filteredTechEquipment = techEquipmentList.Where(o => o.Id.Contains(entrySearchTechEquipment.Text)).ToList();
                if (filteredTechEquipment.Count == 0)
                {
                    //
                }
                else
                {
                    //
                }
            }

            techEquipmentCollection.ItemsSource = filteredTechEquipment;
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

        private async void pickerStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedRequest != null)
            {
                Xamarin.Forms.Picker picker = sender as Xamarin.Forms.Picker;
                selectedRequest.Status = picker.SelectedIndex;
                try
                {
                    _httpClient.PutRepairRequest(selectedRequest);
                }
                catch (Exception ex) 
                {
                    await DisplayAlert("Что-то пошло не так","Мы уже работаем над исправлением ситуации, попробуйте позже","Ок");
                }
                fUser.RepairRequestsReceived.Remove(selectedRequest);
                fUser.RepairRequestsReceived.Add(selectedRequest);
                requestsList.ItemsSource = null;
                requestsList.ItemsSource = fUser.RepairRequestsReceived;
                Sort();
                DependencyService.Get<IToast>().LongToast("Статус: " + picker.SelectedItem);                
            }
        }

        private void btnGoToAddRepairRequest_Clicked(object sender, EventArgs e)
        {
            modalAddRepairRequestTechID.Text = equipment.Id.ToString();
            modalFrameAfterScanning.IsVisible= false;
            modalFrameRequest.IsVisible= false;
            modalFrameAddRepairRequest.IsVisible = true;
        }

        private void ImageButton_Clicked_2(object sender, EventArgs e)
        {
            modalFrameAddRepairRequest.IsVisible= false;
        }

        private void BtnAddRepairRequest_Clicked(object sender, EventArgs e)
        {
            try
            {
                RepairRequest repairRequest = new RepairRequest();
                repairRequest.CreatedDate = DateTime.Now;
                repairRequest.UserFromId = Convert.ToInt32(App.Current.Properties["UserId"]);
                repairRequest.Description = editorDescriptionRequest.Text;
                repairRequest.Status = 0;
                repairRequest.TechEquipmentId = equipment.Id;
                _httpClient.PostRepairRequest(repairRequest);
                DependencyService.Get<IToast>().LongToast("Заявка успешно подана!");
            }
            catch(Exception ex)
            {

            }
            modalFrameAddRepairRequest.IsVisible = false;
        }

        private void entrySearchTechEquipment_TextChanged(object sender, TextChangedEventArgs e)
        {
            SortEquipment();
        }

        

        private void techEquipmentCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedTechEquipment = e.CurrentSelection[0] as TechEquipment;
            modalFrameDetailsTechEquipment.IsVisible = true;
            IdDetailsTechEquipment.Text = selectedTechEquipment.Id;
            IpDetailsTechEquipment.Text = selectedTechEquipment.IpAddress;
            TypeDetailsTechEquipment.Text = selectedTechEquipment.Type.ToString();
        }

        private async void BtnSadeNewIp_Clicked(object sender, EventArgs e)
        {
            string newIp = await DisplayPromptAsync(selectedTechEquipment.Id, "Введите новый IP:");
            if (newIp == null)
                newIp = "";

            bool isIPAddres = false;
            Match match = Regex.Match(newIp, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (match.Success)
            {
                isIPAddres = true;
            }

            if (isIPAddres)
            {
                techEquipmentList.Remove(selectedTechEquipment);
                selectedTechEquipment.IpAddress = newIp;
                _httpClient.PutTechEquipment(selectedTechEquipment);
                techEquipmentList.Add(selectedTechEquipment);
                techEquipmentCollection.ItemsSource = null;
                techEquipmentCollection.ItemsSource = techEquipmentList;
                modalFrameDetailsTechEquipment.IsVisible=false;
                SortEquipment();
                DependencyService.Get<IToast>().LongToast("Новый IP: " + selectedTechEquipment.IpAddress);
            }
            else
            {
                DependencyService.Get<IToast>().ShortToast("Не валидный IP, не обновлено");
            }

        }

        private void ImageButton_Clicked_3(object sender, EventArgs e)
        {
            modalFrameDetailsTechEquipment.IsVisible = false;
        }

        private async void BtnDeleteTechEquipment_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Вы уверены?", "Удалить " + selectedTechEquipment.Id + " ?\nУдаление оборудования " +
                "приведет к удалению связанных с ним заявок.", "Да" , "Нет");
            if (answer)
            {
                _httpClient.DeleteTechEquipment(selectedTechEquipment);
                techEquipmentList.Remove(selectedTechEquipment);
                techEquipmentCollection.ItemsSource = null;
                techEquipmentCollection.ItemsSource = techEquipmentList;

                fUser.RepairRequestsReceived.Remove(selectedRequest);
                requestsList.ItemsSource= null;
                requestsList.ItemsSource= fUser.RepairRequestsReceived;

                modalFrameDetailsTechEquipment.IsVisible = false;
                modalFrameRequest.IsVisible= false;
                SortEquipment(); //requestsList.ItemsSource = fUser.RepairRequestsReceived;
                Sort();

                DependencyService.Get<IToast>().LongToast("Удалено");
            }           
        }

        private void editorRepairNote_Unfocused(object sender, FocusEventArgs e)
        {
            fUser.RepairRequestsReceived.Remove(selectedRequest);
            if (string.IsNullOrEmpty(editorRepairNote.Text))
                editorRepairNote.Text = "";
            try
            {
                selectedRequest.RepairNote = editorRepairNote.Text;
                _httpClient.PutRepairRequest(selectedRequest);
                DependencyService.Get<IToast>().ShortToast("Примечание сохранено");
            }
            catch(Exception ex)
            {

            }
            fUser.RepairRequestsReceived.Add(selectedRequest);
            requestsList.ItemsSource= null;
            requestsList.ItemsSource= fUser.RepairRequestsReceived;
            Sort();
        }

        private void btnGoToTechEquipment_Clicked(object sender, EventArgs e)
        {
            tabbedPage.CurrentPage = tabbedPage.Children[1];
            entrySearchTechEquipment.Text = selectedRequest.TechEquipmentId;
            TechEquipment techEquipment = new TechEquipment();
            techEquipment = techEquipmentList.Where(o => o.Id == selectedRequest.TechEquipmentId).FirstOrDefault();
            techEquipmentCollection.SelectedItem = techEquipment;
        }

        private void btnGoToTechEquipment_Clicked_1(object sender, EventArgs e)
        {
            tabbedPage.CurrentPage = tabbedPage.Children[1];
            entrySearchTechEquipment.Text = modalAfterScanningLabelIdRequest.Text;
            TechEquipment techEquipment = new TechEquipment();
            techEquipment = techEquipmentList.Where(o => o.Id == modalAfterScanningLabelIdRequest.Text).FirstOrDefault();
            techEquipmentCollection.SelectedItem = techEquipment;
        }

        private void btnAddTechEquipment_Clicked(object sender, EventArgs e)
        {
            TechEquipment techEquipment = new TechEquipment();
            techEquipment.Id = modalFrameAddTechEquipmentLabelId.Text;
            if (entryIpAddressAddTechEquipment.Text == null)
                entryIpAddressAddTechEquipment.Text = "";
            techEquipment.IpAddress = entryIpAddressAddTechEquipment.Text;
            techEquipment.Type = (TechType)pickerTypeTechEquipmentForAdd.SelectedIndex;

            bool isIPAddres = false;
            Match match = Regex.Match(techEquipment.IpAddress, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (match.Success)
            {
                isIPAddres = true;
            }

            if (!isIPAddres)
            {
                DependencyService.Get<IToast>().LongToast("Не валидный IP, операция прервана");
                return;
            }

            _httpClient.PostTechEquipment(techEquipment);
            modalFrameAddTechEquipment.IsVisible= false;

            techEquipmentList.Add(techEquipment);
            techEquipmentCollection.ItemsSource = null;
            techEquipmentCollection.ItemsSource = techEquipmentList;
            DependencyService.Get<IToast>().LongToast("Успешно добавлено!");
        }

        private void pickerTypeTechEquipmentForAdd_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ImageButton_Clicked_4(object sender, EventArgs e)
        {
            modalFrameAddTechEquipment.IsVisible = false;
        }
    }
}