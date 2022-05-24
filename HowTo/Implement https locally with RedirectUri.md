# Implementing https locally for RedirectUri

Below are the steps I performed to implement the RedirectUri with https on my local computer using self-signed certificates. I did this to be able to call Quickbooks Online APIs in a Windows application. Currently, I have only verified this works with the Sandbox API (not [http://localhost](http://localhost/)). I use qbo.qbmodels.com as my redirect domain. You can substitute that with whatever domain you want to use.

> Credit to https://stackoverflow.com/questions/11403333/httplistener-with-https-support/11457719#11457719 for helping me figure this out.  Refer to this posting if the steps I have outlined are confusing or don't make sense.

Step 1: The first step to implement https locally is to generate certificates that will allow https to run locally. If you already have valid certificates for your computer, you can skip this step. To generate self signed certificates, load &quot;Developer Command Prompt for Visual Studio&quot; in Administrator mode. From the developer command prompt. Run the following commands in order:

> makecert -n &quot;CN=QbModelsCA&quot; -r -sv QbModelsCA.pvk QbModelsCA.cer
> makecert -sk QbModelsSignedByCA -iv QbModelsCA.pvk -n &quot;CN=QbModels.QBO&quot; -ic QbModelsCA.cer QbModelsSigned.cer -sr localmachine -ss My

The preceding commands will generate three files. QbModelsCA.pvk, QbModelsCA.cer and QbModelsSigned.cer. The QbModelsCA.cer file is your self-signed certificate authority certificate with the QbModelsCA.pvk being the private key for the CA. The QbModelsSigned.cer is your personal self-signed certificate.

At the end of Step 1, you should have successfully generated your self-signed certificates.

Step 2: Import certificates into the Windows certificate store (LocalComputer…not User).

- Run Microsoft Management Console (Start|MMC)
- From MMC, select File menu and then Add/Remove Snap-In (or Ctrl+M)
- From Available Snap-Ins, select Certificates and click the Add button. From the pop-up, select Computer and then Local Computer. Click OK when complete.
 ![](RackMultipart20220523-1-ve4frg_html_ce5d3c58fd7c4883.png)


- Expand the _Certificates (Local Computer)_ tree view and then _Trusted Root Certification Authorities_
- Right click _Certificates_ in the Trusted Root Certification Authorities tree and select All Tasks|import.
- Follow the prompts to import the QbModelsCA.cer certificate first. NOTE: It is important to import the QbModelsCA.cer certificate first.
- After completing the prompts to import the CA certificate, repeat the process for QbModelsSigned.cer certificate.
- Now expand the Personal tree and go to the Certificates sub-tree. If the QbModels.QBO certificate is not listed, right click Certificates and select All Tasks|Import and follow the prompts to import the QbModelsSigned.cer certificate.

At the completion of Step 2, you should imported your valid certificates (self-signed or valid) them into the Windows certificate store for the Local Computer.

Step 3: Get the certificate hash id to bind to SSL.

- While still in MMC Personal tree, double click the QbModels.QBO certificate to view the certificate.
- From the pop-up, select the Details tab.
- Scroll down to Thumbprint and copy the Value.
 ![](RackMultipart20220523-1-ve4frg_html_94db05dd00d5394b.png)

At the completion of Step 3, you should have recorded the certificate hash id that will be needed in the next step.

Step 4: This step will bind the certificate to SSL to be able to run HttpListener locally without administrator privileges.

- From the Developer Command Prompt, run the following command
netsh http add sslcert ipport=0.0.0.0:5778 certhash=[certificate hash id] appid={[Your app id]}
- [certificate hash id] = Certificate hash id from step 3 (Do not type the square brackets)
- [Your app id] = I get this from the .sln file of my Visual Studio project (Do not type the square brackets)
- NOTE: If you get an error from this command, you will need to delete the certificates from the certificate store and start over and try again. If you are receiving an error here, it is most like an issue with the private key not being generated correctly. The only way I got around this is by starting all over again.

At the end of this step, you should have binded the generated certificates to SSL.

Step 5: Open the SSL port for all users so that the Windows app can run HttpListener locally without the need for Administrator privileges. I am using port 5778 but you can use whatever port over 1024 you like.

- From the Developer Command Prompt, run the following command:
> netsh http add urlacl url=&quot;https://qbo.qbmodels.com:5778/callback/&quot; user=everyone

At the end of this step, you now have a valid SSL port that can run locally without needing Administrator privileges.

Step 6: Add a local computer entry to route the https domain back to the local computer.

- Run Command Prompt (Admin) by pressing Alt-X and selecting Command Prompt (Admin)
- From the command prompt, type _cd\windows\system32\drivers\etc_ to change the directory.
- If you are in the correct folder, type _notepad hosts_ to edit the hosts file
- At the end of the file, add the following entry:
127.0.0.1 qbo.qbmodels.com
- Save the file and exit notepad
- To test the entry is successful, run ping to the https domain to validate a response:
ping qbo.qbmodels.com

At the end of this step, you should have successfully added a manual route to your computer to loop the https domain back to the local computer.

If everything went correctly, you should now be able to add [https://qbo.qbmodels.com:5778/callback](https://qbo.qbmodels.com:5778/callback) to your redirect list in your Quickbooks profile. You should now be able to run the QbProcessor Online Test project to get a valid web token.
