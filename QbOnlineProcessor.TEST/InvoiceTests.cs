using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using QbModels.QBO.ENUM;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestInvoiceModels
    {
        readonly string testName = "IMS Invoice";

        [TestMethod]
        public async Task Step_1_QBOInvoiceQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Invoices
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Invoice", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet Invoice failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            InvoiceOnlineRs InvoiceRs = new(qryRs);
            Assert.IsNull(InvoiceRs.ParseError);
            Assert.AreNotEqual(0, InvoiceRs.TotalInvoices);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOInvoiceAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Invoices
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Invoice"), false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Invoice: {await getRs.Content.ReadAsStringAsync()}");
            
            InvoiceOnlineRs InvoiceRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Invoice
            if (InvoiceRs.Invoices.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            Random rdm = new();
            HttpResponseMessage custQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Customer"));
            if (!custQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await custQryRq.Content.ReadAsStringAsync()}");
            CustomerOnlineRs custRs = new(await custQryRq.Content.ReadAsStringAsync());
            CustomerDto cust = custRs.Customers.ElementAt(rdm.Next(0, custRs.TotalCustomers));

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Service'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.ElementAt(rdm.Next(0, itemRs.TotalItems));

            InvoiceAddRq addRq = new();
            addRq.CustomerRef = new(cust.Id, cust.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice * 3,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 3, UnitPrice = item.UnitPrice },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq, true);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            InvoiceOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalInvoices);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOInvoiceModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Invoice
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Invoice"));
            InvoiceOnlineRs InvoiceRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Invoice
            if (InvoiceRs.TotalInvoices <= 0) Assert.Inconclusive("No Invoice to update.");

            InvoiceDto invoice = InvoiceRs.Invoices.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (invoice == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.ElementAt(rdm.Next(0, itemRs.TotalItems));

            InvoiceModRq modRq = new();
            modRq.CopyDto(invoice);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {invoice.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq, false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            InvoiceOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(invoice.PrivateNote, modRs.Invoices?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBOInvoiceEmailTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Invoice
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Invoice"));
            InvoiceOnlineRs InvoiceRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing Invoice
            if (InvoiceRs.TotalInvoices <= 0) Assert.Inconclusive($"No {testName} to email.");

            InvoiceDto Invoice = InvoiceRs.Invoices.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (Invoice == null) Assert.Inconclusive($"{testName} does not exist.");
            
            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/invoice/{Invoice.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            InvoiceOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(Invoice.PrivateNote, modRs.Invoices?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOInvoicePdfTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Invoice
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Invoice"));
            InvoiceOnlineRs invoiceRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Downloading Invoice
            if (invoiceRs.TotalInvoices <= 0) Assert.Inconclusive($"No {testName} to download.");

            InvoiceDto invoice = invoiceRs.Invoices.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (invoice == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage pdfRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/invoice/{invoice.Id}/pdf", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Invoice pdf download failed: {await pdfRs.Content.ReadAsStringAsync()}");

            string modRs = new(await pdfRs.Content.ReadAsStringAsync());
            Assert.AreEqual(".pdf", IMAGE.GetContentType(modRs).FileExtension(), "Webapi result is not a PDF document.");
            #endregion
        }

        [TestMethod]
        public async Task Step_6_QBOInvoiceDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Invoice
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Invoice"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve Invoice to delete: {await getRs.Content.ReadAsStringAsync()}");
            InvoiceOnlineRs invoiceRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Invoice
            if (invoiceRs.TotalInvoices <= 0) Assert.Inconclusive($"No {testName} to delete.");

            InvoiceDto invoice = invoiceRs.Invoices.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (invoice == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("Invoice", invoice.Id, invoice.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            InvoiceOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.Invoices[0].status, $"Invoice status not Deleted: {modRs.Invoices[0].status}");
            #endregion
        }
    }
}
