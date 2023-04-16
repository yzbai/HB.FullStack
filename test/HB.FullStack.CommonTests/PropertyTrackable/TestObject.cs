using System;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using HandlebarsDotNet.Collections;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.CommonTests.PropertyTrackable
{

    [PropertyTrackableObject]
    public partial class TestTestObject
    {

    }

    [PropertyTrackableObject]
    public partial class TestObject
    {
        [AddtionalProperty]
        public string Id { get; set; } = "This is a Id";

        //String
        [TrackProperty]
        private string? _name;

        //Value Type
        [TrackProperty]
        private int _age;

        //Record
        [TrackProperty]
        public TestRecord? _testRecord;


        //ImmutableCollection
        [TrackProperty]
        private ImmutableList<string>? _immutableList;

        //ImmutableCollection
        [TrackProperty]
        private ImmutableArray<string>? _immutableArray;


        //Observable class
        [TrackProperty]
        private ObservableInner? _observableInner;

        //Observable Collection
        [TrackProperty]
        private ObservableCollection2<string>? _observableCollection2;
    }

    public record TestRecord(string? InnerName);

    public partial class ObservableInner : ObservableObject
    {
        [ObservableProperty]
        private string? _innerName;
    }
}