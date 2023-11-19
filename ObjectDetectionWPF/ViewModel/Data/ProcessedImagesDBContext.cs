using LibraryANN;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace ObjectDetectionWPF.ViewModel.Data
{
    internal class ProcessedImagesDBContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=ProcessedImagesDataBase;Username=postgres;Password=J37gi");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ClassName>().HasKey(cn => new { cn.ImageId, cn.Name });
            modelBuilder.Entity<ClassName>()
                   //.HasNoKey()
                   .HasOne(cn => cn.Images)
                   .WithMany(i => i.ClassNames)
                   .HasForeignKey(cn => cn.ImageId);
        }
        public DbSet<Images> Images { get; set; }
        public DbSet<ClassName> ClassNames { get; set; }

        public async Task<long> GetNumberOfImagesAsync()
        {
            return await this.Images.CountAsync();
        }

        public async Task<List<ClassName>> GetClassNamesByImageIdAsync(long imageId)
        {
            return await this.ClassNames.Where(x => x.ImageId == imageId).ToListAsync();
        }

        public async Task<List<ClassName>> GetClassNamesByImageIdRangeAsync(List<long> imageIds)
        {
            return await this.ClassNames.Where(x => imageIds.Contains(x.ImageId)).ToListAsync();
        }

        public async Task<List<String>> GetAllNamesFromClassNamesAsync()
        {
            return await this.ClassNames.Select(x => x.Name).Distinct().ToListAsync();
        }

        public async Task<bool> IsContainImageWithNameAsync(string name)
        {
            return await this.Images.AnyAsync(i => i.Name == name);
        }

        public async Task TruncateAllTables()
        {
            await this.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ClassNames\" CASCADE");
            await this.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Images\" CASCADE");
        }
        public async Task<ImageDbContent> GetImageByNameAsync(string name, CancellationTokenSource tokenSource)
        {
            var query = await (from i in this.Images
                               join cn in this.ClassNames on i.Id equals cn.ImageId
                               where i.Name == name
                               select new ImageDbContent
                               {
                                   Name = i.Name,
                                   ClassNames = Enumerable.Repeat(cn.Name, (int)cn.Count).ToList(),
                                   LoadedImage = i.LoadedImage,
                                   ImageWithObjects = i.ImageWithObjects
                               }).AsNoTracking().FirstOrDefaultAsync(tokenSource.Token);
            return query;
        }

        public async Task<List<ImageDbContent>> GetAllImages(CancellationTokenSource tokenSource)
        {

            var query = await this.ClassNames.Include(x => x.Images).Select(cn => new ImageDbContent
            {
                Name = cn.Images.Name,
                ClassNames = Enumerable.Repeat(cn.Name, (int)cn.Count).ToList(),
                LoadedImage = cn.Images.LoadedImage,
                ImageWithObjects = cn.Images.ImageWithObjects
            }).AsNoTracking().ToListAsync(tokenSource.Token);

            var query2 = query.GroupBy(x => x.Name).Select(cn => new ImageDbContent
            {
                Name = cn.Select(x => x.Name).First(),
                ClassNames = cn.Select(x => x.ClassNames).Aggregate((x, y) => y.Concat(x).ToList()),
                LoadedImage = cn.Select(x => x.LoadedImage).First(),
                ImageWithObjects = cn.Select(x => x.ImageWithObjects).First()

            }).ToList();

            return query2;
        }
            

        public async Task<bool> AddProcessedImage(ChoosenImageInfo choosenImageInfo)
        {
            using var transaction = this.Database.BeginTransaction();
            try
            {
                var namesAndTheirQuantity = choosenImageInfo.ClassNames.Select(x => x)
                    .GroupBy(x => x)
                    .Select(x => new { Name = x.Key, Count = x.Count() });

                var images = new Images()
                {
                    Name = choosenImageInfo.ShortName,
                    LoadedImage = choosenImageInfo.LoadedImageSource.ToByteArray(),
                    ImageWithObjects = choosenImageInfo.ImageWithObjectsSource.ToByteArray(),
                };

                images.ClassNames = new List<ClassName>();

                ClassName className = new ClassName();
                foreach (var item in namesAndTheirQuantity)
                {
                    className = new ClassName()
                    {
                        Name = item.Name,
                        Count = item.Count
                    };

                    images.ClassNames.Add(className);
                    className.Images = images;
                }

                await this.Images.AddAsync(images);
                await this.ClassNames.AddAsync(className);
                await this.SaveChangesAsync();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

