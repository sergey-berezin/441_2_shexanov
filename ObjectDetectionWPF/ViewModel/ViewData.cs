
using LibraryANN;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        //public ImageSource a;

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

                //var processedImage = new ImageSharpImageSource<Rgb24>(task.FirstOrDefault().DetectedObjectImage);

                if (task.Count != 0)
                {
                    //task.FirstOrDefault().SaveAsJpeg();

                    var listOfClassNames = new List<string>();
                    foreach (var item in task)
                    {
                        listOfClassNames.Add(item.ClassName);
                    }
                    var listOfFilesNames = FilesNamesCollection.ToList();

                    IComparer<ChoosenImageInfo> comparer = new ChoosenImageInfoComparer();

                    
                    var image = GetImageSourceWithAllObjects(task);
                    image.RemoveBlackStripes();
                    
                    var choosenImageInfo = new ChoosenImageInfo(fileName, fileName.Split('\\').Last(),
                        Directory.GetCurrentDirectory() + "\\" + task.FirstOrDefault().FileName, listOfClassNames, image);

                    var index = listOfFilesNames.BinarySearch(choosenImageInfo, comparer);

                    if (index < 0)
                    {
                        FilesNamesCollection.Insert(~index, choosenImageInfo);
                    }
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

        

        private ImageSharpImageSource<Rgb24> GetImageSourceWithAllObjects(List<ProcessedImageInfo> processedImageInfo)
        {
            var firstImageInfo = processedImageInfo.FirstOrDefault();
            var imageWithAllObjects = firstImageInfo.DetectedObjectImage;
            if (processedImageInfo.Count == 1)
            {
                return new ImageSharpImageSource<Rgb24>(imageWithAllObjects);
            }
            else
            {
                for (int i = 1; i < processedImageInfo.Count; i++) 
                {
                    AnnotateObject(imageWithAllObjects, processedImageInfo[i].LeftUpperCornerX, processedImageInfo[i].LeftUpperCornerY,
                        processedImageInfo[i].LeftUpperCornerX + processedImageInfo[i].Width,
                        processedImageInfo[i].LeftUpperCornerY + processedImageInfo[i].Height,
                        processedImageInfo[i].ClassName);
                }
                return new ImageSharpImageSource<Rgb24>(imageWithAllObjects);
            }
        }

        private void AnnotateObject(Image<Rgb24> target, double XMin, double YMin, double XMax, double YMax, string label)
        {
            target.Mutate(ctx =>
            {
                ctx.DrawPolygon(
                    Pens.Solid(SixLabors.ImageSharp.Color.Blue, 2),
                    new PointF[] {
                                new PointF((float)XMin, (float)YMin),
                                new PointF((float)XMin,(float)YMax),
                                new PointF((float)XMax,(float)YMax),
                                new PointF((float)XMax,(float)YMin)
                    });

                ctx.DrawText(
                    $"{label}",
                    SystemFonts.Families.First().CreateFont(16),
                    SixLabors.ImageSharp.Color.Blue,
                    new PointF((float)XMin + 2, (float)YMax - 15));
            });
        }
    }
}
