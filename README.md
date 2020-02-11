# LEADTOOLS Business Card Reader Demo for Xamarin

This demo falls under the [license located here.](./LICENSE.md)

Powered by patented artificial intelligence and machine learning algorithms, [LEADTOOLS is a collection of award-winning document, medical, multimedia, and imaging SDKs](https://www.leadtools.com)

This Xamarin demo showcases the [LEADTOOLS `BusinessCardReader` Class](https://www.leadtools.com/help/leadtools/v20/dh/fco/businesscardreader--members.html) and the [LEADTOOLS Xamarin Camera Control](https://www.leadtools.com/help/leadtools/v20/mapping/xamarin-camera.html), allowing users to scan and automatically extract information from business cards.  

This is the source code for the apps hosted in Apple's App Store and Google's Play Store:

- [Apple Business Card Scanner with OCR](https://apps.apple.com/us/app/business-card-scanner-with-ocr/id1454744962)
- [Android Business Card Scanner with OCR](https://play.google.com/store/apps/details?id=com.leadtools.xamarin.bcreader)

## Set Up

In order to use any LEADTOOLS functionality, you must have a valid license. You can obtain a fully functional 30-day license [from our website](https://www.leadtools.com/downloads).

Locate the `RasterSupport.SetLicense(licenseFilePath, developerKey);` line in the application and modify the code to point to use your new license and key.

Open the SLN file in Visual Studio. Build the project to restore the [LEADTOOLS NuGet packages](https://www.leadtools.com/downloads/nuget).

## Use

The LEADTOOLS Business Card Scanner application processes images with automatic OCR (Optical Character Recognition) and barcode recognition to extract information from business cards. Users can easily manage, exchange, search, and access an unlimited number of business cards. The scanned business cards can be saved to a virtual card holder so that users can quickly grab, share, add to contacts, export, and store information. Open the demo and hover the camera over a business card to automatically detect and scan the card and extract the information contained on the card.

## Resources

Website: <https://www.leadtools.com/>

Download Full Evaluation: <https://www.leadtools.com/downloads>

Documentation: <https://www.leadtools.com/help/leadtools/v20/dh/to/introduction.html>

Technical Support: <https://www.leadtools.com/support/chat>

[nuget-profile]: https://www.nuget.org/profiles/LEADTOOLS
