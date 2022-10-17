using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.SQLite
{

    public partial class InnerModel : ObservableObject
    {
        [ObservableProperty]
        private string? _innerName;
    }

    [PropertyTrackableObject]
    public partial class UPTimestampModel : TimestampGuidDbModel
    {
        [TrackProperty]
        private string? _name;

        [TrackProperty]
        private int? _age;

        [TrackProperty]
        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel? _innerModel;

    }

    [PropertyTrackableObject]
    public partial class UPTimelessModel : TimelessGuidDbModel
    {
        [TrackProperty]
        private string? _name;

        [TrackProperty]
        private int? _age;

        [TrackProperty]
        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel? _innerModel;
    }

    [TestClass]
    public class UpdateProperties_Sqlite : BaseTestClass
    {
        private class Mocker
        {
            public static UPTimelessModel MockTimelessModel()
            {
                return new UPTimelessModel
                {
                    Name = SecurityUtil.CreateRandomString(5),
                    Age = SecurityUtil.GetRandomInteger(0, 100),
                    InnerModel = new InnerModel()
                    {
                        InnerName = SecurityUtil.CreateRandomString(10)
                    }
                };
            }

            public static UPTimestampModel MockTimestampModel()
            {
                return new UPTimestampModel
                {
                    Name = SecurityUtil.CreateRandomString(5),
                    Age = SecurityUtil.GetRandomInteger(0, 100),
                    InnerModel = new InnerModel()
                    {
                        InnerName = SecurityUtil.CreateRandomString(10)
                    }
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

            await Db.UpdatePropertiesAsync<UPTimestampModel>(
                model.Id,
                new List<(string propertyName, object? propertyValue)> {
                    (nameof(model.Name), model.Name),
                    (nameof(model.Age), model.Age),
                    (nameof(model.InnerModel), model.InnerModel)
                },
                model.Timestamp,
                "",
                null,
                newTimestamp);

            model.Timestamp = newTimestamp;

            var rt = await Db.ScalarAsync<UPTimestampModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));

        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Timestamp()
        {
            var models = Mocker.MockTimestampList(3);
            await Db.BatchAddAsync(models, "", null);

            var changes = new List<(object id, IList<(string, object?)> properties, long oldTimestamp, long? newTimestamp)>();

            foreach (var model in models)
            {
                model.Name = "ChangedName";
                model.Age = 10000;
                model.InnerModel = null;

                long newTimestamp = TimeUtil.Timestamp;

                changes.Add((
                    model.Id,
                    new List<(string, object?)>
                    {
                        (nameof(model.Name),model.Name),
                        (nameof(model.Age), model.Age),
                        (nameof(model.InnerModel) , model.InnerModel)
                    },
                    model.Timestamp,
                    newTimestamp));

                model.Timestamp = newTimestamp;
            }

            await Db.BatchUpdatePropertiesAsync<UPTimestampModel>(changes, "", null);

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

            await Db.UpdatePropertiesAsync<UPTimestampModel>(
                model.Id,
                new List<(string propertyName, object? oldValue, object? newValue)> {
                    (nameof(model.Name), model.Name, newName),
                    (nameof(model.Age), model.Age, newAge),
                    (nameof(model.InnerModel), model.InnerModel, newInnerModel)
                },
                "",
                null,
                newTimestamp);

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

            await Db.UpdatePropertiesAsync<UPTimelessModel>(
                model.Id,
                new List<(string propertyName, object? oldValue, object? newValue)> {
                    (nameof(model.Name), model.Name, newName),
                    (nameof(model.Age), model.Age, newAge),
                    (nameof(model.InnerModel), model.InnerModel, newInnerModel)
                },
                "",
                null,
                newTimestamp);

            model.Name = newName;
            model.Age = newAge;
            model.InnerModel = newInnerModel;

            var rt = await Db.ScalarAsync<UPTimelessModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Compare()
        {
            var trans = await Trans.BeginTransactionAsync<DeleteTimestampModel>();

            try
            {
                var models = Mocker.MockTimestampList(3);
                await Db.BatchAddAsync(models, "", trans);

                var changes = new List<(object id, IList<(string, object?, object?)> properties, long? newTimestamp)>();

                foreach (var model in models)
                {
                    string? newName = "ChangedName";
                    int newAge = 10000;
                    InnerModel? newInnerModel = null;
                    long newTimestamp = TimeUtil.Timestamp;

                    changes.Add((
                        model.Id,
                        new List<(string, object?, object?)>
                        {
                            (nameof(model.Name), model.Name, newName),
                            (nameof(model.Age), model.Age, newAge),
                            (nameof(model.InnerModel) , model.InnerModel, newInnerModel)
                        },
                        newTimestamp));

                    model.Name = newName;
                    model.Age = newAge;
                    model.InnerModel = newInnerModel;
                    model.Timestamp = newTimestamp;
                }

                await Db.BatchUpdatePropertiesAsync<UPTimestampModel>(changes, "", trans);

                var rts = await Db.RetrieveAsync<UPTimestampModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), trans);

                await trans.CommitAsync();

                Assert.AreEqual(SerializeUtil.ToJson(rts), SerializeUtil.ToJson(models));
            }
            catch
            {
                await trans.RollbackAsync();
            }
        }

        [TestMethod]
        public async Task Test_UpdateProperties_Cps_Timeless()
        {
            var model = Mocker.MockTimelessModel();
            await Db.AddAsync(model, "", null);

            model.StartTrack();

            model.Name = "ChangedName";
            model.Age = 999;
            model.InnerModel.InnerName = "ChangedName_InnerName";

            ChangedPack2 cp = model.GetChangedPack(model.Id);

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
            model.InnerModel.InnerName = "ChangedName_InnerName";
            model.Timestamp = TimeUtil.Timestamp;

            ChangedPack2 cp = model.GetChangedPack(model.Id);

            await Db.UpdatePropertiesAsync<UPTimestampModel>(cp, "", null);

            var rt = await Db.ScalarAsync<UPTimestampModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Cps_Timeless()
        {
            var trans = await Trans.BeginTransactionAsync<DeleteTimestampModel>();

            try
            {
                var models = Mocker.MockTimelessList(3);
                await Db.BatchAddAsync(models, "", trans);

                var cps = new List<ChangedPack2>();

                foreach (var model in models)
                {
                    model.StartTrack();

                    model.Name = "ChangedName";
                    model.Age = 999;
                    model.InnerModel.InnerName = "ChangedName_InnerName";

                    cps.Add(model.GetChangedPack(model.Id));
                }

                await Db.BatchUpdatePropertiesAsync<UPTimelessModel>(cps, "", trans);

                var rts = await Db.RetrieveAsync<UPTimelessModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), trans);

                await trans.CommitAsync();

                Assert.AreEqual(SerializeUtil.ToJson(models), SerializeUtil.ToJson(rts));
            }
            catch
            {
                await trans.RollbackAsync();
            }
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_Cps_TimestampAsync()
        {
            var trans = await Trans.BeginTransactionAsync<DeleteTimestampModel>();
            try
            {
                var models = Mocker.MockTimestampList(3);
                await Db.BatchAddAsync(models, "", trans);

                var cps = new List<ChangedPack2>();

                foreach (var model in models)
                {
                    model.StartTrack();

                    model.Name = "ChangedName";
                    model.Age = 999;
                    model.InnerModel.InnerName = "ChangedName_InnerName";
                    model.Timestamp = TimeUtil.Timestamp;

                    cps.Add(model.GetChangedPack(model.Id));
                }

                await Db.BatchUpdatePropertiesAsync<UPTimestampModel>(cps, "", trans);

                var rts = await Db.RetrieveAsync<UPTimestampModel>(m => SqlStatement.In(m.Id, true, models.Select(i => i.Id).ToList()), trans);

                await trans.CommitAsync();
                Assert.AreEqual(SerializeUtil.ToJson(models), SerializeUtil.ToJson(rts));
            }
            catch
            {
                await trans.RollbackAsync();
            }
        }
    }
}
