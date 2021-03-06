﻿using JapaneseDict.GUI.ViewModels;
using System.Reactive;
using System.Reactive.Linq;
using MVVMSidekick.ViewModels;
using MVVMSidekick.Views;
using MVVMSidekick.Reactive;
using MVVMSidekick.Services;
using MVVMSidekick.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.UI.Input;
using JapaneseDict.Models;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using JapaneseDict.QueryEngine.Models.JmdictModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JapaneseDict.GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultPage : MVVMPage
    {
        string _keyword;
        int _id;
        public ResultPage()
            : this(null)
        {
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //{
            //    HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            //}
            //SetUpPageAnimation();
            //NavigationCacheMode = NavigationCacheMode.Disabled;
        }
        public ResultPage(ResultPage_Model model)
            : base(model)
        {
            this.InitializeComponent();

            /* because this page is cached, the constructor will not be triggered every time. */
            /* so the subscription of BackPressed should be in the handler of OnNavigatedTo event */
            /* because this event will be triggered every time when user navigate to this page */
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //{
            //    HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            //}
            //NavigationCacheMode = NavigationCacheMode.Disabled;
            this.RegisterPropertyChangedCallback(ViewModelProperty, (_, __) =>
            {
                StrongTypeViewModel = this.ViewModel as ResultPage_Model;
            });
            StrongTypeViewModel = this.ViewModel as ResultPage_Model;
            //SetUpPageAnimation();
        }

        //private void SetUpPageAnimation()
        //{
        //    TransitionCollection collection = new TransitionCollection();
        //    NavigationThemeTransition theme = new NavigationThemeTransition();

        //    var info = new DrillInNavigationTransitionInfo();

        //    theme.DefaultNavigationTransitionInfo = info;
        //    collection.Add(theme);
        //    this.Transitions = collection;
        //}
        public ResultPage_Model StrongTypeViewModel
        {
            get { return (ResultPage_Model)GetValue(StrongTypeViewModelProperty); }
            set { SetValue(StrongTypeViewModelProperty, value); }
        }

        public static readonly DependencyProperty StrongTypeViewModelProperty =
                    DependencyProperty.Register("StrongTypeViewModel", typeof(ResultPage_Model), typeof(ResultPage), new PropertyMetadata(null));




        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
            if(e.Parameter is SearchTerm)
            {
                this.ViewModel = new ResultPage_Model(e.Parameter as SearchTerm);
            }
        }
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
           

        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //{
            //    HardwareButtons.BackPressed -= HardwareButtons_BackPressed;

            //}
            //GC.Collect();
            base.OnNavigatedFrom(e);
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;

            }
        }
        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Observable.FromEventPattern<AutoSuggestBoxTextChangedEventArgs>(this.QueryBox, "TextChanged").Throttle(TimeSpan.FromMilliseconds(900)).Subscribe(async x =>
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                this.QueryBox.ItemsSource = await QueryEngine.JmdictQueryEngine.QueryForPreviewAsync(QueryBox.Text);
                                //await Task.Delay(500);
                            }));
            this.QueryBox.Tag = null;
        }

        private void AddToNote_Btn_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Visibility = Visibility.Collapsed;
            ((Button)((Button)sender).Tag).Visibility = Visibility.Visible;
        }

        private void RemoveFromNote_Btn_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Visibility = Visibility.Collapsed;
            ((Button)((Button)sender).Tag).Visibility = Visibility.Visible;
        }

        private void QueryBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            PreviewResult suggest = args.SelectedItem as PreviewResult;
            if (suggest == null)
                return;

            if (suggest.Result == "No local definitions found.")
            {
                sender.Tag = new SearchTerm() { EntryId = -1, Keyword = suggest.JpChar, IsFromSuggestion = true };
            }
            else
            {
                sender.Tag = new SearchTerm() { EntryId = suggest.EntryId, Keyword = suggest.JpChar, IsFromSuggestion = true };
            }

        }

        private void seeOnlineResult_Btn_Click(object sender, RoutedEventArgs e)
        {
            mainPivot.SelectedIndex = 1;
        }

        private void noteItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
            rootFrame.Navigate(typeof(ResultPage),((StackPanel)sender).Tag.ToString());
        }
        private void QueryBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(sender.Text))
            {
                if (sender.Tag is SearchTerm)
                {
                    if ((sender.Tag as SearchTerm).IsFromSuggestion != true)
                    {
                        sender.Tag = new SearchTerm() { EntryId = -1, Keyword = sender.Text, IsFromSuggestion = false };
                    }
                }
                else
                {
                    sender.Tag = new SearchTerm() { EntryId = -1, Keyword = sender.Text, IsFromSuggestion = false };
                }
            }
        }
    }
}
