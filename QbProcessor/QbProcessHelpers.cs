using QbHelpers;
using System;
using System.Windows.Forms;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Processor Helpers

        /// <summary>Converts to view.</summary>
        /// <typeparam name="T">Object of type T to convert XML string into.</typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns>Object of type T</returns>
        public T ToView<T>(string xml) => QbFunctions.ToView<T>(xml);

        /// <summary>Executes the qb request and returns XML string from Quickbooks processor.</summary>
        /// <typeparam name="T">Source request object to read from</typeparam>
        /// <typeparam name="T2">Destination QBXML model object to insert object of T</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>Fully formed XML response from Quickbooks processor</returns>
        public string ExecuteQbRequest<T>(T request) where T : class => QbObjectProcessor<T>(request, Guid.NewGuid());

        /// <summary>Executes the qb request and returns XML string from Quickbooks processor.</summary>
        /// <typeparam name="T">Source request object to read from</typeparam>
        /// <typeparam name="T2">Destination QBXML model object to insert object of T</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>Fully formed XML response from Quickbooks processor</returns>
        private string ExecuteQbRequest<T, T2>(T request) where T2 : new() => QbObjectProcessor<T, T2>(request, Guid.NewGuid());

        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <param name="qbDto">Dto object of type T with the source data to add to Quickbooks.</param>
        /// <returns>String result from Quickbooks processor</returns>
        internal string QbObjectProcessor<T>(T qbRequest, Guid requesterId) where T : class
        {
            var requestXml = qbRequest.ToString();
            string requestResp;

            try
            {
                requestResp = callQB(requestXml);
            }
            catch (Exception ex)
            {
                requestResp = ex.Message;
                if (ex.HResult != -2147220445)
                {
                    MessageBox.Show($"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}");
                }
            }

            InvokeRequestEvent(this, requesterId, typeof(T).Name, requestResp, requestXml);

            return requestResp;
        }

        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <typeparam name="T2">Class object of type T to convert source data to for XML processing.</typeparam>
        /// <param name="qbDto">Dto object of type T with the source data to add to Quickbooks.</param>
        /// <returns>String result from Quickbooks processor</returns>
        private string QbObjectProcessor<T, T2>(T qbDto, Guid requesterId) where T2 : new()
        {
            var requestXml = default(string);
            string requestResp;

            try
            {
                var model = new T2();
                var prop = model.GetType().GetProperty(typeof(T).Name);
                prop.SetValue(model, qbDto);
                requestXml = model.ToPlainXML();
                requestResp = callQB(requestXml);
            }
            catch (Exception ex)
            {
                requestResp = ex.Message;
                if (ex.HResult != -2147220445)
                {
                    MessageBox.Show($"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}");
                }
            }

            InvokeRequestEvent(this, requesterId, typeof(T).Name, requestResp, requestXml);

            return requestResp;
        }

        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <typeparam name="T2">Class object of type T to convert source data to for XML processing.</typeparam>
        /// <typeparam name="T2">Model object of type T to convert source data to for XML processing.</typeparam>
        /// <param name="qbDto">Dto object of type T with the source data to add to Quickbooks.</param>
        /// <returns>String result from Quickbooks processor</returns>
        private string QbObjectProcessor<T, T2, T3>(T qbDto, Guid requesterId) where T2 : new() where T3 : new()
        {
            var requestXml = default(string);
            string requestResp;

            try
            {
                var model = qbDto.ToModel<T, T2, T3>();
                requestXml = model.ToPlainXML();
                requestResp = callQB(requestXml);
            }
            catch (Exception ex)
            {
                requestResp = ex.Message;
                MessageBox.Show($"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}");
            }

            InvokeRequestEvent(this, requesterId, typeof(T2).Name, requestResp, requestXml);

            return requestResp;
        }

        #endregion Processor Helpers
    }
}