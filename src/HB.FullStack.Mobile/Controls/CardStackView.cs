using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using HB.FullStack.Mobile.Base;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace HB.FullStack.Mobile.Controls
{
    public class CardStackView : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(CardStackView), propertyChanged: (b, o, n) => { ((CardStackView)b).OnItemsSourceChanged((IList)o, (IList)n); });

        #region Privates
        private readonly Grid _root;
        private readonly List<Grid> _cards = new List<Grid>();
        private Grid _lastCard = null!;
        private readonly ICommand _tapCommand;
        #endregion
        public CardStackView()
        {
            Content = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                RowDefinitions = Rows.Define(Auto),
                ColumnDefinitions = Columns.Define(Auto)
            }.Assign(out _root);

            SizeChanged += OnSizeChanged;

            _tapCommand = new Command<Grid>(OnTapped);
        }



        #region Inputs

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public IList? ItemsSource { get => (IList)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public DataTemplate? LabelDataTemplate { get; set; }
        public DataTemplate? CardDataTemplate { get; set; }
        public int LabelWidth { get; set; } = 60;

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public IList<Color>? CardBackgroundColors { get; set; }

        #endregion

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }

        private void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
        {
            ReAddCards();

            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnItemsSourceCollectionChanged;
            }
        }

        private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ReAddCards();
        }

        private void OnLabelDataTemplateChanged()
        {
            ReAddCards();
        }

        private void OnCardDataTemplateChanged()
        {
            ReAddCards();
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            ReAddCards();
        }

        private void OnTapped(Grid card)
        {
            int index = _cards.IndexOf(card);

            if (index == _cards.Count - 1)
            {
                return;
            }

            //交换
            _cards[index] = _lastCard;
            _cards[^1] = card;

            _root.Children.Clear();

            //位移
            _lastCard.TranslationX = (_cards.Count - 1 - index) * LabelWidth;
            card.TranslationX = 0;

            //切换卡片中的“可见”与“不可见”
            _lastCard.Children[0].Opacity = 0;
            _lastCard.Children[1].Opacity = 1;
            card.Children[0].Opacity = 1;
            card.Children[1].Opacity = 0;

            _root.Children.AddRange(_cards);

            _lastCard = card;
        }

        private void ReAddCards()
        {
            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                return;
            }

            if (CardDataTemplate == null)
            {
                return;
            }

            if (Width <= 0)
            {
                return;
            }


            if (CardBackgroundColors.IsNullOrEmpty())
            {
                GenerateRandomBackgroudColors();
            }

            double OneCardWidth = Width - ((ItemsSource.Count - 1) * LabelWidth);

            _root.Children.Clear();
            _cards.Clear();

            for (int i = ItemsSource.Count - 1; i >= 0; i--)
            {
                View cardView = (View)CardDataTemplate.CreateContent();
                cardView.BindingContext = ItemsSource[i];

                View labelView;

                if (LabelDataTemplate != null)
                {
                    labelView = (View)LabelDataTemplate.CreateContent();
                    labelView.BindingContext = ItemsSource[i];
                }
                else
                {
                    labelView = new StackLayout();
                }

                Grid card = new Grid
                {
                    RowDefinitions = Rows.Define(Auto),
                    ColumnDefinitions = Columns.Define(Star, LabelWidth),
                    TranslationX = i * LabelWidth,
                    BackgroundColor = CardBackgroundColors!.ElementAt(ItemsSource.Count - i - 1),
                    Children =
                    {
                        //Card Content
                        cardView.Row(0).Column(0,2).Invoke(view=>view.Opacity = 0),
                        
                        //Card Margin
                        labelView.Row(0).Column(1),
                    }
                }.Width(OneCardWidth)
                .Invoke(view => view.GestureRecognizers.Add(new TapGestureRecognizer { Command = _tapCommand, CommandParameter = view }));

                _cards.Add(card);
            }


            _lastCard = _cards.Last();
            _lastCard.Children[0].Opacity = 1;
            _lastCard.Children[1].Opacity = 0;

            _root.Children.AddRange(_cards);
        }

        private void GenerateRandomBackgroudColors()
        {
            CardBackgroundColors = new List<Color>();

            for (int i = 0; i < 10; ++i)
            {
                CardBackgroundColors.Add(ColorUtil.RandomColor().Color);
            }
        }
    }
}