using System.Collections.ObjectModel;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.CommonTests.PropertyTrackable
{
    [PropertyTrackableObject]
    public partial class TestObject
    {
        [TrackProperty]
        private string? _name;

        [TrackProperty]
        private ObservableCollection2<string>? _testCollection;

        [TrackProperty]
        public InnerTestObject? _innerCls;

        [TrackProperty]
        [AddtionalProperty]
        private string _forwordAttributeName = "This is a Addtional";
    }
}