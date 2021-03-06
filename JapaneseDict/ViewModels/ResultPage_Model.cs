﻿using System.Reactive;
using System.Reactive.Linq;
using MVVMSidekick.ViewModels;
using MVVMSidekick.Views;
using MVVMSidekick.Reactive;
using MVVMSidekick.Services;
using MVVMSidekick.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Threading;
using JapaneseDict.Models;
using JapaneseDict.OnlineService;
using Windows.UI.Popups;
using System.Net.Http;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using JapaneseDict.QueryEngine;
using JapaneseDict.QueryEngine.Models.JmdictModels;
using static JapaneseDict.QueryEngine.Models.JmdictModels.JmDict;

namespace JapaneseDict.GUI.ViewModels
{

    [DataContract]
    public class ResultPage_Model : ViewModelBase<ResultPage_Model>,IDisposable
    {
        
        // If you have install the code sniplets, use "propvm + [tab] +[tab]" create a property。
        // 如果您已经安装了 MVVMSidekick 代码片段，请用 propvm +tab +tab 输入属性
        ObservableCollection<Sense> sense;
        ObservableCollection<REle> rele;
        ObservableCollection<KEle> kele;
        ObservableCollection<JoyoKanji> kanjiresults;
        MatchCollection _kanjikeyword;
        int _id;
        string _keyword;
        public ResultPage_Model(SearchTerm term)
        {
            Regex reg = new Regex("[\u4e00-\u9fa5]+"); //extract kanjis
            _id = term.EntryId;
            _keyword = term.Keyword;
            if(_id!=-1)
            {
                QueryAll(_id);
            }
            else
            {
                QueryAll(_keyword);
            }
            _kanjikeyword = reg.Matches(_keyword);
            EnableBackButtonOnTitleBar((sender, args) =>
            {
                BindableBase model=this;

                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame.CanGoBack)
                {
                    rootFrame.GoBack();

                }
                DisableBackButtonOnTitleBar();

            });
            
        }
        public async void QueryAll(int entryid)
        {
            var kele = await QueryEngine.JmdictQueryEngine.QueryKEleAsync(entryid);
            var rele = await QueryEngine.JmdictQueryEngine.QueryREleAsync(entryid);
            var sense = await QueryEngine.JmdictQueryEngine.QuerySensesAsync(entryid);
            foreach(var r in rele)
            {
                r.Kebs = kele;
            }
            this.Result = new ObservableCollection<SearchResult>();
            this.Result.Add(new SearchResult() { Kele = kele, Rele = rele, Sense = sense });
            
        }
        public async void QueryAll(string keyword)
        {
            var res = await QueryEngine.JmdictQueryEngine.QueryFromKeywordAsync(keyword);
            foreach(var r in res)
            {
                foreach(var rele in r.Rele)
                {
                    rele.Kebs = r.Kele;
                }
            }
            this.Result = new ObservableCollection<SearchResult>(res);
        }
        //private async void QueryWord()
        //{

        //    this.results = await JmdictQueryEngine.QueryForUIAsync(_keyword);
        //    this.Results = results;
        //    //cn to jp translate method (obsolete)
        //    //this.Cn2JpResult = await QueryEngine.QueryEngine.MainDictQueryEngine.QueryCn2JpForUIAsync(_keyword);
        //    //this.OnlineResult = await QueryEngine.QueryEngine.OnlineQueryEngine.Query(_keyword);
        //}
        //private async void QueryWord(int id)
        //{

        //    this.results = await JmdictQueryEngine.QueryForUIAsync(id.ToString());
        //    this.Results = results;
        //    //this.OnlineResult = await QueryEngine.QueryEngine.OnlineQueryEngine.Query(results.First().JpChar);
        //}



        public ObservableCollection<SearchResult> Result
        {
            get { return _ResultLocator(this).Value; }
            set { _ResultLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property ObservableCollection<SearchResult> Result Setup        
        protected Property<ObservableCollection<SearchResult>> _Result = new Property<ObservableCollection<SearchResult>> { LocatorFunc = _ResultLocator };
        static Func<BindableBase, ValueContainer<ObservableCollection<SearchResult>>> _ResultLocator = RegisterContainerLocator<ObservableCollection<SearchResult>>(nameof(Result), model => model.Initialize(nameof(Result), ref model._Result, ref _ResultLocator, _ResultDefaultValueFactory));
        static Func<ObservableCollection<SearchResult>> _ResultDefaultValueFactory = () => default(ObservableCollection<SearchResult>);
        #endregion

        public ObservableCollection<JoyoKanji> KanjiResults
        {
            get { return _KanjiResultsLocator(this).Value; }
            set { _KanjiResultsLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property ObservableCollection<JoyoKanji> KanjiResults Setup        
        protected Property<ObservableCollection<JoyoKanji>> _KanjiResults = new Property<ObservableCollection<JoyoKanji>> { LocatorFunc = _KanjiResultsLocator };
        static Func<BindableBase, ValueContainer<ObservableCollection<JoyoKanji>>> _KanjiResultsLocator = RegisterContainerLocator<ObservableCollection<JoyoKanji>>(nameof(KanjiResults), model => model.Initialize(nameof(KanjiResults), ref model._KanjiResults, ref _KanjiResultsLocator, _KanjiResultsDefaultValueFactory));
        static Func<ObservableCollection<JoyoKanji>> _KanjiResultsDefaultValueFactory = () => default(ObservableCollection<JoyoKanji>);
        #endregion

        private void EnableBackButtonOnTitleBar(EventHandler<BackRequestedEventArgs> onBackRequested)
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += onBackRequested;
        }
        private void DisableBackButtonOnTitleBar()
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
        public ResultPage_Model()
        {
            
            EnableBackButtonOnTitleBar((sender, args) =>
            {
                if (this.StageManager.DefaultStage.Frame != null && this.StageManager.DefaultStage.Frame.CanGoBack)
                {
                    if (!this.StageManager.DefaultStage.Frame.CanGoBack)
                    {
                        DisableBackButtonOnTitleBar();
                    }
                    this.StageManager.DefaultStage.Frame.GoBack(); 
                }
                    
            });
        }
        
        public CommandModel<ReactiveCommand, String> CommandQueryOnline
        {
            get { return _CommandQueryOnlineLocator(this).Value; }
            set { _CommandQueryOnlineLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property CommandModel<ReactiveCommand, String> CommandQueryOnline Setup        

        protected Property<CommandModel<ReactiveCommand, String>> _CommandQueryOnline = new Property<CommandModel<ReactiveCommand, String>> { LocatorFunc = _CommandQueryOnlineLocator };
        static Func<BindableBase, ValueContainer<CommandModel<ReactiveCommand, String>>> _CommandQueryOnlineLocator = RegisterContainerLocator<CommandModel<ReactiveCommand, String>>(nameof(CommandQueryOnline), model => model.Initialize(nameof(CommandQueryOnline), ref model._CommandQueryOnline, ref _CommandQueryOnlineLocator, _CommandQueryOnlineDefaultValueFactory));
        static Func<BindableBase, CommandModel<ReactiveCommand, String>> _CommandQueryOnlineDefaultValueFactory =
            model =>
            {
                var resource = nameof(CommandQueryOnline);           // Command resource  
                var commandId = nameof(CommandQueryOnline);
                var vm = CastToCurrentType(model);
                var cmd = new ReactiveCommand(canExecute: true) { ViewModel = model }; //New Command Core

                cmd.DoExecuteUIBusyTask(
                        vm,
                        async e =>
                        {
                            vm.IsOnlineQueryBusy = true;
                            vm.OnlineResult = await OnlineQueryEngine.Query(vm.Result.First().Kele.First().Keb);
                            //Todo: Add QueryOnline logic here, or
                            await MVVMSidekick.Utilities.TaskExHelper.Yield();
                            vm.IsOnlineQueryBusy = false;
                        })
                    .DoNotifyDefaultEventRouter(vm, commandId)
                    .Subscribe()
                    .DisposeWith(vm);

                var cmdmdl = cmd.CreateCommandModel(resource);

                cmdmdl.ListenToIsUIBusy(
                    model: vm,
                    canExecuteWhenBusy: false);
                return cmdmdl;
            };

        #endregion

        public bool IsOnlineQueryBusy
        {
            get { return _IsOnlineQueryBusyLocator(this).Value; }
            set { _IsOnlineQueryBusyLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property bool IsOnlineQueryBusy Setup        
        protected Property<bool> _IsOnlineQueryBusy = new Property<bool> { LocatorFunc = _IsOnlineQueryBusyLocator };
        static Func<BindableBase, ValueContainer<bool>> _IsOnlineQueryBusyLocator = RegisterContainerLocator<bool>(nameof(IsOnlineQueryBusy), model => model.Initialize(nameof(IsOnlineQueryBusy), ref model._IsOnlineQueryBusy, ref _IsOnlineQueryBusyLocator, _IsOnlineQueryBusyDefaultValueFactory));
        static Func<BindableBase, bool> _IsOnlineQueryBusyDefaultValueFactory =
            model =>
            {
                var vm = CastToCurrentType(model);
                //TODO: Add the logic that produce default value from vm current status.
                return default(bool);
            };
        #endregion

        public bool IsLocalQueryBusy
        {
            get { return _IsLocalQueryBusyLocator(this).Value; }
            set { _IsLocalQueryBusyLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property bool IsLocalQueryBusy Setup        
        protected Property<bool> _IsLocalQueryBusy = new Property<bool> { LocatorFunc = _IsLocalQueryBusyLocator };
        static Func<BindableBase, ValueContainer<bool>> _IsLocalQueryBusyLocator = RegisterContainerLocator<bool>(nameof(IsLocalQueryBusy), model => model.Initialize(nameof(IsLocalQueryBusy), ref model._IsLocalQueryBusy, ref _IsLocalQueryBusyLocator, _IsLocalQueryBusyDefaultValueFactory));
        static Func<BindableBase, bool> _IsLocalQueryBusyDefaultValueFactory =
            model =>
            {
                var vm = CastToCurrentType(model);
                //TODO: Add the logic that produce default value from vm current status.
                return default(bool);
            };
        #endregion

        public CommandModel<ReactiveCommand, String> CommandSpeak
        {
            get { return _CommandSpeakLocator(this).Value; }
            set { _CommandSpeakLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property CommandModel<ReactiveCommand, String> CommandSpeak Setup        

        protected Property<CommandModel<ReactiveCommand, String>> _CommandSpeak = new Property<CommandModel<ReactiveCommand, String>> { LocatorFunc = _CommandSpeakLocator };
        static Func<BindableBase, ValueContainer<CommandModel<ReactiveCommand, String>>> _CommandSpeakLocator = RegisterContainerLocator<CommandModel<ReactiveCommand, String>>(nameof(CommandSpeak), model => model.Initialize(nameof(CommandSpeak), ref model._CommandSpeak, ref _CommandSpeakLocator, _CommandSpeakDefaultValueFactory));
        static Func<BindableBase, CommandModel<ReactiveCommand, String>> _CommandSpeakDefaultValueFactory =
            model =>
            {
                var resource = nameof(CommandSpeak);           // Command resource  
                var commandId = nameof(CommandSpeak);
                var vm = CastToCurrentType(model);
                var cmd = new ReactiveCommand(canExecute: true) { ViewModel = model }; //New Command Core

                cmd.DoExecuteUIBusyTask(
                        vm,
                        async e =>
                        {
                            //const string CLIENT_ID = "skylark_jpdict";
                            //const string CLIENT_SECRET = "uzHa5qUm4+GehYnL2pMIw8XtNox8sbqGNq7S+UiM6bk=";
                            const string CLIENT_ID = "skylarkjpdict";
                            const string CLIENT_SECRET = "b155421d3d7746ebbfcb2e7922b60a87";
                            try
                            {
                                SpeechSynthesizer speech = new SpeechSynthesizer(CLIENT_ID, CLIENT_SECRET);
                                string text = e.EventArgs.Parameter.ToString();
                                string language = "ja";
                                // Gets the audio stream.
                                var stream = await speech.GetSpeakStreamAsync(text, language);
                                MediaElement mediaEle = new MediaElement();
                                // Reproduces the audio stream using a MediaElement.
                                mediaEle.SetSource(stream, speech.MimeContentType);
                                mediaEle.Play();

                            }
                            catch(HttpRequestException)
                            {
                                await new MessageDialog("请检查您的网络连接", "出现错误").ShowAsync();
                            }
                            
                            await MVVMSidekick.Utilities.TaskExHelper.Yield();
                        })
                    .DoNotifyDefaultEventRouter(vm, commandId)
                    .Subscribe()
                    .DisposeWith(vm);

                var cmdmdl = cmd.CreateCommandModel(resource);

                cmdmdl.ListenToIsUIBusy(
                    model: vm,
                    canExecuteWhenBusy: false);
                return cmdmdl;
            };

        #endregion

        public CommandModel<ReactiveCommand, String> CommandAddToNotebook
        {
            get { return _CommandAddToNotebookLocator(this).Value; }
            set { _CommandAddToNotebookLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property CommandModel<ReactiveCommand, String> CommandAddToNotebook Setup        

        protected Property<CommandModel<ReactiveCommand, String>> _CommandAddToNotebook = new Property<CommandModel<ReactiveCommand, String>> { LocatorFunc = _CommandAddToNotebookLocator };
        static Func<BindableBase, ValueContainer<CommandModel<ReactiveCommand, String>>> _CommandAddToNotebookLocator = RegisterContainerLocator<CommandModel<ReactiveCommand, String>>(nameof(CommandAddToNotebook), model => model.Initialize(nameof(CommandAddToNotebook), ref model._CommandAddToNotebook, ref _CommandAddToNotebookLocator, _CommandAddToNotebookDefaultValueFactory));
        static Func<BindableBase, CommandModel<ReactiveCommand, String>> _CommandAddToNotebookDefaultValueFactory =
            model =>
            {
                var resource = nameof(CommandAddToNotebook);           // Command resource  
                var commandId = nameof(CommandAddToNotebook);
                var vm = CastToCurrentType(model);
                var cmd = new ReactiveCommand(canExecute: true) { ViewModel = model }; //New Command Core

                cmd.DoExecuteUIBusyTask(
                        vm,
                        async e =>
                        {
                            QueryEngine.QueryEngine.UserDefDictQueryEngine.Add(Convert.ToInt32(e.EventArgs.Parameter));
                            //Todo: Add AddToNotebook logic here, or
                            await MVVMSidekick.Utilities.TaskExHelper.Yield();
                        })
                    .DoNotifyDefaultEventRouter(vm, commandId)
                    .Subscribe()
                    .DisposeWith(vm);

                var cmdmdl = cmd.CreateCommandModel(resource);

                cmdmdl.ListenToIsUIBusy(
                    model: vm,
                    canExecuteWhenBusy: false);
                return cmdmdl;
            };

        #endregion

        public CommandModel<ReactiveCommand, String> CommandRemoveFromNotebook
        {
            get { return _CommandRemoveFromNotebookLocator(this).Value; }
            set { _CommandRemoveFromNotebookLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property CommandModel<ReactiveCommand, String> CommandRemoveFromNotebook Setup        

        protected Property<CommandModel<ReactiveCommand, String>> _CommandRemoveFromNotebook = new Property<CommandModel<ReactiveCommand, String>> { LocatorFunc = _CommandRemoveFromNotebookLocator };
        static Func<BindableBase, ValueContainer<CommandModel<ReactiveCommand, String>>> _CommandRemoveFromNotebookLocator = RegisterContainerLocator<CommandModel<ReactiveCommand, String>>(nameof(CommandRemoveFromNotebook), model => model.Initialize(nameof(CommandRemoveFromNotebook), ref model._CommandRemoveFromNotebook, ref _CommandRemoveFromNotebookLocator, _CommandRemoveFromNotebookDefaultValueFactory));
        static Func<BindableBase, CommandModel<ReactiveCommand, String>> _CommandRemoveFromNotebookDefaultValueFactory =
            model =>
            {
                var resource = nameof(CommandRemoveFromNotebook);           // Command resource  
                var commandId = nameof(CommandRemoveFromNotebook);
                var vm = CastToCurrentType(model);
                var cmd = new ReactiveCommand(canExecute: true) { ViewModel = model }; //New Command Core

                cmd.DoExecuteUIBusyTask(
                        vm,
                        async e =>
                        {
                            QueryEngine.QueryEngine.UserDefDictQueryEngine.Remove(Convert.ToInt32(e.EventArgs.Parameter));
                            //Todo: Add RemoveFromNotebook logic here, or
                            await MVVMSidekick.Utilities.TaskExHelper.Yield();
                        })
                    .DoNotifyDefaultEventRouter(vm, commandId)
                    .Subscribe()
                    .DisposeWith(vm);

                var cmdmdl = cmd.CreateCommandModel(resource);

                cmdmdl.ListenToIsUIBusy(
                    model: vm,
                    canExecuteWhenBusy: false);
                return cmdmdl;
            };

        #endregion

        public CommandModel<ReactiveCommand, String> CommandSendFeedback
        {
            get { return _CommandSendFeedbackLocator(this).Value; }
            set { _CommandSendFeedbackLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property CommandModel<ReactiveCommand, String> CommandSendFeedback Setup        

        protected Property<CommandModel<ReactiveCommand, String>> _CommandSendFeedback = new Property<CommandModel<ReactiveCommand, String>> { LocatorFunc = _CommandSendFeedbackLocator };
        static Func<BindableBase, ValueContainer<CommandModel<ReactiveCommand, String>>> _CommandSendFeedbackLocator = RegisterContainerLocator<CommandModel<ReactiveCommand, String>>(nameof(CommandSendFeedback), model => model.Initialize(nameof(CommandSendFeedback), ref model._CommandSendFeedback, ref _CommandSendFeedbackLocator, _CommandSendFeedbackDefaultValueFactory));
        static Func<BindableBase, CommandModel<ReactiveCommand, String>> _CommandSendFeedbackDefaultValueFactory =
            model =>
            {
                var resource = nameof(CommandSendFeedback);           // Command resource  
                var commandId = nameof(CommandSendFeedback);
                var vm = CastToCurrentType(model);
                var cmd = new ReactiveCommand(canExecute: true) { ViewModel = model }; //New Command Core

                cmd.DoExecuteUIBusyTask(
                        vm,
                        async e =>
                        {
                            if(!string.IsNullOrWhiteSpace(e.EventArgs.Parameter.ToString()))
                            {
                                (Window.Current.Content as Frame).Navigate(typeof(FeedbackPage), e.EventArgs.Parameter.ToString());
                            }
                            //Todo: Add SendFeedback logic here, or
                            await MVVMSidekick.Utilities.TaskExHelper.Yield();
                        })
                    .DoNotifyDefaultEventRouter(vm, commandId)
                    .Subscribe()
                    .DisposeWith(vm);

                var cmdmdl = cmd.CreateCommandModel(resource);

                cmdmdl.ListenToIsUIBusy(
                    model: vm,
                    canExecuteWhenBusy: false);
                return cmdmdl;
            };

        #endregion

        public ObservableCollection<OnlineDict> OnlineResult
        {
            get { return _OnlineResultLocator(this).Value; }
            set { _OnlineResultLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property ObservableCollection<OnlineDict> OnlineResult Setup        
        protected Property<ObservableCollection<OnlineDict>> _OnlineResult = new Property<ObservableCollection<OnlineDict>> { LocatorFunc = _OnlineResultLocator };
        static Func<BindableBase, ValueContainer<ObservableCollection<OnlineDict>>> _OnlineResultLocator = RegisterContainerLocator<ObservableCollection<OnlineDict>>(nameof(OnlineResult), model => model.Initialize(nameof(OnlineResult), ref model._OnlineResult, ref _OnlineResultLocator, _OnlineResultDefaultValueFactory));
        static Func<BindableBase, ObservableCollection<OnlineDict>> _OnlineResultDefaultValueFactory =
            model =>
            {
                var vm = CastToCurrentType(model);
                //TODO: Add the logic that produce default value from vm current status.
                return default(ObservableCollection<OnlineDict>);
            };
        #endregion

        public CommandModel<ReactiveCommand, String> CommandQueryWords
        {
            get { return _CommandQueryWordsLocator(this).Value; }
            set { _CommandQueryWordsLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property CommandModel<ReactiveCommand, String> CommandQueryWords Setup        

        protected Property<CommandModel<ReactiveCommand, String>> _CommandQueryWords = new Property<CommandModel<ReactiveCommand, String>> { LocatorFunc = _CommandQueryWordsLocator };
        static Func<BindableBase, ValueContainer<CommandModel<ReactiveCommand, String>>> _CommandQueryWordsLocator = RegisterContainerLocator<CommandModel<ReactiveCommand, String>>(nameof(CommandQueryWords), model => model.Initialize(nameof(CommandQueryWords), ref model._CommandQueryWords, ref _CommandQueryWordsLocator, _CommandQueryWordsDefaultValueFactory));
        static Func<BindableBase, CommandModel<ReactiveCommand, String>> _CommandQueryWordsDefaultValueFactory =
            model =>
            {
                var resource = nameof(CommandQueryWords);           // Command resource  
                var commandId = nameof(CommandQueryWords);
                var vm = CastToCurrentType(model);
                ObservableCollection<MainDict> results = new ObservableCollection<MainDict>();
                var cmd = new ReactiveCommand(canExecute: true) { ViewModel = model }; //New Command Core
                //SQLiteConnection conn;
                cmd.DoExecuteUIBusyTask(
                        vm,
                        async e =>
                        {

                            if ((e.EventArgs.Parameter is SearchTerm) && (e.EventArgs.Parameter.ToString() != "Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs"))
                            {
                                Frame rootFrame = Window.Current.Content as Frame;
                                rootFrame.Navigate(typeof(ResultPage), e.EventArgs.Parameter as SearchTerm);
                            }
                            await MVVMSidekick.Utilities.TaskExHelper.Yield();
                        })
                    .DoNotifyDefaultEventRouter(vm, commandId)
                    .Subscribe()
                    .DisposeWith(vm);

                var cmdmdl = cmd.CreateCommandModel(resource);

                cmdmdl.ListenToIsUIBusy(
                    model: vm,
                    canExecuteWhenBusy: false);
                return cmdmdl;
            };

        #endregion

        public ObservableCollection<MainDict> Cn2JpResult
        {
            get { return _Cn2JpResultLocator(this).Value; }
            set { _Cn2JpResultLocator(this).SetValueAndTryNotify(value); }
        }
        #region Property ObservableCollection<MainDict> Cn2JpResult Setup        
        protected Property<ObservableCollection<MainDict>> _Cn2JpResult = new Property<ObservableCollection<MainDict>> { LocatorFunc = _Cn2JpResultLocator };
        static Func<BindableBase, ValueContainer<ObservableCollection<MainDict>>> _Cn2JpResultLocator = RegisterContainerLocator<ObservableCollection<MainDict>>(nameof(Cn2JpResult), model => model.Initialize(nameof(Cn2JpResult), ref model._Cn2JpResult, ref _Cn2JpResultLocator, _Cn2JpResultDefaultValueFactory));
        static Func<BindableBase, ObservableCollection<MainDict>> _Cn2JpResultDefaultValueFactory =
            model =>
            {
                var vm = CastToCurrentType(model);
                //TODO: Add the logic that produce default value from vm current status.
                return default(ObservableCollection<MainDict>);
            };
        #endregion
        #region Life Time Event Handling

        ///// <summary>
        ///// This will be invoked by view when this viewmodel instance is set to view's ViewModel property. 
        ///// </summary>
        ///// <param name="view">Set target</param>
        ///// <param name="oldValue">Value before set.</param>
        ///// <returns>Task awaiter</returns>
        //protected override Task OnBindedToView(MVVMSidekick.Views.IView view, IViewModel oldValue)
        //{
        //    return base.OnBindedToView(view, oldValue);
        //}

        ///// <summary>
        ///// This will be invoked by view when this instance of viewmodel in ViewModel property is overwritten.
        ///// </summary>
        ///// <param name="view">Overwrite target view.</param>
        ///// <param name="newValue">The value replacing </param>
        ///// <returns>Task awaiter</returns>
        //protected override Task OnUnbindedFromView(MVVMSidekick.Views.IView view, IViewModel newValue)
        //{
        //    return base.OnUnbindedFromView(view, newValue);
        //}

        ///// <summary>
        ///// This will be invoked by view when the view fires Load event and this viewmodel instance is already in view's ViewModel property
        ///// </summary>
        ///// <param name="view">View that firing Load event</param>
        ///// <returns>Task awaiter</returns>
        //protected override Task OnBindedViewLoad(MVVMSidekick.Views.IView view)
        //{
        //    return base.OnBindedViewLoad(view);
        //}

        ///// <summary>
        ///// This will be invoked by view when the view fires Unload event and this viewmodel instance is still in view's  ViewModel property
        ///// </summary>
        ///// <param name="view">View that firing Unload event</param>
        ///// <returns>Task awaiter</returns>
        protected override Task OnBindedViewUnload(MVVMSidekick.Views.IView view)
        {
            this.Dispose();
            return base.OnBindedViewUnload(view);
        }

        ///// <summary>
        ///// <para>If dispose actions got exceptions, will handled here. </para>
        ///// </summary>
        ///// <param name="exceptions">
        ///// <para>The exception and dispose infomation</para>
        ///// </param>
        //protected override async void OnDisposeExceptions(IList<DisposeInfo> exceptions)
        //{
        //    base.OnDisposeExceptions(exceptions);
        //    await TaskExHelper.Yield();
        //}

        #endregion




    }

}

