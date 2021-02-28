using AsyncAwaitBestPractices;
using HB.FullStack.XamarinForms.Base;
using HB.FullStack.XamarinForms.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace HB.FullStack.XamarinForms.Controls
{
    public class ActionSheetCellOptionItem
    {
        public string Text { get; set; } = null!;

        public ICommand? Command { get; set; }

        public object? CommandParameter { get; set; }

        public object? Tag { get; set; }
    }

    public class ActionSheetCell : StackLayout
    {
        //public string ChevronRight { get; } = MaterialFont.ChevronRight;

        #region Input

        //public static readonly BindableProperty MarginProperty = BindableProperty.Create(
        //    nameof(Margin),
        //    typeof(Thickness),
        //    typeof(ActionSheetCell),
        //    null,
        //    BindingMode.OneWay);

        //public Thickness Margin
        //{
        //    get { return (Thickness)GetValue(MarginProperty); }
        //    set { SetValue(MarginProperty, value); }
        //}

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(ActionSheetCell),
            null,
            BindingMode.OneWay);

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        public static readonly BindableProperty OptionsProperty = BindableProperty.Create(
            nameof(Options),
            typeof(IList<ActionSheetCellOptionItem>),
            typeof(ActionSheetCell),
            null,
            BindingMode.OneWay);

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ActionSheetCellOptionItem> Options
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return (IList<ActionSheetCellOptionItem>)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly BindableProperty ActionSheetTitleProperty = BindableProperty.Create(
            nameof(ActionSheetTitle),
            typeof(string),
            typeof(ActionSheetCell),
            null,
            BindingMode.OneWay);

        public string ActionSheetTitle
        {
            get { return (string)GetValue(ActionSheetTitleProperty); }
            set { SetValue(ActionSheetTitleProperty, value); }
        }

        public static readonly BindableProperty ActionSheetCancelProperty = BindableProperty.Create(
            nameof(ActionSheetCancel),
            typeof(string),
            typeof(ActionSheetCell),
            null,
            BindingMode.OneWay);

        public string ActionSheetCancel
        {
            get { return (string)GetValue(ActionSheetCancelProperty); }
            set { SetValue(ActionSheetCancelProperty, value); }
        }

        #endregion

        #region InputOutput

        public static readonly BindableProperty SelectedDisplayTextProperty = BindableProperty.Create(
            nameof(SelectedDisplayText),
            typeof(string),
            typeof(ActionSheetCell),
            null,
            BindingMode.TwoWay);

        public string? SelectedDisplayText
        {
            get { return (string)GetValue(SelectedDisplayTextProperty); }
            set { SetValue(SelectedDisplayTextProperty, value); }
        }

        #endregion

        #region OutPut

        private string? _selectedOptionText;

        public string? SelectedOptionText
        {
            get { return _selectedOptionText; }
            set { _selectedOptionText = value; OnPropertyChanged(); }
        }

        private object? _selectedOptionTag;

        public object? SelectedOptionTag
        {
            get { return _selectedOptionTag; }
            set { _selectedOptionTag = value; OnPropertyChanged(); }
        }

        #endregion

        public ActionSheetCell()
        {
            var grid = new Grid { 
                RowDefinitions = Rows.Define(Star),
                ColumnDefinitions = Columns.Define(Auto, Star, Auto),
                Children = { 
                    new Label{ }
                        .Row(0).Column(0).Start().TextCenterVertical().FontSize(Device.GetNamedSize(NamedSize.Medium, typeof(Label)))
                        .Bind(Label.TextProperty, nameof(Title)),

                    new Label{ TextColor = Color.Gray }
                        .Row(0).Column(1).End().TextCenterVertical().FontSize(Device.GetNamedSize(NamedSize.Small, typeof(Label)))
                        .Bind(Label.TextProperty, nameof(SelectedDisplayText)),

                    new Label{ Text = MaterialFont.ChevronRight }
                        .Row(0).Column(2).EndExpand().TextCenterVertical().Font("MaterialIcon", Device.GetNamedSize(NamedSize.Medium, typeof(Label)))
                }
            }.Bind(Grid.MarginProperty, nameof(Margin))
            .Invoke(v=>v.GestureRecognizers.Add(new TapGestureRecognizer { }.Invoke(v=>v.Tapped+=TapGestureRecognizer_Tapped)))
            .Invoke(v=>v.BindingContext = this);
            
            Children.Add(grid);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            DisplayActionSheetAndExecuteAsync().Fire();
        }

        private async Task DisplayActionSheetAndExecuteAsync()
        {
            if (Options.IsNullOrEmpty())
            {
                return;
            }

            IList<string> optionTexts = Options.Select(o => o.Text).ToList();

            string optionText = await Application.Current.MainPage.DisplayActionSheet(
                ActionSheetTitle,
                ActionSheetCancel,
                null,
                optionTexts.ToArray()).ConfigureAwait(false);

            int index = optionTexts.IndexOf(optionText);

            if (index >= 0 && index <= optionTexts.Count)
            {
                ActionSheetCellOptionItem optionoItem = Options[index];

                SelectedOptionTag = optionoItem.Tag;
                SelectedOptionText = optionoItem.Text;

                ICommand? command = optionoItem.Command;

                if (command != null && command.CanExecute(optionoItem.CommandParameter))
                {
                    command.Execute(optionoItem.CommandParameter);
                }
            }
        }
    }
}