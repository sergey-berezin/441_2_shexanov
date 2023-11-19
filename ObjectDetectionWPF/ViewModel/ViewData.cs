
using LibraryANN;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ObjectDetectionWPF.ViewModel.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
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
        private readonly IAsyncCommand clearDb;
        private readonly IAsyncCommand loadImagesFromDb;
        
        public IAsyncCommand LoadFiles { get => loadFiles; }
        public IAsyncCommand ClearDb { get => clearDb; }
        public ICommand Cancel { get => cancel; }
        public ICommand ClearCollection { get => clearCollection; }
        public IAsyncCommand LoadImagesFromDb { get => loadImagesFromDb; }

        private readonly IUIFunctions uiFunctions;
        private readonly IExceptionNotifier exceptionNotifier;

        public ObservableCollection<ChoosenImageInfo> ImagesInfoCollection { get; set; }

        private ProcessedImagesDBContext dataBaseContext = new ProcessedImagesDBContext();

        private bool canLoad;
        private bool canCancel;
        
        public ViewData(IUIFunctions _uiFunctions, IExceptionNotifier _exceptionNotifier)
        {
            uiFunctions = _uiFunctions;
            exceptionNotifier = _exceptionNotifier;

            //MainWindow.Init(this);

            canLoad = false;
            canCancel = false;

            ImagesInfoCollection = new ObservableCollection<ChoosenImageInfo>();

            List<string> listOfFileNames;

            clearDb = new AsyncCommand(
                () => ImagesInfoCollection.Count > 0,
                async () =>
                {
                    canLoad = false;
                    await dataBaseContext.TruncateAllTables();
                    canLoad = true;
                });


             loadImagesFromDb = new AsyncCommand(
                () => canLoad,
                async () =>
                {
                    canLoad = false;
                    canCancel = true;
                    tokenSource = new CancellationTokenSource();
                    await LoadAllImagesFromDataBaseAsync(tokenSource);
                });

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
                _ => ImagesInfoCollection.Count > 0,
                _ =>
                {
                    canLoad = false;
                    ImagesInfoCollection.Clear();
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

        public async Task LoadAllImagesFromDataBaseAsync(CancellationTokenSource tokenSource)
        {
            var listOfImagesFromDb = await dataBaseContext.GetAllImages(tokenSource);
            
            foreach (var item in listOfImagesFromDb)
            {
                InsertInAGivenOrder(DbContenetToChoosenImageInfo(item));
            }
            canLoad = true;
            canCancel = false;
        }

        private async Task ProcessingAllFiles(List<string> listOfFileNames)
        {
            try
            {
                foreach (var fileName in listOfFileNames)
                {
                    var shortName = fileName.Split("\\").Last();
                    var imageDbContent = await dataBaseContext.GetImageByNameAsync(shortName, tokenSource);
                    if (imageDbContent != null)
                    {
                        InsertInAGivenOrder(DbContenetToChoosenImageInfo(imageDbContent));
                    }
                    if (!ImagesInfoCollection.Any(x => x.ShortName == shortName))
                    {
                        await ProcessFileAsync(fileName, tokenSource);
                    }
                    /*if (await dataBaseContext.IsContainImageWithNameAsync(shortName))
                    {
                         
                         InsertInAGivenOrder(DbContenetToChoosenImageInfo(imageDbContent));
                    }
                    if (!ImagesInfoCollection.Any(x => x.ShortName == shortName))
                        //&& !fileNamesFromDataBase.Any(x => x == fileName.Split('\\').Last())) // сделать побайтовое сравнение
                        await ProcessFileAsync(fileName, tokenSource);*/
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
                    
                    var imageWithObjects = GetImageSourceWithAllObjects(task);
                    imageWithObjects.RemoveBlackStripes();

                    var loadedImage = new ImageSharpImageSource<Rgb24>(Image.Load<Rgb24>(fileName));
                    
                    var choosenImageInfo = new ChoosenImageInfo(
                        fileName.Split('\\').Last(),
                        listOfClassNames,
                        loadedImage, 
                        imageWithObjects);

                    //var listOfFilesNames = ImagesInfoCollection.ToList();
                    //IComparer<ChoosenImageInfo> comparer = new ChoosenImageInfoComparer();
                    //var index = listOfFilesNames.BinarySearch(choosenImageInfo, comparer);
                    //if (index < 0)
                    //{
                    //    ImagesInfoCollection.Insert(~index, choosenImageInfo);
                    //}
                    InsertInAGivenOrder(choosenImageInfo);

                    if (!await dataBaseContext.AddProcessedImage(choosenImageInfo))
                    {
                        throw new Exception("Error in adding to database");
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

        private ChoosenImageInfo DbContenetToChoosenImageInfo(ImageDbContent imageDbContent)
        {
            Image<Rgb24> loadedImage;
            using (MemoryStream ms = new MemoryStream(imageDbContent.LoadedImage))
            {
                loadedImage = Image.Load<Rgb24>(ms);
            }

            Image<Rgb24> imageWithObjects;
            using (MemoryStream ms = new MemoryStream(imageDbContent.ImageWithObjects))
            {
                imageWithObjects = Image.Load<Rgb24>(ms);
            }

            return new ChoosenImageInfo(
                imageDbContent.Name,
                imageDbContent.ClassNames,
                new ImageSharpImageSource<Rgb24>(loadedImage),
                new ImageSharpImageSource<Rgb24>(imageWithObjects));
        }

        private void InsertInAGivenOrder(ChoosenImageInfo choosenImageInfo)
        {
            var listOfFilesNames = ImagesInfoCollection.ToList();
            IComparer<ChoosenImageInfo> comparer = new ChoosenImageInfoComparer();
            var index = listOfFilesNames.BinarySearch(choosenImageInfo, comparer);
            if (index < 0)
            {
                ImagesInfoCollection.Insert(~index, choosenImageInfo);
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
