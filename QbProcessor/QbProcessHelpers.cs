using QbHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QbModels.QbProcessor
{
    /// <summary>QbProcessor Class</summary>
    public partial class RequestProcessor
    {
        #region Processor Helpers
        /// <summary>Executes the qb request and returns XML string from Quickbooks processor.</summary>
        /// <typeparam name="T">Source request object to read from</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>Fully formed XML response from Quickbooks processor</returns>
        public string ExecuteQbRequest<T>(T request) where T : class, IQbRq => QbObjectProcessor<T>(request, Guid.NewGuid());

        /// <summary>Executes the qb request and returns XML string from Quickbooks processor.</summary>
        /// <typeparam name="T">Source request object to read from</typeparam>
        /// <typeparam name="T2">Destination QBXML model object to insert object of T</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>Fully formed XML response from Quickbooks processor</returns>
        internal string ExecuteQbRequest<T, T2>(T request) where T2 : new() => QbObjectProcessor<T, T2>(request, Guid.NewGuid());

        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <param name="qbRequest">Model object of type T with the source data to add to Quickbooks.</param>
        /// <param name="requesterId">User provided GUID by requester to track sender requests.</param>
        /// <returns>String result from Quickbooks processor</returns>
        internal string QbObjectProcessor<T>(T qbRequest, Guid requesterId) where T : class
        {
            string requestXml = qbRequest.ToString();
            string requestResp;

            try
            {
                requestResp = CallQB(requestXml);
            }
            catch (Exception ex)
            {
                requestResp = ex.Message;
                if (ex.HResult != -2147220445)
                {
                    return $"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}";
                }
            }

            InvokeRequestEvent(this, requesterId, typeof(T).Name, requestResp, requestXml);

            return requestResp;
        }

        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <typeparam name="T2">Class object of type T to convert source data to for XML processing.</typeparam>
        /// <param name="qbDto">Dto object of type T with the source data to add to Quickbooks.</param>
        /// <param name="requesterId">User provided GUID by requester to track sender requests.</param>
        /// <returns>String result from Quickbooks processor</returns>
        internal string QbObjectProcessor<T, T2>(T qbDto, Guid requesterId) where T2 : new()
        {
            string requestXml = default;
            string requestResp;

            try
            {
                var model = new T2();
                var prop = model.GetType().GetProperty(typeof(T).Name);
                prop.SetValue(model, qbDto);
                requestXml = model.ToPlainXML();
                requestResp = CallQB(requestXml);
            }
            catch (Exception ex)
            {
                requestResp = ex.Message;
                if (ex.HResult != -2147220445)
                {
                    return $"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}";
                }
            }

            InvokeRequestEvent(this, requesterId, typeof(T).Name, requestResp, requestXml);

            return requestResp;
        }

        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <typeparam name="T2">Class object of type T to convert source data to for XML processing.</typeparam>
        /// <typeparam name="T3">Model object of type T to convert source data to for XML processing.</typeparam>
        /// <param name="qbDto">Dto object of type T with the source data to add to Quickbooks.</param>
        /// <param name="requesterId">User provided GUID by requester to track sender requests.</param>
        /// <returns>String result from Quickbooks processor</returns>
        internal string QbObjectProcessor<T, T2, T3>(T qbDto, Guid requesterId) where T2 : new() where T3 : new()
        {
            string requestXml;
            string requestResp;

            try
            {
                var model = qbDto.ToModel<T, T2, T3>();
                requestXml = model.ToPlainXML();
                requestResp = CallQB(requestXml);
            }
            catch (Exception ex)
            {
                return $"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}";
            }

            InvokeRequestEvent(this, requesterId, typeof(T2).Name, requestResp, requestXml);

            return requestResp;
        }

        #endregion Processor Helpers
        /// <summary>Executes the qb request and returns XML string from Quickbooks processor.</summary>
        /// <typeparam name="T">Source request object to read from</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>Fully formed XML response from Quickbooks processor</returns>
        public async Task<string> ExecuteQbRequestAsync<T>(T request) where T : class, IQbRq
        {
            return await QbObjectProcessorAsync<T>(request, Guid.NewGuid());
        }


        /// <summary>Build and run a request to add a Quickbooks object</summary>
        /// <typeparam name="T">Dto object of type T with the source data to add to Quickbooks.</typeparam>
        /// <param name="qbRequest">Model object of type T with the source data to add to Quickbooks.</param>
        /// <param name="requesterId">User provided GUID by requester to track sender requests.</param>
        /// <returns>String result from Quickbooks processor</returns>
        internal async Task<string> QbObjectProcessorAsync<T>(T qbRequest, Guid requesterId) where T : class
        {
            string requestXml = qbRequest.ToString();
            string requestResp;

            try
            {
                requestResp = await CallQBAsync(requestXml);
            }
            catch (Exception ex)
            {
                requestResp = ex.Message;
                if (ex.HResult != -2147220445)
                {
                    return $"QbProcessor error: {ex.HResult}{Environment.NewLine}{ex.Message}";
                }
            }

            InvokeRequestEvent(this, requesterId, typeof(T).Name, requestResp, requestXml);

            return requestResp;
        }

    }
}