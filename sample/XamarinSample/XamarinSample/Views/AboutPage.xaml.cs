using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinSample.Models;

namespace XamarinSample.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        async void BtnSave_Clicked(object sender, EventArgs e)
        {
            IList<PublisherEntity> publisherEntities = Mocker.GetPublishers();

            TransactionContext transactionContext = await App.Transaction.BeginTransactionAsync<PublisherEntity>(System.Data.IsolationLevel.ReadCommitted);

            try
            {
                await App.Database.BatchAddAsync<PublisherEntity>(publisherEntities, "lastUser", transactionContext);

                await App.Transaction.CommitAsync(transactionContext);

                await DisplayList();
            }

            catch (Exception ex)
            {
                await App.Transaction.RollbackAsync(transactionContext);
                await DisplayAlert("Error", ex.Message, "Close");
            }

        }

        private async Task DisplayList()
        {
            var dataSource = await App.Database.RetrieveAllAsync<PublisherEntity>(transContext: null);

            list.ItemsSource = dataSource;
        }
    }
}