using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetEye.pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class addRequestPage : ContentPage
    {
        public addRequestPage(string resultScan)
        {
            InitializeComponent();
            techEqId.Text = resultScan;

            #region navBar
            NavigationPage.SetHasBackButton(this, false);


            var titleView = new StackLayout();
            var titleUndView = new StackLayout();
            titleView.Orientation = StackOrientation.Horizontal;
            titleView.Margin = new Thickness(0, 0, 15, 0);
            titleView.HorizontalOptions = LayoutOptions.FillAndExpand;            

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

            titleUndView.Children.Add(titleLabel);
            titleUndView.Children.Add(titleUnderLabel);
            titleView.Children.Add(titleUndView);
            titleView.Children.Add(iconImage);


            NavigationPage.SetTitleView(this, titleView);
            #endregion
        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Отмена","Отменить подачу заявки?","Да","Нет");
            if (answer)
                await Navigation.PopAsync(); 
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void btnSend_Clicked(object sender, EventArgs e)
        {

        }
    }
}