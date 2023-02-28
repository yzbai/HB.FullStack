using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.SQL;
using HB.FullStack.BaseTest.Data.MySqls;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.Database;

namespace HB.FullStack.DatabaseTests.MySQL
{

    [TestClass]
    public class UpdateProperties_MySQL : BaseTestClass
    {
        private class Mocker
        {
            public static UPTimelessModel MockTimelessModel()
            {
                return new UPTimelessModel
                {
                    Name = SecurityUtil.CreateRandomString(5),
                    Age = SecurityUtil.GetRandomInteger(0, 100),
                    InnerModel = new InnerModel(SecurityUtil.CreateRandomString(10))

                };
            }

            public static UPTimestampModel MockTimestampModel()
            {
                return new UPTimestampModel
                {
                    Name = SecurityUtil.CreateRandomString(5),
                    Age = SecurityUtil.GetRandomInteger(0, 100),
                    InnerModel = new InnerModel(SecurityUtil.CreateRandomString(10))

                };
            }

            public static IEnumerable<UPTimelessModel> MockTimelessList(int count = 10)
            {
                var lst = new List<UPTimelessModel>();

                for (int i = 0; i < count; i++)
                {
                    lst.Add(MockTimelessModel());
                }

                return lst;
            }
            public static IEnumerable<UPTimestampModel> MockTimestampList(int count = 10)
            {
                var lst = new List<UPTimestampModel>();

                for (int i = 0; i < count; i++)
                {
                    lst.Add(MockTimestampModel());
                }

                return lst;
            }
        }

        [TestMethod]
        public async Task Test_UpdateProperties_Timestamp()
        {
            var model = Mocker.MockTimestampModel();
            await Db.AddAsync(model, "", null);

            model.Name = "ChangedName";
            model.Age = 10000;
            model.InnerModel = null;

            long newTimestamp = TimeUtil.Timestamp;

            UpdateUsingTimestamp updatePack = new UpdateUsingTimestamp
            {
                Id = model.Id,
                OldTimestamp = model.Timestamp,
                NewTimestamp = newTimestamp,
                PropertyNames = new string[] { nameof(model.Name), nameof(model.Age), nameof(model.InnerModel) },
                NewPropertyValues = new object?[] { model.Name, model.Age, model.InnerModel }
            };

            await Db.UpdatePropertiesAsync<UPTimestampModel>(updatePack, "", null);

            model.Timestamp = newTimestamp;

            var rt = await Db.ScalarAsync<UPTimestampModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));

        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Timestamp()
        {
            var models = Mocker.MockTimestampList(3);
            await Db.BatchAddAsync(models, "", null);

            var updatePacks = new List<UpdateUsingTimestamp>();

            foreach (var model in models)
            {
                model.Name = "ChangedName";
                model.Age = 10000;
                model.InnerModel = null;

                long newTimestamp = TimeUtil.Timestamp;

                updatePacks.Add(new UpdateUsingTimestamp
                {
                    Id = model.Id,
                    OldTimestamp = model.Timestamp,
                    NewTimestamp = newTimestamp,
                    PropertyNames = new string[] { nameof(model.Name), nameof(model.Age), nameof(model.InnerModel) },
                    NewPropertyValues = new object?[] { model.Name, model.Age, model.InnerModel }
                });

                model.Timestamp = newTimestamp;
            }

            await Db.BatchUpdatePropertiesAsync<UPTimestampModel>(updatePacks, "", null);

            var rts = await Db.RetrieveAsync<UPTimestampModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), null);

            Assert.AreEqual(SerializeUtil.ToJson(rts), SerializeUtil.ToJson(models));
        }

        [TestMethod]
        public async Task Test_UpdateProperties_Compare_Timestamp()
        {
            var model = Mocker.MockTimestampModel();
            await Db.AddAsync(model, "", null);

            string? newName = "ChangedName";
            int newAge = 10000;
            InnerModel? newInnerModel = null;

            long newTimestamp = TimeUtil.Timestamp;

            UpdateUsingCompare updatePack = new UpdateUsingCompare
            {
                Id = model.Id,
                NewTimestamp = newTimestamp,
                PropertyNames = new string[] { nameof(model.Name), nameof(model.Age), nameof(model.InnerModel) },
                OldPropertyValues = new object?[] { model.Name, model.Age, model.InnerModel },
                NewPropertyValues = new object?[] { newName, newAge, newInnerModel },
            };

            await Db.UpdatePropertiesAsync<UPTimestampModel>(updatePack, "", null);

            model.Name = newName;
            model.Age = newAge;
            model.InnerModel = newInnerModel;
            model.Timestamp = newTimestamp;

            var rt = await Db.ScalarAsync<UPTimestampModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_UpdateProperties_Compare_Timeless()
        {
            var model = Mocker.MockTimelessModel();
            await Db.AddAsync(model, "", null);

            string? newName = "ChangedName";
            int newAge = 10000;
            InnerModel? newInnerModel = null;

            long newTimestamp = TimeUtil.Timestamp;

            UpdateUsingCompare updatePack = new UpdateUsingCompare
            {
                Id = model.Id,
                NewTimestamp = newTimestamp,
                PropertyNames = new string[] { nameof(model.Name), nameof(model.Age), nameof(model.InnerModel) },
                OldPropertyValues = new object?[] { model.Name, model.Age, model.InnerModel },
                NewPropertyValues = new object?[] { newName, newAge, newInnerModel }
            };

            await Db.UpdatePropertiesAsync<UPTimelessModel>(updatePack, "", null);

            model.Name = newName;
            model.Age = newAge;
            model.InnerModel = newInnerModel;

            var rt = await Db.ScalarAsync<UPTimelessModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Compare()
        {
            var models = Mocker.MockTimestampList(3);
            await Db.BatchAddAsync(models, "", null);

            var updatePacks = new List<UpdateUsingCompare>();

            foreach (var model in models)
            {
                string? newName = "ChangedName";
                int newAge = 10000;
                InnerModel? newInnerModel = null;
                long newTimestamp = TimeUtil.Timestamp;

                var updatePack = new UpdateUsingCompare
                {
                    Id = model.Id,
                    NewTimestamp = newTimestamp,
                    PropertyNames = new string[] { nameof(model.Name), nameof(model.Age), nameof(model.InnerModel) },
                    OldPropertyValues = new object?[] { model.Name, model.Age, model.InnerModel},
                    NewPropertyValues = new object?[] { newName, newAge, newInnerModel},
                };

                updatePacks.Add(updatePack);

                model.Name = newName;
                model.Age = newAge;
                model.InnerModel = newInnerModel;
                model.Timestamp = newTimestamp;
            }

            await Db.BatchUpdatePropertiesAsync<UPTimestampModel>(updatePacks, "", null);

            var rts = await Db.RetrieveAsync<UPTimestampModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), null);

            Assert.AreEqual(SerializeUtil.ToJson(rts), SerializeUtil.ToJson(models));
        }

        [TestMethod]
        public async Task Test_UpdateProperties_Cps_Timeless()
        {
            var model = Mocker.MockTimelessModel();
            await Db.AddAsync(model, "", null);

            model.StartTrack();

            model.Name = "ChangedName";
            model.Age = 999;
            model.InnerModel = model.InnerModel == null ? new InnerModel("ChangedName_InnerName") : model.InnerModel with { InnerName = "ChangedName_InnerName" };
            //model.InnerModel = new InnerModel { InnerName = "ChangedName_InnerName" };

            PropertyChangePack cp = model.GetChangePack();

            await Db.UpdatePropertiesAsync<UPTimelessModel>(cp, "", null);

            var rt = await Db.ScalarAsync<UPTimelessModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_UpdateProperties_Cps_Timestamp()
        {
            var model = Mocker.MockTimestampModel();
            await Db.AddAsync(model, "", null);

            model.StartTrack();

            model.Name = "ChangedName";
            model.Age = 999;
            model.InnerModel = model.InnerModel == null ? new InnerModel("ChangedName_InnerName") : model.InnerModel with { InnerName = "ChangedName_InnerName" };
            model.Timestamp = TimeUtil.Timestamp;

            PropertyChangePack cp = model.GetChangePack();

            await Db.UpdatePropertiesAsync<UPTimestampModel>(cp, "", null);

            var rt = await Db.ScalarAsync<UPTimestampModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Cps_Timeless()
        {
            var models = Mocker.MockTimelessList(3);
            await Db.BatchAddAsync(models, "", null);

            var cps = new List<PropertyChangePack>();

            foreach (var model in models)
            {
                model.StartTrack();

                model.Name = "ChangedName";
                model.Age = 999;
                model.InnerModel = model.InnerModel == null ? new InnerModel("ChangedName_InnerName") : model.InnerModel with { InnerName = "ChangedName_InnerName" };

                cps.Add(model.GetChangePack());
            }

            await Db.BatchUpdatePropertiesAsync<UPTimelessModel>(cps, "", null);

            var rts = await Db.RetrieveAsync<UPTimelessModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), null);

            Assert.AreEqual(SerializeUtil.ToJson(models), SerializeUtil.ToJson(rts));
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Cps_TimestampAsync()
        {
            var models = Mocker.MockTimestampList(3);
            await Db.BatchAddAsync(models, "", null);

            var cps = new List<PropertyChangePack>();

            foreach (var model in models)
            {
                model.StartTrack();

                model.Name = "ChangedName";
                model.Age = 999;
                model.InnerModel = model.InnerModel == null ? new InnerModel("ChangedName_InnerName") : model.InnerModel with { InnerName = "ChangedName_InnerName" };
                model.Timestamp = TimeUtil.Timestamp;

                cps.Add(model.GetChangePack());
            }

            await Db.BatchUpdatePropertiesAsync<UPTimestampModel>(cps, "", null);

            var rts = await Db.RetrieveAsync<UPTimestampModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), null);

            Assert.AreEqual(SerializeUtil.ToJson(models), SerializeUtil.ToJson(rts));
        }
    }
}
