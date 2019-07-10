using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.Framework.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DatabaseAspnetcoreSample.Pages
{
    public class IndexModel : PageModel
    {

        public IList<PublisherEntity> Publishers { get; set; } = new List<PublisherEntity>();

        private IDatabase database;

        public IndexModel(IDatabase database)
        {
            this.database = database;
        }

        public async void OnGetAsync()
        {
            Publishers = await database.RetrieveAllAsync<PublisherEntity>(transContext:null);
        }

        public async Task<IActionResult> OnPostAsync()
        {

            TransactionContext tContext = await database.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IList<PublisherEntity> lst = Mocker.GetPublishers();

                DatabaseResult databaseResult = await database.BatchAddAsync(lst, "", tContext);

                //new id can be found in databaseResult.Ids

                if (!databaseResult.IsSucceeded())
                {
                    await database.RollbackAsync(tContext);
                    throw new Exception("did not work.");
                }

                await database.CommitAsync(tContext);
            }
            catch(Exception ex)
            {
                await database.RollbackAsync(tContext);
                throw ex;
            }

            return RedirectToPage("/Index");
        }
    }
}
