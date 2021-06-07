using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Input;
using HB.FullStack.XamarinForms.Base;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Controls
{
    public class ChipGroupItem
    {
        public string Text { get; set; } = null!;

        public object? Tag { get; set; }

        public Color SelectedBackgroundColor { get; set; } = Color.Gray;

        public Color UnSelectedBackgroundColor { get; set; } = Color.White;

        public ICommand? SelectCommand { get; set; }

        public object? SelectCommandParameter { get; set; }

        //TODO:加上图片
    }

    public class ChipGroup : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList<ChipGroupItem>),
            typeof(ChipGroup),
            null,
            BindingMode.OneWay,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((ChipGroup)bindable).OnItemsSourceChanged((IList<ChipGroupItem>)oldValue, (IList<ChipGroupItem>)newValue);
            });

        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
            nameof(SelectedBackgroundColor),
            typeof(Color),
            typeof(ChipGroup),
            Color.LightGray);

        public static readonly BindableProperty UnSelectedBackgroundColorProperty = BindableProperty.Create(
            nameof(UnSelectedBackgroundColor),
            typeof(Color),
            typeof(ChipGroup),
            Color.White);

        public static readonly BindableProperty ItemMarginProperty = BindableProperty.Create(
            nameof(ItemMargin),
            typeof(Thickness),
            typeof(ChipGroup),
            new Thickness(10));

        /// <summary>
        /// 这里只能保证当ItemsSource整个re new的时候，相应变化，而不是集合本身的变化。通过ItemsSourceProperty的OnItemsSourceChanged来注册集合变化
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ChipGroupItem> ItemsSource
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return (IList<ChipGroupItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public Color SelectedBackgroundColor
        {
            get { return (Color)GetValue(SelectedBackgroundColorProperty); }
            set { SetValue(SelectedBackgroundColorProperty, value); }
        }

        public Color UnSelectedBackgroundColor
        {
            get { return (Color)GetValue(UnSelectedBackgroundColorProperty); }
            set { SetValue(UnSelectedBackgroundColorProperty, value); }
        }

        public Thickness ItemMargin
        {
            get { return (Thickness)GetValue(ItemMarginProperty); }
            set { SetValue(ItemMarginProperty, value); }
        }

        private string? _selectedText;

        public string? SelectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
                OnPropertyChanged(nameof(SelectedText));
            }
        }

        private object? _selectedTag;

        public object? SelectedTag
        {
            get { return _selectedTag; }
            set { _selectedTag = value; OnPropertyChanged(); }
        }

        public string GroupName { get; set; } = SecurityUtil.CreateUniqueToken();

        //private WeakEventManager _eventManager = new WeakEventManager();

        //public event EventHandler OnClicked
        //{
        //    add => _eventManager.AddEventHandler(value);
        //    remove => _eventManager.RemoveEventHandler(value);
        //}

        //public event EventHandler OnClose
        //{
        //    add => _eventManager.AddEventHandler(value);
        //    remove => _eventManager.RemoveEventHandler(value);
        //}

        //public event EventHandler OnSelect
        //{
        //    add => _eventManager.AddEventHandler(value);
        //    remove => _eventManager.RemoveEventHandler(value);
        //}

        //public event EventHandler OnUnselect
        //{
        //    add => _eventManager.AddEventHandler(value);
        //    remove => _eventManager.RemoveEventHandler(value);
        //}

        private readonly FlexLayout _rootLayout;

        public ChipGroup()
        {
            Content = _rootLayout = new FlexLayout { Direction = FlexDirection.Row, Wrap = FlexWrap.Wrap };
        }

        private void OnItemsSourceChanged(IList<ChipGroupItem>? oldValue, IList<ChipGroupItem>? newValue)
        {
            //应对ItemsSource重新被赋值，renew
            if (oldValue is INotifyCollectionChanged oldObservable)
            {
                oldObservable.CollectionChanged -= CollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newObservable)
            {
                newObservable.CollectionChanged += CollectionChanged;
            }

            if (newValue == null)
            {
                //清空Chips
                ClearItems();
            }
            else
            {
                ResetItems();
            }
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InsertItems(e);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e);
                    break;
                default:
                    ResetItems();
                    break;
            }
        }

        private void RemoveItems(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove)
            {
                return;
            }

            int index = e.OldStartingIndex;

            if (e.OldItems != null)
            {
                foreach (ChipGroupItem _ in e.OldItems)
                {
                    _rootLayout.Children.RemoveAt(index++);
                }
            }
        }

        private void ResetItems()
        {
            if (ItemsSource == null)
            {
                return;
            }

            foreach (ChipGroupItem obj in ItemsSource)
            {
                _rootLayout.Children.Add(CreateChip(obj));
            }
        }

        private void ClearItems()
        {
            _rootLayout.Children.Clear();
        }

        private void InsertItems(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            int index = e.NewStartingIndex;

            if (e.NewItems != null)
            {
                foreach (ChipGroupItem newItem in e.NewItems)
                {
                    _rootLayout.Children.Insert(index++, CreateChip(newItem));
                }
            }
        }

        private Chip CreateChip(ChipGroupItem obj)
        {
            Chip chip = new Chip
            {
                IsToggleable = true,
                AutoToggle = true,
                GroupName = GroupName,
                SelectedHasShadow = true,
                UnselectedHasShadow = true,
                Margin = ItemMargin,
                Tag = obj.Tag
            };

            chip.SetBinding(Chip.TextProperty, nameof(Chip.Text));
            chip.SetBinding(Chip.SelectedBackgroundColorProperty, nameof(Chip.SelectedBackgroundColor));
            chip.SetBinding(Chip.UnselectedBackgroundColorProperty, nameof(Chip.UnselectedBackgroundColor));
            chip.SetBinding(Chip.SelectCommandProperty, nameof(Chip.SelectCommand));
            chip.SetBinding(Chip.SelectCommandParameterProperty, nameof(Chip.SelectCommandParameter));

            chip.BindingContext = obj;

            //chip.OnClicked += OnClicked;
            //chip.OnClose += OnClose;
            //chip.OnSelect += OnSelect;
            //chip.OnUnselect += OnUnselect;

            chip.OnSelect += Chip_OnSelect;

            return chip;
        }

        private static void Chip_OnSelect(object? sender, EventArgs e)
        {
            if (sender is Chip chip)
            {
                if (chip.Parent?.Parent is ChipGroup chipGroup)
                {
                    chipGroup.SelectedText = chip.Text;
                    chipGroup.SelectedTag = chip.Tag;
                }
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            base.OnAppearing();
        }

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }
    }
}