/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Client.MauiLib.Base;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace HB.FullStack.Client.MauiLib.Controls
{
    public class CardStackView : BaseView
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
            Content = _root = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SizeChanged += OnSizeChanged;

            _tapCommand = new Command<Grid>(OnTapped);
        }

        #region Inputs

        public IList? ItemsSource { get => (IList)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

        public DataTemplate? LabelDataTemplate { get; set; }

        public DataTemplate? CardDataTemplate { get; set; }

        public int LabelWidth { get; set; } = 60;

        public IList<Color>? CardBackgroundColors { get; set; }

        #endregion

        public override void OnPageAppearing()
        {
            base.OnPageAppearing();
        }

        public override void OnPageDisappearing()
        {
            base.OnPageDisappearing();
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
            _cards[_cards.Count - 1] = card;

            _root.Children.Clear();

            //位移
            _lastCard.TranslationX = (_cards.Count - 1 - index) * LabelWidth;
            card.TranslationX = 0;

            //切换卡片中的“可见”与“不可见”
            (_lastCard.Children[0] as VisualElement)!.Opacity = 0;
            (_lastCard.Children[1] as VisualElement)!.Opacity = 1;
            (card.Children[0] as VisualElement)!.Opacity = 1;
            (card.Children[1] as VisualElement)!.Opacity = 0;

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
                cardView.Opacity = 0;

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
                    WidthRequest = OneCardWidth
                };
                card.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                card.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                card.ColumnDefinitions.Add(new ColumnDefinition { Width = LabelWidth });
                card.TranslationX = i * LabelWidth;
                card.BackgroundColor = CardBackgroundColors!.ElementAt(ItemsSource.Count - i - 1);
                card.GestureRecognizers.Add(new TapGestureRecognizer { Command = _tapCommand, CommandParameter = card });

                Grid.SetRow(cardView, 0);
                Grid.SetColumn(cardView, 0);
                Grid.SetColumnSpan(cardView, 2);

                card.Children.Add(cardView);

                Grid.SetRow(labelView, 0);
                Grid.SetColumn(labelView, 1);

                card.Children.Add(labelView);

                _cards.Add(card);
            }

            _lastCard = _cards.Last();
            (_lastCard.Children[0] as VisualElement)!.Opacity = 1;
            (_lastCard.Children[1] as VisualElement)!.Opacity = 0;

            _root.Children.AddRange(_cards);
        }

        private void GenerateRandomBackgroudColors()
        {
            CardBackgroundColors = new List<Color>();

            for (int i = 0; i < 10; ++i)
            {
                CardBackgroundColors.Add(Colors.Aquamarine);
            }
        }
    }
}