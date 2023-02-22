using System;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.CommonTests.PropertyTrackable
{
    [PropertyTrackableObject]
    public partial class TestObject
    {
        [TrackProperty]
        private string? _name;

        //[TrackProperty]
        //private ObservableCollection2<string>? _testCollection;

        //[TrackProperty]
        //public InnerTestObject? _innerCls;

        [TrackProperty]
        public InnerTestRecord? _innerRecord;

        [TrackProperty]
        [AddtionalProperty]
        private string _forwordAttributeName = "This is a Addtional";

        [TrackProperty]
        private ImmutableList<string>? _immutables;

        [TrackProperty]
        private ImmutableArray<string>? _immutables2;


        //TODO: solve this
        [TrackProperty]
        private InnerModel? _innerModel;


        //TODO: solve this
        [TrackProperty]
        private ObservableCollection<string>? _collection;



    }



    public partial class InnerModel : ObservableObject
    {
        [ObservableProperty]
        private string? _innerName;
    }
}