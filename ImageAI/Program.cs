using Path = System.IO.Path;
using LibraryANN;

namespace YOLO_csharp
{
    class Program
    {

        static SemaphoreSlim csvFileLock = new SemaphoreSlim(1, 1);
        static ObjectDetection objectDetection = new ObjectDetection();
        static void Main(string[] args)
        {
            var csvFileName = DateTime.Now.ToString().Replace(" ", "_").Replace(",", "_").Replace(":", "_") + ".csv";
            var tokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (o, e) => tokenSource.Cancel();

            Task[] arrayOfTasks;

            try
            {
                var listOfFileNames = ProcessFiles(args);

                arrayOfTasks = new Task[listOfFileNames.Count];

                var currentTaskCount = 0;
                foreach (var fileName in listOfFileNames)
                {
                    //var task = objectDetection.StartTaskWithGetInfo(fileName, tokenSource.Token);
                    var task = ProcessFileAsync(fileName, tokenSource, csvFileName);
                    arrayOfTasks[currentTaskCount] = task;
                    currentTaskCount++;
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
                return;
            }

            Task.WaitAll(arrayOfTasks);
        }

        static async Task<List<ProcessedImageInfo>> ProcessFileAsync(string fileName, 
            CancellationTokenSource tokenSource, string csvFileName)
        {
            var task = await objectDetection.StartTaskWithGetInfo(fileName, tokenSource.Token);
            foreach (var item in task)
            {
                item.SaveAsJpeg();
                csvFileLock.Wait();
                if (!File.Exists(csvFileName))
                    File.AppendAllLines(csvFileName, new List<string>() {
                            $"{nameof(ProcessedImageInfo.fileName)}, " +
                            $"{nameof(ProcessedImageInfo.classNumber)}, " +
                            $"{nameof(ProcessedImageInfo.className)}, " +
                            $"{nameof(ProcessedImageInfo.leftUpperCornerX)}, " +
                            $"{nameof(ProcessedImageInfo.leftUpperCornerY)}, " +
                            $"{nameof(ProcessedImageInfo.width)}, " +
                            $"{nameof(ProcessedImageInfo.height)}" });
                File.AppendAllLines(csvFileName, new List<string>() { item.ToString() });
                csvFileLock.Release();
            }
            return task;
        }

        static List<string> ProcessFiles(string[] fileNames)
        {
            if (fileNames.Length == 0)
                throw new Exception("Enter name(s) of file(s) as an argument.");
            var ListOfFiles = new List<string>();
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName) &&
                    Path.GetExtension(fileName).Contains("jpg") ||
                    Path.GetExtension(fileName).Contains("jpeg") ||
                    Path.GetExtension(fileName).Contains("png") ||
                    Path.GetExtension(fileName).Contains("bmp"))
                    ListOfFiles.Add(fileName);
                else
                    Console.WriteLine($"Invalid format of file named {fileName}, or it doesnt exist");
            }
            if (ListOfFiles.Count == 0)
            {
                throw new Exception("No files can be processed");
            }
            return ListOfFiles;
            
        }
    }
}
