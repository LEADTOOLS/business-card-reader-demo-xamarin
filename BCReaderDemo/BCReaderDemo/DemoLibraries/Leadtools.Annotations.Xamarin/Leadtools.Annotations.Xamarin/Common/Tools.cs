using Leadtools.Annotations.Engine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Leadtools.Annotations.Xamarin
{
   public static class Tools
   {
      private static byte[] LoadDataFromAssets(string resourcePath, string resourceName)
      {
         var assembly = typeof(Tools).GetTypeInfo().Assembly;
         var stream = assembly.GetManifestResourceStream(resourcePath + resourceName);
         using (MemoryStream ms = new MemoryStream())
         {
            stream.CopyTo(ms);
            return ms.ToArray();
         }
      }

      private static  AnnPicture LoadImageFromResource(string resourcePath, string resourceName)
      {
         byte[] data =  LoadDataFromAssets(resourcePath, resourceName);
         return new AnnPicture(data);
      }

      public static AnnResources LoadResources()
      {
         AnnResources resources = new AnnResources();
         Dictionary<AnnRubberStampType, AnnPicture> rubberStampsResources = resources.RubberStamps;

         IList<AnnPicture> imagesResources = resources.Images;

         string resourcePath = "Leadtools.Annotations.Xamarin.Assets.Objects.Stamps.";
         rubberStampsResources.Add(AnnRubberStampType.StampApproved,  LoadImageFromResource(resourcePath, "Approved.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampAssigned, LoadImageFromResource(resourcePath, "Assigned.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampClient, LoadImageFromResource(resourcePath, "Client.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampChecked, LoadImageFromResource(resourcePath, "Checked.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampCopy, LoadImageFromResource(resourcePath, "Copy.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampDraft, LoadImageFromResource(resourcePath, "Draft.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampExtended, LoadImageFromResource(resourcePath, "Extended.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampFax, LoadImageFromResource(resourcePath, "Fax.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampFaxed, LoadImageFromResource(resourcePath, "Faxed.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampImportant, LoadImageFromResource(resourcePath, "Important.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampInvoice, LoadImageFromResource(resourcePath, "Invoice.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampNotice, LoadImageFromResource(resourcePath, "Notice.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampPaid, LoadImageFromResource(resourcePath, "Paid.png"));

         rubberStampsResources.Add(AnnRubberStampType.StampOfficial, LoadImageFromResource(resourcePath, "Official.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampOnFile, LoadImageFromResource(resourcePath, "OnFile.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampPassed, LoadImageFromResource(resourcePath, "Passed.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampPending, LoadImageFromResource(resourcePath, "Pending.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampProcessed, LoadImageFromResource(resourcePath, "Processed.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampReceived, LoadImageFromResource(resourcePath, "Received.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampRejected, LoadImageFromResource(resourcePath, "Rejected.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampRelease, LoadImageFromResource(resourcePath, "Release.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampSent, LoadImageFromResource(resourcePath, "Sent.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampShipped, LoadImageFromResource(resourcePath, "Shipped.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampTopSecret, LoadImageFromResource(resourcePath, "TopSecret.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampUrgent, LoadImageFromResource(resourcePath, "Urgent.png"));
         rubberStampsResources.Add(AnnRubberStampType.StampVoid, LoadImageFromResource(resourcePath, "Void.png"));

         resourcePath = "Leadtools.Annotations.Xamarin.Assets.Objects.";

         imagesResources.Add(LoadImageFromResource(resourcePath, "Point.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "Lock.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "Hotspot.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "Audio.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "Video.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "EncryptPrimary.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "EncryptSecondary.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "Content.png"));
         imagesResources.Add(LoadImageFromResource(resourcePath, "StickyNote.png"));

         return resources;
      }
   }
}
