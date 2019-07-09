using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DatabaseXamarinSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void BtnSave_Clicked(object sender, EventArgs e)
        {
            IList<PublisherEntity> publisherEntities = Mocker.GetPublishers();

            TransactionContext transactionContext = await App.Database.BeginTransactionAsync<PublisherEntity>();

            try
            {
                DatabaseResult databaseResult = await App.Database.BatchAddAsync<PublisherEntity>(publisherEntities, "", transactionContext);

                if (!databaseResult.IsSucceeded())
                {
                    throw new Exception(databaseResult.Exception?.Message, databaseResult.Exception);
                }

                await App.Database.CommitAsync(transactionContext);

                await DisplayList();
            }

            catch(Exception ex)
            {
                await App.Database.RollbackAsync(transactionContext);
                await DisplayAlert("Error", ex.Message, "Close");
            }

        }

        private async Task DisplayList()
        {
            var dataSource = await App.Database.RetrieveAllAsync<PublisherEntity>(null);

            list.ItemsSource = dataSource;
        }
    }
}
