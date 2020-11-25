// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leadtools.Demos.Utils
{
   public class DownloadProgressChangedEventArgs : EventArgs
   {
      public DownloadProgressChangedEventArgs(string sourceFileUrl, string targetFilePath, long? totalDownloadSize, long bytesReceived, double? percentage)
      {
         SourceFileUrl = sourceFileUrl;
         TargetFilePath = targetFilePath;
         TotalDownloadSize = (totalDownloadSize.HasValue) ? totalDownloadSize.Value : 0;
         BytesReceived = bytesReceived;
         Percentage = (percentage.HasValue) ? percentage.Value : 0;
      }

      public string SourceFileUrl { get; private set; }
      public string TargetFilePath { get; private set; }
      public long TotalDownloadSize { get; private set; }
      public long BytesReceived { get; private set; }
      public double Percentage { get; private set; }
   }

   public class DownloadCompletedEventArgs : EventArgs
   {
      public DownloadCompletedEventArgs(string sourceFileUrl, string targetFilePath, bool cancelled, Exception error)
      {
         SourceFileUrl = sourceFileUrl;
         TargetFilePath = targetFilePath;
         Cancelled = cancelled;
         Error = error;
      }

      public string SourceFileUrl { get; private set; }
      public string TargetFilePath { get; private set; }
      public bool Cancelled { get; private set; }
      public Exception Error { get; private set; }
   }

   public class HttpClientDownloader : IDisposable
   {
      private HttpClient _httpClient = new HttpClient();

      public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

      public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

      public HttpClientDownloader()
      {
      }

      public async Task DownloadFileAsync(string downloadUrl, string targetFilePath, CancellationToken? cancellationToken = null)
      {
         try
         {
            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
               await DownloadFileFromHttpResponseMessage(response, downloadUrl, targetFilePath, cancellationToken);
         }
         catch(Exception ex)
         {
            TriggerDownloadCompleted(downloadUrl, targetFilePath, false, ex);
         }
      }

      private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response, string sourceFileUrl, string targetFilePath, CancellationToken? cancellationToken = null)
      {
         response.EnsureSuccessStatusCode();

         var totalDownloadSize = response.Content.Headers.ContentLength;

         using (var contentStream = await response.Content.ReadAsStreamAsync())
            await ProcessContentStream(sourceFileUrl, targetFilePath, totalDownloadSize, contentStream, cancellationToken);
      }

      private async Task ProcessContentStream(string sourceFileUrl, string targetFilePath, long? totalDownloadSize, Stream contentStream, CancellationToken? cancellationToken = null)
      {
         var totalBytesRead = 0L;
         var buffer = new byte[8192];
         var isMoreToRead = true;
         Exception error = null;

         try
         {
            using (var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true))
            {
               do
               {
                  int bytesRead = 0;
                  if (cancellationToken.HasValue)
                  {
                     bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken.Value);
                  }
                  else
                  {
                     bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                  }

                  if (bytesRead == 0)
                  {
                     isMoreToRead = false;
                     continue;
                  }

                  await fileStream.WriteAsync(buffer, 0, bytesRead);

                  totalBytesRead += bytesRead;
                  TriggerProgressChanged(sourceFileUrl, targetFilePath, totalDownloadSize, totalBytesRead);
               }
               while (isMoreToRead);

            }
         }
         catch(Exception ex)
         {
            error = ex;
         }
         finally
         {
            //the last progress trigger should occur after the file handle has been released or you may get file locked error
            TriggerProgressChanged(sourceFileUrl, targetFilePath, totalDownloadSize, totalBytesRead);
            TriggerDownloadCompleted(sourceFileUrl, targetFilePath, (cancellationToken.HasValue) ? cancellationToken.Value.IsCancellationRequested : false, error);
         }
      }

      private void TriggerProgressChanged(string sourceFileUrl, string targetFilePath, long? totalDownloadSize, long totalBytesRead)
      {
         double? progressPercentage = null;
         if (totalDownloadSize.HasValue)
            progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

         DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs(sourceFileUrl, targetFilePath, totalDownloadSize, totalBytesRead, progressPercentage));
      }

      private void TriggerDownloadCompleted(string sourceFileUrl, string targetFilePath, bool cancelled, Exception error)
      {
         DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(sourceFileUrl, targetFilePath, cancelled, error));
      }

      public void Dispose()
      {
         _httpClient?.Dispose();
      }
   }
}
