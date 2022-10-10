using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.MySQL
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
        public void Test_Batch_UpdateProperties_Timestamp()
        {

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
        public void Test_Batch_UpdateProperties_Compare()
        {

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

            ChangedPack cp = model.GetChangedPack(model.Id);

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

            ChangedPack cp = model.GetChangedPack(model.Id);

            await Db.UpdatePropertiesAsync<UPTimelessModel>(cp, "", null);

            var rt = await Db.ScalarAsync<UPTimelessModel>(model.Id, null);

            Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public void Test_Batch_UpdateProperties_Cps()
        {

        }
    }
}
