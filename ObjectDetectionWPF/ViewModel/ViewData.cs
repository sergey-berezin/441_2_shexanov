using LibraryANN;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ObjectDetectionWPF.ViewModel
{
    public class ViewData
    {
        private ObjectDetection objectDetection = new ObjectDetection();
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly IAsyncCommand loadFiles;
        private readonly ICommand cancel;
        private readonly ICommand clearCollection;
        
        public IAsyncCommand LoadFiles { get => loadFiles; }
        public ICommand Cancel { get => cancel; }
        public ICommand ClearCollection { get => clearCollection; }

        private readonly IUIFunctions uiFunctions;
        private readonly IExceptionNotifier exceptionNotifier;

        public ObservableCollection<ChoosenImageInfo> FilesNamesCollection { get; set; }

        private bool canLoad;
        private bool canCancel;
        
        public ViewData(IUIFunctions _uiFunctions, IExceptionNotifier _exceptionNotifier)
        {
            uiFunctions = _uiFunctions;
            exceptionNotifier = _exceptionNotifier;

            canLoad = false;
            canCancel = false;

            FilesNamesCollection = new ObservableCollection<ChoosenImageInfo>();

            List<string> listOfFileNames;
            
            loadFiles = new AsyncCommand(
                () => canLoad,
                async () =>
                {
                    canLoad = false;
                    canCancel = true;
                    tokenSource = new CancellationTokenSource();
                    listOfFileNames = new List<string>(uiFunctions.GetFileNames());
                    await ProcessingAllFiles(listOfFileNames);
                });

            cancel = new RelayCommand(
                _ => canCancel,
                _ =>
                {
                    tokenSource.Cancel();
                    canLoad = true;
                    canCancel = false;
                });

            clearCollection = new RelayCommand(
                _ => FilesNamesCollection.Count > 0,
                _ =>
                {
                    canLoad = false;
                    FilesNamesCollection.Clear();
                    canLoad = true;
                });
        }

        public async Task SessionInitialization()
        {
            try
            {
                await objectDetection.SessionInitializationAsync();
                canLoad = true;
            }
            catch (Exception ex)
            {
                exceptionNotifier.ShowErrorMessage("1" + ex.Message);
            }

        }

        private async Task ProcessingAllFiles(List<string> listOfFileNames)
        {
            try
            {
                foreach (var fileName in listOfFileNames)
                {
                    if (!FilesNamesCollection.Any(x => x.FullName == fileName))
                        await ProcessFileAsync(fileName, tokenSource);
                }
                canLoad = true;
                canCancel = false;
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                exceptionNotifier.ShowErrorMessage("2" + ex.Message);
                canLoad = true;
                canCancel = false;
            }

        }

        private async Task<List<ProcessedImageInfo>> ProcessFileAsync(string fileName,
            CancellationTokenSource tokenSource)
        {
            try
            {
                var task = await objectDetection.GetInfoAsync(fileName, tokenSource.Token);
                if (task.Count != 0)
                {
                    task.FirstOrDefault().SaveAsJpeg();

                    var listOfClassNames = new List<string>();
                    foreach (var item in task)
                    {
                        listOfClassNames.Add(item.ClassName);
                    }
                    var listOfFilesNames = FilesNamesCollection.ToList();

                    IComparer<ChoosenImageInfo> comparer = new ChoosenImageInfoComparer();

                    var choosenImageInfo = new ChoosenImageInfo(fileName, fileName.Split('\\').Last(),
                        Directory.GetCurrentDirectory() + "\\" + task.FirstOrDefault().FileName, listOfClassNames);

                    var index = listOfFilesNames.BinarySearch(choosenImageInfo, comparer);

                    if (index < 0)
                    {
                        FilesNamesCollection.Insert(~index, choosenImageInfo);
                    }
                    //var sortedList = FilesNamesCollection.OrderByDescending(x => x.ClassNames.Count).ThenBy(x => x.ShortName).ToList();
                }
                //else
                //{
                //    exceptionNotifier.ShowErrorMessage("No objects were found on one of pictures");
                //}
                return task;
            }
            catch (TaskCanceledException) 
            {
                throw new TaskCanceledException();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
