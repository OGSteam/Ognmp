using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ognmp.Components.Events;

namespace Ognmp.Components.Installation
{
    /// <summary>
    /// Handles downloading component files with progress tracking.
    /// </summary>
    public class ComponentDownloader
    {
        private readonly HttpClient _httpClient;

        public event EventHandler<DownloadProgressEventArgs> ProgressChanged;

        public ComponentDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Downloads a file from the specified URL to the output path with progress tracking.
        /// </summary>
        /// <param name="url">The download URL</param>
        /// <param name="outputPath">The destination file path</param>
        /// <param name="componentName">The component name for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if download succeeded, false otherwise</returns>
        public async Task<bool> DownloadAsync(
            string url,
            string outputPath,
            string componentName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error("Download URL cannot be null or empty");
                return false;
            }
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                Log.Error("Output path cannot be null or empty");
                return false;
            }

            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                var canReportProgress = totalBytes > 0;

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    if (canReportProgress)
                    {
                        var progressPercentage = (int)((totalRead * 100) / totalBytes);
                        OnProgressChanged(componentName, progressPercentage, totalRead, totalBytes);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Download failed: {ex.Message}");
                return false;
            }
        }

        private void OnProgressChanged(string componentName, int progressPercentage, long bytesReceived, long totalBytes)
        {
            ProgressChanged?.Invoke(this, new DownloadProgressEventArgs(componentName, progressPercentage, bytesReceived, totalBytes));
        }
    }
}
