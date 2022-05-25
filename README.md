# QbModels.QbProcessor

Simple QuickBooks requests processor.  Used to send QBXML requests to Quickbooks Request Processor and receive the QBXML response string.

 - Requires assembly dependency on the QbModels.dll that can be found at https://github.com/sflagp/QBXML-Object-Models
 - Only works with a running Quickbooks Desktop with an open company data file.  
 - Requires the QBXMLRP2Installer.exe be installed from the Quickbooks SDK.

UPDATE: 5/25/2022
 - Confirmed that instructions running https locally works with production client.

UPDATE: 5/23/2022
 - Uploaded first draft on how to implement https running locally without needing Administrator priveleges.

UPDATE: 5/18/2022
 - Added new projects QbOnline and QbOnlineProcessor.TEST
 - QbOnline is a project to send and receive responses to QuickBooks Online.  The default namespace to use with this project is QbModels.QBOProcessor.  This requires the QbOnlineModels dll using the QbModels.QBO namespace.  The dll will be uploaded to the QBXML_QbModels repository.
 - QbOnlineProcessor.TEST is my unit testing.  You can view the supported models in this project.  I also made https work locally to not use localhost in the redirect uri and without needing to run VS in Administrator mode.  Will eventually be publishing the steps I did to do that.
